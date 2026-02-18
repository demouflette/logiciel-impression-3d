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
