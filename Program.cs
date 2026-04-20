using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace logiciel_d_impression_3d
{
    static class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ── Nettoyer les fichiers .old laissés par une mise à jour précédente ──
            NettoyerFichiersOld();

            // ── Enregistrer le protocole URL logiciel3d:// ────────────────────────
            EnregistrerProtocoleUrl();

            // ── Gestion du protocole logiciel3d://scratch?cle=XXXX ────────────────
            string cleScratch = null;
            if (args.Length > 0 && args[0].StartsWith("logiciel3d://scratch", StringComparison.OrdinalIgnoreCase))
                cleScratch = ExtraireCle(args[0]);

            if (cleScratch != null)
            {
                using (var scratch = new ScratchCardForm(cleScratch))
                    scratch.ShowDialog();

                if (!LicenceManager.ActiverCle(cleScratch, out string erreurActivation))
                {
                    MessageBox.Show(
                        $"Clé révélée mais activation échouée :\n{erreurActivation}\n\nVotre clé a aussi été envoyée par email.",
                        "Avertissement", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
            }

            // ── Vérification des mises à jour (silencieuse) ───────────────────────
            UpdateManager.VerifierMiseAJour(afficherSiAJour: false);
            // Si une MAJ est acceptée et réussie, Environment.Exit(0) est appelé
            // dans UpdateManager — le code suivant ne s'exécute pas.

            // ── Authentification ──────────────────────────────────────────────────
            UserManager userManager = new UserManager();
            LoginForm loginForm = new LoginForm(userManager);
            if (loginForm.ShowDialog() != DialogResult.OK)
                return;

            if (loginForm.ModeDemo)
                userManager.ConnecterEnModeDemo();

            // ── Vérification de la licence ────────────────────────────────────────
            bool estAdmin = userManager.CurrentUser?.Role == "admin";
            LicenceManager.Initialiser(userManager.CurrentUser?.Email ?? "");

            if (!estAdmin && !loginForm.ModeDemo)
            {
                EtatLicence etatLicence = LicenceManager.ObtenirEtat();
                ParametresImpressionForm.ReinitialiserParametresPremium();

                if (etatLicence == EtatLicence.Essai)
                {
                    int joursRestants = LicenceManager.JoursRestantsEssai();
                    MessageBox.Show(
                        $"Vous utilisez le logiciel en mode d'essai.\n" +
                        $"Il vous reste {joursRestants} jour(s) avant la fin de votre période d'essai.\n\n" +
                        "Pour continuer après cette période, activez une licence via le menu d'activation.",
                        "Mode essai", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (etatLicence == EtatLicence.EssaiExpire ||
                         etatLicence == EtatLicence.Expiree      ||
                         etatLicence == EtatLicence.NonActivee)
                {
                    string emailUtilisateur = userManager.CurrentUser?.Email ?? "";
                    ActivationForm activationForm = new ActivationForm(etatLicence, emailUtilisateur);
                    if (activationForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                        return;
                }
            }

            Application.Run(new Form1(userManager));
        }

        // ── Nettoyage ─────────────────────────────────────────────────────────────

        private static void NettoyerFichiersOld()
        {
            try
            {
                string exeOld = Application.ExecutablePath + ".old";
                if (File.Exists(exeOld))
                    File.Delete(exeOld);
            }
            catch { }
        }

        // ── Protocole URL logiciel3d:// ───────────────────────────────────────────

        private static void EnregistrerProtocoleUrl()
        {
            try
            {
                string exe = Application.ExecutablePath;
                using (var k = Registry.CurrentUser.CreateSubKey(@"Software\Classes\logiciel3d"))
                {
                    k.SetValue("", "URL:Crea-Cout 3D");
                    k.SetValue("URL Protocol", "");
                    using (var cmd = k.CreateSubKey(@"shell\open\command"))
                        cmd.SetValue("", $"\"{exe}\" \"%1\"");
                }
            }
            catch { }
        }

        private static string ExtraireCle(string url)
        {
            int i = url.IndexOf("cle=", StringComparison.OrdinalIgnoreCase);
            if (i < 0) return null;
            string val = url.Substring(i + 4);
            int amp = val.IndexOf('&');
            val = amp >= 0 ? val.Substring(0, amp) : val;
            val = Uri.UnescapeDataString(val);
            return string.IsNullOrWhiteSpace(val) ? null : val.ToUpper().Trim();
        }
    }
}
