using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    // ═══════════════════════════════════════════════════════
    // MODÈLE
    // ═══════════════════════════════════════════════════════

    public class EntreeHistorique
    {
        public DateTime Date { get; set; }
        public string Titre { get; set; }
        public decimal PrixTotalTTC { get; set; }
        public string ContenuDevis { get; set; }
    }

    // ═══════════════════════════════════════════════════════
    // MANAGER
    // ═══════════════════════════════════════════════════════

    public static class HistoriqueManager
    {
        private static readonly string FichierHistorique = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "historique_devis.dat");

        public static void Sauvegarder(EntreeHistorique entree)
        {
            // Échapper les caractères spéciaux dans le contenu du devis
            string contenuEchappe = entree.ContenuDevis
                .Replace("|", "§")
                .Replace("\r\n", "¶")
                .Replace("\n", "¶");

            string ligne = $"{entree.Date:yyyy-MM-dd HH:mm:ss}|{entree.Titre}|{entree.PrixTotalTTC:F2}|{contenuEchappe}";

            using (StreamWriter sw = new StreamWriter(FichierHistorique, true, Encoding.UTF8))
            {
                sw.WriteLine(ligne);
            }
        }

        public static List<EntreeHistorique> Charger()
        {
            List<EntreeHistorique> historique = new List<EntreeHistorique>();

            if (!File.Exists(FichierHistorique))
                return historique;

            string[] lignes = File.ReadAllLines(FichierHistorique, Encoding.UTF8);

            foreach (string ligne in lignes)
            {
                if (string.IsNullOrWhiteSpace(ligne)) continue;

                string[] parties = ligne.Split(new[] { '|' }, 4);
                if (parties.Length < 4) continue;

                try
                {
                    EntreeHistorique entree = new EntreeHistorique
                    {
                        Date = DateTime.ParseExact(parties[0].Trim(), "yyyy-MM-dd HH:mm:ss",
                            System.Globalization.CultureInfo.InvariantCulture),
                        Titre = parties[1],
                        PrixTotalTTC = decimal.Parse(parties[2],
                            System.Globalization.CultureInfo.InvariantCulture),
                        ContenuDevis = parties[3]
                            .Replace("§", "|")
                            .Replace("¶", "\r\n")
                    };
                    historique.Add(entree);
                }
                catch
                {
                    // Ignorer les lignes corrompues
                }
            }

            return historique;
        }
    }

    // ═══════════════════════════════════════════════════════
    // FORMULAIRE HISTORIQUE
    // ═══════════════════════════════════════════════════════

    public class HistoriqueForm : Form
    {
        private DataGridView dgvHistorique;
        private Button btnRevoir;
        private Button btnSupprimer;
        private Button btnFermer;
        private Panel panelBoutons;
        private List<EntreeHistorique> historique;

        public HistoriqueForm()
        {
            historique = HistoriqueManager.Charger();
            InitialiserComposants();
            AppliquerTheme();
            RemplirGrille();
        }

        private void InitialiserComposants()
        {
            this.Text = "Historique des devis";
            this.Size = new Size(750, 500);
            this.MinimumSize = new Size(600, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            // DataGridView
            dgvHistorique = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvHistorique.Columns.Add("Date", "Date");
            dgvHistorique.Columns.Add("Titre", "Titre");
            dgvHistorique.Columns.Add("PrixTTC", "Prix TTC");
            dgvHistorique.Columns["Date"].FillWeight = 30;
            dgvHistorique.Columns["Titre"].FillWeight = 50;
            dgvHistorique.Columns["PrixTTC"].FillWeight = 20;
            dgvHistorique.Columns["PrixTTC"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvHistorique.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) RevoirDevis(); };

            // Panel boutons
            panelBoutons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                Padding = new Padding(10)
            };

            btnRevoir = new Button
            {
                Text = "Revoir le devis",
                Size = new Size(140, 35),
                Location = new Point(10, 10)
            };
            btnRevoir.Click += (s, e) => RevoirDevis();

            btnSupprimer = new Button
            {
                Text = "Supprimer",
                Size = new Size(120, 35),
                Location = new Point(160, 10)
            };
            btnSupprimer.Click += BtnSupprimer_Click;

            btnFermer = new Button
            {
                Text = "Fermer",
                Size = new Size(120, 35),
                Location = new Point(290, 10)
            };
            btnFermer.Click += (s, e) => this.Close();

            panelBoutons.Controls.AddRange(new Control[] { btnRevoir, btnSupprimer, btnFermer });
            this.Controls.Add(dgvHistorique);
            this.Controls.Add(panelBoutons);
            this.AcceptButton = btnFermer;
        }

        private void AppliquerTheme()
        {
            ThemeManager.ApplyThemeToForm(this);
            ThemeManager.StyleDataGridView(dgvHistorique);
            ThemeManager.StyleButton(btnRevoir, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);
            ThemeManager.StyleButton(btnSupprimer, ThemeManager.DangerRed, ThemeManager.DangerRedDark);
            ThemeManager.StyleButton(btnFermer, ThemeManager.NeutralGray, ThemeManager.NeutralGrayDark);
        }

        private void RemplirGrille()
        {
            dgvHistorique.Rows.Clear();
            // Afficher du plus récent au plus ancien
            for (int i = historique.Count - 1; i >= 0; i--)
            {
                var e = historique[i];
                dgvHistorique.Rows.Add(e.Date.ToString("dd/MM/yyyy HH:mm"), e.Titre, $"{e.PrixTotalTTC:F2} €");
            }
        }

        private void RevoirDevis()
        {
            if (dgvHistorique.CurrentRow == null) return;
            int indexInverse = dgvHistorique.CurrentRow.Index;
            int indexOriginal = historique.Count - 1 - indexInverse;
            if (indexOriginal < 0 || indexOriginal >= historique.Count) return;

            var entree = historique[indexOriginal];
            new DevisForm($"Devis du {entree.Date:dd/MM/yyyy HH:mm} - {entree.Titre}", entree.ContenuDevis).ShowDialog();
        }

        private void BtnSupprimer_Click(object sender, EventArgs e)
        {
            if (dgvHistorique.CurrentRow == null) return;
            int indexInverse = dgvHistorique.CurrentRow.Index;
            int indexOriginal = historique.Count - 1 - indexInverse;
            if (indexOriginal < 0 || indexOriginal >= historique.Count) return;

            var entree = historique[indexOriginal];
            if (MessageBox.Show($"Supprimer le devis du {entree.Date:dd/MM/yyyy HH:mm} ?",
                "Confirmer la suppression", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                historique.RemoveAt(indexOriginal);
                ReécrireFichier();
                RemplirGrille();
            }
        }

        private void ReécrireFichier()
        {
            string fichier = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "historique_devis.dat");
            using (StreamWriter sw = new StreamWriter(fichier, false, Encoding.UTF8))
            {
                foreach (var entree in historique)
                {
                    string contenuEchappe = entree.ContenuDevis
                        .Replace("|", "§")
                        .Replace("\r\n", "¶")
                        .Replace("\n", "¶");
                    sw.WriteLine($"{entree.Date:yyyy-MM-dd HH:mm:ss}|{entree.Titre}|{entree.PrixTotalTTC:F2}|{contenuEchappe}");
                }
            }
        }
    }
}
