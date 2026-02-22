using System;
using System.Drawing;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public partial class PromoForm : Form
    {
        private readonly int _promoId;
        private readonly string _type;
        private readonly int _joursOfferts;
        private readonly string _email;
        private readonly string _machineId;

        public PromoForm(int promoId, string titre, string message, string type,
                         int joursOfferts, string code,
                         string email, string machineId)
        {
            InitializeComponent();
            _promoId = promoId;
            _type = type;
            _joursOfferts = joursOfferts;
            _email = email;
            _machineId = machineId;

            lblTitre.Text = titre;
            lblMessage.Text = message;

            AppliquerTheme();

            bool estCode = (type ?? "").Trim().Equals("code", StringComparison.OrdinalIgnoreCase);

            if (estCode)
            {
                panelCode.Visible = true;
                lblJours.Text = joursOfferts > 0
                    ? $"Ce code vous offrira {joursOfferts} jour(s) de licence supplémentaires."
                    : "Saisissez le code promotionnel pour bénéficier de l'offre.";

                // Afficher le code en grand et gras au-dessus du champ de saisie
                if (!string.IsNullOrEmpty(code))
                {
                    var lblCodeValeur = new Label();
                    lblCodeValeur.Text = code.ToUpper();
                    lblCodeValeur.Font = new Font("Courier New", 22f, FontStyle.Bold);
                    lblCodeValeur.ForeColor = ThemeManager.PrimaryBlue;
                    lblCodeValeur.AutoSize = false;
                    lblCodeValeur.Size = new Size(432, 52);
                    lblCodeValeur.Location = new Point(0, lblJours.Bottom + 8);
                    lblCodeValeur.TextAlign = ContentAlignment.MiddleCenter;
                    lblCodeValeur.BackColor = Color.Transparent;
                    panelCode.Controls.Add(lblCodeValeur);

                    // Décaler les contrôles sous le label du code
                    int decalage = lblCodeValeur.Bottom + 8;
                    lblCodeLabel.Top = decalage;
                    txtCode.Top = decalage + lblCodeLabel.Height + 4;
                    btnValider.Top = txtCode.Top;
                    panelCode.Height = btnValider.Bottom + 8;

                    // Pré-remplir le champ avec le code
                    txtCode.Text = code.ToUpper();
                }

                this.AcceptButton = btnValider;

                // Ajuster la hauteur totale de la fenêtre
                lblInfo.Top = panelCode.Bottom + 8;
                btnFermer.Top = lblInfo.Bottom + 8;
                this.ClientSize = new Size(this.ClientSize.Width, btnFermer.Bottom + 16);
            }
            else
            {
                // Type "info" : masquer le panel code et réduire la hauteur
                panelCode.Visible = false;
                lblInfo.Visible = false;
                btnFermer.Top = lblMessage.Bottom + 20;
                this.ClientSize = new Size(this.ClientSize.Width, btnFermer.Bottom + 16);
                btnFermer.DialogResult = DialogResult.Cancel;
            }
        }

        private void AppliquerTheme()
        {
            ThemeManager.ApplyThemeToForm(this);
            ThemeManager.StyleAllControls(this);

            lblTitre.Font = ThemeManager.FontTitle;
            lblTitre.ForeColor = ThemeManager.PrimaryBlue;
            lblMessage.ForeColor = ThemeManager.TextPrimary;
            lblJours.ForeColor = ThemeManager.AccentOrange;
            lblInfo.ForeColor = ThemeManager.TextSecondary;

            ThemeManager.StyleButton(btnValider, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);
            ThemeManager.StyleButton(btnFermer, ThemeManager.NeutralGray, ThemeManager.NeutralGrayDark);
        }

        private void btnValider_Click(object sender, EventArgs e)
        {
            string code = txtCode.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(code))
            {
                lblInfo.Text = "Veuillez saisir le code promotionnel.";
                lblInfo.ForeColor = ThemeManager.DangerRed;
                return;
            }

            btnValider.Enabled = false;
            btnValider.Text = "Validation…";
            lblInfo.Text = "";
            Cursor = Cursors.WaitCursor;

            try
            {
                string url = LicenceManager.UrlServeur + "/api/promo/utiliser";
                string corps = string.Format(
                    "{{\"promo_id\":{0},\"email\":{1},\"machine_id\":{2},\"code\":{3}}}",
                    _promoId,
                    JsonStr(_email),
                    JsonStr(_machineId),
                    JsonStr(code));

                using (var client = new WebClient())
                {
                    client.Headers.Add("Content-Type", "application/json");
                    client.Headers.Add("User-Agent", "LogicielImpression3D-Client/1.0");
                    string reponse = client.UploadString(url, "POST", corps);

                    bool ok = reponse.Contains("\"ok\":true") || reponse.Contains("\"ok\": true");
                    if (ok)
                    {
                        string msg = _joursOfferts > 0
                            ? $"Code validé ! {_joursOfferts} jour(s) ajouté(s) à votre licence."
                            : "Code validé ! Merci.";
                        MessageBox.Show(msg, "Promotion activée", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        lblInfo.Text = "Code incorrect ou promotion déjà utilisée.";
                        lblInfo.ForeColor = ThemeManager.DangerRed;
                        btnValider.Enabled = true;
                        btnValider.Text = "Valider le code";
                    }
                }
            }
            catch (WebException ex) when (ex.Response != null)
            {
                try
                {
                    using (var stream = ex.Response.GetResponseStream())
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        string body = reader.ReadToEnd();
                        // Extraire "detail" du JSON d'erreur FastAPI
                        int idx = body.IndexOf("\"detail\"", StringComparison.Ordinal);
                        if (idx >= 0)
                        {
                            idx = body.IndexOf('"', idx + 8);
                            if (idx >= 0)
                            {
                                idx++;
                                int end = body.IndexOf('"', idx);
                                if (end > idx)
                                    lblInfo.Text = body.Substring(idx, end - idx);
                                else lblInfo.Text = "Erreur serveur.";
                            }
                            else lblInfo.Text = "Erreur serveur.";
                        }
                        else lblInfo.Text = "Code incorrect ou promotion expirée.";
                    }
                }
                catch { lblInfo.Text = "Code incorrect ou promotion expirée."; }
                lblInfo.ForeColor = ThemeManager.DangerRed;
                btnValider.Enabled = true;
                btnValider.Text = "Valider le code";
            }
            catch
            {
                lblInfo.Text = "Erreur de connexion au serveur.";
                lblInfo.ForeColor = ThemeManager.DangerRed;
                btnValider.Enabled = true;
                btnValider.Text = "Valider le code";
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnFermer_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private static string JsonStr(string s)
        {
            if (s == null) return "null";
            return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }
    }
}
