using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace logiciel_d_impression_3d
{
    static class Program
    {
        // Clés de registre Inno Setup possibles selon la version du .iss
        // (le AppId génère parfois }}_is1 au lieu de }_is1 selon l'échappement)
        private static readonly string[] ClesRegistreInstall = new[]
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{B7F2E8A9-3C1D-4F6B-9A2E-7D5C8B4F1A3E}}_is1",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{B7F2E8A9-3C1D-4F6B-9A2E-7D5C8B4F1A3E}_is1",
        };

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ── Correction mise à jour depuis %TEMP% ──────────────────────────
            // Si l'exe tourne depuis le dossier temporaire Windows (ex: mis à jour
            // par une ancienne version sans in-place update), on se recopie dans
            // le dossier d'installation et on relance depuis là.
            if (SeRelocaliserSiNecessaire())
                return; // Le .bat va relancer l'appli depuis le bon dossier

            // Vérifier les mises à jour au démarrage (silencieux)
            UpdateManager.VerifierMiseAJour(afficherSiAJour: false);

            // ── Vérification de la licence ────────────────────────────────
            EtatLicence etatLicence = LicenceManager.ObtenirEtat();

            if (etatLicence == EtatLicence.Essai)
            {
                int joursRestants = LicenceManager.JoursRestantsEssai();
                MessageBox.Show(
                    $"Vous utilisez le logiciel en mode d'essai.\n" +
                    $"Il vous reste {joursRestants} jour(s) avant la fin de votre période d'essai.\n\n" +
                    "Pour continuer après cette période, activez une licence via le menu d'activation.",
                    "Mode essai",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else if (etatLicence == EtatLicence.EssaiExpire ||
                     etatLicence == EtatLicence.Expiree ||
                     etatLicence == EtatLicence.NonActivee)
            {
                ActivationForm activationForm = new ActivationForm(etatLicence);
                if (activationForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    // L'utilisateur a fermé sans activer → quitter
                    return;
                }
            }
            // EtatLicence.Valide → on continue directement

            // ── Authentification ──────────────────────────────────────────
            // Créer le UserManager
            UserManager userManager = new UserManager();

            // Afficher d'abord la fenêtre de connexion
            LoginForm loginForm = new LoginForm(userManager);
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                // Si connexion réussie, afficher la fenêtre principale avec le UserManager
                Application.Run(new Form1(userManager));
            }
        }

        /// <summary>
        /// Détecte si l'exe tourne depuis %TEMP%. Si oui, cherche le dossier
        /// d'installation réel via le registre Inno Setup et crée un script
        /// .bat pour se recopier là-bas et se relancer correctement.
        /// </summary>
        /// <returns>true si une relocalisation est en cours (l'app doit quitter)</returns>
        private static bool SeRelocaliserSiNecessaire()
        {
            try
            {
                string exeActuel = Application.ExecutablePath;
                string tempDir = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string startupDir = Application.StartupPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                // Vérifie si on tourne depuis %TEMP% (ou un sous-dossier)
                if (!startupDir.StartsWith(tempDir, StringComparison.OrdinalIgnoreCase))
                    return false;

                // Chercher le dossier d'installation via le registre Inno Setup
                string dossierInstall = null;

                // Essai HKCU en priorité (installation utilisateur), puis HKLM
                foreach (var hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
                {
                    foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
                    {
                        foreach (var cle in ClesRegistreInstall)
                        {
                            try
                            {
                                using (var baseKey = RegistryKey.OpenBaseKey(hive, view))
                                using (var key = baseKey.OpenSubKey(cle))
                                {
                                    if (key != null)
                                    {
                                        string loc = key.GetValue("InstallLocation") as string;
                                        if (!string.IsNullOrEmpty(loc) && Directory.Exists(loc))
                                        {
                                            dossierInstall = loc.TrimEnd(Path.DirectorySeparatorChar);
                                            break;
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                        if (dossierInstall != null) break;
                    }
                    if (dossierInstall != null) break;
                }

                if (string.IsNullOrEmpty(dossierInstall))
                {
                    // Fallback : chemins d'installation courants
                    string[] fallbacks = new[]
                    {
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "Programs", "Logiciel d'Impression 3D"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                            "Logiciel d'Impression 3D"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                            "Logiciel d'Impression 3D"),
                    };
                    foreach (var f in fallbacks)
                    {
                        if (Directory.Exists(f)) { dossierInstall = f; break; }
                    }
                }

                if (string.IsNullOrEmpty(dossierInstall))
                    return false; // Impossible de trouver le dossier → on continue normalement

                // Trouver le nom de l'exe DÉJÀ installé dans le dossier d'installation
                // (ex: "logiciel d'impression 3d.exe"), pas le nom de l'exe temporaire
                // (ex: "logiciel_impression_3d_v1.5.6.exe")
                string nomExeInstalle = null;
                foreach (string f in Directory.GetFiles(dossierInstall, "*.exe"))
                {
                    string fname = Path.GetFileName(f);
                    // Ignorer le désinstalleur et les exes temporaires de mise à jour (contiennent "_v")
                    if (!fname.StartsWith("unins", StringComparison.OrdinalIgnoreCase)
                        && !fname.Contains("_v"))
                    {
                        nomExeInstalle = fname;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(nomExeInstalle))
                    nomExeInstalle = Path.GetFileName(exeActuel); // fallback

                string cibleExe = Path.Combine(dossierInstall, nomExeInstalle);
                string cheminBat = Path.Combine(Path.GetTempPath(), "relocate_impression3d.bat");

                string contenuBat =
                    "@echo off\r\n" +
                    "timeout /t 2 /nobreak >nul\r\n" +
                    $"copy /y \"{exeActuel}\" \"{cibleExe}\"\r\n" +
                    $"start \"\" \"{cibleExe}\"\r\n" +
                    $"del \"{exeActuel}\"\r\n" +
                    "del \"%~f0\"\r\n";

                File.WriteAllText(cheminBat, contenuBat, System.Text.Encoding.Default);

                var psi = new ProcessStartInfo
                {
                    FileName = cheminBat,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };
                Process.Start(psi);
                return true; // Signal : quitter l'appli immédiatement
            }
            catch
            {
                return false; // En cas d'erreur, continuer normalement
            }
        }
    }
}
