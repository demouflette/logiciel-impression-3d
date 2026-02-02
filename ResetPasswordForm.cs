using System;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public partial class ResetPasswordForm : Form
    {
        private UserManager userManager;

        public ResetPasswordForm(UserManager userManager)
        {
            InitializeComponent();
            this.userManager = userManager;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string newPassword = txtNewPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || 
                string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Les mots de passe ne correspondent pas.", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("Le mot de passe doit contenir au moins 6 caractères.", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (userManager.ResetPassword(username, email, newPassword))
            {
                MessageBox.Show("Votre mot de passe a été réinitialisé avec succès !", "Succès", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Nom d'utilisateur ou email incorrect.", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}