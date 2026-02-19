using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace logiciel_d_impression_3d
{
    public enum EtatLicence
    {
        Valide,
        Essai,
        EssaiExpire,
        Expiree,
        NonActivee
    }

    public static class LicenceManager
    {
        // ── Configuration ──────────────────────────────────────────────────
        public const string UrlServeur = "https://licence.mondomaine.fr";
        private const int GracePeriodeJours = 7;   // Offline grace period
        private const int DureeEssaiJours = 14;
        private const int IntervalleVerifHeures = 24;

        private static readonly string FichierLicence =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "licence.dat");
        private static readonly string FichierEssai =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "essai.dat");

        // ── Données en mémoire ─────────────────────────────────────────────
        private static string _cleActivee;
        private static string _typeLicence;
        private static DateTime _dateExpiration;
        private static DateTime _derniereVerification;
        private static DateTime _debutEssai;

        // ── Machine ID ────────────────────────────────────────────────────
        public static string ObtenirMachineId()
        {
            try
            {
                using (var cle = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Cryptography", false))
                {
                    if (cle != null)
                    {
                        string guid = cle.GetValue("MachineGuid")?.ToString() ?? "";
                        using (SHA256 sha = SHA256.Create())
                        {
                            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(guid));
                            return BitConverter.ToString(hash).Replace("-", "").ToLower();
                        }
                    }
                }
            }
            catch { }

            // Fallback : nom de machine hashé
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(Environment.MachineName));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        // ── Chiffrement DPAPI ─────────────────────────────────────────────
        private static byte[] Chiffrer(string texte)
        {
            byte[] données = Encoding.UTF8.GetBytes(texte);
            return ProtectedData.Protect(données, null, DataProtectionScope.LocalMachine);
        }

        private static string Déchiffrer(byte[] données)
        {
            byte[] déchiffré = ProtectedData.Unprotect(données, null, DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(déchiffré);
        }

        // ── Persistance locale ────────────────────────────────────────────
        private static void SauvegarderLicence()
        {
            string contenu = string.Join("|",
                _cleActivee,
                ObtenirMachineId(),
                _typeLicence,
                _dateExpiration.ToString("O"),
                _derniereVerification.ToString("O")
            );
            File.WriteAllBytes(FichierLicence, Chiffrer(contenu));
        }

        private static bool ChargerLicence()
        {
            if (!File.Exists(FichierLicence)) return false;
            try
            {
                byte[] données = File.ReadAllBytes(FichierLicence);
                string contenu = Déchiffrer(données);
                string[] parts = contenu.Split('|');
                if (parts.Length < 5) return false;

                _cleActivee = parts[0];
                string machineIdStocke = parts[1];
                _typeLicence = parts[2];
                _dateExpiration = DateTime.Parse(parts[3]);
                _derniereVerification = DateTime.Parse(parts[4]);

                // Vérifier machine binding
                if (machineIdStocke != ObtenirMachineId()) return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Clé de registre pour la date de début d'essai (anti-rollback)
        private const string RegistreCleEssai = @"SOFTWARE\LogicielImpression3D";
        private const string RegistreValeurEssai = "EssaiDebut";

        private static DateTime ChargerOuInitierEssai()
        {
            DateTime? dateFichier = null;
            DateTime? dateRegistre = null;

            // Lire depuis le fichier essai.dat
            if (File.Exists(FichierEssai))
            {
                try
                {
                    string contenu = File.ReadAllText(FichierEssai).Trim();
                    if (DateTime.TryParse(contenu, null,
                        System.Globalization.DateTimeStyles.RoundtripKind, out DateTime d))
                        dateFichier = d;
                }
                catch { }
            }

            // Lire depuis le registre Windows (HKCU)
            try
            {
                using (var cle = Registry.CurrentUser.OpenSubKey(RegistreCleEssai, false))
                {
                    if (cle != null)
                    {
                        string val = cle.GetValue(RegistreValeurEssai) as string;
                        if (!string.IsNullOrEmpty(val) &&
                            DateTime.TryParse(val, null,
                                System.Globalization.DateTimeStyles.RoundtripKind, out DateTime d))
                            dateRegistre = d;
                    }
                }
            }
            catch { }

            DateTime debutEssai;

            if (dateFichier == null && dateRegistre == null)
            {
                // Première exécution sur cette machine
                debutEssai = DateTime.UtcNow;
            }
            else if (dateFichier == null)
            {
                // Fichier supprimé — registre fait foi, date la plus ancienne gagne
                debutEssai = dateRegistre.Value;
            }
            else if (dateRegistre == null)
            {
                // Registre absent (première fois sur ce profil) — fichier fait foi
                debutEssai = dateFichier.Value;
            }
            else
            {
                // Les deux existent : on prend la date la plus ANCIENNE
                // (si quelqu'un restaure un vieux fichier, le registre garde la vraie date)
                debutEssai = dateFichier.Value < dateRegistre.Value
                    ? dateFichier.Value
                    : dateRegistre.Value;
            }

            // Persister dans les deux emplacements (mise à jour silencieuse si manquant)
            try { File.WriteAllText(FichierEssai, debutEssai.ToString("O")); } catch { }
            try
            {
                using (var cle = Registry.CurrentUser.CreateSubKey(RegistreCleEssai))
                    cle?.SetValue(RegistreValeurEssai, debutEssai.ToString("O"));
            }
            catch { }

            return debutEssai;
        }

        // ── État de la licence ────────────────────────────────────────────
        public static EtatLicence ObtenirEtat()
        {
            bool licenceChargee = ChargerLicence();

            if (!licenceChargee)
            {
                // Mode essai
                _debutEssai = ChargerOuInitierEssai();
                int joursEcoules = (int)(DateTime.UtcNow - _debutEssai).TotalDays;

                if (joursEcoules < DureeEssaiJours)
                    return EtatLicence.Essai;
                else
                    return EtatLicence.EssaiExpire;
            }

            // Licence présente — vérifier expiration locale
            if (DateTime.UtcNow > _dateExpiration)
            {
                // Essayer re-vérification serveur (peut avoir été renouvelée)
                if (VerifierServeur())
                    return EtatLicence.Valide;
                return EtatLicence.Expiree;
            }

            // Vérification serveur périodique (toutes les 24h)
            double heuresDepuisVerif = (DateTime.UtcNow - _derniereVerification).TotalHours;
            if (heuresDepuisVerif >= IntervalleVerifHeures)
            {
                if (!VerifierServeur())
                {
                    // Serveur injoignable — grace period
                    double joursHorsLigne = (DateTime.UtcNow - _derniereVerification).TotalDays;
                    if (joursHorsLigne > GracePeriodeJours)
                        return EtatLicence.Expiree;
                }
            }

            return EtatLicence.Valide;
        }

        /// <summary>
        /// Retourne true si l'utilisateur a accès aux fonctionnalités premium
        /// (abonnement actif OU période d'essai en cours).
        /// </summary>
        public static bool EstPremiumActif()
        {
            EtatLicence etat = ObtenirEtat();
            return etat == EtatLicence.Valide || etat == EtatLicence.Essai;
        }

        public static int JoursRestantsEssai()
        {
            _debutEssai = ChargerOuInitierEssai();
            int restants = DureeEssaiJours - (int)(DateTime.UtcNow - _debutEssai).TotalDays;
            return Math.Max(0, restants);
        }

        public static int JoursRestantsLicence()
        {
            return Math.Max(0, (int)(_dateExpiration - DateTime.UtcNow).TotalDays);
        }

        // ── Activation ────────────────────────────────────────────────────
        public static bool ActiverCle(string cle, out string messageErreur)
        {
            messageErreur = "";
            cle = cle.ToUpper().Trim();

            if (!FormatCleValide(cle))
            {
                messageErreur = "Format de clé invalide (XXXX-XXXX-XXXX-XXXX attendu)";
                return false;
            }

            try
            {
                string machineId = ObtenirMachineId();
                string corps = $"{{\"cle\":\"{cle}\",\"machine_id\":\"{machineId}\"}}";

                string réponse = AppelerApi("/api/activer", corps);
                if (réponse == null)
                {
                    messageErreur = "Impossible de joindre le serveur. Vérifiez votre connexion Internet.";
                    return false;
                }

                // Parser la réponse JSON manuellement
                string dateExp = ExtraireValeurJson(réponse, "date_expiration");
                string type = ExtraireValeurJson(réponse, "type");

                if (string.IsNullOrEmpty(dateExp))
                {
                    messageErreur = "Réponse serveur invalide.";
                    return false;
                }

                _cleActivee = cle;
                _typeLicence = type ?? "monthly";
                _dateExpiration = DateTime.Parse(dateExp);
                _derniereVerification = DateTime.UtcNow;

                SauvegarderLicence();
                LogManager.Info($"Licence activée : {cle} (expire {_dateExpiration:d})");
                return true;
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse resp)
                {
                    switch ((int)resp.StatusCode)
                    {
                        case 403: messageErreur = "Clé déjà utilisée sur un autre PC, révoquée ou expirée."; break;
                        case 404: messageErreur = "Clé introuvable. Vérifiez votre saisie."; break;
                        default: messageErreur = $"Erreur serveur ({(int)resp.StatusCode})."; break;
                    }
                }
                else
                {
                    messageErreur = "Impossible de joindre le serveur. Vérifiez votre connexion Internet.";
                }
                LogManager.Avertissement($"Échec activation clé {cle} : {messageErreur}");
                return false;
            }
            catch (Exception ex)
            {
                messageErreur = "Erreur inattendue lors de l'activation.";
                LogManager.Erreur("Erreur activation licence", ex);
                return false;
            }
        }

        // ── Vérification serveur ──────────────────────────────────────────
        public static bool VerifierServeur()
        {
            if (string.IsNullOrEmpty(_cleActivee)) return false;

            try
            {
                string machineId = ObtenirMachineId();
                string corps = $"{{\"cle\":\"{_cleActivee}\",\"machine_id\":\"{machineId}\"}}";

                string réponse = AppelerApi("/api/verifier", corps);
                if (réponse == null) return false;

                string dateExp = ExtraireValeurJson(réponse, "date_expiration");
                if (!string.IsNullOrEmpty(dateExp))
                    _dateExpiration = DateTime.Parse(dateExp);

                _derniereVerification = DateTime.UtcNow;
                SauvegarderLicence();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ── Utilitaires réseau ────────────────────────────────────────────
        private static string AppelerApi(string chemin, string corps)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(UrlServeur + chemin);
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Timeout = 8000;
                req.UserAgent = "LogicielImpression3D-LicenceClient/1.0";

                byte[] data = Encoding.UTF8.GetBytes(corps);
                req.ContentLength = data.Length;

                using (var stream = req.GetRequestStream())
                    stream.Write(data, 0, data.Length);

                using (var resp = (HttpWebResponse)req.GetResponse())
                using (var reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    return reader.ReadToEnd();
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse)
            {
                throw; // Re-lancer pour que l'appelant traite le code HTTP
            }
            catch
            {
                return null; // Réseau indisponible
            }
        }

        // ── Parsing JSON minimal ──────────────────────────────────────────
        private static string ExtraireValeurJson(string json, string cle)
        {
            string pattern = $"\"{cle}\":\"";
            int debut = json.IndexOf(pattern);
            if (debut < 0) return null;
            debut += pattern.Length;
            int fin = json.IndexOf("\"", debut);
            if (fin < 0) return null;
            return json.Substring(debut, fin - debut);
        }

        // ── Validation format clé ─────────────────────────────────────────
        public static bool FormatCleValide(string cle)
        {
            if (string.IsNullOrEmpty(cle)) return false;
            string[] parts = cle.Split('-');
            if (parts.Length != 4) return false;
            foreach (string p in parts)
            {
                if (p.Length != 4) return false;
                foreach (char c in p)
                    if (!char.IsLetterOrDigit(c)) return false;
            }
            return true;
        }
    }
}
