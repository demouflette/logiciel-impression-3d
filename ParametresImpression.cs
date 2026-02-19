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

            // Configurer l'onglet imprimantes personnalisées
            ConfigurerColonnesImprimantesCustom();
            ChargerImprimantesCustom();
            btnAjouterImprimante.Click += BtnAjouterImprimante_Click;
            btnSupprimerImprimante.Click += BtnSupprimerImprimante_Click;

            // Configurer l'onglet entreprise
            CreerOngletEntreprise();

            // Verrouiller les paramètres premium si pas d'abonnement actif
            AppliquerVerrouillageParametres();
        }

        private void AppliquerVerrouillageParametres()
        {
            if (LicenceManager.EstPremiumActif()) return;

            // Griser les 3 GroupBoxes contenant les 4 paramètres premium
            groupBox1.Enabled = false;  // Coût électricité
            groupBox2.Enabled = false;  // Purge AMS
            groupBox3.Enabled = false;  // TVA + Marge

            // Afficher un message informatif sous les groupes verrouillés
            var lblVerrou = new Label
            {
                Text = "Ces paramètres nécessitent un abonnement actif. Les valeurs par défaut sont appliquées.",
                ForeColor = ThemeManager.AccentOrange,
                AutoSize = true,
                Font = new System.Drawing.Font(ThemeManager.FontSmall.FontFamily, 9f, System.Drawing.FontStyle.Italic),
                Location = new System.Drawing.Point(20, groupBox3.Bottom + 8)
            };
            tabPageGeneral.Controls.Add(lblVerrou);
        }

        // ═══════════════════════════════════════════════════════
        // ONGLET ENTREPRISE
        // ═══════════════════════════════════════════════════════

        private TextBox txtNomEntreprise;
        private TextBox txtAdresseEntreprise;
        private TextBox txtTelephoneEntreprise;
        private TextBox txtEmailEntreprise;
        private TextBox txtSiretEntreprise;
        private CheckBox chkAfficherNom;
        private CheckBox chkAfficherAdresse;
        private CheckBox chkAfficherTelephone;
        private CheckBox chkAfficherEmail;
        private CheckBox chkAfficherSiret;

        private void CreerOngletEntreprise()
        {
            var tabPageEntreprise = new TabPage
            {
                Text = "Entreprise",
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(3)
            };

            var groupBox = new GroupBox
            {
                Text = "Coordonnées de l'entreprise",
                Location = new Point(20, 20),
                Size = new Size(700, 320),
                Font = new Font("Segoe UI", 10f)
            };

            int y = 35;
            int lblX = 20;
            int txtX = 180;
            int txtW = 480;
            int espacement = 50;

            var lblNom = new Label { Text = "Nom :", Location = new Point(lblX, y + 3), AutoSize = true };
            txtNomEntreprise = new TextBox { Location = new Point(txtX, y), Size = new Size(txtW, 25) };
            y += espacement;

            var lblAdresse = new Label { Text = "Adresse :", Location = new Point(lblX, y + 3), AutoSize = true };
            txtAdresseEntreprise = new TextBox { Location = new Point(txtX, y), Size = new Size(txtW, 25) };
            y += espacement;

            var lblTelephone = new Label { Text = "Téléphone :", Location = new Point(lblX, y + 3), AutoSize = true };
            txtTelephoneEntreprise = new TextBox { Location = new Point(txtX, y), Size = new Size(txtW, 25) };
            y += espacement;

            var lblEmail = new Label { Text = "Email :", Location = new Point(lblX, y + 3), AutoSize = true };
            txtEmailEntreprise = new TextBox { Location = new Point(txtX, y), Size = new Size(txtW, 25) };
            y += espacement;

            var lblSiret = new Label { Text = "SIRET :", Location = new Point(lblX, y + 3), AutoSize = true };
            txtSiretEntreprise = new TextBox { Location = new Point(txtX, y), Size = new Size(txtW, 25) };
            y += espacement + 10;

            // Groupe "Afficher sur le devis"
            var groupBoxAffichage = new GroupBox
            {
                Text = "Afficher sur le devis",
                Location = new Point(20, 350),
                Size = new Size(700, 80),
                Font = new Font("Segoe UI", 10f)
            };

            int chkX = 20;
            int chkY = 30;
            int chkEspacement = 135;

            chkAfficherNom = new CheckBox { Text = "Nom", Location = new Point(chkX, chkY), AutoSize = true };
            chkX += chkEspacement;
            chkAfficherAdresse = new CheckBox { Text = "Adresse", Location = new Point(chkX, chkY), AutoSize = true };
            chkX += chkEspacement;
            chkAfficherTelephone = new CheckBox { Text = "Téléphone", Location = new Point(chkX, chkY), AutoSize = true };
            chkX += chkEspacement;
            chkAfficherEmail = new CheckBox { Text = "Email", Location = new Point(chkX, chkY), AutoSize = true };
            chkX += chkEspacement;
            chkAfficherSiret = new CheckBox { Text = "SIRET", Location = new Point(chkX, chkY), AutoSize = true };

            groupBoxAffichage.Controls.AddRange(new Control[] {
                chkAfficherNom, chkAfficherAdresse, chkAfficherTelephone,
                chkAfficherEmail, chkAfficherSiret
            });

            groupBox.Controls.AddRange(new Control[] {
                lblNom, txtNomEntreprise,
                lblAdresse, txtAdresseEntreprise,
                lblTelephone, txtTelephoneEntreprise,
                lblEmail, txtEmailEntreprise,
                lblSiret, txtSiretEntreprise
            });

            tabPageEntreprise.Controls.Add(groupBox);
            tabPageEntreprise.Controls.Add(groupBoxAffichage);
            tabControl1.TabPages.Add(tabPageEntreprise);

            // Charger les valeurs
            txtNomEntreprise.Text = parametres.NomEntreprise ?? "";
            txtAdresseEntreprise.Text = parametres.AdresseEntreprise ?? "";
            txtTelephoneEntreprise.Text = parametres.TelephoneEntreprise ?? "";
            txtEmailEntreprise.Text = parametres.EmailEntreprise ?? "";
            txtSiretEntreprise.Text = parametres.SiretEntreprise ?? "";
            chkAfficherNom.Checked = parametres.AfficherNomEntreprise;
            chkAfficherAdresse.Checked = parametres.AfficherAdresseEntreprise;
            chkAfficherTelephone.Checked = parametres.AfficherTelephoneEntreprise;
            chkAfficherEmail.Checked = parametres.AfficherEmailEntreprise;
            chkAfficherSiret.Checked = parametres.AfficherSiretEntreprise;

            // Appliquer le thème
            ThemeManager.StyleAllControls(tabPageEntreprise);
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
            // Validation des saisies
            if (numCoutElectricite.Value < 0)
            {
                MessageBox.Show("Le coût de l'électricité ne peut pas être négatif.", "Erreur de saisie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (numTVA.Value < 0 || numTVA.Value > 100)
            {
                MessageBox.Show("La TVA doit être entre 0% et 100%.", "Erreur de saisie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (numMarge.Value < 0)
            {
                MessageBox.Show("La marge ne peut pas être négative.", "Erreur de saisie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validation des bobines
            foreach (DataRow row in dtBobines.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;
                decimal poidsBob = row["Poids bobine (g)"] != DBNull.Value ? Convert.ToDecimal(row["Poids bobine (g)"]) : 0;
                decimal prixBob = row["Prix bobine (€)"] != DBNull.Value ? Convert.ToDecimal(row["Prix bobine (€)"]) : 0;
                if (poidsBob <= 0 || prixBob <= 0)
                {
                    MessageBox.Show("Chaque bobine doit avoir un poids et un prix supérieurs à 0.", "Erreur de saisie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Garde-fou : si pas d'abonnement actif, forcer les valeurs par défaut
            if (!LicenceManager.EstPremiumActif())
            {
                numCoutElectricite.Value = 0.15m;
                numPourcentagePurge.Value = 10m;
                numTVA.Value = 20m;
                numMarge.Value = 50m;
            }

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

            // Sauvegarder les imprimantes personnalisées
            var imprimantesCustom = new System.Collections.Generic.List<SpecsImprimante>();
            foreach (DataGridViewRow row in dgvImprimantesCustom.Rows)
            {
                if (row.IsNewRow) continue;
                string nom = row.Cells["ColNomImp"].Value?.ToString();
                if (string.IsNullOrWhiteSpace(nom)) continue;

                imprimantesCustom.Add(new SpecsImprimante
                {
                    Nom = nom,
                    PuissanceMaxWatts = row.Cells["ColPuissanceMax"].Value != null ? Convert.ToDecimal(row.Cells["ColPuissanceMax"].Value) : 250,
                    ConsommationMoyenneWatts = row.Cells["ColConsoMoy"].Value != null ? Convert.ToDecimal(row.Cells["ColConsoMoy"].Value) : 150,
                    CoefficientVitesse = row.Cells["ColCoeffVit"].Value != null ? Convert.ToDecimal(row.Cells["ColCoeffVit"].Value) : 1.0m,
                    CoefficientDechetAMS = row.Cells["ColCoeffDechet"].Value != null ? Convert.ToDecimal(row.Cells["ColCoeffDechet"].Value) : 1.0m
                });
            }
            ImprimanteSpecsManager.SauvegarderImprimantesCustom(imprimantesCustom);

            // Sauvegarder les infos entreprise
            parametres.NomEntreprise = txtNomEntreprise.Text.Trim();
            parametres.AdresseEntreprise = txtAdresseEntreprise.Text.Trim();
            parametres.TelephoneEntreprise = txtTelephoneEntreprise.Text.Trim();
            parametres.EmailEntreprise = txtEmailEntreprise.Text.Trim();
            parametres.SiretEntreprise = txtSiretEntreprise.Text.Trim();
            parametres.AfficherNomEntreprise = chkAfficherNom.Checked;
            parametres.AfficherAdresseEntreprise = chkAfficherAdresse.Checked;
            parametres.AfficherTelephoneEntreprise = chkAfficherTelephone.Checked;
            parametres.AfficherEmailEntreprise = chkAfficherEmail.Checked;
            parametres.AfficherSiretEntreprise = chkAfficherSiret.Checked;

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

        private void ConfigurerColonnesImprimantesCustom()
        {
            dgvImprimantesCustom.Columns.Clear();
            dgvImprimantesCustom.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColNomImp", HeaderText = "Nom", FillWeight = 30 });
            dgvImprimantesCustom.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColPuissanceMax", HeaderText = "Puissance max (W)", FillWeight = 18 });
            dgvImprimantesCustom.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColConsoMoy", HeaderText = "Conso. moy. (W)", FillWeight = 18 });
            dgvImprimantesCustom.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColCoeffVit", HeaderText = "Coeff. vitesse", FillWeight = 17 });
            dgvImprimantesCustom.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColCoeffDechet", HeaderText = "Coeff. déchet AMS", FillWeight = 17 });
        }

        private void ChargerImprimantesCustom()
        {
            dgvImprimantesCustom.Rows.Clear();
            var customs = ImprimanteSpecsManager.ChargerImprimantesCustom();
            foreach (var imp in customs)
            {
                dgvImprimantesCustom.Rows.Add(imp.Nom, imp.PuissanceMaxWatts, imp.ConsommationMoyenneWatts,
                    imp.CoefficientVitesse, imp.CoefficientDechetAMS);
            }
        }

        private void BtnAjouterImprimante_Click(object sender, EventArgs e)
        {
            dgvImprimantesCustom.Rows.Add("Nouvelle imprimante", 250, 150, 1.0, 1.0);
        }

        private void BtnSupprimerImprimante_Click(object sender, EventArgs e)
        {
            if (dgvImprimantesCustom.CurrentRow != null && !dgvImprimantesCustom.CurrentRow.IsNewRow)
            {
                dgvImprimantesCustom.Rows.Remove(dgvImprimantesCustom.CurrentRow);
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
                        // Infos entreprise (6 lignes optionnelles)
                        if (lines.Length > debutBobines && !lines[debutBobines].Contains("|"))
                        {
                            param.NomEntreprise = lines[debutBobines];
                            debutBobines++;
                        }
                        if (lines.Length > debutBobines && !lines[debutBobines].Contains("|"))
                        {
                            param.AdresseEntreprise = lines[debutBobines];
                            debutBobines++;
                        }
                        if (lines.Length > debutBobines && !lines[debutBobines].Contains("|"))
                        {
                            param.TelephoneEntreprise = lines[debutBobines];
                            debutBobines++;
                        }
                        if (lines.Length > debutBobines && !lines[debutBobines].Contains("|"))
                        {
                            param.EmailEntreprise = lines[debutBobines];
                            debutBobines++;
                        }
                        if (lines.Length > debutBobines && !lines[debutBobines].Contains("|"))
                        {
                            param.SiretEntreprise = lines[debutBobines];
                            debutBobines++;
                        }
                        if (lines.Length > debutBobines && !lines[debutBobines].Contains("|"))
                        {
                            // Format: N,A,T,E,S (5 flags séparés par virgule)
                            string[] flags = lines[debutBobines].Split(',');
                            if (flags.Length >= 5)
                            {
                                param.AfficherNomEntreprise = flags[0] == "1";
                                param.AfficherAdresseEntreprise = flags[1] == "1";
                                param.AfficherTelephoneEntreprise = flags[2] == "1";
                                param.AfficherEmailEntreprise = flags[3] == "1";
                                param.AfficherSiretEntreprise = flags[4] == "1";
                            }
                            else if (flags[0] == "1")
                            {
                                // Rétrocompatibilité ancien format (un seul bool)
                                param.AfficherNomEntreprise = true;
                                param.AfficherAdresseEntreprise = true;
                                param.AfficherTelephoneEntreprise = true;
                                param.AfficherEmailEntreprise = true;
                                param.AfficherSiretEntreprise = true;
                            }
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
                    param.CheminSlicer ?? "",
                    param.NomEntreprise ?? "",
                    param.AdresseEntreprise ?? "",
                    param.TelephoneEntreprise ?? "",
                    param.EmailEntreprise ?? "",
                    param.SiretEntreprise ?? "",
                    $"{(param.AfficherNomEntreprise ? "1" : "0")},{(param.AfficherAdresseEntreprise ? "1" : "0")},{(param.AfficherTelephoneEntreprise ? "1" : "0")},{(param.AfficherEmailEntreprise ? "1" : "0")},{(param.AfficherSiretEntreprise ? "1" : "0")}"
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

        /// <summary>
        /// Si l'abonnement est expiré, réinitialise les 4 paramètres premium aux valeurs
        /// par défaut dans le fichier .dat. Appelé au démarrage de l'application.
        /// </summary>
        public static void ReinitialiserParametresPremium()
        {
            if (LicenceManager.EstPremiumActif()) return;
            if (!File.Exists(FichierParametres)) return;
            try
            {
                string[] lignes = File.ReadAllLines(FichierParametres, Encoding.UTF8);
                if (lignes.Length < 4) return;
                string[] defauts = { "0.15", "10", "20", "50" };
                bool modifie = false;
                for (int i = 0; i < 4; i++)
                    if (lignes[i] != defauts[i]) { lignes[i] = defauts[i]; modifie = true; }
                if (modifie)
                {
                    File.WriteAllLines(FichierParametres, lignes, Encoding.UTF8);
                    parametresCache = null;
                }
            }
            catch { }
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
        public string NomEntreprise { get; set; } = "";
        public string AdresseEntreprise { get; set; } = "";
        public string TelephoneEntreprise { get; set; } = "";
        public string EmailEntreprise { get; set; } = "";
        public string SiretEntreprise { get; set; } = "";
        public bool AfficherNomEntreprise { get; set; } = false;
        public bool AfficherAdresseEntreprise { get; set; } = false;
        public bool AfficherTelephoneEntreprise { get; set; } = false;
        public bool AfficherEmailEntreprise { get; set; } = false;
        public bool AfficherSiretEntreprise { get; set; } = false;
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