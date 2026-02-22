using System;
using System.Drawing;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public partial class LoginForm : Form
    {
        private UserManager userManager;

        public bool ModeDemo { get; private set; }

        public LoginForm(UserManager manager)
        {
            InitializeComponent();
            userManager = manager;
            this.AcceptButton = btnLogin;
            tabControl1.SelectedIndexChanged += (s, e) =>
            {
                this.AcceptButton = tabControl1.SelectedIndex == 0 ? btnLogin : btnRegister;
            };
            AppliquerTheme();
            AjouterBoutonDemo();
        }

        private void AjouterBoutonDemo()
        {
            bool disponible = DemoManager.EstDemoDisponible();
            int jours = DemoManager.JoursRestants();

            var btnDemo = new Button();
            btnDemo.Width = btnLogin.Width;
            btnDemo.Height = 32;
            // Position calculée à partir du Bottom réel de btnLogin (déjà mis à l'échelle par AutoScale/DPI)
            btnDemo.Location = new Point(btnLogin.Left, btnLogin.Bottom + 15);
            btnDemo.FlatStyle = FlatStyle.Flat;
            btnDemo.FlatAppearance.BorderSize = 0;
            btnDemo.Font = new Font("Segoe UI", 8.5F);
            btnDemo.Cursor = disponible ? Cursors.Hand : Cursors.Default;
            btnDemo.Enabled = disponible;

            if (disponible)
            {
                btnDemo.BackColor = Color.FromArgb(52, 152, 219);
                btnDemo.ForeColor = Color.White;
                btnDemo.Text = $"Essayer sans compte ({jours} jour(s))";
                btnDemo.Click += (s, e) =>
                {
                    ModeDemo = true;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };
            }
            else
            {
                btnDemo.BackColor = Color.FromArgb(220, 220, 220);
                btnDemo.ForeColor = Color.FromArgb(120, 120, 120);
                btnDemo.Text = "Démo expirée — créez un compte";
            }

            // Agrandir la fenêtre si nécessaire pour que le bouton soit visible
            int hauteurRequise = tabPageLogin.Location.Y + btnDemo.Bottom + 20;
            if (this.ClientSize.Height < hauteurRequise)
                this.ClientSize = new Size(this.ClientSize.Width, hauteurRequise);

            tabPageLogin.Controls.Add(btnDemo);
        }

        private void AppliquerTheme()
        {
            ThemeManager.ApplyThemeToForm(this);
            ThemeManager.StyleAllControls(this);

            // Titres colorés
            lblLoginTitle.ForeColor = ThemeManager.PrimaryBlue;
            lblLoginTitle.Font = ThemeManager.FontTitle;
            lblRegisterTitle.ForeColor = ThemeManager.SecondaryGreen;
            lblRegisterTitle.Font = ThemeManager.FontTitle;

            // Boutons spécifiques
            ThemeManager.StyleButton(btnLogin, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);
            ThemeManager.StyleButton(btnRegister, ThemeManager.SecondaryGreen, ThemeManager.SecondaryGreenDark);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtLoginUsername.Text.Trim();
            string password = txtLoginPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (userManager.AuthenticateUser(username, password))
            {
                if (userManager.ConnexionHorsLigne)
                {
                    MessageBox.Show(
                        "Le serveur est temporairement inaccessible.\n" +
                        "Vous êtes connecté en mode hors-ligne.\n\n" +
                        "Certaines fonctionnalités nécessitent une connexion internet.",
                        "Mode hors-ligne",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Nom d'utilisateur ou mot de passe incorrect.", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtRegisterUsername.Text.Trim();
            string email = txtRegisterEmail.Text.Trim();
            string password = txtRegisterPassword.Text;
            string confirmPassword = txtRegisterConfirmPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || 
                string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Veuillez entrer une adresse email valide.", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Les mots de passe ne correspondent pas.", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Le mot de passe doit contenir au moins 6 caractères.", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (userManager.RegisterUser(username, password, email, out string erreurInscription))
            {
                // Passer à l'onglet de connexion et pré-remplir
                tabControl1.SelectedIndex = 0;
                txtLoginUsername.Text = username;
                txtRegisterUsername.Clear();
                txtRegisterEmail.Clear();
                txtRegisterPassword.Clear();
                txtRegisterConfirmPassword.Clear();

                // Déclencher la vérification email
                var verifForm = new VerificationEmailForm(userManager, username, email);
                if (verifForm.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show(
                        $"Inscription réussie et email vérifié !\nBienvenue {username}.",
                        "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Inscription réussie ! Vérifiez votre email quand vous le souhaitez.\n" +
                        "Un rappel vous sera affiché à la prochaine connexion.",
                        "Inscription réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show(
                    string.IsNullOrEmpty(erreurInscription)
                        ? "Ce nom d'utilisateur ou cet email existe déjà."
                        : erreurInscription,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lnkForgotPassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ResetPasswordForm resetForm = new ResetPasswordForm(userManager);
            if (resetForm.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Vous pouvez maintenant vous connecter avec votre nouveau mot de passe.", 
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}