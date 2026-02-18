using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public class DevisForm : Form
    {
        private TextBox txtDevis;
        private Button btnCopier;
        private Button btnExporter;
        private Button btnFermer;
        private Panel panelBoutons;
        private string contenuDevis;

        public DevisForm(string titre, string devis)
        {
            contenuDevis = devis;
            InitialiserComposants(titre);
            AppliquerTheme();
        }

        private void InitialiserComposants(string titre)
        {
            this.Text = titre;
            this.Size = new Size(650, 550);
            this.MinimumSize = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            // TextBox principal pour afficher le devis
            txtDevis = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10f),
                Text = contenuDevis,
                WordWrap = false
            };

            // Panel pour les boutons
            panelBoutons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                Padding = new Padding(10)
            };

            btnCopier = new Button
            {
                Text = "Copier",
                Size = new Size(120, 35),
                Location = new Point(10, 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCopier.Click += BtnCopier_Click;

            btnExporter = new Button
            {
                Text = "Exporter (.txt)",
                Size = new Size(120, 35),
                Location = new Point(140, 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExporter.Click += BtnExporter_Click;

            btnFermer = new Button
            {
                Text = "Fermer",
                Size = new Size(120, 35),
                Location = new Point(270, 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnFermer.Click += (s, e) => this.Close();

            panelBoutons.Controls.AddRange(new Control[] { btnCopier, btnExporter, btnFermer });
            this.Controls.Add(txtDevis);
            this.Controls.Add(panelBoutons);

            this.AcceptButton = btnFermer;
        }

        private void AppliquerTheme()
        {
            ThemeManager.ApplyThemeToForm(this);
            ThemeManager.StyleAllControls(this);
            ThemeManager.StyleButton(btnCopier, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);
            ThemeManager.StyleButton(btnExporter, ThemeManager.SecondaryGreen, ThemeManager.SecondaryGreenDark);
            ThemeManager.StyleButton(btnFermer, ThemeManager.NeutralGray, ThemeManager.NeutralGrayDark);

            txtDevis.BackColor = Color.White;
            txtDevis.ForeColor = ThemeManager.TextPrimary;
        }

        private void BtnCopier_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(contenuDevis);
            MessageBox.Show("Devis copié dans le presse-papiers.", "Copié",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnExporter_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Fichier texte (*.txt)|*.txt";
                sfd.FileName = $"Devis_{DateTime.Now:yyyy-MM-dd_HHmmss}.txt";
                sfd.Title = "Exporter le devis";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(sfd.FileName, contenuDevis, Encoding.UTF8);
                    MessageBox.Show($"Devis exporté vers :\n{sfd.FileName}", "Exporté",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
