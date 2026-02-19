using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public partial class Form1 : Form
    {
        private readonly UserManager userManager;
        private DataTable dtCouleurs;
        private List<NumericUpDown> tempsPlateauxControls;
        private Dictionary<string, Color> couleursDictionnaire;
        private ThreeMFParser.ThreeMFFile fichier3mfAnalyse;
        private bool valeurSlicerUtilisees = false;

        public Form1(UserManager manager)
        {
            InitializeComponent();
            userManager = manager;
            InitializeForm();

            // Connecter les événements des boutons 3MF
            btnBrowse3mf.Click += BtnBrowse3mf_Click;
            btnAnalyser3mf.Click += BtnAnalyser3mf_Click;
            btnAnalyserLot.Click += BtnAnalyserLot_Click;

            // Connecter les événements de calibration
            rdo3mfAuto.CheckedChanged += Rdo3mfMode_CheckedChanged;
            rdo3mfManuel.CheckedChanged += Rdo3mfMode_CheckedChanged;
            btnEnregistrerCalibration.Click += BtnEnregistrerCalibration_Click;
            btnPartagerCalibration.Click += BtnPartagerCalibration_Click;
            cmb3mfMatiere.SelectedIndex = 0; // PLA par défaut

            // Ajouter le menu Administration (visible uniquement pour les admins)
            AjouterMenuAdministration();

            // Afficher le nombre de données de calibration
            MettreAJourInfoCalibration();

            // Nettoyage automatique de l'historique (> 365 jours)
            HistoriqueManager.NettoyerHistorique();

            // Charger le thème sauvegardé et appliquer
            ChargerThemeSauvegarde();
            AppliquerTheme();

            // S'abonner aux changements de thème
            ThemeManager.ThemeChanged += () =>
            {
                AppliquerTheme();
                thèmeToolStripMenuItem.Text = ThemeManager.ThemeActuel == Theme.Sombre
                    ? "Thème clair" : "Thème sombre";
                SauvegarderTheme();
            };
        }

        private string GenererEnteteEntreprise(ParametresImpression parametres)
        {
            var sb = new StringBuilder();

            if (parametres.AfficherNomEntreprise && !string.IsNullOrWhiteSpace(parametres.NomEntreprise))
                sb.AppendLine($"  {parametres.NomEntreprise}");
            if (parametres.AfficherAdresseEntreprise && !string.IsNullOrWhiteSpace(parametres.AdresseEntreprise))
                sb.AppendLine($"  {parametres.AdresseEntreprise}");

            var contact = new List<string>();
            if (parametres.AfficherTelephoneEntreprise && !string.IsNullOrWhiteSpace(parametres.TelephoneEntreprise))
                contact.Add($"Tél: {parametres.TelephoneEntreprise}");
            if (parametres.AfficherEmailEntreprise && !string.IsNullOrWhiteSpace(parametres.EmailEntreprise))
                contact.Add(parametres.EmailEntreprise);
            if (contact.Count > 0)
                sb.AppendLine($"  {string.Join("  |  ", contact)}");

            if (parametres.AfficherSiretEntreprise && !string.IsNullOrWhiteSpace(parametres.SiretEntreprise))
                sb.AppendLine($"  SIRET: {parametres.SiretEntreprise}");

            if (sb.Length > 0)
                sb.AppendLine();

            return sb.ToString();
        }

        private void AppliquerTheme()
        {
            ThemeManager.ApplyThemeToForm(this);

            // En-tête bleu
            panelHeader.BackColor = ThemeManager.PrimaryBlue;
            lblWelcomeUser.ForeColor = ThemeManager.TextOnDark;
            lblWelcomeUser.Font = ThemeManager.FontSubtitle;

            // Styler tous les contrôles récursivement
            ThemeManager.StyleAllControls(this);

            // Boutons principaux (style large + couleurs spécifiques)
            ThemeManager.StyleButtonLarge(btnCalculerDevis, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);
            ThemeManager.StyleButtonLarge(btnCalculerDevis3mf, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);

            // Boutons secondaires avec couleurs spécifiques
            ThemeManager.StyleButton(btnAnalyser3mf, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);
            ThemeManager.StyleButton(btnBrowse3mf, ThemeManager.NeutralGray, ThemeManager.NeutralGrayDark);
            ThemeManager.StyleButton(btnAnalyserLot, ThemeManager.SecondaryGreen, ThemeManager.SecondaryGreenDark);
            ThemeManager.StyleButton(btnEnregistrerCalibration, ThemeManager.SecondaryGreen, ThemeManager.SecondaryGreenDark);
            ThemeManager.StyleButton(btnPartagerCalibration, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);

            // Label info calibration
            lblCalibrationInfo.ForeColor = ThemeManager.TextSecondary;

            // Label statut AMS
            if (!chkAMS.Checked)
                lblAMSStatus.ForeColor = ThemeManager.TextSecondary;

            // Compteur couleurs
            lblNombreCouleurs.ForeColor = ThemeManager.PrimaryBlue;
            lblNombreCouleurs.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        private void InitializeForm()
        {
            // Afficher le nom de l'utilisateur connecté
            lblWelcomeUser.Text = $"Bienvenue, {userManager.CurrentUser.Username}";
            
            // Détecter le slicer Bambu Studio
            if (SlicerManager.EstInstalle())
            {
                chkUtiliserSlicer.Visible = true;
                chkUtiliserSlicer.Checked = true;
                lblStatutSlicer.Text = "";
            }
            else
            {
                chkUtiliserSlicer.Visible = false;
                chkUtiliserSlicer.Checked = false;
                lblStatutSlicer.Text = "";
            }

            // Initialiser le dictionnaire des couleurs
            InitialiserCouleursDictionnaire();
            
            // Initialiser les imprimantes depuis ImprimanteSpecsManager (un seul appel pour les 2 combos)
            var listeImprimantes = ImprimanteSpecsManager.ObtenirListeImprimantes().ToArray();
            cmbPrinter.Items.AddRange(listeImprimantes);
            cmbPrinter.SelectedIndex = 0;
            cmb3mfPrinter.Items.AddRange(listeImprimantes);
            cmb3mfPrinter.SelectedIndex = 0;

            // Initialiser les marques de filament
            string[] marques = new string[] { "Bambu Lab", "Creality", "eSUN", "Polymaker", "Prusament", "Generic" };
            
            // Initialiser le DataTable pour les couleurs
            dtCouleurs = new DataTable();
            dtCouleurs.Columns.Add("Plateau", typeof(int));
            dtCouleurs.Columns.Add("Couleur", typeof(string));
            dtCouleurs.Columns.Add("Type", typeof(string));
            dtCouleurs.Columns.Add("Marque", typeof(string));
            dtCouleurs.Columns.Add("Poids (g)", typeof(decimal));
            
            ConfigurerColonnesDataGrid(marques);
            
            dgvCouleurs.DataSource = dtCouleurs;
            dgvCouleurs.CellFormatting += DgvCouleurs_CellFormatting;
            
            AjouterLigneVide();
            
            // Initialiser les contrôles de temps par plateau
            tempsPlateauxControls = new List<NumericUpDown>();
            InitialiserTempsPlateaux(1);
            
            // Désactiver AMS et multi-couleur au démarrage
            chkAMS.Enabled = false;
            rdoMultiCouleur.Enabled = false;

            // Verrouiller les fonctionnalités premium si pas d'abonnement actif
            AppliquerVerrouillageAbonnement();
        }

        private void AppliquerVerrouillageAbonnement()
        {
            if (LicenceManager.EstPremiumActif()) return;

            // Griser tous les contrôles de l'onglet 3MF
            tabPage3mf.Enabled = false;

            // Bloquer la navigation vers l'onglet verrouillé
            tabControlMain.Selecting += (s, e) =>
            {
                if (e.TabPage == tabPage3mf)
                    e.Cancel = true;
            };

            // Ajouter un panneau d'information par-dessus le contenu grisé
            var panel = new System.Windows.Forms.Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(235, 237, 240)
            };
            var lbl = new System.Windows.Forms.Label
            {
                Text = "Fonctionnalité réservée aux abonnés\r\n\r\n" +
                       "L'analyse de fichiers 3MF (Bambu Lab) nécessite un abonnement actif.\r\n\r\n" +
                       "Activez votre licence depuis le menu Aide > Activer une licence.",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                ForeColor = ThemeManager.TextSecondary,
                Font = ThemeManager.FontBody,
                BackColor = System.Drawing.Color.Transparent
            };
            panel.Controls.Add(lbl);
            tabPage3mf.Controls.Add(panel);
            panel.BringToFront();
        }

        /// <summary>
        /// Événement pour parcourir et sélectionner un fichier 3MF
        /// </summary>
        private void BtnBrowse3mf_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Fichiers 3MF (*.3mf)|*.3mf|Tous les fichiers (*.*)|*.*";
                openFileDialog.Title = "Sélectionnez un fichier 3MF";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txt3mfFile.Text = openFileDialog.FileName;
                }
            }
        }

        /// <summary>
        /// Événement pour analyser un lot de fichiers 3MF
        /// </summary>
        private void BtnAnalyserLot_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Fichiers 3MF (*.3mf)|*.3mf";
                ofd.Title = "Sélectionnez plusieurs fichiers 3MF";
                ofd.Multiselect = true;
                ofd.RestoreDirectory = true;

                if (ofd.ShowDialog() != DialogResult.OK || ofd.FileNames.Length == 0) return;

                var parametres = ParametresImpressionForm.ObtenirParametres();
                StringBuilder sb = new StringBuilder();
                sb.Append(GenererEnteteEntreprise(parametres));
                sb.AppendLine("═══════════════════════════════════════════════════");
                sb.AppendLine("           DEVIS GROUPÉ - LOT DE FICHIERS 3MF");
                sb.AppendLine("═══════════════════════════════════════════════════");
                sb.AppendLine($"🖨️  Imprimante: {cmb3mfPrinter.SelectedItem}");
                sb.AppendLine($"📦 Nombre de fichiers: {ofd.FileNames.Length}");
                sb.AppendLine();

                decimal totalPoidsg = 0;
                decimal totalTempsMin = 0;
                decimal totalPrixTTC = 0;
                int fichierOK = 0;

                foreach (string fichier in ofd.FileNames)
                {
                    try
                    {
                        var parsed = ThreeMFParser.ParseFile(fichier);
                        if (parsed.TotalVertices <= 0) continue;

                        string matiere = cmb3mfMatiere.SelectedItem?.ToString() ?? "PLA";
                        int infill = (int)num3mfInfill.Value;
                        decimal poids = CalibrationManager.EstimerPoids(parsed.TotalVertices, matiere, infill);
                        string imprimante = cmb3mfPrinter.SelectedItem?.ToString() ?? "";
                        decimal tempsMin = CalibrationManager.EstimerTemps(parsed.TotalVertices, imprimante);
                        decimal tempsH = tempsMin / 60m;

                        // Coûts
                        decimal prixKg = ObtenirPrixMoyenBobines(parametres, matiere);
                        decimal coutMatiere = (poids / 1000m) * prixKg;
                        var specs = ImprimanteSpecsManager.ObtenirSpecs(imprimante);
                        decimal coutElec = specs.ConsommationMoyenneKw * tempsH * parametres.CoutElectriciteKwh;
                        decimal coutMO = tempsH * parametres.CoutMainOeuvreHeure;
                        decimal coutAmort = tempsH * parametres.AmortissementMachineHeure;
                        decimal coutProd = coutMatiere + coutElec + coutMO + coutAmort;
                        decimal marge = coutProd * (parametres.MargeParObjet / 100m);
                        decimal sousTotal = coutProd + marge;
                        decimal tva = sousTotal * (parametres.TVA / 100m);
                        decimal prixTTC = sousTotal + tva;

                        totalPoidsg += poids;
                        totalTempsMin += tempsMin;
                        totalPrixTTC += prixTTC;
                        fichierOK++;

                        sb.AppendLine($"───────────────────────────────────────────────────");
                        sb.AppendLine($"📄 {System.IO.Path.GetFileName(fichier)}");
                        sb.AppendLine($"   Poids: {poids:F2} g | Temps: {tempsH:F2}h | Prix TTC: {prixTTC:F2} €");
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"───────────────────────────────────────────────────");
                        sb.AppendLine($"❌ {System.IO.Path.GetFileName(fichier)} : {ex.Message}");
                    }
                }

                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════════════");
                sb.AppendLine($"   TOTAL : {fichierOK} fichier(s)");
                sb.AppendLine($"   Poids total: {totalPoidsg:F2} g");
                sb.AppendLine($"   Temps total: {totalTempsMin / 60:F2} heures");
                sb.AppendLine($"   💰 PRIX TOTAL TTC: {totalPrixTTC:F2} €");
                sb.AppendLine("═══════════════════════════════════════════════════");

                string contenu = sb.ToString();
                HistoriqueManager.Sauvegarder(new EntreeHistorique
                {
                    Date = DateTime.Now,
                    Titre = $"Lot - {fichierOK} fichiers 3MF",
                    PrixTotalTTC = totalPrixTTC,
                    ContenuDevis = contenu
                });

                new DevisForm("Devis groupé - Lot 3MF", contenu).ShowDialog();
            }
        }

        /// <summary>
        /// Événement pour analyser le fichier 3MF sélectionné
        /// </summary>
        private void BtnAnalyser3mf_Click(object sender, EventArgs e)
        {
            // Vérifier qu'un fichier est sélectionné
            if (string.IsNullOrWhiteSpace(txt3mfFile.Text))
            {
                MessageBox.Show("Veuillez sélectionner un fichier 3MF.", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!System.IO.File.Exists(txt3mfFile.Text))
            {
                MessageBox.Show("Le fichier sélectionné n'existe pas.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Analyser le fichier 3MF
                fichier3mfAnalyse = ThreeMFParser.ParseFile(txt3mfFile.Text);
                
                // Générer le rapport
                string rapport = ThreeMFParser.GenerateReport(fichier3mfAnalyse);
                
                // Afficher le rapport dans la zone de texte
                txt3mfInfo.Text = rapport;
                txt3mfInfo.Select(0, 0); // Aller au début
                
                // Pré-remplir le poids estimé si disponible
                if (fichier3mfAnalyse.TotalVertices > 0)
                {
                    // ═══════════════════════════════════════════════════════════════════
                    // FORMULE MONO-COULEUR (SANS AMS) - BASÉE SUR VERTICES
                    // ═══════════════════════════════════════════════════════════════════
                    // Référence Crocs Keychain:
                    //   • Vertices: 613 492
                    //   • Poids: 24,27g
                    // Référence Dragon:
                    //   • Vertices: 1 326 122
                    //   • Poids: 41,54g
                    // Ratio moyen: ~0.0000395 g/vertex (40g par million de vertices)
                    // ═══════════════════════════════════════════════════════════════════
                    
                    // Utiliser l'algorithme calibré si des données sont disponibles
                    string matiere3mf = cmb3mfMatiere.SelectedItem?.ToString() ?? "PLA";
                    int infill3mf = (int)num3mfInfill.Value;
                    decimal poidsEstime = CalibrationManager.EstimerPoids(
                        fichier3mfAnalyse.TotalVertices, matiere3mf, infill3mf);

                    num3mfPoidsFilament.Value = Math.Round(poidsEstime, 2);
                }
                
                // ═══════════════════════════════════════════════════════════════════
                // FORMULE TEMPS MONO-COULEUR (SANS AMS) - BASÉE SUR VERTICES
                // ═══════════════════════════════════════════════════════════════════
                // Référence Crocs Keychain:
                //   • Vertices: 613 492
                //   • Temps: 93 minutes
                // Référence Dragon:
                //   • Vertices: 1 326 122
                //   • Temps: 165 minutes
                // Ratio moyen: ~0.000124 min/vertex (7,44 min par million de vertices)
                // ═══════════════════════════════════════════════════════════════════
                
                if (fichier3mfAnalyse.TotalVertices > 0)
                {
                    // Utiliser l'algorithme calibré pour le temps
                    string imprimante3mf = cmb3mfPrinter.SelectedItem?.ToString() ?? "";
                    decimal tempsEstimeMinutes = CalibrationManager.EstimerTemps(
                        fichier3mfAnalyse.TotalVertices, imprimante3mf);

                    decimal tempsEstimeHeures = tempsEstimeMinutes / 60m;
                    num3mfTempsImpression.Value = Math.Round(tempsEstimeHeures, 2);
                }
                
                // Pré-remplir les informations d'impression si disponibles
                if (fichier3mfAnalyse.PrintInfo != null)
                {
                    var printInfo = fichier3mfAnalyse.PrintInfo;
                    
                    // Déterminer si multi-couleur basé sur les métadonnées
                    if (printInfo.FilamentColors != null && printInfo.FilamentColors.Count > 1)
                    {
                        chk3mfAMS.Checked = true;
                        num3mfNombreCouleurs.Value = printInfo.FilamentColors.Count;
                    }
                }
                
                // Lancer le slicing précis en arrière-plan si activé
                if (chkUtiliserSlicer.Checked && SlicerManager.EstInstalle())
                {
                    valeurSlicerUtilisees = false;
                    lblStatutSlicer.Text = "Slicing en cours...";
                    lblStatutSlicer.ForeColor = ThemeManager.PrimaryBlue;
                    progressBarSlicer.Visible = true;
                    btnAnalyser3mf.Enabled = false;

                    SlicerManager.SlicerEnArrierePlan(txt3mfFile.Text, (resultat) =>
                    {
                        this.Invoke((Action)(() =>
                        {
                            btnAnalyser3mf.Enabled = true;
                            progressBarSlicer.Visible = false;
                            if (resultat.Succes)
                            {
                                valeurSlicerUtilisees = true;
                                lblStatutSlicer.Text = "Valeurs précises (Bambu Studio) - supports inclus";
                                lblStatutSlicer.ForeColor = ThemeManager.SecondaryGreen;

                                if (resultat.PoidsFilamentGrammes > 0)
                                    num3mfPoidsFilament.Value = Math.Round(resultat.PoidsFilamentGrammes, 2);

                                if (resultat.TempsMinutes > 0)
                                    num3mfTempsImpression.Value = Math.Round(resultat.TempsMinutes / 60m, 2);
                            }
                            else
                            {
                                valeurSlicerUtilisees = false;
                                lblStatutSlicer.Text = $"Slicer : {resultat.MessageErreur}";
                                lblStatutSlicer.ForeColor = ThemeManager.DangerRed;
                            }
                        }));
                    });

                    // Pas de MessageBox bloquant — la ProgressBar + le label suffisent
                }
                else
                {
                    MessageBox.Show($"Analyse terminée : {fichier3mfAnalyse.Objects.Count} objet(s) détecté(s).",
                        "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (InvalidOperationException ex)
            {
                txt3mfInfo.Text = $"❌ Erreur lors de l'analyse du fichier 3MF:\n\n{ex.Message}";
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur d'analyse",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                txt3mfInfo.Text = $"❌ Erreur inattendue:\n\n{ex.Message}";
                MessageBox.Show($"Erreur inattendue: {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Événement checkbox AMS pour l'onglet 3MF
        /// </summary>
        private void chk3mfAMS_CheckedChanged(object sender, EventArgs e)
        {
            lbl3mfNombreCouleurs.Visible = chk3mfAMS.Checked;
            num3mfNombreCouleurs.Visible = chk3mfAMS.Checked;
        }

        /// <summary>
        /// Calculer le devis basé sur le fichier 3MF
        /// </summary>
        private void btnCalculerDevis3mf_Click(object sender, EventArgs e)
        {
            // Vérifier qu'un fichier a été analysé
            if (fichier3mfAnalyse == null)
            {
                MessageBox.Show("Veuillez d'abord analyser un fichier 3MF.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Valider les entrées
            if (num3mfTempsImpression.Value == 0)
            {
                MessageBox.Show("Veuillez entrer le temps d'impression.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (num3mfPoidsFilament.Value == 0)
            {
                MessageBox.Show("Veuillez entrer le poids de filament.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Charger les paramètres
                var parametres = ParametresImpressionForm.ObtenirParametres();

                // Calculer les coûts
                decimal poidsFilament = num3mfPoidsFilament.Value;
                decimal tempsHeures = num3mfTempsImpression.Value;
                int nombreCouleurs = chk3mfAMS.Checked ? (int)num3mfNombreCouleurs.Value : 1;

                // Calcul du poids de purge si AMS
                // Si le slicer est utilisé, le poids inclut déjà supports + purge
                decimal poidsPurge = 0;
                if (!valeurSlicerUtilisees && chk3mfAMS.Checked && nombreCouleurs > 1)
                {
                    var specsImprimante = ImprimanteSpecsManager.ObtenirSpecs(cmb3mfPrinter.SelectedItem?.ToString() ?? "");
                    poidsPurge = poidsFilament * (parametres.PourcentagePurgeAMS / 100m) * specsImprimante.CoefficientDechetAMS;
                }

                decimal poidsTotal = poidsFilament + poidsPurge;

                // Calcul du coût matière (prix moyen des bobines PLA ou prix par défaut)
                decimal prixKgFilament = ObtenirPrixMoyenBobines(parametres, "PLA");
                decimal coutMatiere = (poidsTotal / 1000m) * prixKgFilament;

                // Calcul du coût électricité selon l'imprimante sélectionnée
                var specs3mf = ImprimanteSpecsManager.ObtenirSpecs(cmb3mfPrinter.SelectedItem?.ToString() ?? "");
                decimal puissanceKw = specs3mf.ConsommationMoyenneKw;

                decimal consommationKwh = puissanceKw * tempsHeures;
                decimal coutElectricite = consommationKwh * parametres.CoutElectriciteKwh;

                // Main-d'oeuvre et amortissement
                decimal coutMainOeuvre = tempsHeures * parametres.CoutMainOeuvreHeure;
                decimal coutAmortissement = tempsHeures * parametres.AmortissementMachineHeure;

                // Coût de production HT
                decimal coutProductionHT = coutMatiere + coutElectricite + coutMainOeuvre + coutAmortissement;

                // Appliquer la marge (en pourcentage)
                decimal marge = coutProductionHT * (parametres.MargeParObjet / 100m);
                decimal sousTotalHT = coutProductionHT + marge;

                // Calculer la TVA
                decimal montantTVA = sousTotalHT * (parametres.TVA / 100m);

                // Prix total TTC
                decimal prixTotalTTC = sousTotalHT + montantTVA;

                // Afficher le résultat
                StringBuilder sb = new StringBuilder();
                sb.Append(GenererEnteteEntreprise(parametres));
                sb.AppendLine("═══════════════════════════════════════════════════");
                sb.AppendLine("           DEVIS D'IMPRESSION 3D - BAMBU LAB");
                sb.AppendLine("═══════════════════════════════════════════════════");
                sb.AppendLine();
                sb.AppendLine($"📄 Fichier: {fichier3mfAnalyse.FileName}");
                sb.AppendLine($"🖨️  Imprimante: {cmb3mfPrinter.SelectedItem}");
                sb.AppendLine($"📦 Nombre d'objets: {fichier3mfAnalyse.Objects.Count}");
                if (valeurSlicerUtilisees)
                    sb.AppendLine($"✅ Source: Bambu Studio (valeurs précises, supports inclus)");
                sb.AppendLine();
                sb.AppendLine("───────────────────────────────────────────────────");
                sb.AppendLine("  DÉTAILS DE L'IMPRESSION");
                sb.AppendLine("───────────────────────────────────────────────────");
                sb.AppendLine($"⏱️  Temps d'impression: {tempsHeures:F2} heures");
                sb.AppendLine($"🎨 Mode: {(chk3mfAMS.Checked ? $"Multi-couleur ({nombreCouleurs} couleurs)" : "Mono-couleur")}");
                if (valeurSlicerUtilisees)
                {
                    sb.AppendLine($"📊 Poids filament total: {poidsFilament:F2} g (supports + purge inclus)");
                }
                else
                {
                    sb.AppendLine($"📊 Poids filament net: {poidsFilament:F2} g");
                    if (poidsPurge > 0)
                        sb.AppendLine($"🔄 Poids purge AMS ({parametres.PourcentagePurgeAMS}%): {poidsPurge:F2} g");
                    sb.AppendLine($"📦 Poids total: {poidsTotal:F2} g");
                }
                sb.AppendLine();
                sb.AppendLine("───────────────────────────────────────────────────");
                sb.AppendLine("  DÉTAILS DES COÛTS");
                sb.AppendLine("───────────────────────────────────────────────────");
                sb.AppendLine($"💰 Coût matière: {coutMatiere:F2} €");
                sb.AppendLine($"    ({prixKgFilament:F2} €/kg × {poidsTotal / 1000:F3} kg)");
                sb.AppendLine($"⚡ Coût électricité: {coutElectricite:F2} €");
                sb.AppendLine($"    ({consommationKwh:F3} kWh × {parametres.CoutElectriciteKwh:F2} €/kWh)");
                if (coutMainOeuvre > 0)
                    sb.AppendLine($"👷 Main-d'oeuvre: {coutMainOeuvre:F2} € ({tempsHeures:F2}h × {parametres.CoutMainOeuvreHeure:F2} €/h)");
                if (coutAmortissement > 0)
                    sb.AppendLine($"🏭 Amortissement: {coutAmortissement:F2} € ({tempsHeures:F2}h × {parametres.AmortissementMachineHeure:F2} €/h)");
                sb.AppendLine($"🔧 Coût production HT: {coutProductionHT:F2} €");
                sb.AppendLine($"📈 Marge ({parametres.MargeParObjet}%): {marge:F2} €");
                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════════════");
                sb.AppendLine($"   Sous-total HT: {sousTotalHT:F2} €");
                sb.AppendLine($"   TVA ({parametres.TVA}%): {montantTVA:F2} €");
                sb.AppendLine("───────────────────────────────────────────────────");
                sb.AppendLine($"   💰 PRIX TOTAL TTC: {prixTotalTTC:F2} €");
                sb.AppendLine("═══════════════════════════════════════════════════");

                // Sauvegarder dans l'historique
                string contenu3mf = sb.ToString();
                HistoriqueManager.Sauvegarder(new EntreeHistorique
                {
                    Date = DateTime.Now,
                    Titre = $"3MF - {fichier3mfAnalyse.FileName}",
                    PrixTotalTTC = prixTotalTTC,
                    ContenuDevis = contenu3mf
                });

                new DevisForm("Devis d'impression 3D - 3MF", contenu3mf).ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du calcul:\n{ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitialiserCouleursDictionnaire()
        {
            couleursDictionnaire = ThemeManager.CouleursDictionnaire;
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

            // Validation des poids dans la grille
            for (int i = 0; i < dtCouleurs.Rows.Count; i++)
            {
                object valPoids = dtCouleurs.Rows[i]["Poids (g)"];
                if (valPoids == DBNull.Value || Convert.ToDecimal(valPoids) <= 0)
                {
                    MessageBox.Show($"Le poids de la ligne {i + 1} doit être supérieur à 0.", "Erreur de saisie",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Validation des temps par plateau
            foreach (var numTemps in tempsPlateauxControls)
            {
                if (numTemps.Value <= 0)
                {
                    MessageBox.Show("Le temps d'impression de chaque plateau doit être supérieur à 0.", "Erreur de saisie",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Charger les paramètres
            ParametresImpression parametres = ParametresImpressionForm.ObtenirParametres();
            
            // Calculer les coûts détaillés
            DetailsCouts details = CalculerCoutsDetailles(parametres);
            
            int nbPlateaux = (int)numNombrePlateaux.Value;
            int nbObjets = (int)numNombreObjets.Value;
            
            string detailsParPlateau = ObtenirDetailsParPlateau();
            string detailsTemps = ObtenirDetailsTemps();
            
            string enteteEntreprise = GenererEnteteEntreprise(parametres);
            string message = enteteEntreprise +
                           $"═══════════════════════════════════════════\n" +
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
                           (details.CoutMainOeuvre > 0 ? $"  Main-d'oeuvre :\n    • {details.TempsTotal / 60:F2}h × {parametres.CoutMainOeuvreHeure:C2}/h = {details.CoutMainOeuvre:C2}\n\n" : "") +
                           (details.CoutAmortissement > 0 ? $"  Amortissement machine :\n    • {details.TempsTotal / 60:F2}h × {parametres.AmortissementMachineHeure:C2}/h = {details.CoutAmortissement:C2}\n\n" : "") +
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

            // Sauvegarder dans l'historique
            HistoriqueManager.Sauvegarder(new EntreeHistorique
            {
                Date = DateTime.Now,
                Titre = "Calcul manuel",
                PrixTotalTTC = details.PrixTotalTTC,
                ContenuDevis = message
            });

            new DevisForm("Devis - Prix de revient", message).ShowDialog();
        }

        private DetailsCouts CalculerCoutsDetailles(ParametresImpression parametres)
        {
            DetailsCouts details = new DetailsCouts();
            
            // 1. Calculer le poids net
            details.PoidsNet = CalculerPoidsTotal();
            
            // 2. Calculer le poids de purge si AMS activé (avec coefficient imprimante)
            var specsImprimanteManuel = ImprimanteSpecsManager.ObtenirSpecs(cmbPrinter.SelectedItem?.ToString() ?? "");
            if (chkAMS.Checked)
            {
                details.PoidsPurge = details.PoidsNet * (parametres.PourcentagePurgeAMS / 100m) * specsImprimanteManuel.CoefficientDechetAMS;
            }

            details.PoidsTotal = details.PoidsNet + details.PoidsPurge;

            // 3. Calculer le coût de la matière en utilisant les bobines des paramètres
            decimal coutMatiereNet = CalculerCoutMatiere(parametres);
            // Appliquer la purge proportionnellement au coût matière (avec coefficient imprimante)
            details.CoutMatiere = chkAMS.Checked
                ? coutMatiereNet * (1 + (parametres.PourcentagePurgeAMS / 100m) * specsImprimanteManuel.CoefficientDechetAMS)
                : coutMatiereNet;
            
            // 4. Calculer le temps total
            details.TempsTotal = CalculerTempsTotal();
            
            // 5. Calculer la consommation électrique selon l'imprimante sélectionnée
            decimal puissanceMoyenneKw = specsImprimanteManuel.ConsommationMoyenneKw;
            details.ConsommationKwh = (details.TempsTotal / 60m) * puissanceMoyenneKw;
            details.CoutElectricite = details.ConsommationKwh * parametres.CoutElectriciteKwh;
            
            // 6. Main-d'oeuvre et amortissement machine
            decimal tempsHeures = details.TempsTotal / 60m;
            details.CoutMainOeuvre = tempsHeures * parametres.CoutMainOeuvreHeure;
            details.CoutAmortissement = tempsHeures * parametres.AmortissementMachineHeure;

            // 7. Coût de production HT
            details.CoutProductionHT = details.CoutMatiere + details.CoutElectricite + details.CoutMainOeuvre + details.CoutAmortissement;
            
            // 7. Marge
            int nbObjets = (int)numNombreObjets.Value;
            details.Marge = details.CoutProductionHT * (parametres.MargeParObjet / 100m);
            
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
                    
                    // Chercher la bobine correspondante dans les paramètres (insensible à la casse)
                    Bobine bobine = parametres.Bobines.FirstOrDefault(b =>
                        string.Equals(b.Matiere, type, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(b.Couleur, couleur, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(b.Marque, marque, StringComparison.OrdinalIgnoreCase));

                    // Calculer le prix/kg (bobine ou prix par défaut)
                    decimal prixParKg = bobine != null ? bobine.PrixParKg : ObtenirPrixFilament(type);

                    // Le poids de purge est déjà inclus dans PoidsTotal via CalculerCoutsDetailles()
                    // Ici on calcule le coût proportionnel de chaque couleur sur le poids net
                    coutTotal += (poids / 1000m) * prixParKg;
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

        private decimal ObtenirPrixMoyenBobines(ParametresImpression parametres, string type)
        {
            var bobinesType = parametres.Bobines
                .Where(b => string.Equals(b.Matiere, type, StringComparison.OrdinalIgnoreCase) && b.PrixParKg > 0)
                .ToList();

            if (bobinesType.Count > 0)
            {
                return bobinesType.Average(b => b.PrixParKg);
            }

            return ObtenirPrixFilament(type);
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

        private void historiqueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new HistoriqueForm().ShowDialog();
        }

        private void statistiquesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new StatistiquesForm().ShowDialog();
        }

        private void templatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new TemplatesForm())
            {
                if (form.ShowDialog() == DialogResult.OK && form.TemplateSelectionne != null)
                {
                    var t = form.TemplateSelectionne;
                    // Appliquer le template aux contrôles du calcul manuel
                    for (int i = 0; i < cmbPrinter.Items.Count; i++)
                    {
                        if (cmbPrinter.Items[i].ToString() == t.Imprimante)
                        { cmbPrinter.SelectedIndex = i; break; }
                    }
                    if (t.AMS)
                    {
                        rdoMultiCouleur.Checked = true;
                        chkAMS.Checked = true;
                        numNombreCouleurs.Value = t.NombreCouleurs;
                    }
                    else
                    {
                        rdoMonoCouleur.Checked = true;
                    }
                    numNombrePlateaux.Value = t.NombrePlateaux;
                    numNombreObjets.Value = t.NombreObjets;
                    MessageBox.Show($"Template \"{t.Nom}\" chargé.", "Template", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void sauvegarderTemplateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string nom = "";
            using (var inputForm = new Form())
            {
                inputForm.Text = "Nom du template";
                inputForm.Size = new Size(350, 150);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;
                ThemeManager.ApplyThemeToForm(inputForm);

                var lbl = new Label { Text = "Nom :", Location = new Point(15, 20), AutoSize = true };
                var txt = new TextBox { Location = new Point(60, 17), Width = 250 };
                ThemeManager.StyleTextBox(txt);
                var btnOK = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(130, 60), Size = new Size(80, 30) };
                ThemeManager.StyleButton(btnOK, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);

                inputForm.Controls.AddRange(new Control[] { lbl, txt, btnOK });
                inputForm.AcceptButton = btnOK;

                if (inputForm.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txt.Text))
                    nom = txt.Text.Trim();
            }

            if (string.IsNullOrEmpty(nom)) return;

            var template = new TemplateDevis
            {
                Nom = nom,
                Imprimante = cmbPrinter.SelectedItem?.ToString() ?? "",
                Matiere = dtCouleurs.Rows.Count > 0 ? dtCouleurs.Rows[0]["Type"]?.ToString() ?? "PLA" : "PLA",
                Marque = dtCouleurs.Rows.Count > 0 ? dtCouleurs.Rows[0]["Marque"]?.ToString() ?? "Bambu Lab" : "Bambu Lab",
                AMS = chkAMS.Checked,
                NombreCouleurs = (int)numNombreCouleurs.Value,
                NombrePlateaux = (int)numNombrePlateaux.Value,
                NombreObjets = (int)numNombreObjets.Value
            };

            TemplateManager.Sauvegarder(template);
            MessageBox.Show($"Template \"{nom}\" sauvegardé.", "Template", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void thèmeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ThemeManager.BasculerTheme();
        }

        private void ChargerThemeSauvegarde()
        {
            string fichierTheme = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "theme.dat");
            if (System.IO.File.Exists(fichierTheme))
            {
                string contenu = System.IO.File.ReadAllText(fichierTheme).Trim();
                if (contenu == "Sombre")
                {
                    ThemeManager.DefinirTheme(Theme.Sombre);
                    thèmeToolStripMenuItem.Text = "Thème clair";
                }
            }
        }

        private void SauvegarderTheme()
        {
            string fichierTheme = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "theme.dat");
            System.IO.File.WriteAllText(fichierTheme, ThemeManager.ThemeActuel.ToString());
        }

        private void profilToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProfileForm profileForm = new ProfileForm(userManager);
            profileForm.ShowDialog();
        }

        private void AjouterMenuAdministration()
        {
            if (userManager.CurrentUser == null || !userManager.CurrentUser.EstAdmin) return;

            var menuAdmin = new ToolStripMenuItem("Administration");
            menuAdmin.Font = new System.Drawing.Font(menuAdmin.Font, System.Drawing.FontStyle.Bold);
            menuAdmin.ForeColor = ThemeManager.PrimaryBlue;
            menuAdmin.Click += (s, e) => OuvrirAdministration();

            // Insérer avant "Déconnexion" dans le menu Fichier
            var menuFichier = fichierToolStripMenuItem;
            int indexDeconnexion = -1;
            for (int i = 0; i < menuFichier.DropDownItems.Count; i++)
            {
                if (menuFichier.DropDownItems[i].Name == "déconnexionToolStripMenuItem")
                {
                    indexDeconnexion = i;
                    break;
                }
            }
            if (indexDeconnexion >= 0)
                menuFichier.DropDownItems.Insert(indexDeconnexion, menuAdmin);
            else
                menuFichier.DropDownItems.Add(menuAdmin);
        }

        private void OuvrirAdministration()
        {
            AdminForm adminForm = new AdminForm(userManager);
            adminForm.ShowDialog();
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

        private void Rdo3mfMode_CheckedChanged(object sender, EventArgs e)
        {
            bool modeManuel = rdo3mfManuel.Checked;
            lbl3mfPoidsReel.Visible = modeManuel;
            num3mfPoidsReel.Visible = modeManuel;
            lbl3mfTempsReel.Visible = modeManuel;
            num3mfTempsReel.Visible = modeManuel;
            btnEnregistrerCalibration.Visible = modeManuel;
        }

        private void BtnEnregistrerCalibration_Click(object sender, EventArgs e)
        {
            if (fichier3mfAnalyse == null)
            {
                MessageBox.Show("Veuillez d'abord analyser un fichier 3MF.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (num3mfPoidsReel.Value == 0 || num3mfTempsReel.Value == 0)
            {
                MessageBox.Show("Veuillez entrer le poids réel et le temps réel.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var donnee = new DonneeCalibration
            {
                NomFichier = System.IO.Path.GetFileName(txt3mfFile.Text),
                NombreVertices = fichier3mfAnalyse.TotalVertices,
                PoidsReel = num3mfPoidsReel.Value,
                TempsReel = num3mfTempsReel.Value,
                Matiere = cmb3mfMatiere.SelectedItem?.ToString() ?? "PLA",
                PourcentageInfill = (int)num3mfInfill.Value,
                Imprimante = cmb3mfPrinter.SelectedItem?.ToString() ?? "",
                DateSaisie = DateTime.Now,
                Utilisateur = userManager.CurrentUser.Username
            };

            CalibrationManager.EnregistrerDonnee(donnee);
            MettreAJourInfoCalibration();

            MessageBox.Show(
                $"Données de calibration enregistrées !\n\n" +
                $"Fichier : {donnee.NomFichier}\n" +
                $"Vertices : {donnee.NombreVertices:N0}\n" +
                $"Poids réel : {donnee.PoidsReel:F2} g\n" +
                $"Temps réel : {donnee.TempsReel:F1} min\n" +
                $"Ratio poids : {donnee.RatioPoids:F7} g/vertex\n" +
                $"Ratio temps : {donnee.RatioTemps:F7} min/vertex",
                "Calibration enregistrée",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnPartagerCalibration_Click(object sender, EventArgs e)
        {
            try
            {
                var parametres = ParametresImpressionForm.ObtenirParametres();
                CalibrationManager.PartagerDonnees(parametres.TokenGithub);
                MessageBox.Show("Données de calibration partagées avec succès !",
                    "Partage réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Erreur de partage",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du partage :\n{ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MettreAJourInfoCalibration()
        {
            try
            {
                int nbDonnees = CalibrationManager.ObtenirNombreDonnees();
                lblCalibrationInfo.Text = $"{nbDonnees} donnée(s) de calibration disponible(s)";
            }
            catch
            {
                lblCalibrationInfo.Text = "0 donnée(s) de calibration disponible(s)";
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
        public decimal CoutMainOeuvre { get; set; }
        public decimal CoutAmortissement { get; set; }
        public decimal CoutProductionHT { get; set; }
        public decimal Marge { get; set; }
        public decimal SousTotalHT { get; set; }
        public decimal MontantTVA { get; set; }
        public decimal PrixTotalTTC { get; set; }
        public decimal PrixParObjet { get; set; }
    }
}
