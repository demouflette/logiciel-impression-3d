using System;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public partial class ResetPasswordForm : Form
    {
        private UserManager userManager;
        private bool etape2 = false;
        private string emailSaisi = "";

        public ResetPasswordForm(UserManager userManager)
        {
            InitializeComponent();
            this.userManager = userManager;
            ConfigurerEtape1();
            AppliquerTheme();
            this.AcceptButton = btnReset;
        }

        /// <summary>
        /// Démarre directement à l'étape 2 (code + nouveau mot de passe), email déjà connu.
        /// Utilisé depuis le profil utilisateur quand le code a déjà été envoyé.
        /// </summary>
        public ResetPasswordForm(UserManager userManager, string emailPreRempli)
        {
            InitializeComponent();
            this.userManager = userManager;
            emailSaisi = emailPreRempli;
            ConfigurerEtape2();
            AppliquerTheme();
            this.AcceptButton = btnReset;
        }

        private void AppliquerTheme()
        {
            ThemeManager.ApplyThemeToForm(this);
            ThemeManager.StyleAllControls(this);
            ThemeManager.StyleButton(btnReset, ThemeManager.DangerRed, ThemeManager.DangerRedDark);
            ThemeManager.StyleButton(btnCancel, ThemeManager.NeutralGray, ThemeManager.NeutralGrayDark);
        }

        // ── Étape 1 : saisie de l'email ──────────────────────────────────────

        private void ConfigurerEtape1()
        {
            etape2 = false;
            lblTitle.Text = "Réinitialisation du mot de passe";

            // Seul le champ email est visible
            lblEmail.Visible = true;
            txtEmail.Visible = true;
            txtEmail.Text = "";

            // Le champ "username" sert de champ code en étape 2 — caché pour l'instant
            lblUsername.Text = "Code reçu par email";
            lblUsername.Visible = false;
            txtUsername.Visible = false;
            txtUsername.UseSystemPasswordChar = false;
            txtUsername.Text = "";

            lblNewPassword.Visible = false;
            txtNewPassword.Visible = false;
            lblConfirmPassword.Visible = false;
            txtConfirmPassword.Visible = false;

            btnReset.Text = "Envoyer le code";
            btnReset.Enabled = true;
        }

        // ── Étape 2 : saisie du code + nouveau mot de passe ──────────────────

        private void ConfigurerEtape2()
        {
            etape2 = true;
            lblTitle.Text = "Saisir le code de vérification";

            lblEmail.Visible = false;
            txtEmail.Visible = false;

            lblUsername.Visible = true;
            txtUsername.Visible = true;

            lblNewPassword.Visible = true;
            txtNewPassword.Visible = true;
            txtNewPassword.Text = "";
            lblConfirmPassword.Visible = true;
            txtConfirmPassword.Visible = true;
            txtConfirmPassword.Text = "";

            btnReset.Text = "Réinitialiser";
            btnReset.Enabled = true;
            txtUsername.Focus();
        }

        // ── Clic sur le bouton principal ──────────────────────────────────────

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (!etape2)
                TraiterEtape1();
            else
                TraiterEtape2();
        }

        private void TraiterEtape1()
        {
            string email = txtEmail.Text.Trim();
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Veuillez entrer votre adresse email.", "Champ manquant",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnReset.Enabled = false;
            btnReset.Text = "Envoi en cours...";
            Application.DoEvents();

            if (userManager.DemanderResetPassword(email, out string erreur))
            {
                emailSaisi = email;
                ConfigurerEtape2();
                MessageBox.Show(
                    $"Un code de vérification à 6 chiffres a été envoyé à :\n{email}\n\n" +
                    "Saisissez-le ci-dessous. Il est valable 15 minutes.",
                    "Code envoyé", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                btnReset.Text = "Envoyer le code";
                btnReset.Enabled = true;
                MessageBox.Show(
                    string.IsNullOrEmpty(erreur) ? "Adresse email introuvable." : erreur,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TraiterEtape2()
        {
            string code = txtUsername.Text.Trim();
            string nouveauPassword = txtNewPassword.Text;
            string confirmerPassword = txtConfirmPassword.Text;

            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show("Veuillez saisir le code reçu par email.", "Champ manquant",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(nouveauPassword))
            {
                MessageBox.Show("Veuillez saisir un nouveau mot de passe.", "Champ manquant",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (nouveauPassword != confirmerPassword)
            {
                MessageBox.Show("Les mots de passe ne correspondent pas.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (nouveauPassword.Length < 6)
            {
                MessageBox.Show("Le mot de passe doit contenir au moins 6 caractères.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnReset.Enabled = false;
            btnReset.Text = "Vérification...";
            Application.DoEvents();

            if (userManager.ConfirmerResetPassword(emailSaisi, code, nouveauPassword, out string erreur))
            {
                MessageBox.Show(
                    "Votre mot de passe a été réinitialisé avec succès !\n" +
                    "Vous pouvez maintenant vous connecter.",
                    "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                btnReset.Text = "Réinitialiser";
                btnReset.Enabled = true;
                MessageBox.Show(
                    string.IsNullOrEmpty(erreur) ? "Code incorrect ou expiré." : erreur,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}