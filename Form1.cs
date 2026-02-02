using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public partial class Form1 : Form
    {
        private readonly UserManager userManager;
        private DataTable dtCouleurs;
        private List<NumericUpDown> tempsPlateauxControls;
        private Dictionary<string, Color> couleursDictionnaire;

        public Form1(UserManager manager)
        {
            InitializeComponent();
            userManager = manager;
            InitializeForm();
        }

        private void InitializeForm()
        {
            // Afficher le nom de l'utilisateur connecté
            lblWelcomeUser.Text = $"Bienvenue, {userManager.CurrentUser.Username}";
            
            // Initialiser le dictionnaire des couleurs
            InitialiserCouleursDictionnaire();
            
            // Initialiser les imprimantes disponibles
            cmbPrinter.Items.AddRange(new string[] 
            { 
                "Bambu Lab X1 Carbon",
                "Bambu Lab P1P",
                "Bambu Lab P2S",
                "Bambu Lab A1 Mini",
                "Creality Ender 3",
                "Prusa i3 MK3S+",
                "Anycubic Kobra"
            });
            cmbPrinter.SelectedIndex = 0;

            // Initialiser les marques de filament
            string[] marques = new string[] { "Bambu Lab", "Creality", "eSUN", "Polymaker", "Prusament", "Generic" };
            
            // Initialiser le DataTable pour les couleurs
            dtCouleurs = new DataTable();
            dtCouleurs.Columns.Add("Plateau", typeof(int));
            dtCouleurs.Columns.Add("Couleur", typeof(string));
            dtCouleurs.Columns.Add("Type", typeof(string));
            dtCouleurs.Columns.Add("Marque", typeof(string));
            dtCouleurs.Columns.Add("Poids (g)", typeof(decimal));

            // Configuration du DataGridView
            dgvCouleurs.DataSource = dtCouleurs;
            dgvCouleurs.AutoGenerateColumns = false;
            dgvCouleurs.AllowUserToAddRows = true;
            dgvCouleurs.AllowUserToDeleteRows = true;
            
            // Configurer les colonnes avec ComboBox pour Type et Marque
            ConfigurerColonnesDataGrid(marques);
            
            // Ajouter l'événement pour colorier les cellules
            dgvCouleurs.CellFormatting += DgvCouleurs_CellFormatting;

            // Valeurs par défaut
            rdoMonoCouleur.Checked = true;
            chkAMS.Checked = false;
            numNombreAMS.Value = 1;
            numNombreAMS.Enabled = false;
            numNombrePlateaux.Value = 1;
            numNombrePlateaux.Enabled = false;
            numNombreCouleurs.Value = 1;
            numNombreCouleurs.Enabled = false;
            numNombreObjets.Value = 1;
            
            // Initialiser la liste des temps de plateaux
            tempsPlateauxControls = new List<NumericUpDown>();
            
            // Mode mono-couleur par défaut - ajouter une seule ligne vide
            AjouterLigneVide();
            InitialiserTempsPlateaux(1);
            
            lblNombreCouleurs.Text = "Couleurs: 0 | Plateaux: 1";
        }

        private void InitialiserCouleursDictionnaire()
        {
            couleursDictionnaire = new Dictionary<string, Color>
            {
                { "Rouge", Color.Red },
                { "Bleu", Color.Blue },
                { "Vert", Color.Green },
                { "Jaune", Color.Yellow },
                { "Orange", Color.Orange },
                { "Violet", Color.Purple },
                { "Rose", Color.Pink },
                { "Noir", Color.Black },
                { "Blanc", Color.White },
                { "Gris", Color.Gray },
                { "Marron", Color.Brown },
                { "Cyan", Color.Cyan },
                { "Magenta", Color.Magenta },
                { "Beige", Color.Beige },
                { "Turquoise", Color.Turquoise },
                { "Or", Color.Gold },
                { "Argent", Color.Silver },
                { "Bronze", Color.FromArgb(205, 127, 50) }
            };
        }

        private void DgvCouleurs_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Colorier uniquement la colonne "Couleur"
            if (dgvCouleurs.Columns[e.ColumnIndex].Name == "Couleur" && e.Value != null)
            {
                string nomCouleur = e.Value.ToString();
                
                if (couleursDictionnaire.ContainsKey(nomCouleur))
                {
                    Color couleur = couleursDictionnaire[nomCouleur];
                    e.CellStyle.BackColor = couleur;
                    
                    // Ajuster la couleur du texte pour une meilleure lisibilité
                    int luminosite = (int)((couleur.R * 0.299) + (couleur.G * 0.587) + (couleur.B * 0.114));
                    e.CellStyle.ForeColor = luminosite > 128 ? Color.Black : Color.White;
                }
            }
        }

        private void AjouterLigneVide()
        {
            DataRow row = dtCouleurs.NewRow();
            row["Plateau"] = 1;
            row["Couleur"] = "Rouge";
            row["Type"] = "PLA";
            row["Marque"] = "Bambu Lab";
            row["Poids (g)"] = 10.0m;
            dtCouleurs.Rows.Add(row);
        }

        private void ConfigurerColonnesDataGrid(string[] marques)
        {
            dgvCouleurs.Columns.Clear();
            
            // Colonne Plateau (numérique)
            DataGridViewTextBoxColumn colPlateau = new DataGridViewTextBoxColumn
            {
                Name = "Plateau",
                HeaderText = "Plateau N°",
                DataPropertyName = "Plateau",
                Width = 80
            };
            dgvCouleurs.Columns.Add(colPlateau);
            
            // Colonne Couleur (ComboBox avec vraies couleurs)
            DataGridViewComboBoxColumn colCouleur = new DataGridViewComboBoxColumn
            {
                Name = "Couleur",
                HeaderText = "Couleur",
                DataPropertyName = "Couleur",
                Width = 120
            };
            colCouleur.Items.AddRange(couleursDictionnaire.Keys.ToArray());
            dgvCouleurs.Columns.Add(colCouleur);
            
            // Colonne Type (ComboBox)
            DataGridViewComboBoxColumn colType = new DataGridViewComboBoxColumn
            {
                Name = "Type",
                HeaderText = "Type",
                DataPropertyName = "Type",
                Width = 100
            };
            colType.Items.AddRange(new string[] { "PLA", "PETG", "ABS", "TPU", "ASA", "Nylon" });
            dgvCouleurs.Columns.Add(colType);
            
            // Colonne Marque (ComboBox)
            DataGridViewComboBoxColumn colMarque = new DataGridViewComboBoxColumn
            {
                Name = "Marque",
                HeaderText = "Marque",
                DataPropertyName = "Marque",
                Width = 120
            };
            colMarque.Items.AddRange(marques);
            dgvCouleurs.Columns.Add(colMarque);
            
            // Colonne Poids (numérique)
            DataGridViewTextBoxColumn colPoids = new DataGridViewTextBoxColumn
            {
                Name = "Poids",
                HeaderText = "Poids (g)",
                DataPropertyName = "Poids (g)",
                Width = 100
            };
            dgvCouleurs.Columns.Add(colPoids);
        }

        private void InitialiserTempsPlateaux(int nombre)
        {
            // Nettoyer les anciens contrôles
            foreach (var ctrl in tempsPlateauxControls)
            {
                panelTemps.Controls.Remove(ctrl);
                if (ctrl.Tag is Label lbl)
                {
                    panelTemps.Controls.Remove(lbl);
                }
            }
            tempsPlateauxControls.Clear();
            
            // Créer les nouveaux contrôles
            for (int i = 0; i < nombre; i++)
            {
                Label lbl = new Label
                {
                    Text = $"Plateau {i + 1} (min):",
                    AutoSize = true,
                    Location = new System.Drawing.Point(10, 10 + (i * 35))
                };
                
                NumericUpDown num = new NumericUpDown
                {
                    Minimum = 1,
                    Maximum = 10000,
                    Value = 60,
                    Location = new System.Drawing.Point(130, 8 + (i * 35)),
                    Width = 80,
                    Tag = lbl
                };
                
                panelTemps.Controls.Add(lbl);
                panelTemps.Controls.Add(num);
                tempsPlateauxControls.Add(num);
            }
            
            // Ajuster la hauteur du panel si nécessaire
            if (nombre > 0)
            {
                int hauteurNecessaire = 15 + (nombre * 35);
                panelTemps.AutoScroll = hauteurNecessaire > 200;
            }
        }

        private void rdoMonoCouleur_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoMonoCouleur.Checked)
            {
                chkAMS.Enabled = false;
                chkAMS.Checked = false;
                numNombreAMS.Enabled = false;
                numNombreAMS.Value = 1;
                numNombrePlateaux.Enabled = false;
                numNombrePlateaux.Value = 1;
                numNombreCouleurs.Enabled = false;
                numNombreCouleurs.Value = 1;
                
                InitialiserTempsPlateaux(1);
                UpdateStatutCouleurs();
            }
        }

        private void rdoMultiCouleur_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoMultiCouleur.Checked)
            {
                chkAMS.Enabled = true;
                numNombrePlateaux.Enabled = true;
                numNombreCouleurs.Enabled = false;
                UpdateStatutCouleurs();
            }
        }

        private void chkAMS_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAMS.Checked)
            {
                numNombreAMS.Enabled = true;
                lblAMSStatus.Text = "AMS activé";
                lblAMSStatus.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                numNombreAMS.Enabled = false;
                numNombreAMS.Value = 1;
                lblAMSStatus.Text = "AMS désactivé";
                lblAMSStatus.ForeColor = System.Drawing.Color.Gray;
            }
            UpdateStatutCouleurs();
        }

        private void numNombreAMS_ValueChanged(object sender, EventArgs e)
        {
            UpdateStatutCouleurs();
        }

        private void numNombrePlateaux_ValueChanged(object sender, EventArgs e)
        {
            InitialiserTempsPlateaux((int)numNombrePlateaux.Value);
            UpdateStatutCouleurs();
        }

        private void numNombreCouleurs_ValueChanged(object sender, EventArgs e)
        {
            UpdateStatutCouleurs();
        }

        private void UpdateStatutCouleurs()
        {
            int nombreCouleurs = dtCouleurs.Rows.Count;
            int nombrePlateaux = (int)numNombrePlateaux.Value;
            int maxCouleurs = chkAMS.Checked ? (int)numNombreAMS.Value * 4 : 24;
            
            lblNombreCouleurs.Text = chkAMS.Checked 
                ? $"Couleurs: {nombreCouleurs} (max {maxCouleurs}) | Plateaux: {nombrePlateaux}"
                : $"Couleurs: {nombreCouleurs} | Plateaux: {nombrePlateaux}";
        }

        private void btnCalculerDevis_Click(object sender, EventArgs e)
        {
            // Vérifier qu'il y a au moins une couleur
            if (dtCouleurs.Rows.Count == 0)
            {
                MessageBox.Show("Veuillez ajouter au moins une couleur.", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Charger les paramètres
            ParametresImpression parametres = ParametresImpressionForm.ObtenirParametres();
            
            // Calculer les coûts détaillés
            DetailsCouts details = CalculerCoutsDetailles(parametres);
            
            int nbPlateaux = (int)numNombrePlateaux.Value;
            int nbObjets = (int)numNombreObjets.Value;
            
            string detailsParPlateau = ObtenirDetailsParPlateau();
            string detailsTemps = ObtenirDetailsTemps();
            
            string message = $"═══════════════════════════════════════════\n" +
                           $"  DEVIS D'IMPRESSION 3D\n" +
                           $"  Calcul du prix de revient\n" +
                           $"═══════════════════════════════════════════\n\n" +
                           $"CONFIGURATION :\n" +
                           $"  • Imprimante : {cmbPrinter.SelectedItem}\n" +
                           $"  • Mode : {(rdoMonoCouleur.Checked ? "Mono-couleur" : "Multi-couleur")}\n" +
                           (chkAMS.Checked ? $"  • AMS : {numNombreAMS.Value} unité(s)\n" : "") +
                           $"  • Plateaux : {nbPlateaux}\n" +
                           $"  • Objets à produire : {nbObjets}\n" +
                           $"  • Couleurs utilisées : {dtCouleurs.Rows.Count}\n\n" +
                           $"───────────────────────────────────────────\n" +
                           $"DÉTAILS PAR PLATEAU :\n" +
                           $"───────────────────────────────────────────\n" +
                           detailsParPlateau +
                           $"\n───────────────────────────────────────────\n" +
                           $"TEMPS D'IMPRESSION :\n" +
                           $"───────────────────────────────────────────\n" +
                           detailsTemps +
                           $"  Total : {details.TempsTotal} min ({details.TempsTotal / 60:F1}h)\n\n" +
                           $"───────────────────────────────────────────\n" +
                           $"DÉCOMPOSITION DES COÛTS :\n" +
                           $"───────────────────────────────────────────\n" +
                           $"  Matière première :\n" +
                           $"    • Poids net utilisé : {details.PoidsNet}g\n" +
                           (chkAMS.Checked ? $"    • Poids purge AMS ({parametres.PourcentagePurgeAMS}%) : +{details.PoidsPurge}g\n" : "") +
                           $"    • Poids total consommé : {details.PoidsTotal}g\n" +
                           $"    • Coût matière : {details.CoutMatiere:C2}\n\n" +
                           $"  Électricité :\n" +
                           $"    • Consommation estimée : {details.ConsommationKwh:F2} kWh\n" +
                           $"    • Tarif : {parametres.CoutElectriciteKwh:C4}/kWh\n" +
                           $"    • Coût électricité : {details.CoutElectricite:C2}\n\n" +
                           $"  ─────────────────────────────────────────\n" +
                           $"  Coût de production HT : {details.CoutProductionHT:C2}\n" +
                           $"  Marge ({parametres.MargeParObjet}% × {nbObjets} objets) : +{details.Marge:C2}\n" +
                           $"  ─────────────────────────────────────────\n" +
                           $"  Sous-total HT : {details.SousTotalHT:C2}\n" +
                           $"  TVA ({parametres.TVA}%) : +{details.MontantTVA:C2}\n\n" +
                           $"═══════════════════════════════════════════\n" +
                           $"  PRIX DE VENTE TTC : {details.PrixTotalTTC:C2}\n" +
                           $"  Prix unitaire par objet : {details.PrixParObjet:C2}\n" +
                           $"═══════════════════════════════════════════\n\n" +
                           $"RENTABILITÉ :\n" +
                           $"  • Coût de revient/objet : {details.CoutProductionHT / nbObjets:C2}\n" +
                           $"  • Prix de vente/objet : {details.PrixParObjet:C2}\n" +
                           $"  • Bénéfice/objet (HT) : {(details.SousTotalHT - details.CoutProductionHT) / nbObjets:C2}\n" +
                           $"  • Marge brute : {((details.SousTotalHT - details.CoutProductionHT) / details.SousTotalHT * 100):F1}%";

            MessageBox.Show(message, "Devis - Prix de revient", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private DetailsCouts CalculerCoutsDetailles(ParametresImpression parametres)
        {
            DetailsCouts details = new DetailsCouts();
            
            // 1. Calculer le poids net
            details.PoidsNet = CalculerPoidsTotal();
            
            // 2. Calculer le poids de purge si AMS activé
            if (chkAMS.Checked)
            {
                details.PoidsPurge = details.PoidsNet * (parametres.PourcentagePurgeAMS / 100m);
            }
            
            details.PoidsTotal = details.PoidsNet + details.PoidsPurge;
            
            // 3. Calculer le coût de la matière en utilisant les bobines des paramètres
            details.CoutMatiere = CalculerCoutMatiere(parametres);
            
            // 4. Calculer le temps total
            details.TempsTotal = CalculerTempsTotal();
            
            // 5. Calculer la consommation électrique (estimation moyenne : 0.15 kW pour une imprimante 3D)
            decimal puissanceMoyenneKw = 0.15m;
            details.ConsommationKwh = (details.TempsTotal / 60m) * puissanceMoyenneKw;
            details.CoutElectricite = details.ConsommationKwh * parametres.CoutElectriciteKwh;
            
            // 6. Coût de production HT
            details.CoutProductionHT = details.CoutMatiere + details.CoutElectricite;
            
            // 7. Marge (par objet)
            int nbObjets = (int)numNombreObjets.Value;
            details.Marge = details.CoutProductionHT * (parametres.MargeParObjet / 100m) * nbObjets;
            
            // 8. Sous-total HT
            details.SousTotalHT = details.CoutProductionHT + details.Marge;
            
            // 9. TVA
            details.MontantTVA = details.SousTotalHT * (parametres.TVA / 100m);
            
            // 10. Prix total TTC
            details.PrixTotalTTC = details.SousTotalHT + details.MontantTVA;
            
            // 11. Prix par objet
            details.PrixParObjet = nbObjets > 0 ? details.PrixTotalTTC / nbObjets : 0;
            
            return details;
        }

        private decimal CalculerCoutMatiere(ParametresImpression parametres)
        {
            decimal coutTotal = 0;
            
            foreach (DataRow row in dtCouleurs.Rows)
            {
                if (row.RowState != DataRowState.Deleted && 
                    row["Type"] != DBNull.Value && 
                    row["Couleur"] != DBNull.Value &&
                    row["Marque"] != DBNull.Value &&
                    row["Poids (g)"] != DBNull.Value)
                {
                    string type = row["Type"].ToString();
                    string couleur = row["Couleur"].ToString();
                    string marque = row["Marque"].ToString();
                    decimal poids = Convert.ToDecimal(row["Poids (g)"]);
                    
                    // Ajouter le poids de purge si AMS
                    decimal poidsAvecPurge = poids;
                    if (chkAMS.Checked)
                    {
                        poidsAvecPurge = poids * (1 + parametres.PourcentagePurgeAMS / 100m);
                    }
                    
                    // Chercher la bobine correspondante dans les paramètres
                    Bobine bobine = parametres.Bobines.FirstOrDefault(b => 
                        b.Matiere == type && 
                        b.Couleur == couleur && 
                        b.Marque == marque);
                    
                    if (bobine != null)
                    {
                        // Utiliser le prix de la bobine
                        coutTotal += (poidsAvecPurge / 1000m) * bobine.PrixParKg;
                    }
                    else
                    {
                        // Utiliser un prix par défaut si la bobine n'est pas trouvée
                        decimal prixParKg = ObtenirPrixFilament(type);
                        coutTotal += (poidsAvecPurge / 1000m) * prixParKg;
                    }
                }
            }
            
            return coutTotal;
        }

        private decimal CalculerPoidsTotal()
        {
            decimal total = 0;
            foreach (DataRow row in dtCouleurs.Rows)
            {
                if (row.RowState != DataRowState.Deleted && row["Poids (g)"] != DBNull.Value)
                {
                    total += Convert.ToDecimal(row["Poids (g)"]);
                }
            }
            return total;
        }

        private decimal CalculerTempsTotal()
        {
            decimal total = 0;
            foreach (var num in tempsPlateauxControls)
            {
                total += num.Value;
            }
            return total;
        }

        private string ObtenirDetailsParPlateau()
        {
            string details = "";
            
            var plateaux = dtCouleurs.AsEnumerable()
                .Where(r => r.RowState != DataRowState.Deleted && r["Plateau"] != DBNull.Value)
                .GroupBy(r => Convert.ToInt32(r["Plateau"]))
                .OrderBy(g => g.Key);
            
            foreach (var plateau in plateaux)
            {
                int numPlateau = plateau.Key;
                details += $"\n  Plateau {numPlateau} :\n";
                
                foreach (var row in plateau)
                {
                    string couleur = row["Couleur"]?.ToString() ?? "N/A";
                    string type = row["Type"]?.ToString() ?? "N/A";
                    string marque = row["Marque"]?.ToString() ?? "N/A";
                    decimal poids = row["Poids (g)"] != DBNull.Value ? Convert.ToDecimal(row["Poids (g)"]) : 0;
                    
                    details += $"    • {couleur} ({type} {marque}) : {poids}g\n";
                }
            }
            
            return details;
        }

        private string ObtenirDetailsTemps()
        {
            string details = "";
            for (int i = 0; i < tempsPlateauxControls.Count; i++)
            {
                details += $"  Plateau {i + 1} : {tempsPlateauxControls[i].Value} min\n";
            }
            return details;
        }

        private decimal CalculerCout()
        {
            ParametresImpression parametres = ParametresImpressionForm.ObtenirParametres();
            return CalculerCoutsDetailles(parametres).PrixTotalTTC;
        }

        private decimal ObtenirPrixFilament(string type)
        {
            switch (type)
            {
                case "PLA":
                    return 20.00m;
                case "PETG":
                    return 25.00m;
                case "ABS":
                    return 22.00m;
                case "TPU":
                    return 35.00m;
                case "ASA":
                    return 30.00m;
                case "Nylon":
                    return 40.00m;
                default:
                    return 20.00m;
            }
        }

        private void paramètresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParametresImpressionForm parametresForm = new ParametresImpressionForm();
            parametresForm.ShowDialog();
        }

        private void profilToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProfileForm profileForm = new ProfileForm(userManager);
            profileForm.ShowDialog();
        }

        private void déconnexionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Voulez-vous vraiment vous déconnecter ?", 
                "Déconnexion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void quitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Voulez-vous vraiment quitter l'application ?", 
                "Quitter", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void vérifierMiseÀJourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateManager.VerifierMiseAJour(afficherSiAJour: true);
        }

        private void rafraîchirSpécsImprimantesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ImprimanteSpecsManager.RafraichirSpecsDepuisInternet();
                MessageBox.Show("Spécifications des imprimantes mises à jour avec succès !", 
                    "Mise à jour", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la mise à jour :\n{ex.Message}", 
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void àProposToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            MessageBox.Show(
                $"Logiciel de Calcul de Devis pour Impression 3D\n\n" +
                $"Version : {version}\n" +
                $"© 2024 - Tous droits réservés\n\n" +
                $"Fonctionnalités :\n" +
                $"  • Calcul automatique du prix de revient\n" +
                $"  • Support multi-couleurs avec AMS Bambu Lab\n" +
                $"  • Gestion des bobines et tarifs\n" +
                $"  • Mise à jour automatique des specs imprimantes\n" +
                $"  • Calcul TVA et marge personnalisable",
                "À propos",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    // Classe pour stocker les détails de coûts
    public class DetailsCouts
    {
        public decimal PoidsNet { get; set; }
        public decimal PoidsPurge { get; set; }
        public decimal PoidsTotal { get; set; }
        public decimal CoutMatiere { get; set; }
        public decimal TempsTotal { get; set; }
        public decimal ConsommationKwh { get; set; }
        public decimal CoutElectricite { get; set; }
        public decimal CoutProductionHT { get; set; }
        public decimal Marge { get; set; }
        public decimal SousTotalHT { get; set; }
        public decimal MontantTVA { get; set; }
        public decimal PrixTotalTTC { get; set; }
        public decimal PrixParObjet { get; set; }
    }
}
