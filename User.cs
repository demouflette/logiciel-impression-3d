using System;

namespace logiciel_d_impression_3d
{
    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime DerniereConnexion { get; set; }

        public User()
        {
            DateCreation = DateTime.Now;
            DerniereConnexion = DateTime.Now;
        }

        public string ToFileString()
        {
            return $"{Username}|{PasswordHash}|{Email}|{DateCreation:O}|{DerniereConnexion:O}";
        }

        public static User FromFileString(string line)
        {
            string[] parts = line.Split('|');
            if (parts.Length >= 2)
            {
                return new User
                {
                    Username = parts[0],
                    PasswordHash = parts[1],
                    Email = parts.Length > 2 ? parts[2] : "",
                    DateCreation = parts.Length > 3 ? DateTime.Parse(parts[3]) : DateTime.Now,
                    DerniereConnexion = parts.Length > 4 ? DateTime.Parse(parts[4]) : DateTime.Now
                };
            }
            return null;
        }
    }
}