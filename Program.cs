using System;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
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
    }
}
