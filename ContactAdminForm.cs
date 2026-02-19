using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public partial class ContactAdminForm : Form
    {
        public ContactAdminForm()
        {
            InitializeComponent();
            AppliquerTheme();
        }

        private void AppliquerTheme()
        {
            BackColor = ThemeManager.BackgroundMain;
            ForeColor = ThemeManager.TextPrimary;

            lblEmail.ForeColor   = ThemeManager.TextSecondary;
            lblSujet.ForeColor   = ThemeManager.TextSecondary;
            lblMessage.ForeColor = ThemeManager.TextSecondary;
            lblInfo.ForeColor    = ThemeManager.TextSecondary;

            txtEmail.BackColor   = ThemeManager.BackgroundInput;
            txtEmail.ForeColor   = ThemeManager.TextPrimary;
            txtEmail.BorderStyle = BorderStyle.FixedSingle;

            txtSujet.BackColor   = ThemeManager.BackgroundInput;
            txtSujet.ForeColor   = ThemeManager.TextPrimary;
            txtSujet.BorderStyle = BorderStyle.FixedSingle;

            txtMessage.BackColor   = ThemeManager.BackgroundInput;
            txtMessage.ForeColor   = ThemeManager.TextPrimary;
            txtMessage.BorderStyle = BorderStyle.FixedSingle;

            ThemeManager.StyleButton(btnEnvoyer, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);
            ThemeManager.StyleButton(btnAnnuler, ThemeManager.BackgroundCard, ThemeManager.BorderLight);
        }

        public void PreRemplir(string sujet, string message)
        {
            txtSujet.Text   = sujet;
            txtMessage.Text = message;
        }

        private void btnEnvoyer_Click(object sender, EventArgs e)
        {
            string email   = txtEmail.Text.Trim();
            string sujet   = txtSujet.Text.Trim();
            string message = txtMessage.Text.Trim();

            // Validation
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(sujet) || string.IsNullOrEmpty(message))
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Champs manquants",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!email.Contains("@") || !email.Contains("."))
            {
                MessageBox.Show("Veuillez saisir une adresse email valide.", "Email invalide",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            if (message.Length > 5000)
            {
                MessageBox.Show($"Le message est trop long ({message.Length}/5000 caractères).",
                    "Message trop long", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnEnvoyer.Enabled = false;
            btnEnvoyer.Text = "Envoi...";

            try
            {
                string corps = $"{{\"email_expediteur\":\"{EchapperJson(email)}\"," +
                               $"\"sujet\":\"{EchapperJson(sujet)}\"," +
                               $"\"message\":\"{EchapperJson(message)}\"}}";

                var req = (HttpWebRequest)WebRequest.Create(LicenceManager.UrlServeur + "/api/contact");
                req.Method      = "POST";
                req.ContentType = "application/json";
                req.Timeout     = 10000;
                req.UserAgent   = "LogicielImpression3D-Client/1.0";

                byte[] data = Encoding.UTF8.GetBytes(corps);
                req.ContentLength = data.Length;

                using (var stream = req.GetRequestStream())
                    stream.Write(data, 0, data.Length);

                using (var resp = (HttpWebResponse)req.GetResponse())
                {
                    // 200 OK
                }

                MessageBox.Show(
                    "Votre message a été transmis à l'administration.\n" +
                    "Vous recevrez une réponse à l'adresse indiquée.",
                    "Message envoyé", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse resp)
            {
                string detail = "";
                try
                {
                    using (var reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                        detail = reader.ReadToEnd();
                }
                catch { }
                MessageBox.Show($"Erreur serveur ({(int)resp.StatusCode}) : {detail}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                MessageBox.Show(
                    "Impossible de joindre le serveur.\n" +
                    "Vérifiez votre connexion Internet et réessayez.",
                    "Erreur de connexion", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnEnvoyer.Enabled = true;
                btnEnvoyer.Text    = "Envoyer";
            }
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private static string EchapperJson(string texte)
        {
            return texte
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r\n", "\\n")
                .Replace("\n", "\\n")
                .Replace("\r", "\\n");
        }
    }
}
