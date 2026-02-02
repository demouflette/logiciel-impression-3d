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
            
            // OPTIONNEL : Créer un utilisateur de test si aucun utilisateur n'existe
            // Supprimez ce code après vos tests
            if (!System.IO.File.Exists("users.dat") || new System.IO.FileInfo("users.dat").Length == 0)
            {
                userManager.RegisterUser("admin", "admin123", "admin@test.com");
                MessageBox.Show("Utilisateur de test créé :\nNom : admin\nMot de passe : admin123", 
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
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
