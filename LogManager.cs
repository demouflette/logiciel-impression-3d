using System;
using System.IO;

namespace logiciel_d_impression_3d
{
    /// <summary>
    /// Gestionnaire de logs centralisé. Thread-safe avec rotation automatique.
    /// </summary>
    public static class LogManager
    {
        private static readonly string FichierLog = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "logs.txt");
        private static readonly string FichierLogAncien = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "logs_old.txt");
        private static readonly object verrou = new object();
        private const long TailleMaxOctets = 5 * 1024 * 1024; // 5 Mo

        public static void Info(string message)
        {
            Ecrire("INFO", message);
        }

        public static void Avertissement(string message)
        {
            Ecrire("AVERT", message);
        }

        public static void Erreur(string message, Exception ex = null)
        {
            string ligne = ex != null ? $"{message} | {ex.GetType().Name}: {ex.Message}" : message;
            Ecrire("ERREUR", ligne);
        }

        private static void Ecrire(string niveau, string message)
        {
            lock (verrou)
            {
                try
                {
                    // Rotation si le fichier dépasse la taille max
                    if (File.Exists(FichierLog))
                    {
                        var info = new FileInfo(FichierLog);
                        if (info.Length > TailleMaxOctets)
                        {
                            if (File.Exists(FichierLogAncien))
                                File.Delete(FichierLogAncien);
                            File.Move(FichierLog, FichierLogAncien);
                        }
                    }

                    string ligne = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{niveau}] {message}";
                    File.AppendAllText(FichierLog, ligne + Environment.NewLine);
                }
                catch
                {
                    // Ne pas propager les erreurs de logging
                }
            }
        }
    }
}
