using System;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public partial class VerificationEmailForm : Form
    {
        private readonly UserManager _userManager;
        private readonly string _username;
        private readonly string _email;

        public VerificationEmailForm(UserManager userManager, string username, string email)
        {
            InitializeComponent();
            _userManager = userManager;
            _username = username;
            _email = email;
            this.AcceptButton = btnVerifier;
            lblEmailInfo.Text = $"Un code à 6 chiffres a été envoyé à {email}";
            AppliquerTheme();
        }

        private void AppliquerTheme()
        {
            ThemeManager.ApplyThemeToForm(this);
            ThemeManager.StyleAllControls(this);
            lblTitre.ForeColor = ThemeManager.PrimaryBlue;
            lblTitre.Font = ThemeManager.FontTitle;
            lblEmailInfo.ForeColor = ThemeManager.TextSecondary;
            lblInfo.ForeColor = ThemeManager.TextSecondary;
            ThemeManager.StyleButton(btnVerifier, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);
            ThemeManager.StyleButton(btnRenvoyer, ThemeManager.NeutralGray, ThemeManager.NeutralGrayDark);
        }

        // ── Auto-formatage code (tirets tous les 3 chiffres : 123-456) ─────
        private bool _enFormatage = false;

        private void txtCode_TextChanged(object sender, EventArgs e)
        {
            if (_enFormatage) return;
            _enFormatage = true;

            int caret = txtCode.SelectionStart;
            string brut = txtCode.Text.Replace("-", "");
            var sb = new System.Text.StringBuilder();
            int count = 0;
            foreach (char c in brut)
            {
                if (char.IsDigit(c) && count < 6)
                {
                    sb.Append(c);
                    count++;
                }
            }
            string val = sb.ToString();
            // Format : XXX-XXX
            string formate = val.Length > 3 ? val.Substring(0, 3) + "-" + val.Substring(3) : val;
            txtCode.Text = formate;
            txtCode.SelectionStart = Math.Min(caret + (formate.Length - txtCode.Text.Length + 1), formate.Length);

            _enFormatage = false;
            btnVerifier.Enabled = val.Length == 6;
        }

        // ── Vérifier le code ──────────────────────────────────────────────
        private void btnVerifier_Click(object sender, EventArgs e)
        {
            string code = txtCode.Text.Replace("-", "").Trim();
            if (code.Length != 6) return;

            btnVerifier.Enabled = false;
            btnVerifier.Text = "Vérification...";
            lblInfo.Text = "";
            Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            try
            {
                if (_userManager.VerifierCodeEmail(_email, code, out string erreur))
                {
                    MessageBox.Show(
                        "✓ Votre adresse email est maintenant vérifiée !\nVous pouvez vous connecter.",
                        "Email vérifié",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    lblInfo.Text = erreur;
                    lblInfo.ForeColor = ThemeManager.DangerRed;
                    btnVerifier.Enabled = true;
                    btnVerifier.Text = "Vérifier";
                }
            }
            finally
            {
                Cursor = Cursors.Default;
                if (btnVerifier.Text == "Vérification...") btnVerifier.Text = "Vérifier";
            }
        }

        // ── Renvoyer le code ──────────────────────────────────────────────
        private void btnRenvoyer_Click(object sender, EventArgs e)
        {
            btnRenvoyer.Enabled = false;
            Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            try
            {
                bool ok = _userManager.RenvoyerCodeVerification(_username, _email);
                if (ok)
                {
                    lblInfo.Text = "✓ Nouveau code envoyé — vérifiez votre boîte mail.";
                    lblInfo.ForeColor = ThemeManager.SecondaryGreen;
                }
                else
                {
                    lblInfo.Text = "Impossible d'envoyer le code. Réessayez plus tard.";
                    lblInfo.ForeColor = ThemeManager.DangerRed;
                }
            }
            finally
            {
                Cursor = Cursors.Default;
                // Ré-activer après 30 secondes
                var timer = new System.Windows.Forms.Timer { Interval = 30000 };
                timer.Tick += (s, ev) => { btnRenvoyer.Enabled = true; timer.Stop(); };
                timer.Start();
            }
        }

        // ── Ignorer (plus tard) ───────────────────────────────────────────
        private void btnPlusTard_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // ── Lien confirmation web ─────────────────────────────────────────
        private void lnkVerifierWeb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(
                "Vérifiez votre boîte email et cliquez sur le lien « Confirmer mon email ».\n\n" +
                "Une fois confirmé via le navigateur, cliquez sur « Vérifier » avec le code reçu.",
                "Confirmation via email",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
    }
}
