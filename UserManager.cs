using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace logiciel_d_impression_3d
{
    public class UserManager
    {
        private static readonly string UsersFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.dat");
        private readonly List<User> users;
        public User CurrentUser { get; private set; }
        public DateTime PrecedenteConnexion { get; private set; }

        public UserManager()
        {
            users = new List<User>();
            LoadUsers();
        }

        public bool RegisterUser(string username, string password, string email)
        {
            if (users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            string sel = GenererSel();
            string hashedPassword = HashPasswordAvecSel(password, sel);
            var nouvelUtilisateur = new User
            {
                Username = username,
                PasswordHash = $"{sel}:{hashedPassword}",
                Email = email,
                DateCreation = DateTime.Now,
                DerniereConnexion = DateTime.Now,
                Verifie = false,
                Role = "user"
            };
            users.Add(nouvelUtilisateur);
            SaveUsers();

            // Synchroniser avec le serveur (non bloquant sur erreur)
            SynchroniserInscription(username, email);

            return true;
        }

        /// <summary>
        /// Envoie l'inscription au serveur pour déclencher l'email de vérification.
        /// Silencieux en cas d'échec réseau.
        /// </summary>
        public void SynchroniserInscription(string username, string email)
        {
            try
            {
                string corps = $"{{\"nom_utilisateur\":\"{username}\",\"email\":\"{email}\"}}";
                var req = (HttpWebRequest)WebRequest.Create(LicenceManager.UrlServeur + "/api/utilisateurs/inscrire");
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Timeout = 5000;
                req.UserAgent = "LogicielImpression3D-Client/1.0";
                byte[] data = Encoding.UTF8.GetBytes(corps);
                req.ContentLength = data.Length;
                using (var stream = req.GetRequestStream())
                    stream.Write(data, 0, data.Length);
                using (var resp = (HttpWebResponse)req.GetResponse())
                    LogManager.Info($"Inscription synchronisée avec le serveur pour {username}");
            }
            catch (Exception ex)
            {
                LogManager.Avertissement($"Synchronisation inscription impossible : {ex.Message}");
            }
        }

        /// <summary>
        /// Vérifie le code reçu par email auprès du serveur.
        /// Retourne true si vérifié, false sinon.
        /// </summary>
        public bool VerifierCodeEmail(string email, string code, out string erreur)
        {
            erreur = "";
            try
            {
                string corps = $"{{\"email\":\"{email}\",\"code\":\"{code}\"}}";
                var req = (HttpWebRequest)WebRequest.Create(LicenceManager.UrlServeur + "/api/utilisateurs/verifier-code");
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Timeout = 8000;
                req.UserAgent = "LogicielImpression3D-Client/1.0";
                byte[] data = Encoding.UTF8.GetBytes(corps);
                req.ContentLength = data.Length;
                using (var stream = req.GetRequestStream())
                    stream.Write(data, 0, data.Length);
                using ((HttpWebResponse)req.GetResponse()) { }

                // Marquer localement comme vérifié
                User user = users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                if (user != null) { user.Verifie = true; SaveUsers(); }
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

        /// <summary>
        /// Demande le renvoi d'un nouveau code de vérification.
        /// </summary>
        public bool RenvoyerCodeVerification(string username, string email)
        {
            try
            {
                string corps = $"{{\"nom_utilisateur\":\"{username}\",\"email\":\"{email}\"}}";
                var req = (HttpWebRequest)WebRequest.Create(LicenceManager.UrlServeur + "/api/utilisateurs/renvoyer-code");
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Timeout = 8000;
                req.UserAgent = "LogicielImpression3D-Client/1.0";
                byte[] data = Encoding.UTF8.GetBytes(corps);
                req.ContentLength = data.Length;
                using (var stream = req.GetRequestStream())
                    stream.Write(data, 0, data.Length);
                using ((HttpWebResponse)req.GetResponse()) { }
                return true;
            }
            catch { return false; }
        }

        public bool AuthenticateUser(string username, string password)
        {
            User user = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (user == null) return false;

            bool valide = false;

            if (user.PasswordHash.Contains(":"))
            {
                // Nouveau format : sel:hash
                string[] parties = user.PasswordHash.Split(new[] { ':' }, 2);
                string sel = parties[0];
                string hashStocke = parties[1];
                valide = HashPasswordAvecSel(password, sel) == hashStocke;
            }
            else
            {
                // Ancien format sans sel — vérifier et migrer
                string ancienHash = HashPasswordSansSel(password);
                valide = user.PasswordHash == ancienHash;
                if (valide)
                {
                    // Migration vers le nouveau format
                    string sel = GenererSel();
                    user.PasswordHash = $"{sel}:{HashPasswordAvecSel(password, sel)}";
                    LogManager.Info($"Migration du hash de {user.Username} vers format avec sel");
                }
            }

            if (valide)
            {
                CurrentUser = user;
                PrecedenteConnexion = user.DerniereConnexion;
                user.DerniereConnexion = DateTime.Now;
                SaveUsers();
                return true;
            }
            return false;
        }

        public bool ResetPassword(string username, string email, string newPassword)
        {
            User user = users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                string sel = GenererSel();
                user.PasswordHash = $"{sel}:{HashPasswordAvecSel(newPassword, sel)}";
                SaveUsers();
                return true;
            }
            return false;
        }

        private string GenererSel()
        {
            byte[] selBytes = new byte[16];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(selBytes);
            }
            StringBuilder sb = new StringBuilder();
            foreach (byte b in selBytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private string HashPasswordAvecSel(string password, string sel)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sel + password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        private string HashPasswordSansSel(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        private void SaveUsers()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(UsersFilePath))
                {
                    foreach (User user in users)
                    {
                        writer.WriteLine(user.ToFileString());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}");
            }
        }

        private void LoadUsers()
        {
            if (!File.Exists(UsersFilePath))
                return;

            try
            {
                using (StreamReader reader = new StreamReader(UsersFilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        User user = User.FromFileString(line);
                        if (user != null)
                        {
                            users.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Erreur lors du chargement : {ex.Message}");
            }
        }
    }
}