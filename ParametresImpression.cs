using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public partial class ParametresImpressionForm : Form
    {
        private ParametresImpression parametres;
        private DataTable dtBobines;
        private static readonly string FichierParametres = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "parametres_impression.dat");

        public ParametresImpressionForm()
        {
            InitializeComponent();
            InitialiserFormulaire();
            AppliquerTheme();
        }

        private void AppliquerTheme()
        {
            ThemeManager.ApplyThemeToForm(this);
            ThemeManager.StyleAllControls(this);

            // Boutons spécifiques
            ThemeManager.StyleButton(btnEnregistrer, ThemeManager.SecondaryGreen, ThemeManager.SecondaryGreenDark);
            ThemeManager.StyleButton(btnAnnuler, ThemeManager.NeutralGray, ThemeManager.NeutralGrayDark);
            ThemeManager.StyleButton(btnAjouterBobine, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);
            ThemeManager.StyleButton(btnSupprimerBobine, ThemeManager.DangerRed, ThemeManager.DangerRedDark);
        }

        private void InitialiserFormulaire()
        {
            // Charger les paramètres existants ou créer nouveaux
            parametres = ChargerParametres();

            // Initialiser le DataTable pour les bobines
            dtBobines = new DataTable();
            dtBobines.Columns.Add("Couleur", typeof(string));
            dtBobines.Columns.Add("Matière", typeof(string));
            dtBobines.Columns.Add("Marque", typeof(string));
            dtBobines.Columns.Add("Poids bobine (g)", typeof(decimal));
            dtBobines.Columns.Add("Prix bobine (€)", typeof(decimal));
            dtBobines.Columns.Add("Prix/kg (€)", typeof(decimal));

            // Charger les bobines dans le DataTable
            if (parametres.Bobines != null)
            {
                foreach (var bobine in parametres.Bobines)
                {
                    AjouterBobineAuDataTable(bobine);
                }
            }

            dgvBobines.DataSource = dtBobines;
            ConfigurerColonnesBobines();

            // Charger les valeurs dans les contrôles
            numCoutElectricite.Value = parametres.CoutElectriciteKwh;
            numPourcentagePurge.Value = parametres.PourcentagePurgeAMS;
            numTVA.Value = parametres.TVA;
            numMarge.Value = parametres.MargeParObjet;
            txtTokenGithub.Text = parametres.TokenGithub ?? "";
            numCoutMainOeuvre.Value = parametres.CoutMainOeuvreHeure;
            numAmortissement.Value = parametres.AmortissementMachineHeure;
            txtCheminSlicer.Text = parametres.CheminSlicer ?? "";
            MettreAJourStatutSlicer();

            // Événements
            dgvBobines.CellValueChanged += DgvBobines_CellValueChanged;
            dgvBobines.CellFormatting += DgvBobines_CellFormatting;
        }

        private void ConfigurerColonnesBobines()
        {
            dgvBobines.AutoGenerateColumns = false;
            dgvBobines.Columns.Clear();

            // Colonne Couleur
            DataGridViewComboBoxColumn colCouleur = new DataGridViewComboBoxColumn
            {
                Name = "Couleur",
                HeaderText = "Couleur",
                DataPropertyName = "Couleur",
                Width = 120
            };
            colCouleur.Items.AddRange(new string[] {
                "Rouge", "Bleu", "Vert", "Jaune", "Orange", "Violet",
                "Rose", "Noir", "Blanc", "Gris", "Marron", "Cyan",
                "Magenta", "Beige", "Turquoise", "Or", "Argent", "Bronze"
            });
            dgvBobines.Columns.Add(colCouleur);

            // Colonne Matière
            DataGridViewComboBoxColumn colMatiere = new DataGridViewComboBoxColumn
            {
                Name = "Matière",
                HeaderText = "Matière",
                DataPropertyName = "Matière",
                Width = 100
            };
            colMatiere.Items.AddRange(new string[] { "PLA", "PETG", "ABS", "TPU", "ASA", "Nylon" });
            dgvBobines.Columns.Add(colMatiere);

            // Colonne Marque
            DataGridViewComboBoxColumn colMarque = new DataGridViewComboBoxColumn
            {
                Name = "Marque",
                HeaderText = "Marque",
                DataPropertyName = "Marque",
                Width = 120
            };
            colMarque.Items.AddRange(new string[] { "Bambu Lab", "Creality", "eSUN", "Polymaker", "Prusament", "Generic" });
            dgvBobines.Columns.Add(colMarque);

            // Colonne Poids bobine
            DataGridViewTextBoxColumn colPoids = new DataGridViewTextBoxColumn
            {
                Name = "Poids",
                HeaderText = "Poids bobine (g)",
                DataPropertyName = "Poids bobine (g)",
                Width = 120
            };
            dgvBobines.Columns.Add(colPoids);

            // Colonne Prix bobine
            DataGridViewTextBoxColumn colPrix = new DataGridViewTextBoxColumn
            {
                Name = "Prix",
                HeaderText = "Prix bobine (€)",
                DataPropertyName = "Prix bobine (€)",
                Width = 120
            };
            dgvBobines.Columns.Add(colPrix);

            // Colonne Prix/kg (calculé automatiquement)
            DataGridViewTextBoxColumn colPrixKg = new DataGridViewTextBoxColumn
            {
                Name = "PrixKg",
                HeaderText = "Prix/kg (€)",
                DataPropertyName = "Prix/kg (€)",
                Width = 100,
                ReadOnly = true
            };
            dgvBobines.Columns.Add(colPrixKg);
        }

        private void DgvBobines_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvBobines.Rows.Count)
            {
                DataGridViewRow row = dgvBobines.Rows[e.RowIndex];
                if (row.Cells["Poids"].Value != null && row.Cells["Prix"].Value != null)
                {
                    try
                    {
                        decimal poids = Convert.ToDecimal(row.Cells["Poids"].Value);
                        decimal prix = Convert.ToDecimal(row.Cells["Prix"].Value);

                        if (poids > 0)
                        {
                            decimal prixKg = (prix / poids) * 1000;
                            row.Cells["PrixKg"].Value = Math.Round(prixKg, 2);
                        }
                    }
                    catch { }
                }
            }
        }

        private void DgvBobines_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && dgvBobines.Columns[e.ColumnIndex].Name == "Couleur" && e.Value != null)
            {
                string nomCouleur = e.Value.ToString();
                var dictCouleurs = ThemeManager.CouleursDictionnaire;

                if (dictCouleurs.ContainsKey(nomCouleur))
                {
                    Color couleur = dictCouleurs[nomCouleur];
                    e.CellStyle.BackColor = couleur;
                    int luminosite = (int)((couleur.R * 0.299) + (couleur.G * 0.587) + (couleur.B * 0.114));
                    e.CellStyle.ForeColor = luminosite > 128 ? Color.Black : Color.White;
                }
            }
        }

        private void AjouterBobineAuDataTable(Bobine bobine)
        {
            if (bobine == null) return;
            
            DataRow row = dtBobines.NewRow();
            row["Couleur"] = bobine.Couleur ?? "Rouge";
            row["Matière"] = bobine.Matiere ?? "PLA";
            row["Marque"] = bobine.Marque ?? "Generic";
            row["Poids bobine (g)"] = bobine.PoidsBobine > 0 ? bobine.PoidsBobine : 1000;
            row["Prix bobine (€)"] = bobine.PrixBobine > 0 ? bobine.PrixBobine : 20;
            decimal prixKg = bobine.PoidsBobine > 0 ? (bobine.PrixBobine / bobine.PoidsBobine) * 1000 : 20;
            row["Prix/kg (€)"] = Math.Round(prixKg, 2);
            dtBobines.Rows.Add(row);
        }

        private void btnAjouterBobine_Click(object sender, EventArgs e)
        {
            DataRow row = dtBobines.NewRow();
            row["Couleur"] = "Rouge";
            row["Matière"] = "PLA";
            row["Marque"] = "Bambu Lab";
            row["Poids bobine (g)"] = 1000m;
            row["Prix bobine (€)"] = 20.00m;
            row["Prix/kg (€)"] = 20.00m;
            dtBobines.Rows.Add(row);
        }

        private void btnSupprimerBobine_Click(object sender, EventArgs e)
        {
            if (dgvBobines.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvBobines.SelectedRows)
                {
                    if (!row.IsNewRow)
                    {
                        dgvBobines.Rows.Remove(row);
                    }
                }
            }
        }

        private void btnEnregistrer_Click(object sender, EventArgs e)
        {
            // Sauvegarder les paramètres
            parametres.CoutElectriciteKwh = numCoutElectricite.Value;
            parametres.PourcentagePurgeAMS = numPourcentagePurge.Value;
            parametres.TVA = numTVA.Value;
            parametres.MargeParObjet = numMarge.Value;
            parametres.TokenGithub = txtTokenGithub.Text.Trim();
            parametres.CoutMainOeuvreHeure = numCoutMainOeuvre.Value;
            parametres.AmortissementMachineHeure = numAmortissement.Value;
            parametres.CheminSlicer = txtCheminSlicer.Text.Trim();
            SlicerManager.InvaliderCache();

            // Sauvegarder les bobines
            parametres.Bobines.Clear();
            foreach (DataRow row in dtBobines.Rows)
            {
                if (row.RowState != DataRowState.Deleted)
                {
                    parametres.Bobines.Add(new Bobine
                    {
                        Couleur = row["Couleur"]?.ToString() ?? "Rouge",
                        Matiere = row["Matière"]?.ToString() ?? "PLA",
                        Marque = row["Marque"]?.ToString() ?? "Generic",
                        PoidsBobine = row["Poids bobine (g)"] != DBNull.Value ? Convert.ToDecimal(row["Poids bobine (g)"]) : 1000,
                        PrixBobine = row["Prix bobine (€)"] != DBNull.Value ? Convert.ToDecimal(row["Prix bobine (€)"]) : 20
                    });
                }
            }

            SauvegarderParametres(parametres);
            InvaliderCache();
            MessageBox.Show("Paramètres enregistrés avec succès !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnParcourirSlicer_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Sélectionner l'exécutable Bambu Studio";
                ofd.Filter = "Exécutable (*.exe)|*.exe";
                ofd.InitialDirectory = @"C:\Program Files";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtCheminSlicer.Text = ofd.FileName;
                    MettreAJourStatutSlicer();
                }
            }
        }

        private void MettreAJourStatutSlicer()
        {
            string chemin = txtCheminSlicer.Text.Trim();
            if (!string.IsNullOrEmpty(chemin) && System.IO.File.Exists(chemin))
            {
                lblStatutSlicerParam.Text = "Slicer détecté";
                lblStatutSlicerParam.ForeColor = System.Drawing.Color.FromArgb(46, 204, 113);
            }
            else if (SlicerManager.EstInstalle())
            {
                txtCheminSlicer.Text = SlicerManager.ObtenirCheminSlicer();
                lblStatutSlicerParam.Text = "Auto-détecté";
                lblStatutSlicerParam.ForeColor = System.Drawing.Color.FromArgb(46, 204, 113);
            }
            else
            {
                lblStatutSlicerParam.Text = "Non trouvé - installez Bambu Studio";
                lblStatutSlicerParam.ForeColor = System.Drawing.Color.FromArgb(231, 76, 60);
            }
        }

        private static ParametresImpression ChargerParametres()
        {
            try
            {
                if (File.Exists(FichierParametres))
                {
                    string[] lines = File.ReadAllLines(FichierParametres, Encoding.UTF8);
                    if (lines.Length >= 4)
                    {
                        ParametresImpression param = new ParametresImpression();
                        
                        param.CoutElectriciteKwh = ParseDecimal(lines[0], 0.15m);
                        param.PourcentagePurgeAMS = ParseDecimal(lines[1], 10m);
                        param.TVA = ParseDecimal(lines[2], 20m);
                        param.MargeParObjet = ParseDecimal(lines[3], 50m);

                        // Lignes suivantes : token GitHub + coûts optionnels (compatibilité ancien format)
                        int debutBobines = 4;
                        if (lines.Length > 4 && !lines[4].Contains("|"))
                        {
                            param.TokenGithub = lines[4];
                            debutBobines = 5;
                        }
                        if (lines.Length > debutBobines && !lines[debutBobines].Contains("|"))
                        {
                            param.CoutMainOeuvreHeure = ParseDecimal(lines[debutBobines], 0m);
                            debutBobines++;
                        }
                        if (lines.Length > debutBobines && !lines[debutBobines].Contains("|"))
                        {
                            param.AmortissementMachineHeure = ParseDecimal(lines[debutBobines], 0m);
                            debutBobines++;
                        }
                        if (lines.Length > debutBobines && !lines[debutBobines].Contains("|"))
                        {
                            param.CheminSlicer = lines[debutBobines];
                            debutBobines++;
                        }

                        // Lire les bobines
                        for (int i = debutBobines; i < lines.Length; i++)
                        {
                            string[] parts = lines[i].Split('|');
                            if (parts.Length >= 5)
                            {
                                param.Bobines.Add(new Bobine
                                {
                                    Couleur = parts[0],
                                    Matiere = parts[1],
                                    Marque = parts[2],
                                    PoidsBobine = ParseDecimal(parts[3], 1000),
                                    PrixBobine = ParseDecimal(parts[4], 20)
                                });
                            }
                        }
                        return param;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur chargement: {ex.Message}");
            }
            return CreerParametresParDefaut();
        }

        private static decimal ParseDecimal(string value, decimal defaultValue)
        {
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }
            return defaultValue;
        }

        private void SauvegarderParametres(ParametresImpression param)
        {
            try
            {
                List<string> lines = new List<string>
                {
                    param.CoutElectriciteKwh.ToString(CultureInfo.InvariantCulture),
                    param.PourcentagePurgeAMS.ToString(CultureInfo.InvariantCulture),
                    param.TVA.ToString(CultureInfo.InvariantCulture),
                    param.MargeParObjet.ToString(CultureInfo.InvariantCulture),
                    param.TokenGithub ?? "",
                    param.CoutMainOeuvreHeure.ToString(CultureInfo.InvariantCulture),
                    param.AmortissementMachineHeure.ToString(CultureInfo.InvariantCulture),
                    param.CheminSlicer ?? ""
                };

                foreach (var bobine in param.Bobines)
                {
                    lines.Add($"{bobine.Couleur}|{bobine.Matiere}|{bobine.Marque}|{bobine.PoidsBobine.ToString(CultureInfo.InvariantCulture)}|{bobine.PrixBobine.ToString(CultureInfo.InvariantCulture)}");
                }

                File.WriteAllLines(FichierParametres, lines, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde :\n{ex.Message}",
                    "Erreur de sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static ParametresImpression CreerParametresParDefaut()
        {
            return new ParametresImpression
            {
                CoutElectriciteKwh = 0.15m,
                PourcentagePurgeAMS = 10m,
                TVA = 20m,
                MargeParObjet = 50m,
                Bobines = new List<Bobine>
                {
                    new Bobine { Couleur = "Noir", Matiere = "PLA", Marque = "Bambu Lab", PoidsBobine = 1000, PrixBobine = 20 },
                    new Bobine { Couleur = "Blanc", Matiere = "PLA", Marque = "Bambu Lab", PoidsBobine = 1000, PrixBobine = 20 }
                }
            };
        }

        private static ParametresImpression parametresCache;

        public static ParametresImpression ObtenirParametres()
        {
            if (parametresCache == null)
                parametresCache = ChargerParametres();
            return parametresCache;
        }

        public static void InvaliderCache()
        {
            parametresCache = null;
        }
    }

    public class ParametresImpression
    {
        public decimal CoutElectriciteKwh { get; set; } = 0.15m;
        public decimal PourcentagePurgeAMS { get; set; } = 10m;
        public decimal TVA { get; set; } = 20m;
        public decimal MargeParObjet { get; set; } = 50m;
        public string TokenGithub { get; set; } = "";
        public decimal CoutMainOeuvreHeure { get; set; } = 0m;
        public decimal AmortissementMachineHeure { get; set; } = 0m;
        public string CheminSlicer { get; set; } = "";
        public List<Bobine> Bobines { get; set; } = new List<Bobine>();
    }

    public class Bobine
    {
        public string Couleur { get; set; } = "Rouge";
        public string Matiere { get; set; } = "PLA";
        public string Marque { get; set; } = "Generic";
        public decimal PoidsBobine { get; set; } = 1000;
        public decimal PrixBobine { get; set; } = 20;

        public decimal PrixParKg
        {
            get { return PoidsBobine > 0 ? (PrixBobine / PoidsBobine) * 1000 : 0; }
        }
    }
}