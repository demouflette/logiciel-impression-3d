using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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

        /// <summary>
        /// Supprime les entrées plus vieilles que le nombre de jours spécifié
        /// </summary>
        public static void NettoyerHistorique(int joursConservation = 365)
        {
            var historique = Charger();
            DateTime dateLimite = DateTime.Now.AddDays(-joursConservation);
            int avant = historique.Count;
            historique.RemoveAll(e => e.Date < dateLimite);
            int supprimees = avant - historique.Count;

            if (supprimees > 0)
            {
                // Réécrire le fichier
                using (StreamWriter sw = new StreamWriter(FichierHistorique, false, Encoding.UTF8))
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
                LogManager.Info($"Nettoyage historique : {supprimees} entrée(s) supprimée(s) (> {joursConservation} jours)");
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
        private Button btnExporterCSV;
        private Button btnFermer;
        private Panel panelBoutons;
        private Panel panelRecherche;
        private TextBox txtRecherche;
        private DateTimePicker dtpDebut;
        private DateTimePicker dtpFin;
        private Button btnFiltrer;
        private Button btnReinitialiser;
        private List<EntreeHistorique> historique;
        private List<EntreeHistorique> historiqueFiltré;

        public HistoriqueForm()
        {
            historique = HistoriqueManager.Charger();
            historiqueFiltré = new List<EntreeHistorique>(historique);
            InitialiserComposants();
            AppliquerTheme();
            RemplirGrille();
        }

        private void InitialiserComposants()
        {
            this.Text = "Historique des devis";
            this.Size = new Size(850, 550);
            this.MinimumSize = new Size(700, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            // Panel recherche (en haut)
            panelRecherche = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                Padding = new Padding(10, 8, 10, 5)
            };

            txtRecherche = new TextBox
            {
                Location = new Point(10, 10),
                Size = new Size(200, 25),
                Text = "Rechercher...",
                ForeColor = Color.Gray
            };
            txtRecherche.GotFocus += (s, e) =>
            {
                if (txtRecherche.Text == "Rechercher...")
                {
                    txtRecherche.Text = "";
                    txtRecherche.ForeColor = ThemeManager.TextPrimary;
                }
            };
            txtRecherche.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtRecherche.Text))
                {
                    txtRecherche.Text = "Rechercher...";
                    txtRecherche.ForeColor = Color.Gray;
                }
            };

            var lblDu = new Label { Text = "Du:", AutoSize = true, Location = new Point(220, 13) };
            dtpDebut = new DateTimePicker
            {
                Location = new Point(245, 9),
                Size = new Size(130, 25),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddYears(-1)
            };

            var lblAu = new Label { Text = "Au:", AutoSize = true, Location = new Point(385, 13) };
            dtpFin = new DateTimePicker
            {
                Location = new Point(410, 9),
                Size = new Size(130, 25),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };

            btnFiltrer = new Button
            {
                Text = "Filtrer",
                Size = new Size(80, 28),
                Location = new Point(555, 8),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnFiltrer.Click += (s, e) => AppliquerFiltre();

            btnReinitialiser = new Button
            {
                Text = "Tout",
                Size = new Size(60, 28),
                Location = new Point(640, 8),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnReinitialiser.Click += (s, e) => ReinitialiserFiltre();

            // Recherche au fur et à mesure
            txtRecherche.TextChanged += (s, e) => AppliquerFiltre();

            panelRecherche.Controls.AddRange(new Control[] { txtRecherche, lblDu, dtpDebut, lblAu, dtpFin, btnFiltrer, btnReinitialiser });

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
                Location = new Point(10, 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRevoir.Click += (s, e) => RevoirDevis();

            btnSupprimer = new Button
            {
                Text = "Supprimer",
                Size = new Size(120, 35),
                Location = new Point(160, 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSupprimer.Click += BtnSupprimer_Click;

            btnExporterCSV = new Button
            {
                Text = "Exporter CSV",
                Size = new Size(120, 35),
                Location = new Point(290, 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExporterCSV.Click += BtnExporterCSV_Click;

            btnFermer = new Button
            {
                Text = "Fermer",
                Size = new Size(120, 35),
                Location = new Point(420, 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnFermer.Click += (s, e) => this.Close();

            panelBoutons.Controls.AddRange(new Control[] { btnRevoir, btnSupprimer, btnExporterCSV, btnFermer });
            this.Controls.Add(dgvHistorique);
            this.Controls.Add(panelRecherche);
            this.Controls.Add(panelBoutons);
            this.AcceptButton = btnFermer;
        }

        private void AppliquerTheme()
        {
            ThemeManager.ApplyThemeToForm(this);
            ThemeManager.StyleAllControls(this);
            ThemeManager.StyleDataGridView(dgvHistorique);
            ThemeManager.StyleButton(btnRevoir, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);
            ThemeManager.StyleButton(btnSupprimer, ThemeManager.DangerRed, ThemeManager.DangerRedDark);
            ThemeManager.StyleButton(btnExporterCSV, ThemeManager.SecondaryGreen, ThemeManager.SecondaryGreenDark);
            ThemeManager.StyleButton(btnFermer, ThemeManager.NeutralGray, ThemeManager.NeutralGrayDark);
            ThemeManager.StyleButton(btnFiltrer, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);
            ThemeManager.StyleButton(btnReinitialiser, ThemeManager.NeutralGray, ThemeManager.NeutralGrayDark);
            ThemeManager.StyleTextBox(txtRecherche);
            panelRecherche.BackColor = ThemeManager.BackgroundCard;
        }

        private void RemplirGrille()
        {
            dgvHistorique.Rows.Clear();
            // Afficher du plus récent au plus ancien
            for (int i = historiqueFiltré.Count - 1; i >= 0; i--)
            {
                var e = historiqueFiltré[i];
                dgvHistorique.Rows.Add(e.Date.ToString("dd/MM/yyyy HH:mm"), e.Titre, $"{e.PrixTotalTTC:F2} €");
            }
        }

        private void AppliquerFiltre()
        {
            string texte = txtRecherche.Text == "Rechercher..." ? "" : txtRecherche.Text;
            string recherche = texte.Trim().ToLowerInvariant();
            DateTime dateDebut = dtpDebut.Value.Date;
            DateTime dateFin = dtpFin.Value.Date.AddDays(1); // Inclure toute la journée de fin

            historiqueFiltré = historique.Where(e =>
                (string.IsNullOrEmpty(recherche) || e.Titre.ToLowerInvariant().Contains(recherche)) &&
                e.Date >= dateDebut && e.Date < dateFin
            ).ToList();

            RemplirGrille();
        }

        private void ReinitialiserFiltre()
        {
            txtRecherche.Text = "";
            dtpDebut.Value = DateTime.Now.AddYears(-1);
            dtpFin.Value = DateTime.Now;
            historiqueFiltré = new List<EntreeHistorique>(historique);
            RemplirGrille();
        }

        private EntreeHistorique ObtenirEntreeSelectionnee()
        {
            if (dgvHistorique.CurrentRow == null) return null;
            int indexInverse = dgvHistorique.CurrentRow.Index;
            int indexFiltré = historiqueFiltré.Count - 1 - indexInverse;
            if (indexFiltré < 0 || indexFiltré >= historiqueFiltré.Count) return null;
            return historiqueFiltré[indexFiltré];
        }

        private void RevoirDevis()
        {
            var entree = ObtenirEntreeSelectionnee();
            if (entree == null) return;
            new DevisForm($"Devis du {entree.Date:dd/MM/yyyy HH:mm} - {entree.Titre}", entree.ContenuDevis).ShowDialog();
        }

        private void BtnSupprimer_Click(object sender, EventArgs e)
        {
            var entree = ObtenirEntreeSelectionnee();
            if (entree == null) return;

            if (MessageBox.Show($"Supprimer le devis du {entree.Date:dd/MM/yyyy HH:mm} ?",
                "Confirmer la suppression", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                historique.Remove(entree);
                historiqueFiltré.Remove(entree);
                ReécrireFichier();
                RemplirGrille();
            }
        }

        private void BtnExporterCSV_Click(object sender, EventArgs e)
        {
            using (var sfd = new System.Windows.Forms.SaveFileDialog())
            {
                sfd.Filter = "Fichier CSV (*.csv)|*.csv";
                sfd.FileName = $"Historique_Devis_{DateTime.Now:yyyy-MM-dd}.csv";
                sfd.Title = "Exporter l'historique en CSV";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    var sb = new StringBuilder();
                    // BOM UTF-8 pour Excel
                    sb.AppendLine("Date;Titre;Prix TTC (€)");
                    foreach (var entry in historiqueFiltré)
                    {
                        string titre = entry.Titre.Replace(";", ",");
                        sb.AppendLine($"{entry.Date:dd/MM/yyyy HH:mm};{titre};{entry.PrixTotalTTC:F2}");
                    }
                    File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
                    MessageBox.Show($"Historique exporté ({historiqueFiltré.Count} entrées) :\n{sfd.FileName}",
                        "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
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
