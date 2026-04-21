using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public class AmsConfigForm : Form
    {
        private readonly List<Bobine> bobines;
        private readonly List<ComboBox> combos = new List<ComboBox>();

        public List<AmsSlot> Slots { get; private set; } = new List<AmsSlot>();

        public AmsConfigForm(int nombreSlots, List<AmsSlot> slotsActuels, List<Bobine> bobinesCatalogue)
        {
            bobines = bobinesCatalogue ?? new List<Bobine>();
            InitialiserForm(nombreSlots, slotsActuels);
            ThemeManager.ApplyThemeToForm(this);
            ThemeManager.StyleAllControls(this);
        }

        private void InitialiserForm(int nombreSlots, List<AmsSlot> slotsActuels)
        {
            int rowH = 40;
            int headerH = 45;
            int footerH = 80;
            int formH = headerH + nombreSlots * rowH + footerH + 20;

            this.Text = "Sélectionner les bobines AMS";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(480, formH);

            var lblTitre = new Label
            {
                Text = "Sélectionnez une bobine pour chaque slot AMS :",
                Left = 15, Top = 12,
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            this.Controls.Add(lblTitre);

            // Construire la liste d'affichage des bobines
            var items = bobines
                .Select(b => new BobineItem(b))
                .ToList();

            if (items.Count == 0)
            {
                // Aucune bobine dans le catalogue — afficher un message
                var lblVide = new Label
                {
                    Text = "Aucune bobine dans le catalogue.\nAjoutez des bobines dans Paramètres > Catalogue bobines.",
                    Left = 15, Top = headerH,
                    Width = 440, Height = 50,
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.OrangeRed
                };
                this.Controls.Add(lblVide);
            }

            for (int i = 0; i < nombreSlots; i++)
            {
                int y = headerH + i * rowH;

                var lblSlot = new Label
                {
                    Text = $"AMS {i + 1} :",
                    Left = 15, Top = y + 10,
                    Width = 60,
                    Font = new Font("Segoe UI", 9F)
                };

                var cb = new ComboBox
                {
                    Left = 80, Top = y + 7,
                    Width = 370, Height = 26,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 9F)
                };

                foreach (var item in items)
                    cb.Items.Add(item);

                // Pré-sélectionner la bobine actuelle si possible
                if (slotsActuels != null && i < slotsActuels.Count)
                {
                    var slotActuel = slotsActuels[i];
                    var match = items.FirstOrDefault(it =>
                        it.Bobine.Couleur == slotActuel.Couleur &&
                        it.Bobine.Matiere == slotActuel.Matiere);
                    if (match != null)
                        cb.SelectedItem = match;
                }

                if (cb.SelectedIndex < 0 && cb.Items.Count > 0)
                    cb.SelectedIndex = 0;

                this.Controls.AddRange(new Control[] { lblSlot, cb });
                combos.Add(cb);
            }

            // Boutons
            int yBtn = headerH + nombreSlots * rowH + 10;
            var btnOk = new Button
            {
                Text = "OK",
                Left = 265, Top = yBtn,
                Width = 90, Height = 30,
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat
            };
            var btnAnnuler = new Button
            {
                Text = "Annuler",
                Left = 365, Top = yBtn,
                Width = 90, Height = 30,
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat
            };

            btnOk.Click += (s, e) =>
            {
                Slots = combos.Select(cb =>
                {
                    if (cb.SelectedItem is BobineItem bi)
                        return new AmsSlot
                        {
                            Couleur = bi.Bobine.Couleur,
                            Matiere = bi.Bobine.Matiere,
                            PrixKg = bi.Bobine.PrixParKg
                        };
                    return new AmsSlot();
                }).ToList();
            };

            this.Controls.AddRange(new Control[] { btnOk, btnAnnuler });
            this.AcceptButton = btnOk;
            this.CancelButton = btnAnnuler;

            ThemeManager.StyleButton(btnOk, ThemeManager.PrimaryBlue, ThemeManager.PrimaryBlueDark);
            ThemeManager.StyleButton(btnAnnuler, ThemeManager.NeutralGray, ThemeManager.NeutralGrayDark);
        }

        // Classe interne pour afficher les bobines dans le ComboBox
        private class BobineItem
        {
            public Bobine Bobine { get; }
            public BobineItem(Bobine b) { Bobine = b; }
            public override string ToString()
            {
                return $"{Bobine.Couleur} — {Bobine.Matiere} — {Bobine.Marque}  ({Bobine.PrixParKg:F2} €/kg)";
            }
        }
    }
}
