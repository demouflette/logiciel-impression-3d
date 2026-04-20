using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public partial class ActivationForm : Form
    {
        private readonly EtatLicence _etatInitial;
        private readonly string _emailUtilisateur;

        // Contrôles de la section code promo (ajoutés dynamiquement)
        private LinkLabel _lnkCodePromo;
        private Panel     _pnlCodePromo;
        private TextBox   _txtCodePromo;
        private Button    _btnAppliquerPromo;
        private Label     _lblInfoPromo;

        public ActivationForm(EtatLicence etatInitial, string emailUtilisateur = "")
        {
            InitializeComponent();
            _etatInitial        = etatInitial;
            _emailUtilisateur   = emailUtilisateur;
            this.AcceptButton   = btnActiver;
            AppliquerTheme();
            AfficherEtat();
            InitialiserSectionPromo();
        }

        // ── Thème ─────────────────────────────────────────────────────────
        private void AppliquerTheme()
        {
            ThemeManager.ApplyThemeToForm(this);
            ThemeManager.StyleAllControls(this);

            // Remplacer lblTitre par le logo image
            lblTitre.Visible = false;
            AjouterLogoTitre();

            lblSousTitre.ForeColor = ThemeManager.TextSecondary;
            lblInfo.ForeColor = ThemeManager.TextSecondary;

            ThemeManager.StyleButton(btnActiver, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);
            ThemeManager.StyleButton(btnEssai, ThemeManager.NeutralGray, ThemeManager.NeutralGrayDark);
        }

        private void AjouterLogoTitre()
        {
            try
            {
                // Charger l'image embarquée dans l'assembly
                using (var stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("logiciel_d_impression_3d.logo_activation.png"))
                {
                    if (stream == null) return;

                    var logo = Image.FromStream(stream);
                    int logoH = 70;
                    int logoW = (int)(logo.Width * (logoH / (double)logo.Height));
                    logoW = Math.Min(logoW, this.ClientSize.Width - 40);

                    var picLogo = new PictureBox
                    {
                        Image    = logo,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Location = new Point((this.ClientSize.Width - logoW) / 2, lblTitre.Top),
                        Size     = new Size(logoW, logoH),
                    };
                    this.Controls.Add(picLogo);

                    // Décaler tous les contrôles sous le titre vers le bas
                    int delta = logoH - lblTitre.Height;
                    foreach (Control c in this.Controls)
                    {
                        if (c != picLogo && c.Top > lblTitre.Top)
                            c.Top += delta;
                    }
                    this.Height += delta;
                }
            }
            catch { }
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

        // ── Achat PayPal ──────────────────────────────────────────────────
        private void btnAcheterPaypal_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(LicenceManager.UrlServeur + "/acheter");
            }
            catch { }
        }

        // ── Lien contact ─────────────────────────────────────────────────
        private void lnkAcheter_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var contact = new ContactAdminForm();
            contact.PreRemplir("Demande de clé de licence", "Bonjour,\n\nJe souhaite obtenir une clé de licence pour le Logiciel d'impression 3D.\n\nMerci.");
            contact.ShowDialog(this);
        }

        // ── Section code promo ────────────────────────────────────────────
        private void InitialiserSectionPromo()
        {
            // Lien "J'ai un code promo ?" positionné sous lnkAcheter
            _lnkCodePromo = new LinkLabel
            {
                Text      = "J'ai un code promo",
                Location  = new Point(lnkAcheter.Left, lnkAcheter.Bottom + 10),
                Size      = new Size(200, 18),
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = ThemeManager.PrimaryBlue,
            };
            _lnkCodePromo.LinkClicked += (s, e) => BasculerSectionPromo();

            // Panel conteneur (masqué par défaut)
            _pnlCodePromo = new Panel
            {
                Location = new Point(lnkAcheter.Left, _lnkCodePromo.Bottom + 8),
                Size     = new Size(this.ClientSize.Width - lnkAcheter.Left * 2, 62),
                Visible  = false,
            };

            // Champ de saisie du code
            _txtCodePromo = new TextBox
            {
                Location        = new Point(0, 0),
                Size            = new Size(230, 30),
                Font            = new Font("Courier New", 11f, FontStyle.Bold),
                CharacterCasing = CharacterCasing.Upper,
            };

            // Bouton Appliquer
            _btnAppliquerPromo = new Button
            {
                Text     = "Appliquer",
                Location = new Point(_txtCodePromo.Right + 8, 0),
                Size     = new Size(95, 30),
                Font     = new Font("Segoe UI", 9f, FontStyle.Bold),
            };
            ThemeManager.StyleButton(_btnAppliquerPromo, ThemeManager.AccentOrange, ThemeManager.AccentOrange);
            _btnAppliquerPromo.Click += BtnAppliquerPromo_Click;

            // Label d'information promo
            _lblInfoPromo = new Label
            {
                Location  = new Point(0, _txtCodePromo.Bottom + 4),
                Size      = new Size(_pnlCodePromo.Width, 18),
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = ThemeManager.TextSecondary,
                Text      = "",
            };

            _pnlCodePromo.Controls.AddRange(new Control[] { _txtCodePromo, _btnAppliquerPromo, _lblInfoPromo });

            this.Controls.Add(_lnkCodePromo);
            this.Controls.Add(_pnlCodePromo);
        }

        private void BasculerSectionPromo()
        {
            bool visible = !_pnlCodePromo.Visible;
            _pnlCodePromo.Visible = visible;

            // Agrandir / réduire le formulaire
            int delta = _pnlCodePromo.Height + 10;
            this.Height += visible ? delta : -delta;

            if (visible)
                _txtCodePromo.Focus();
        }

        private void BtnAppliquerPromo_Click(object sender, EventArgs e)
        {
            string code = _txtCodePromo.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                _lblInfoPromo.Text      = "Veuillez saisir un code promo.";
                _lblInfoPromo.ForeColor = ThemeManager.DangerRed;
                return;
            }

            if (string.IsNullOrEmpty(_emailUtilisateur))
            {
                _lblInfoPromo.Text      = "Impossible d'identifier votre compte. Reconnectez-vous.";
                _lblInfoPromo.ForeColor = ThemeManager.DangerRed;
                return;
            }

            _btnAppliquerPromo.Enabled = false;
            _btnAppliquerPromo.Text    = "...";
            _lblInfoPromo.Text         = "";
            Cursor                     = Cursors.WaitCursor;
            Application.DoEvents();

            try
            {
                if (LicenceManager.AppliquerCodePromo(_emailUtilisateur, code, out string erreur))
                {
                    MessageBox.Show(
                        "✓ Code promo appliqué avec succès !\nVotre accès a été prolongé.",
                        "Code promo accepté",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    _lblInfoPromo.Text      = erreur;
                    _lblInfoPromo.ForeColor = ThemeManager.DangerRed;
                }
            }
            finally
            {
                Cursor                     = Cursors.Default;
                _btnAppliquerPromo.Enabled = true;
                _btnAppliquerPromo.Text    = "Appliquer";
            }
        }
    }
}
