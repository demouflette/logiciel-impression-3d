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
        public bool Verifie { get; set; }
        public string Role { get; set; }  // "user" ou "admin"

        public bool EstAdmin => Role == "admin";

        public User()
        {
            DateCreation = DateTime.Now;
            DerniereConnexion = DateTime.Now;
            Verifie = false;
            Role = "user";
        }

        public string ToFileString()
        {
            return $"{Username}|{PasswordHash}|{Email}|{DateCreation:O}|{DerniereConnexion:O}|{Verifie}|{Role}";
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
                    DerniereConnexion = parts.Length > 4 ? DateTime.Parse(parts[4]) : DateTime.Now,
                    Verifie = parts.Length > 5 && bool.TryParse(parts[5], out bool v) && v,
                    Role = parts.Length > 6 ? parts[6] : "user"
                };
            }
            return null;
        }
    }
}