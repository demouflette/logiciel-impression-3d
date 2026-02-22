using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace logiciel_d_impression_3d
{
    /// <summary>
    /// Gère l'authentification centralisée côté serveur avec cache hors-ligne.
    /// Plus aucun mot de passe n'est stocké localement.
    ///
    /// Règles hors-ligne (les DEUX conditions doivent être remplies) :
    ///   - Dernière connexion en ligne il y a moins de JOURS_OFFLINE_MAX jours
    ///   - Moins de CONNEXIONS_OFFLINE_MAX connexions depuis la dernière synchro
    /// </summary>
    public class UserManager
    {
        // ── Limites hors-ligne ────────────────────────────────────────────────
        private const int JOURS_OFFLINE_MAX = 30;
        private const int CONNEXIONS_OFFLINE_MAX = 10;

        // ── Cache de session ─────────────────────────────────────────────────
        private static readonly string DossierAppData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DemouFlette", "Logiciel3D");

        private static readonly string CachePath = Path.Combine(DossierAppData, "session_cache.dat");

        // ── État courant ──────────────────────────────────────────────────────
        public User CurrentUser { get; private set; }
        public DateTime PrecedenteConnexion { get; private set; }

        // Indique si la dernière connexion a utilisé le cache hors-ligne
        public bool ConnexionHorsLigne { get; private set; }

        // Indique si l'utilisateur utilise le mode démonstration sans compte
        public bool EstModeDemo { get; private set; }

        public void ConnecterEnModeDemo()
        {
            CurrentUser = new User
            {
                Username = "Démonstration",
                Email = "demo@local",
                Role = "user",
                Verifie = false
            };
            EstModeDemo = true;
            ConnexionHorsLigne = false;
        }

        public UserManager()
        {
            Directory.CreateDirectory(DossierAppData);
        }

        // ── Inscription ───────────────────────────────────────────────────────

        /// <summary>
        /// Inscrit un nouvel utilisateur via le serveur.
        /// Retourne true si réussi, false si le nom existe déjà côté serveur.
        /// Lance une exception en cas d'erreur réseau.
        /// </summary>
        public bool RegisterUser(string username, string password, string email, out string erreur)
        {
            erreur = "";
            try
            {
                string corps = $"{{\"nom_utilisateur\":\"{EchapperJson(username)}\"," +
                               $"\"email\":\"{EchapperJson(email)}\"," +
                               $"\"password\":\"{EchapperJson(password)}\"}}";

                string reponse = AppelerApi("/api/utilisateurs/inscrire", "POST", corps);
                return reponse != null;
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse resp)
            {
                switch ((int)resp.StatusCode)
                {
                    case 409: erreur = "Ce nom d'utilisateur ou cet email est déjà utilisé."; break;
                    case 400: erreur = LireCorpsErreur(ex); break;
                    default:  erreur = $"Erreur serveur ({(int)resp.StatusCode})."; break;
                }
                return false;
            }
            catch (Exception ex)
            {
                erreur = $"Impossible de joindre le serveur : {ex.Message}";
                return false;
            }
        }

        // ── Authentification ──────────────────────────────────────────────────

        /// <summary>
        /// Authentifie l'utilisateur. Essaie d'abord le serveur, puis le cache hors-ligne.
        /// </summary>
        public bool AuthenticateUser(string username, string password)
        {
            ConnexionHorsLigne = false;

            // 1. Tentative de connexion en ligne
            try
            {
                return ConnecterEnLigne(username, password);
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse resp)
            {
                // Réponse du serveur : identifiants incorrects ou email non vérifié → pas de fallback
                if ((int)resp.StatusCode == 401 || (int)resp.StatusCode == 403)
                    return false;

                // Autre erreur HTTP (5xx, etc.) → tentative hors-ligne
                LogManager.Avertissement($"Erreur serveur {(int)resp.StatusCode} — tentative connexion hors-ligne");
            }
            catch (Exception ex)
            {
                // Erreur réseau (timeout, pas de connexion) → tentative hors-ligne
                LogManager.Avertissement($"Serveur inaccessible ({ex.Message}) — tentative connexion hors-ligne");
            }

            // 2. Fallback : connexion hors-ligne via le cache
            return ConnecterHorsLigne(username, password);
        }

        private bool ConnecterEnLigne(string username, string password)
        {
            string corps = $"{{\"nom_utilisateur\":\"{EchapperJson(username)}\"," +
                           $"\"password\":\"{EchapperJson(password)}\"}}";

            string json = AppelerApi("/api/utilisateurs/connexion", "POST", corps, timeout: 8000);
            if (json == null) return false;

            // Parser la réponse
            string sessionId   = ExtraireString(json, "session_id");
            string nom         = ExtraireString(json, "nom_utilisateur");
            string email       = ExtraireString(json, "email");
            string role        = ExtraireString(json, "role");
            string dateExp     = ExtraireString(json, "date_expiration");

            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(nom))
                return false;

            // Mettre à jour le cache
            var cache = new SessionCache
            {
                Username = nom,
                Email = email,
                Role = role,
                SessionId = sessionId,
                DateExpirationSession = DateTime.TryParse(dateExp, out DateTime exp) ? exp : DateTime.UtcNow.AddDays(90),
                DateDerniereConnexionOnline = DateTime.UtcNow,
                CompteurOffline = 0,
                // Hash du password pour la vérification hors-ligne
                PasswordHashLocal = HashPasswordLocal(password)
            };
            SauvegarderCache(cache);

            // Remplir CurrentUser
            var ancien = ChargeCacheActuel();
            PrecedenteConnexion = ancien?.DateDerniereConnexionOnline ?? DateTime.Now;
            CurrentUser = new User
            {
                Username = nom,
                Email = email,
                Role = role,
                Verifie = true,
                DateCreation = DateTime.Now,
                DerniereConnexion = DateTime.Now
            };
            ConnexionHorsLigne = false;
            return true;
        }

        private bool ConnecterHorsLigne(string username, string password)
        {
            SessionCache cache = ChargeCacheActuel();
            if (cache == null) return false;

            // Vérifier que c'est bien le même utilisateur
            if (!cache.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
                return false;

            // Vérifier le mot de passe avec le hash local
            if (!VerifierPasswordLocal(password, cache.PasswordHashLocal))
                return false;

            // Vérifier les limites hors-ligne (les DEUX conditions)
            TimeSpan depuisDerniereConnexion = DateTime.UtcNow - cache.DateDerniereConnexionOnline;
            if (depuisDerniereConnexion.TotalDays > JOURS_OFFLINE_MAX)
            {
                LogManager.Avertissement("Limite hors-ligne atteinte : plus de 30 jours sans connexion serveur");
                return false;
            }
            if (cache.CompteurOffline >= CONNEXIONS_OFFLINE_MAX)
            {
                LogManager.Avertissement("Limite hors-ligne atteinte : 10 connexions sans serveur");
                return false;
            }

            // Incrémenter le compteur et sauvegarder
            PrecedenteConnexion = cache.DateDerniereConnexionOnline;
            cache.CompteurOffline++;
            SauvegarderCache(cache);

            CurrentUser = new User
            {
                Username = cache.Username,
                Email = cache.Email,
                Role = cache.Role,
                Verifie = true,
                DateCreation = DateTime.Now,
                DerniereConnexion = DateTime.Now
            };
            ConnexionHorsLigne = true;
            return true;
        }

        // ── Déconnexion ───────────────────────────────────────────────────────

        public void Deconnecter()
        {
            try
            {
                SessionCache cache = ChargeCacheActuel();
                if (cache != null && !string.IsNullOrEmpty(cache.SessionId))
                {
                    string corps = $"{{\"session_id\":\"{cache.SessionId}\"}}";
                    AppelerApi("/api/utilisateurs/deconnexion", "POST", corps, timeout: 5000);
                }
            }
            catch { }

            // Supprimer le cache local
            try { if (File.Exists(CachePath)) File.Delete(CachePath); } catch { }
            CurrentUser = null;
        }

        // ── Vérification code email ───────────────────────────────────────────

        public bool VerifierCodeEmail(string email, string code, out string erreur)
        {
            erreur = "";
            try
            {
                string corps = $"{{\"email\":\"{EchapperJson(email)}\",\"code\":\"{EchapperJson(code)}\"}}";
                AppelerApi("/api/utilisateurs/verifier-code", "POST", corps, timeout: 8000);
                return true;
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse resp)
            {
                switch ((int)resp.StatusCode)
                {
                    case 400: erreur = "Code incorrect ou expiré."; break;
                    case 404: erreur = "Utilisateur introuvable sur le serveur."; break;
                    default:  erreur = "Erreur serveur."; break;
                }
                return false;
            }
            catch
            {
                erreur = "Impossible de joindre le serveur. Réessayez plus tard.";
                return false;
            }
        }

        public bool RenvoyerCodeVerification(string username, string email)
        {
            try
            {
                string corps = $"{{\"nom_utilisateur\":\"{EchapperJson(username)}\"," +
                               $"\"email\":\"{EchapperJson(email)}\"}}";
                AppelerApi("/api/utilisateurs/renvoyer-code", "POST", corps, timeout: 8000);
                return true;
            }
            catch { return false; }
        }

        public void SynchroniserInscription(string username, string email)
        {
            // Gardé pour rétrocompatibilité — l'inscription se fait maintenant directement
            // via RegisterUser qui appelle le serveur. Cette méthode ne fait rien.
        }

        // ── Réinitialisation mot de passe ─────────────────────────────────────

        public bool ResetPassword(string username, string email, string newPassword)
        {
            // Dans la nouvelle architecture, le reset passe par le serveur.
            // Le serveur doit implémenter un endpoint de reset (pas encore disponible).
            // Pour l'instant : non supporté (retourne false).
            return false;
        }

        // ── Cache de session ──────────────────────────────────────────────────

        private SessionCache ChargeCacheActuel()
        {
            try
            {
                if (!File.Exists(CachePath)) return null;
                string ligne = File.ReadAllText(CachePath, Encoding.UTF8).Trim();
                return SessionCache.Deserialiser(ligne);
            }
            catch { return null; }
        }

        private void SauvegarderCache(SessionCache cache)
        {
            try
            {
                File.WriteAllText(CachePath, cache.Serialiser(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                LogManager.Avertissement($"Impossible de sauvegarder le cache de session : {ex.Message}");
            }
        }

        // ── Hash local pour la vérification hors-ligne ───────────────────────
        // Le hash local est utilisé UNIQUEMENT pour la connexion hors-ligne.
        // Il est stocké dans session_cache.dat (pas le mot de passe en clair).

        private static string HashPasswordLocal(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes("offline_salt_3d:" + password));
                var sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private static bool VerifierPasswordLocal(string password, string hashStocke)
        {
            if (string.IsNullOrEmpty(hashStocke)) return false;
            return HashPasswordLocal(password) == hashStocke;
        }

        // ── Appel HTTP générique ──────────────────────────────────────────────

        private static string AppelerApi(string chemin, string methode, string corps = null, int timeout = 10000)
        {
            var req = (HttpWebRequest)WebRequest.Create(LicenceManager.UrlServeur + chemin);
            req.Method = methode;
            req.ContentType = "application/json";
            req.Timeout = timeout;
            req.UserAgent = "LogicielImpression3D-Client/1.0";

            if (!string.IsNullOrEmpty(corps))
            {
                byte[] data = Encoding.UTF8.GetBytes(corps);
                req.ContentLength = data.Length;
                using (var stream = req.GetRequestStream())
                    stream.Write(data, 0, data.Length);
            }

            using (var resp = (HttpWebResponse)req.GetResponse())
            using (var reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                return reader.ReadToEnd();
        }

        private static string LireCorpsErreur(WebException ex)
        {
            try
            {
                using (var reader = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8))
                {
                    string body = reader.ReadToEnd();
                    string detail = ExtraireString(body, "detail");
                    return string.IsNullOrEmpty(detail) ? body : detail;
                }
            }
            catch { return ex.Message; }
        }

        // ── Utilitaires JSON ──────────────────────────────────────────────────

        private static string ExtraireString(string json, string cle)
        {
            string patron = $"\"{cle}\":\"";
            int debut = json.IndexOf(patron, StringComparison.Ordinal);
            if (debut < 0) return "";
            debut += patron.Length;
            int fin = debut;
            var sb = new StringBuilder();
            while (fin < json.Length && json[fin] != '"')
            {
                if (json[fin] == '\\' && fin + 1 < json.Length)
                {
                    fin++;
                    switch (json[fin])
                    {
                        case '"':  sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case 'n':  sb.Append('\n'); break;
                        case 'r':  sb.Append('\r'); break;
                        case 't':  sb.Append('\t'); break;
                        case 'u':
                            if (fin + 4 < json.Length && int.TryParse(json.Substring(fin + 1, 4),
                                System.Globalization.NumberStyles.HexNumber, null, out int cp))
                            {
                                sb.Append((char)cp);
                                fin += 4;
                            }
                            break;
                        default: sb.Append(json[fin]); break;
                    }
                }
                else
                {
                    sb.Append(json[fin]);
                }
                fin++;
            }
            return sb.ToString();
        }

        private static string EchapperJson(string valeur)
        {
            if (valeur == null) return "";
            return valeur.Replace("\\", "\\\\").Replace("\"", "\\\"")
                         .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }
    }

    // ── Cache de session ──────────────────────────────────────────────────────

    /// <summary>
    /// Données de session sauvegardées localement dans %APPDATA%.
    /// Format pipe-délimité : username|email|role|session_id|date_exp|date_online|compteur|password_hash_local
    /// </summary>
    internal class SessionCache
    {
        public string Username               { get; set; }
        public string Email                  { get; set; }
        public string Role                   { get; set; }
        public string SessionId              { get; set; }
        public DateTime DateExpirationSession        { get; set; }
        public DateTime DateDerniereConnexionOnline  { get; set; }
        public int CompteurOffline           { get; set; }
        public string PasswordHashLocal      { get; set; }  // SHA-256 local pour hors-ligne

        public string Serialiser()
        {
            return string.Join("|", new[]
            {
                Username ?? "",
                Email ?? "",
                Role ?? "user",
                SessionId ?? "",
                DateExpirationSession.ToString("O"),
                DateDerniereConnexionOnline.ToString("O"),
                CompteurOffline.ToString(),
                PasswordHashLocal ?? ""
            });
        }

        public static SessionCache Deserialiser(string ligne)
        {
            if (string.IsNullOrWhiteSpace(ligne)) return null;
            string[] p = ligne.Split('|');
            if (p.Length < 7) return null;
            return new SessionCache
            {
                Username = p[0],
                Email = p[1],
                Role = p[2],
                SessionId = p[3],
                DateExpirationSession = DateTime.TryParse(p[4], null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out DateTime exp) ? exp : DateTime.MinValue,
                DateDerniereConnexionOnline = DateTime.TryParse(p[5], null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out DateTime online) ? online : DateTime.MinValue,
                CompteurOffline = int.TryParse(p[6], out int c) ? c : 0,
                PasswordHashLocal = p.Length > 7 ? p[7] : ""
            };
        }
    }
}
