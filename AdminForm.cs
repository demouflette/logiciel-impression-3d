using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public partial class AdminForm : Form
    {
        private readonly UserManager _userManager;

        public AdminForm(UserManager userManager)
        {
            InitializeComponent();
            _userManager = userManager;
            AppliquerTheme();
            AjouterMenuContextuelUtilisateurs();
            ChargerDonnees();
        }

        private void AppliquerTheme()
        {
            ThemeManager.ApplyThemeToForm(this);
            ThemeManager.StyleAllControls(this);
            lblTitre.ForeColor = ThemeManager.PrimaryBlue;
            lblTitre.Font = ThemeManager.FontTitle;
            ThemeManager.StyleButton(btnActualiser, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);
            ThemeManager.StyleButton(btnOuvrirAdmin, ThemeManager.SecondaryGreen, ThemeManager.SecondaryGreenDark);
        }

        // ── Chargement des données ────────────────────────────────────────
        private void ChargerDonnees()
        {
            btnActualiser.Enabled = false;
            lblStatut.Text = "Chargement...";
            lblStatut.ForeColor = ThemeManager.TextSecondary;
            Application.DoEvents();

            ChargerUtilisateurs();
            ChargerLicences();

            lblStatut.Text = $"Actualisé le {DateTime.Now:HH:mm:ss}";
            lblStatut.ForeColor = ThemeManager.SecondaryGreen;
            btnActualiser.Enabled = true;
        }

        private void ChargerUtilisateurs()
        {
            try
            {
                string json = AppelerApiAdmin("/api/admin/utilisateurs");
                if (json == null)
                {
                    AfficherErreurGrille(dgvUtilisateurs, "Serveur inaccessible");
                    return;
                }

                var items = ParseJsonArray(json);
                dgvUtilisateurs.Rows.Clear();
                foreach (var item in items)
                {
                    string verifie = item.ContainsKey("verifie") && item["verifie"] == "1" ? "✓ Oui" : "En attente";
                    dgvUtilisateurs.Rows.Add(
                        Val(item,"nom_utilisateur", ""),
                        Val(item,"email", ""),
                        Val(item,"role", "user"),
                        Val(item,"date_inscription", "")?.Length >= 10
                            ? item["date_inscription"].Substring(0, 10) : "",
                        verifie
                    );
                    int rowIdx = dgvUtilisateurs.Rows.Count - 1;
                    dgvUtilisateurs.Rows[rowIdx].Cells[4].Style.ForeColor =
                        verifie.StartsWith("✓") ? ThemeManager.SecondaryGreen : ThemeManager.AccentOrange;
                }
                lblNbUtilisateurs.Text = $"{dgvUtilisateurs.Rows.Count} utilisateur(s)";
            }
            catch (Exception ex)
            {
                AfficherErreurGrille(dgvUtilisateurs, $"Erreur : {ex.Message}");
            }
        }

        private void ChargerLicences()
        {
            try
            {
                string json = AppelerApiAdmin("/api/admin/licences");
                if (json == null)
                {
                    AfficherErreurGrille(dgvLicences, "Serveur inaccessible");
                    return;
                }

                var items = ParseJsonArray(json);
                dgvLicences.Rows.Clear();
                foreach (var item in items)
                {
                    string statut = Val(item,"statut", "");
                    string type = Val(item,"type", "") == "monthly" ? "Mensuel" : "À vie";
                    string machineId = Val(item,"machine_id", "");
                    string machineAffiche = string.IsNullOrEmpty(machineId) ? "Non activée"
                        : machineId.Length > 12 ? machineId.Substring(0, 12) + "…" : machineId;

                    dgvLicences.Rows.Add(
                        Val(item,"cle", ""),
                        Val(item,"email", ""),
                        type,
                        Val(item,"date_expiration", "")?.Length >= 10
                            ? item["date_expiration"].Substring(0, 10) : "",
                        machineAffiche,
                        statut
                    );
                    int rowIdx = dgvLicences.Rows.Count - 1;
                    Color couleurStatut = statut == "active" ? ThemeManager.SecondaryGreen
                        : statut == "revoked" ? ThemeManager.DangerRed
                        : ThemeManager.AccentOrange;
                    dgvLicences.Rows[rowIdx].Cells[5].Style.ForeColor = couleurStatut;
                }
                lblNbLicences.Text = $"{dgvLicences.Rows.Count} licence(s)";
            }
            catch (Exception ex)
            {
                AfficherErreurGrille(dgvLicences, $"Erreur : {ex.Message}");
            }
        }

        // ── Menu contextuel utilisateurs ──────────────────────────────────
        private void AjouterMenuContextuelUtilisateurs()
        {
            var menu = new ContextMenuStrip();

            var itemVerifier = new ToolStripMenuItem("Forcer la vérification email");
            itemVerifier.Click += (s, e) => MenuContextuel_ForcerVerification();

            var itemPassword = new ToolStripMenuItem("Réinitialiser le mot de passe");
            itemPassword.Click += (s, e) => MenuContextuel_ResetPassword();

            var itemTemps = new ToolStripMenuItem("Ajouter du temps d'accès");
            itemTemps.Click += (s, e) => MenuContextuel_AjouterTemps();

            menu.Items.Add(itemVerifier);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemPassword);
            menu.Items.Add(itemTemps);

            dgvUtilisateurs.ContextMenuStrip = menu;
        }

        private string ObtenirEmailSelectionne()
        {
            if (dgvUtilisateurs.SelectedRows.Count == 0 && dgvUtilisateurs.CurrentRow == null)
            {
                MessageBox.Show("Sélectionnez un utilisateur.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            int idx = dgvUtilisateurs.CurrentRow?.Index ?? -1;
            if (idx < 0) return null;
            return dgvUtilisateurs.Rows[idx].Cells[1].Value?.ToString();
        }

        private void MenuContextuel_ForcerVerification()
        {
            string email = ObtenirEmailSelectionne();
            if (string.IsNullOrEmpty(email)) return;

            int idx = dgvUtilisateurs.CurrentRow.Index;
            string verifie = dgvUtilisateurs.Rows[idx].Cells[4].Value?.ToString();
            if (verifie?.StartsWith("✓") == true)
            {
                MessageBox.Show($"L'utilisateur {email} est déjà vérifié.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Forcer la vérification de l'email pour {email} ?",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ForcerVerificationUtilisateur(email, idx);
            }
        }

        private void MenuContextuel_ResetPassword()
        {
            string email = ObtenirEmailSelectionne();
            if (string.IsNullOrEmpty(email)) return;

            using (var form = new SaisieNouveauPasswordForm(email))
            {
                if (form.ShowDialog() == DialogResult.OK)
                    ResetPasswordUtilisateur(email, form.NouveauPassword);
            }
        }

        private void MenuContextuel_AjouterTemps()
        {
            string email = ObtenirEmailSelectionne();
            if (string.IsNullOrEmpty(email)) return;

            using (var form = new SaisieJoursForm(email))
            {
                if (form.ShowDialog() == DialogResult.OK)
                    AjouterTempsAcces(email, form.Jours);
            }
        }

        private void ResetPasswordUtilisateur(string email, string nouveauPassword)
        {
            try
            {
                string corps = $"{{\"email\":\"{EchapperJson(email)}\",\"nouveau_password\":\"{EchapperJson(nouveauPassword)}\"}}";
                string reponse = AppelerApiAdmin("/api/admin/reinitialiser-password", corps);
                if (reponse == null)
                {
                    MessageBox.Show("Serveur inaccessible ou endpoint non disponible.", "Erreur",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                lblStatut.Text = $"✓ Mot de passe réinitialisé pour {email}";
                lblStatut.ForeColor = ThemeManager.SecondaryGreen;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AjouterTempsAcces(string email, int jours)
        {
            try
            {
                string corps = $"{{\"email\":\"{EchapperJson(email)}\",\"jours\":{jours}}}";
                string reponse = AppelerApiAdmin("/api/admin/ajouter-temps", corps);
                if (reponse == null)
                {
                    MessageBox.Show("Serveur inaccessible ou endpoint non disponible.", "Erreur",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                lblStatut.Text = $"✓ {jours} jour(s) ajouté(s) pour {email}";
                lblStatut.ForeColor = ThemeManager.SecondaryGreen;
                ChargerLicences();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string EchapperJson(string valeur)
        {
            if (valeur == null) return "";
            return valeur.Replace("\\", "\\\\").Replace("\"", "\\\"")
                         .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }

        // ── Actions ───────────────────────────────────────────────────────
        private void btnActualiser_Click(object sender, EventArgs e)
        {
            ChargerDonnees();
        }

        private void btnOuvrirAdmin_Click(object sender, EventArgs e)
        {
            try
            {
                string url = LicenceManager.UrlServeur + "/admin";
                System.Diagnostics.Process.Start(url);
            }
            catch { }
        }

        private void dgvUtilisateurs_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string email = dgvUtilisateurs.Rows[e.RowIndex].Cells[1].Value?.ToString();
            string verifie = dgvUtilisateurs.Rows[e.RowIndex].Cells[4].Value?.ToString();
            if (string.IsNullOrEmpty(email)) return;

            if (verifie?.StartsWith("✓") == true)
            {
                MessageBox.Show($"L'utilisateur {email} est déjà vérifié.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Forcer la vérification de l'email pour {email} ?",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ForcerVerificationUtilisateur(email, e.RowIndex);
            }
        }

        private void dgvLicences_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string cle = dgvLicences.Rows[e.RowIndex].Cells[0].Value?.ToString();
            string statut = dgvLicences.Rows[e.RowIndex].Cells[5].Value?.ToString();
            if (string.IsNullOrEmpty(cle)) return;

            if (statut != "active")
            {
                MessageBox.Show($"La licence {cle} n'est pas active.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Révoquer la licence ?\n\n{cle}",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                RevoquerLicence(cle, e.RowIndex);
            }
        }

        private void ForcerVerificationUtilisateur(string email, int rowIndex)
        {
            try
            {
                string corps = $"{{\"nom_utilisateur\":\"\",\"email\":\"{email}\"}}";
                AppelerApiAdmin("/api/admin/verifier-utilisateur", corps);
                dgvUtilisateurs.Rows[rowIndex].Cells[4].Value = "✓ Oui";
                dgvUtilisateurs.Rows[rowIndex].Cells[4].Style.ForeColor = ThemeManager.SecondaryGreen;
                lblStatut.Text = $"✓ Utilisateur {email} vérifié";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RevoquerLicence(string cle, int rowIndex)
        {
            try
            {
                string corps = $"{{\"cle\":\"{cle}\"}}";
                AppelerApiAdmin("/api/admin/revoquer", corps);
                dgvLicences.Rows[rowIndex].Cells[5].Value = "revoked";
                dgvLicences.Rows[rowIndex].Cells[5].Style.ForeColor = ThemeManager.DangerRed;
                lblStatut.Text = $"✓ Licence {cle} révoquée";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Appels API ────────────────────────────────────────────────────
        private string AppelerApiAdmin(string chemin, string corps = null)
        {
            string adminKey = LireAdminKey();
            if (string.IsNullOrEmpty(adminKey)) return null;

            try
            {
                var req = (HttpWebRequest)WebRequest.Create(LicenceManager.UrlServeur + chemin);
                req.Method = corps == null ? "GET" : "POST";
                req.ContentType = "application/json";
                req.Timeout = 10000;
                req.UserAgent = "LogicielImpression3D-Admin/1.0";
                req.Headers.Add("X-Admin-Key", adminKey);

                if (corps != null)
                {
                    byte[] data = Encoding.UTF8.GetBytes(corps);
                    req.ContentLength = data.Length;
                    using (var stream = req.GetRequestStream())
                        stream.Write(data, 0, data.Length);
                }

                using (var resp = (HttpWebResponse)req.GetResponse())
                using (var reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    return reader.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }

        private string LireAdminKey()
        {
            // Lire la clé admin depuis le fichier de paramètres (ligne 5 = token GitHub existant ou dédié)
            // Pour l'admin serveur, on utilise un fichier séparé admin_key.dat
            string fichier = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "admin_key.dat");
            if (File.Exists(fichier))
            {
                string cle = File.ReadAllText(fichier).Trim();
                if (!string.IsNullOrEmpty(cle)) return cle;
            }

            // Demander la clé si non configurée
            using (var form = new SaisieAdminKeyForm())
            {
                if (form.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(form.AdminKey))
                {
                    File.WriteAllText(fichier, form.AdminKey);
                    return form.AdminKey;
                }
            }
            return null;
        }

        // ── Parser JSON minimal ───────────────────────────────────────────
        private List<Dictionary<string, string>> ParseJsonArray(string json)
        {
            var result = new List<Dictionary<string, string>>();
            // Format attendu : [{"cle":"val",...},...]
            // On découpe par objets {} et on extrait les paires clé/valeur
            int i = 0;
            while (i < json.Length)
            {
                int debut = json.IndexOf('{', i);
                if (debut < 0) break;
                int fin = json.IndexOf('}', debut);
                if (fin < 0) break;
                string obj = json.Substring(debut + 1, fin - debut - 1);
                var dict = new Dictionary<string, string>();
                // Parser les paires "cle":"valeur" et "cle":valeur
                int j = 0;
                while (j < obj.Length)
                {
                    int kDebut = obj.IndexOf('"', j);
                    if (kDebut < 0) break;
                    int kFin = obj.IndexOf('"', kDebut + 1);
                    if (kFin < 0) break;
                    string cle = obj.Substring(kDebut + 1, kFin - kDebut - 1);
                    j = kFin + 1;
                    int colon = obj.IndexOf(':', j);
                    if (colon < 0) break;
                    j = colon + 1;
                    while (j < obj.Length && obj[j] == ' ') j++;
                    string valeur;
                    if (j < obj.Length && obj[j] == '"')
                    {
                        int vFin = obj.IndexOf('"', j + 1);
                        while (vFin > 0 && obj[vFin - 1] == '\\') vFin = obj.IndexOf('"', vFin + 1);
                        valeur = vFin >= 0 ? obj.Substring(j + 1, vFin - j - 1) : "";
                        j = vFin >= 0 ? vFin + 1 : obj.Length;
                    }
                    else
                    {
                        int vFin = obj.IndexOfAny(new[] { ',', '}' }, j);
                        valeur = vFin >= 0 ? obj.Substring(j, vFin - j).Trim() : obj.Substring(j).Trim();
                        if (valeur == "null") valeur = "";
                        j = vFin >= 0 ? vFin + 1 : obj.Length;
                    }
                    dict[cle] = valeur;
                }
                result.Add(dict);
                i = fin + 1;
            }
            return result;
        }

        private void AfficherErreurGrille(DataGridView dgv, string message)
        {
            dgv.Rows.Clear();
        }

        private static string Val(Dictionary<string, string> d, string k, string defaut = "")
        {
            string v;
            return d.TryGetValue(k, out v) ? v ?? defaut : defaut;
        }
    }

    // ── Mini-formulaire saisie nouveau mot de passe ───────────────────────────
    internal class SaisieNouveauPasswordForm : Form
    {
        public string NouveauPassword { get; private set; }
        private TextBox _txtPassword;
        private TextBox _txtConfirm;
        private Label   _lblErreur;

        public SaisieNouveauPasswordForm(string email)
        {
            this.Text = "Réinitialiser le mot de passe";
            this.Size = new Size(380, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var lbl1 = new Label { Text = $"Nouveau mot de passe pour {email} :", Location = new Point(16, 14), Size = new Size(340, 18) };
            _txtPassword = new TextBox { Location = new Point(16, 36), Size = new Size(330, 26), UseSystemPasswordChar = true };
            var lbl2 = new Label { Text = "Confirmer le mot de passe :", Location = new Point(16, 72), Size = new Size(200, 18) };
            _txtConfirm  = new TextBox { Location = new Point(16, 92), Size = new Size(330, 26), UseSystemPasswordChar = true };
            _lblErreur   = new Label { Location = new Point(16, 124), Size = new Size(330, 18), ForeColor = Color.Red, Font = new Font("Segoe UI", 8f) };

            var btn = new Button { Text = "Réinitialiser", Location = new Point(220, 120), Size = new Size(126, 30) };
            btn.Click += (s, e) =>
            {
                string pwd = _txtPassword.Text.Trim();
                string conf = _txtConfirm.Text.Trim();
                if (pwd.Length < 6) { _lblErreur.Text = "Minimum 6 caractères."; return; }
                if (pwd != conf)    { _lblErreur.Text = "Les mots de passe ne correspondent pas."; return; }
                NouveauPassword = pwd;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.AcceptButton = btn;
            this.Controls.AddRange(new Control[] { lbl1, _txtPassword, lbl2, _txtConfirm, _lblErreur, btn });
        }
    }

    // ── Mini-formulaire saisie nombre de jours ────────────────────────────────
    internal class SaisieJoursForm : Form
    {
        public int Jours { get; private set; }
        private NumericUpDown _numJours;

        public SaisieJoursForm(string email)
        {
            this.Text = "Ajouter du temps d'accès";
            this.Size = new Size(340, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var lbl = new Label { Text = $"Jours à ajouter pour {email} :", Location = new Point(16, 16), Size = new Size(290, 18) };
            _numJours = new NumericUpDown
            {
                Location = new Point(16, 40),
                Size     = new Size(120, 28),
                Minimum  = 1,
                Maximum  = 3650,
                Value    = 30,
                Font     = new Font("Segoe UI", 10f)
            };
            var btn = new Button { Text = "Ajouter", Location = new Point(210, 74), Size = new Size(100, 30) };
            btn.Click += (s, e) =>
            {
                Jours = (int)_numJours.Value;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.AcceptButton = btn;
            this.Controls.AddRange(new Control[] { lbl, _numJours, btn });
        }
    }

    // ── Mini-formulaire saisie clé admin ─────────────────────────────────────
    internal class SaisieAdminKeyForm : Form
    {
        public string AdminKey { get; private set; }
        private TextBox txtKey;

        public SaisieAdminKeyForm()
        {
            this.Text = "Clé d'administration";
            this.Size = new Size(360, 140);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var lbl = new Label { Text = "Clé admin du serveur :", Location = new Point(16, 16), Size = new Size(200, 20) };
            txtKey = new TextBox { Location = new Point(16, 40), Size = new Size(310, 26), UseSystemPasswordChar = true };
            var btn = new Button
            {
                Text = "OK", Location = new Point(230, 74), Size = new Size(96, 30),
                DialogResult = DialogResult.OK
            };
            btn.Click += (s, e) => { AdminKey = txtKey.Text.Trim(); this.Close(); };
            this.AcceptButton = btn;
            this.Controls.AddRange(new Control[] { lbl, txtKey, btn });
        }
    }
}
