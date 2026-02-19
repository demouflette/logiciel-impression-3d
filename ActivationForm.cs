using System;
using System.Drawing;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public partial class ActivationForm : Form
    {
        private readonly EtatLicence _etatInitial;

        public ActivationForm(EtatLicence etatInitial)
        {
            InitializeComponent();
            _etatInitial = etatInitial;
            this.AcceptButton = btnActiver;
            AppliquerTheme();
            AfficherEtat();
        }

        // ── Thème ─────────────────────────────────────────────────────────
        private void AppliquerTheme()
        {
            ThemeManager.ApplyThemeToForm(this);
            ThemeManager.StyleAllControls(this);

            lblTitre.ForeColor = ThemeManager.PrimaryBlue;
            lblTitre.Font = ThemeManager.FontTitle;
            lblSousTitre.ForeColor = ThemeManager.TextSecondary;
            lblInfo.ForeColor = ThemeManager.TextSecondary;

            ThemeManager.StyleButton(btnActiver, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);
            ThemeManager.StyleButton(btnEssai, ThemeManager.NeutralGray, ThemeManager.NeutralGrayDark);
        }

        // ── Affichage selon l'état ────────────────────────────────────────
        private void AfficherEtat()
        {
            switch (_etatInitial)
            {
                case EtatLicence.EssaiExpire:
                    lblSousTitre.Text = "Votre période d'essai est terminée.";
                    lblSousTitre.ForeColor = ThemeManager.DangerRed;
                    btnEssai.Visible = false;
                    break;

                case EtatLicence.Expiree:
                    lblSousTitre.Text = "Votre abonnement a expiré. Renouvelez votre licence.";
                    lblSousTitre.ForeColor = ThemeManager.AccentOrange;
                    btnEssai.Visible = false;
                    break;

                case EtatLicence.NonActivee:
                    lblSousTitre.Text = "Entrez votre clé de licence pour activer le logiciel.";
                    btnEssai.Visible = false;
                    break;

                case EtatLicence.Essai:
                    int joursRestants = LicenceManager.JoursRestantsEssai();
                    lblSousTitre.Text = $"Mode essai — {joursRestants} jour(s) restant(s).";
                    lblSousTitre.ForeColor = ThemeManager.AccentOrange;
                    btnEssai.Visible = true;
                    btnEssai.Text = $"Continuer l'essai ({joursRestants}j)";
                    break;
            }
        }

        // ── Formatage automatique clé (XXXX-XXXX-XXXX-XXXX) ─────────────
        private bool _enFormatage = false;

        private void txtCle_TextChanged(object sender, EventArgs e)
        {
            if (_enFormatage) return;
            _enFormatage = true;

            int caretPos = txtCle.SelectionStart;
            string brut = txtCle.Text.Replace("-", "").ToUpper();

            // Limiter à 16 caractères alphanumériques
            var sb = new System.Text.StringBuilder();
            int count = 0;
            foreach (char c in brut)
            {
                if (char.IsLetterOrDigit(c) && count < 16)
                {
                    sb.Append(c);
                    count++;
                }
            }

            // Reformater avec tirets
            string formatee = "";
            string val = sb.ToString();
            for (int i = 0; i < val.Length; i++)
            {
                if (i > 0 && i % 4 == 0) formatee += "-";
                formatee += val[i];
            }

            txtCle.Text = formatee;

            // Repositionner le curseur intelligemment
            int newCaret = Math.Min(caretPos + (formatee.Length - txtCle.Text.Length + 1), formatee.Length);
            txtCle.SelectionStart = Math.Max(0, Math.Min(newCaret, txtCle.Text.Length));

            _enFormatage = false;

            // Activer le bouton si format complet
            bool complet = LicenceManager.FormatCleValide(txtCle.Text);
            btnActiver.Enabled = complet;
            lblInfo.Text = complet ? "" : "Format : XXXX-XXXX-XXXX-XXXX";
        }

        // ── Activation ────────────────────────────────────────────────────
        private void btnActiver_Click(object sender, EventArgs e)
        {
            string cle = txtCle.Text.Trim();

            btnActiver.Enabled = false;
            btnActiver.Text = "Activation en cours...";
            lblInfo.Text = "";
            Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            try
            {
                if (LicenceManager.ActiverCle(cle, out string erreur))
                {
                    MessageBox.Show(
                        "✓ Licence activée avec succès !\nMerci pour votre confiance.",
                        "Activation réussie",
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
                    btnActiver.Enabled = true;
                    btnActiver.Text = "Activer";
                }
            }
            finally
            {
                Cursor = Cursors.Default;
                if (btnActiver.Text == "Activation en cours...") btnActiver.Text = "Activer";
            }
        }

        // ── Continuer l'essai ─────────────────────────────────────────────
        private void btnEssai_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // ── Lien achat ────────────────────────────────────────────────────
        private void lnkAcheter_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var contact = new ContactAdminForm();
            contact.PreRemplir("Demande de clé de licence", "Bonjour,\n\nJe souhaite obtenir une clé de licence pour le Logiciel d'impression 3D.\n\nMerci.");
            contact.ShowDialog(this);
        }
    }
}
