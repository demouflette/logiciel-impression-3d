using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            users.Add(new User
            {
                Username = username,
                PasswordHash = $"{sel}:{hashedPassword}",
                Email = email,
                DateCreation = DateTime.Now,
                DerniereConnexion = DateTime.Now
            });
            SaveUsers();
            return true;
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