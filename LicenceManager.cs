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
        public const string UrlServeur = "https://licence.demouflette.fr";
        private const int GracePeriodeJours = 7;   // Offline grace period
        private const int DureeEssaiJours = 14;
        private const int IntervalleVerifHeures = 24;

        // Licence stockée par utilisateur dans %APPDATA% (DPAPI scope CurrentUser)
        private static string FichierLicence => Path.Combine(DossierAppData, "licence.dat");

        // Essai stocké par machine (dossier d'installation partagé)
        private static readonly string FichierEssai =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "essai.dat");

        private static readonly string DossierAppData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DemouFlette", "Logiciel3D");

        // ── Données en mémoire ─────────────────────────────────────────────
        private static string _cleActivee;
        private static string _typeLicence;
        private static DateTime _dateExpiration;
        private static DateTime _derniereVerification;
        private static DateTime _debutEssai;

        // Email de l'utilisateur connecté — initialisé via Initialiser() avant ObtenirEtat()
        private static string _emailUtilisateur = "";

        /// <summary>
        /// Doit être appelé après le login, avant ObtenirEtat().
        /// Associe la vérification de licence à l'email de l'utilisateur connecté.
        /// </summary>
        public static void Initialiser(string email)
        {
            _emailUtilisateur = (email ?? "").Trim().ToLower();
            // Réinitialiser l'état en mémoire pour éviter la pollution entre sessions
            _cleActivee = null;
            _typeLicence = null;
            _dateExpiration = default;
            _derniereVerification = default;
            Directory.CreateDirectory(DossierAppData);
        }

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

        // ── Chiffrement DPAPI (scope CurrentUser = par utilisateur Windows) ──
        private static byte[] Chiffrer(string texte)
        {
            byte[] données = Encoding.UTF8.GetBytes(texte);
            return ProtectedData.Protect(données, null, DataProtectionScope.CurrentUser);
        }

        private static string Déchiffrer(byte[] données)
        {
            byte[] déchiffré = ProtectedData.Unprotect(données, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(déchiffré);
        }

        // ── Persistance locale ────────────────────────────────────────────
        private static void SauvegarderLicence()
        {
            Directory.CreateDirectory(DossierAppData);
            // Format : cle|machineId|type|dateExp|derniereVerif|email
            string contenu = string.Join("|",
                _cleActivee,
                ObtenirMachineId(),
                _typeLicence,
                _dateExpiration.ToString("O"),
                _derniereVerification.ToString("O"),
                _emailUtilisateur
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

                _cleActivee           = parts[0];
                string machineIdStocke = parts[1];
                _typeLicence          = parts[2];
                _dateExpiration       = DateTime.Parse(parts[3]);
                _derniereVerification = DateTime.Parse(parts[4]);

                // Vérifier machine binding
                if (machineIdStocke != ObtenirMachineId()) return false;

                // Vérifier que la licence appartient bien à l'utilisateur connecté
                if (parts.Length >= 6 && !string.IsNullOrEmpty(_emailUtilisateur))
                {
                    string emailStocke = parts[5].Trim().ToLower();
                    if (!emailStocke.Equals(_emailUtilisateur, StringComparison.OrdinalIgnoreCase))
                        return false;
                }

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

        public static DateTime DateExpiration => _dateExpiration;

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
                string corps = $"{{\"cle\":\"{_cleActivee}\",\"machine_id\":\"{machineId}\",\"email\":\"{_emailUtilisateur}\"}}";

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

        // ── Code promo ────────────────────────────────────────────────────
        /// <summary>
        /// Applique un code promo pour l'email donné. Si succès, sauvegarde la licence localement.
        /// </summary>
        public static bool AppliquerCodePromo(string email, string code, out string messageErreur)
        {
            messageErreur = "";
            code = code.Trim().ToUpper();

            if (string.IsNullOrEmpty(code))
            {
                messageErreur = "Veuillez saisir un code promo.";
                return false;
            }

            try
            {
                string machineId = ObtenirMachineId();
                string corps = $"{{\"email\":\"{EchapperJson(email)}\",\"machine_id\":\"{machineId}\",\"code\":\"{EchapperJson(code)}\"}}";

                var req = (HttpWebRequest)WebRequest.Create(UrlServeur + "/api/promo/appliquer-code");
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Timeout = 8000;
                req.UserAgent = "LogicielImpression3D-LicenceClient/1.0";

                byte[] data = Encoding.UTF8.GetBytes(corps);
                req.ContentLength = data.Length;
                using (var stream = req.GetRequestStream())
                    stream.Write(data, 0, data.Length);

                string réponse;
                using (var resp = (HttpWebResponse)req.GetResponse())
                using (var reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    réponse = reader.ReadToEnd();

                string cle     = ExtraireValeurJson(réponse, "cle") ?? "PROMO";
                string dateExp = ExtraireValeurJson(réponse, "date_expiration");
                string jours   = ExtraireValeurJson(réponse, "jours_ajoutes");

                if (string.IsNullOrEmpty(dateExp))
                {
                    messageErreur = "Réponse serveur invalide.";
                    return false;
                }

                // Sauvegarder la licence localement
                _cleActivee          = cle;
                _typeLicence         = "promo";
                _dateExpiration      = DateTime.Parse(dateExp, null, System.Globalization.DateTimeStyles.RoundtripKind);
                _derniereVerification = DateTime.UtcNow;
                SauvegarderLicence();

                LogManager.Info($"Code promo appliqué : +{jours} jours, expire {_dateExpiration:d}");
                return true;
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse resp)
            {
                switch ((int)resp.StatusCode)
                {
                    case 404: messageErreur = "Code invalide, déjà utilisé ou non destiné à votre compte."; break;
                    case 400: messageErreur = "Données incorrectes. Vérifiez le code saisi."; break;
                    default:  messageErreur = $"Erreur serveur ({(int)resp.StatusCode})."; break;
                }
                return false;
            }
            catch (Exception)
            {
                messageErreur = "Impossible de joindre le serveur. Vérifiez votre connexion.";
                return false;
            }
        }

        private static string EchapperJson(string valeur)
        {
            if (valeur == null) return "";
            return valeur.Replace("\\", "\\\\").Replace("\"", "\\\"")
                         .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }

        // ── Publicité bannière ────────────────────────────────────────────
        public static PubInfo ChercherPubActive()
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(UrlServeur + "/api/pub");
                req.Method = "GET";
                req.Timeout = 5000;
                req.UserAgent = "LogicielImpression3D-LicenceClient/1.0";

                using (var resp = (HttpWebResponse)req.GetResponse())
                using (var reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                {
                    string json = reader.ReadToEnd();
                    if (json.Contains("\"active\":false") || json.Contains("\"active\": false"))
                        return null;

                    return new PubInfo
                    {
                        Titre           = ExtraireValeurJson(json, "titre") ?? "",
                        MessageDefilant = ExtraireValeurJson(json, "message_defilant") ?? "",
                        ImageUrl        = ExtraireValeurJson(json, "image_url") ?? "",
                        Lien            = ExtraireValeurJson(json, "lien") ?? "",
                        CouleurFond     = ExtraireValeurJson(json, "couleur_fond") ?? "#3498db",
                        CouleurTexte    = ExtraireValeurJson(json, "couleur_texte") ?? "#ffffff",
                    };
                }
            }
            catch { return null; }
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

    public class PubInfo
    {
        public string Titre           { get; set; }
        public string MessageDefilant { get; set; }
        public string ImageUrl        { get; set; }
        public string Lien            { get; set; }
        public string CouleurFond     { get; set; }
        public string CouleurTexte    { get; set; }
    }
}
