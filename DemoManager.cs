using System;
using System.IO;
using Microsoft.Win32;

namespace logiciel_d_impression_3d
{
    /// <summary>
    /// Gère la période de démonstration sans compte (7 jours).
    /// Stocke la date de début dans un fichier + le registre (anti-triche).
    /// </summary>
    public static class DemoManager
    {
        private const int DureeJours = 7;
        private const string CleRegistre = @"SOFTWARE\LogicielImpression3D";
        private const string ValeurRegistre = "DemoDebut";

        private static readonly string FichierDemo = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DemouFlette", "Logiciel3D", "demo.dat");

        public static bool EstDemoDisponible()
        {
            return JoursRestants() > 0;
        }

        public static int JoursRestants()
        {
            DateTime debut = InitierOuCharger();
            int restants = DureeJours - (int)(DateTime.UtcNow - debut).TotalDays;
            return Math.Max(0, restants);
        }

        private static DateTime InitierOuCharger()
        {
            DateTime? dateFichier = null;
            DateTime? dateRegistre = null;

            // Lire depuis le fichier
            if (File.Exists(FichierDemo))
            {
                try
                {
                    string contenu = File.ReadAllText(FichierDemo).Trim();
                    if (DateTime.TryParse(contenu, null,
                        System.Globalization.DateTimeStyles.RoundtripKind, out DateTime d))
                        dateFichier = d;
                }
                catch { }
            }

            // Lire depuis le registre
            try
            {
                using (var cle = Registry.CurrentUser.OpenSubKey(CleRegistre, false))
                {
                    if (cle != null)
                    {
                        string val = cle.GetValue(ValeurRegistre) as string;
                        if (!string.IsNullOrEmpty(val) &&
                            DateTime.TryParse(val, null,
                                System.Globalization.DateTimeStyles.RoundtripKind, out DateTime d))
                            dateRegistre = d;
                    }
                }
            }
            catch { }

            DateTime debut;

            if (dateFichier == null && dateRegistre == null)
                debut = DateTime.UtcNow;  // Première utilisation
            else if (dateFichier == null)
                debut = dateRegistre.Value;
            else if (dateRegistre == null)
                debut = dateFichier.Value;
            else
                // Anti-triche : on prend la date la plus ancienne
                debut = dateFichier.Value < dateRegistre.Value ? dateFichier.Value : dateRegistre.Value;

            // Persister dans les deux emplacements
            try
            {
                string dir = Path.GetDirectoryName(FichierDemo);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(FichierDemo, debut.ToString("O"));
            }
            catch { }

            try
            {
                using (var cle = Registry.CurrentUser.CreateSubKey(CleRegistre))
                    cle?.SetValue(ValeurRegistre, debut.ToString("O"));
            }
            catch { }

            return debut;
        }
    }
}
