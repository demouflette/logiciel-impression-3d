using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public class StatistiquesForm : Form
    {
        public StatistiquesForm()
        {
            InitialiserComposants();
            AppliquerTheme();
            CalculerStatistiques();
        }

        private Label lblTitre;
        private Panel panelContenu;
        private GroupBox grpGeneral;
        private GroupBox grpMensuel;
        private GroupBox grpTop;
        private Label lblNbDevis;
        private Label lblCaTotal;
        private Label lblPrixMoyen;
        private Label lblMoisActuel;
        private Label lblMoisPrecedent;
        private Label lblTopFichiers;
        private Button btnFermer;

        private void InitialiserComposants()
        {
            this.Text = "Statistiques";
            this.Size = new Size(550, 500);
            this.MinimumSize = new Size(450, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            panelContenu = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(15)
            };

            lblTitre = new Label
            {
                Text = "Tableau de bord",
                Font = ThemeManager.FontTitle,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Groupe général
            grpGeneral = new GroupBox
            {
                Text = "Vue d'ensemble",
                Location = new Point(15, 50),
                Size = new Size(490, 110),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            lblNbDevis = new Label { Location = new Point(15, 25), AutoSize = true, Font = ThemeManager.FontBody };
            lblCaTotal = new Label { Location = new Point(15, 50), AutoSize = true, Font = ThemeManager.FontBody };
            lblPrixMoyen = new Label { Location = new Point(15, 75), AutoSize = true, Font = ThemeManager.FontBody };
            grpGeneral.Controls.AddRange(new Control[] { lblNbDevis, lblCaTotal, lblPrixMoyen });

            // Groupe mensuel
            grpMensuel = new GroupBox
            {
                Text = "Comparaison mensuelle",
                Location = new Point(15, 170),
                Size = new Size(490, 85),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            lblMoisActuel = new Label { Location = new Point(15, 25), AutoSize = true, Font = ThemeManager.FontBody };
            lblMoisPrecedent = new Label { Location = new Point(15, 50), AutoSize = true, Font = ThemeManager.FontBody };
            grpMensuel.Controls.AddRange(new Control[] { lblMoisActuel, lblMoisPrecedent });

            // Groupe top fichiers
            grpTop = new GroupBox
            {
                Text = "Top 5 des fichiers les plus devisés",
                Location = new Point(15, 265),
                Size = new Size(490, 140),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            lblTopFichiers = new Label
            {
                Location = new Point(15, 25),
                Size = new Size(460, 105),
                Font = ThemeManager.FontBody
            };
            grpTop.Controls.Add(lblTopFichiers);

            panelContenu.Controls.AddRange(new Control[] { lblTitre, grpGeneral, grpMensuel, grpTop });

            // Bouton fermer
            var panelBas = new Panel { Dock = DockStyle.Bottom, Height = 55, Padding = new Padding(10) };
            btnFermer = new Button
            {
                Text = "Fermer",
                Size = new Size(120, 35),
                Location = new Point(10, 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnFermer.Click += (s, e) => this.Close();
            panelBas.Controls.Add(btnFermer);

            this.Controls.Add(panelContenu);
            this.Controls.Add(panelBas);
            this.AcceptButton = btnFermer;
        }

        private void AppliquerTheme()
        {
            ThemeManager.ApplyThemeToForm(this);
            ThemeManager.StyleAllControls(this);
            ThemeManager.StyleButton(btnFermer, ThemeManager.NeutralGray, ThemeManager.NeutralGrayDark);
            lblTitre.ForeColor = ThemeManager.PrimaryBlue;
        }

        private void CalculerStatistiques()
        {
            var historique = HistoriqueManager.Charger();

            // Vue d'ensemble
            int nbDevis = historique.Count;
            decimal caTotal = historique.Sum(h => h.PrixTotalTTC);
            decimal prixMoyen = nbDevis > 0 ? caTotal / nbDevis : 0;

            lblNbDevis.Text = $"Nombre total de devis : {nbDevis}";
            lblCaTotal.Text = $"Chiffre d'affaires estimé : {caTotal:F2} €";
            lblPrixMoyen.Text = $"Prix moyen par devis : {prixMoyen:F2} €";

            // Comparaison mensuelle
            DateTime debutMoisActuel = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime debutMoisPrecedent = debutMoisActuel.AddMonths(-1);

            var devisCeMois = historique.Where(h => h.Date >= debutMoisActuel).ToList();
            var devisMoisPrecedent = historique.Where(h => h.Date >= debutMoisPrecedent && h.Date < debutMoisActuel).ToList();

            decimal caMois = devisCeMois.Sum(h => h.PrixTotalTTC);
            decimal caMoisPrec = devisMoisPrecedent.Sum(h => h.PrixTotalTTC);

            lblMoisActuel.Text = $"Ce mois-ci : {devisCeMois.Count} devis — {caMois:F2} €";
            lblMoisPrecedent.Text = $"Mois précédent : {devisMoisPrecedent.Count} devis — {caMoisPrec:F2} €";

            // Top 5 fichiers
            var top5 = historique
                .GroupBy(h => h.Titre)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .ToList();

            if (top5.Count > 0)
            {
                var lignes = new List<string>();
                for (int i = 0; i < top5.Count; i++)
                {
                    lignes.Add($"{i + 1}. {top5[i].Key} ({top5[i].Count()} fois)");
                }
                lblTopFichiers.Text = string.Join("\r\n", lignes);
            }
            else
            {
                lblTopFichiers.Text = "Aucun devis dans l'historique.";
            }
        }
    }
}
