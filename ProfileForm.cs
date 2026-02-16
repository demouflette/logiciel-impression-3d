using System;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public partial class ProfileForm : Form
    {
        private readonly UserManager userManager;
        private readonly User currentUser;

        public ProfileForm(UserManager manager)
        {
            InitializeComponent();
            userManager = manager;
            currentUser = userManager.CurrentUser;
            LoadUserInfo();
            AppliquerTheme();
        }

        private void AppliquerTheme()
        {
            ThemeManager.ApplyThemeToForm(this);
            ThemeManager.StyleAllControls(this);
            ThemeManager.StyleButton(btnChangePassword, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);
            ThemeManager.StyleButton(btnClose, ThemeManager.NeutralGray, ThemeManager.NeutralGrayDark);
        }

        private void LoadUserInfo()
        {
            if (currentUser != null)
            {
                lblUsernameValue.Text = currentUser.Username;
                lblEmailValue.Text = currentUser.Email;
                lblDateCreationValue.Text = currentUser.DateCreation.ToString("dd/MM/yyyy HH:mm");
                lblDerniereConnexionValue.Text = currentUser.DerniereConnexion.ToString("dd/MM/yyyy HH:mm");
            }
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            string currentPassword = txtCurrentPassword.Text;
            string newPassword = txtNewPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || 
                string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // V�rifier le mot de passe actuel
            if (!userManager.AuthenticateUser(currentUser.Username, currentPassword))
            {
                MessageBox.Show("Le mot de passe actuel est incorrect.", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Les nouveaux mots de passe ne correspondent pas.", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("Le nouveau mot de passe doit contenir au moins 6 caract�res.", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (userManager.ResetPassword(currentUser.Username, currentUser.Email, newPassword))
            {
                MessageBox.Show("Mot de passe modifi� avec succ�s !", "Succ�s", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCurrentPassword.Clear();
                txtNewPassword.Clear();
                txtConfirmPassword.Clear();
            }
            else
            {
                MessageBox.Show("Erreur lors de la modification du mot de passe.", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}