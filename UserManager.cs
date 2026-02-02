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
        private const string UsersFilePath = "users.dat";
        private readonly List<User> users;
        public User CurrentUser { get; private set; }

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

            string hashedPassword = HashPassword(password);
            users.Add(new User
            {
                Username = username,
                PasswordHash = hashedPassword,
                Email = email,
                DateCreation = DateTime.Now,
                DerniereConnexion = DateTime.Now
            });
            SaveUsers();
            return true;
        }

        public bool AuthenticateUser(string username, string password)
        {
            string hashedPassword = HashPassword(password);
            User user = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)
                && u.PasswordHash == hashedPassword);

            if (user != null)
            {
                user.DerniereConnexion = DateTime.Now;
                CurrentUser = user;
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
                user.PasswordHash = HashPassword(newPassword);
                SaveUsers();
                return true;
            }
            return false;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
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