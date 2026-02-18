using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    // ═══════════════════════════════════════════════════════
    // MODÈLE
    // ═══════════════════════════════════════════════════════

    public class TemplateDevis
    {
        public string Nom { get; set; } = "";
        public string Imprimante { get; set; } = "";
        public string Matiere { get; set; } = "PLA";
        public string Marque { get; set; } = "Bambu Lab";
        public bool AMS { get; set; }
        public int NombreCouleurs { get; set; } = 1;
        public int NombrePlateaux { get; set; } = 1;
        public int NombreObjets { get; set; } = 1;
        public DateTime DateCreation { get; set; } = DateTime.Now;
    }

    // ═══════════════════════════════════════════════════════
    // MANAGER
    // ═══════════════════════════════════════════════════════

    public static class TemplateManager
    {
        private static readonly string FichierTemplates = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "templates_devis.dat");

        public static void Sauvegarder(TemplateDevis template)
        {
            var templates = Charger();
            // Remplacer si même nom
            templates.RemoveAll(t => string.Equals(t.Nom, template.Nom, StringComparison.OrdinalIgnoreCase));
            templates.Add(template);
            EcrireFichier(templates);
        }

        public static List<TemplateDevis> Charger()
        {
            var templates = new List<TemplateDevis>();
            if (!File.Exists(FichierTemplates)) return templates;

            try
            {
                string[] lignes = File.ReadAllLines(FichierTemplates, Encoding.UTF8);
                foreach (string ligne in lignes)
                {
                    if (string.IsNullOrWhiteSpace(ligne)) continue;
                    string[] p = ligne.Split('|');
                    if (p.Length < 8) continue;

                    try
                    {
                        templates.Add(new TemplateDevis
                        {
                            Nom = p[0],
                            Imprimante = p[1],
                            Matiere = p[2],
                            Marque = p[3],
                            AMS = p[4] == "1",
                            NombreCouleurs = int.TryParse(p[5], out int nc) ? nc : 1,
                            NombrePlateaux = int.TryParse(p[6], out int np) ? np : 1,
                            NombreObjets = int.TryParse(p[7], out int no) ? no : 1,
                            DateCreation = p.Length > 8 && DateTime.TryParseExact(p[8], "yyyy-MM-dd HH:mm:ss",
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt) ? dt : DateTime.Now
                        });
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                LogManager.Erreur("Chargement templates devis", ex);
            }
            return templates;
        }

        public static void Supprimer(string nom)
        {
            var templates = Charger();
            templates.RemoveAll(t => string.Equals(t.Nom, nom, StringComparison.OrdinalIgnoreCase));
            EcrireFichier(templates);
        }

        private static void EcrireFichier(List<TemplateDevis> templates)
        {
            try
            {
                var sb = new StringBuilder();
                foreach (var t in templates)
                {
                    sb.AppendLine($"{t.Nom}|{t.Imprimante}|{t.Matiere}|{t.Marque}|{(t.AMS ? "1" : "0")}|{t.NombreCouleurs}|{t.NombrePlateaux}|{t.NombreObjets}|{t.DateCreation:yyyy-MM-dd HH:mm:ss}");
                }
                File.WriteAllText(FichierTemplates, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                LogManager.Erreur("Sauvegarde templates devis", ex);
            }
        }
    }

    // ═══════════════════════════════════════════════════════
    // FORMULAIRE
    // ═══════════════════════════════════════════════════════

    public class TemplatesForm : Form
    {
        private DataGridView dgvTemplates;
        private Button btnCharger;
        private Button btnSupprimer;
        private Button btnFermer;
        private Panel panelBoutons;
        private List<TemplateDevis> templates;

        /// <summary>
        /// Le template sélectionné pour chargement (null si aucun)
        /// </summary>
        public TemplateDevis TemplateSelectionne { get; private set; }

        public TemplatesForm()
        {
            templates = TemplateManager.Charger();
            InitialiserComposants();
            AppliquerTheme();
            RemplirGrille();
        }

        private void InitialiserComposants()
        {
            this.Text = "Templates de devis";
            this.Size = new Size(650, 400);
            this.MinimumSize = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            dgvTemplates = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvTemplates.Columns.Add("Nom", "Nom");
            dgvTemplates.Columns.Add("Imprimante", "Imprimante");
            dgvTemplates.Columns.Add("Matiere", "Matière");
            dgvTemplates.Columns.Add("Config", "Configuration");
            dgvTemplates.Columns["Nom"].FillWeight = 30;
            dgvTemplates.Columns["Imprimante"].FillWeight = 25;
            dgvTemplates.Columns["Matiere"].FillWeight = 15;
            dgvTemplates.Columns["Config"].FillWeight = 30;
            dgvTemplates.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) ChargerTemplate(); };

            panelBoutons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                Padding = new Padding(10)
            };

            btnCharger = new Button
            {
                Text = "Charger",
                Size = new Size(120, 35),
                Location = new Point(10, 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCharger.Click += (s, e) => ChargerTemplate();

            btnSupprimer = new Button
            {
                Text = "Supprimer",
                Size = new Size(120, 35),
                Location = new Point(140, 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSupprimer.Click += BtnSupprimer_Click;

            btnFermer = new Button
            {
                Text = "Fermer",
                Size = new Size(120, 35),
                Location = new Point(270, 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnFermer.Click += (s, e) => this.Close();

            panelBoutons.Controls.AddRange(new Control[] { btnCharger, btnSupprimer, btnFermer });
            this.Controls.Add(dgvTemplates);
            this.Controls.Add(panelBoutons);
            this.AcceptButton = btnFermer;
        }

        private void AppliquerTheme()
        {
            ThemeManager.ApplyThemeToForm(this);
            ThemeManager.StyleDataGridView(dgvTemplates);
            ThemeManager.StyleButton(btnCharger, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);
            ThemeManager.StyleButton(btnSupprimer, ThemeManager.DangerRed, ThemeManager.DangerRedDark);
            ThemeManager.StyleButton(btnFermer, ThemeManager.NeutralGray, ThemeManager.NeutralGrayDark);
        }

        private void RemplirGrille()
        {
            dgvTemplates.Rows.Clear();
            foreach (var t in templates)
            {
                string config = t.AMS ? $"AMS, {t.NombreCouleurs} couleurs, {t.NombrePlateaux} plateaux" :
                    $"Mono, {t.NombrePlateaux} plateau(x)";
                dgvTemplates.Rows.Add(t.Nom, t.Imprimante, t.Matiere, config);
            }
        }

        private void ChargerTemplate()
        {
            if (dgvTemplates.CurrentRow == null) return;
            int index = dgvTemplates.CurrentRow.Index;
            if (index < 0 || index >= templates.Count) return;

            TemplateSelectionne = templates[index];
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnSupprimer_Click(object sender, EventArgs e)
        {
            if (dgvTemplates.CurrentRow == null) return;
            int index = dgvTemplates.CurrentRow.Index;
            if (index < 0 || index >= templates.Count) return;

            var t = templates[index];
            if (MessageBox.Show($"Supprimer le template \"{t.Nom}\" ?",
                "Confirmer", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                TemplateManager.Supprimer(t.Nom);
                templates.RemoveAt(index);
                RemplirGrille();
            }
        }
    }
}
