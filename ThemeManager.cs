using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public static class ThemeManager
    {
        // ═══════════════════════════════════════════════════════
        // PALETTE DE COULEURS
        // ═══════════════════════════════════════════════════════

        // Fonds
        public static readonly Color BackgroundMain = Color.FromArgb(245, 247, 250);
        public static readonly Color BackgroundCard = Color.White;

        // Couleurs d'accentuation
        public static readonly Color PrimaryBlue = Color.FromArgb(52, 152, 219);
        public static readonly Color PrimaryBlueDark = Color.FromArgb(41, 128, 185);
        public static readonly Color PrimaryBlueLight = Color.FromArgb(232, 240, 254);
        public static readonly Color SecondaryGreen = Color.FromArgb(46, 204, 113);
        public static readonly Color SecondaryGreenDark = Color.FromArgb(39, 174, 96);
        public static readonly Color AccentOrange = Color.FromArgb(243, 156, 18);
        public static readonly Color AccentOrangeDark = Color.FromArgb(211, 132, 0);
        public static readonly Color DangerRed = Color.FromArgb(231, 76, 60);
        public static readonly Color DangerRedDark = Color.FromArgb(192, 57, 43);
        public static readonly Color NeutralGray = Color.FromArgb(149, 165, 166);
        public static readonly Color NeutralGrayDark = Color.FromArgb(127, 140, 141);

        // Textes
        public static readonly Color TextPrimary = Color.FromArgb(44, 62, 80);
        public static readonly Color TextSecondary = Color.FromArgb(127, 140, 141);
        public static readonly Color TextOnDark = Color.White;

        // Bordures
        public static readonly Color BorderLight = Color.FromArgb(220, 225, 230);

        // ═══════════════════════════════════════════════════════
        // POLICES
        // ═══════════════════════════════════════════════════════

        public static readonly Font FontTitle = new Font("Segoe UI", 18F, FontStyle.Bold);
        public static readonly Font FontSubtitle = new Font("Segoe UI", 13F, FontStyle.Bold);
        public static readonly Font FontCardTitle = new Font("Segoe UI", 11F, FontStyle.Bold);
        public static readonly Font FontBody = new Font("Segoe UI", 10F);
        public static readonly Font FontSmall = new Font("Segoe UI", 9F);
        public static readonly Font FontButton = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font FontButtonLarge = new Font("Segoe UI", 12F, FontStyle.Bold);

        // ═══════════════════════════════════════════════════════
        // CONSTANTES
        // ═══════════════════════════════════════════════════════

        public const int CardCornerRadius = 12;
        public const int CardPadding = 16;

        // ═══════════════════════════════════════════════════════
        // MÉTHODES DE STYLING
        // ═══════════════════════════════════════════════════════

        public static void ApplyThemeToForm(Form form)
        {
            form.BackColor = BackgroundMain;
            form.ForeColor = TextPrimary;
            form.Font = FontBody;

            // Activer le double buffering via réflexion
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(form, true, null);

            // Styler la MenuStrip si présente
            foreach (Control ctrl in form.Controls)
            {
                if (ctrl is MenuStrip menu)
                {
                    StyleMenuStrip(menu);
                }
            }
        }

        public static void StyleButton(Button btn, Color bgColor, Color hoverColor)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = bgColor;
            btn.ForeColor = TextOnDark;
            btn.Font = FontButton;
            btn.Cursor = Cursors.Hand;

            // Supprimer les anciens handlers pour éviter les doublons
            btn.MouseEnter -= Button_MouseEnter;
            btn.MouseLeave -= Button_MouseLeave;

            // Stocker la couleur hover dans le Tag (ou utiliser une approche par dictionnaire)
            btn.Tag = new ButtonColors { Normal = bgColor, Hover = hoverColor };
            btn.MouseEnter += Button_MouseEnter;
            btn.MouseLeave += Button_MouseLeave;
        }

        public static void StyleButtonLarge(Button btn, Color bgColor, Color hoverColor)
        {
            StyleButton(btn, bgColor, hoverColor);
            btn.Font = FontButtonLarge;
        }

        private static void Button_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is ButtonColors colors)
            {
                btn.BackColor = colors.Hover;
            }
        }

        private static void Button_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is ButtonColors colors)
            {
                btn.BackColor = colors.Normal;
            }
        }

        public static void StyleTextBox(TextBox txt)
        {
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.BackColor = Color.White;
            txt.ForeColor = TextPrimary;
            txt.Font = FontBody;
        }

        public static void StyleComboBox(ComboBox cmb)
        {
            cmb.FlatStyle = FlatStyle.Flat;
            cmb.BackColor = Color.White;
            cmb.ForeColor = TextPrimary;
            cmb.Font = FontBody;
        }

        public static void StyleNumericUpDown(NumericUpDown num)
        {
            num.BorderStyle = BorderStyle.FixedSingle;
            num.BackColor = Color.White;
            num.ForeColor = TextPrimary;
            num.Font = FontBody;
        }

        public static void StyleLabel(Label lbl, bool isTitle = false)
        {
            lbl.ForeColor = isTitle ? PrimaryBlue : TextPrimary;
            lbl.Font = isTitle ? FontSubtitle : FontBody;
        }

        public static void StyleCheckBox(CheckBox chk)
        {
            chk.ForeColor = TextPrimary;
            chk.Font = FontBody;
        }

        public static void StyleRadioButton(RadioButton rdo)
        {
            rdo.ForeColor = TextPrimary;
            rdo.Font = FontBody;
        }

        public static void StyleLinkLabel(LinkLabel lnk)
        {
            lnk.LinkColor = PrimaryBlue;
            lnk.ActiveLinkColor = PrimaryBlueDark;
            lnk.VisitedLinkColor = PrimaryBlue;
            lnk.Font = FontSmall;
        }

        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.BorderStyle = BorderStyle.None;
            dgv.BackgroundColor = BackgroundCard;
            dgv.GridColor = Color.FromArgb(230, 230, 230);
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = PrimaryBlue;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = FontButton;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = PrimaryBlue;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersHeight = 36;

            dgv.DefaultCellStyle.Font = FontBody;
            dgv.DefaultCellStyle.ForeColor = TextPrimary;
            dgv.DefaultCellStyle.SelectionBackColor = PrimaryBlueLight;
            dgv.DefaultCellStyle.SelectionForeColor = TextPrimary;
            dgv.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
            dgv.RowTemplate.Height = 30;

            dgv.RowHeadersVisible = false;
        }

        public static void StyleTabControl(TabControl tab)
        {
            tab.DrawMode = TabDrawMode.OwnerDrawFixed;
            tab.SizeMode = TabSizeMode.Fixed;
            tab.ItemSize = new Size(200, 36);
            tab.DrawItem += TabControl_DrawItem;

            foreach (TabPage page in tab.TabPages)
            {
                page.BackColor = BackgroundMain;
            }
        }

        private static void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tab = (TabControl)sender;
            TabPage page = tab.TabPages[e.Index];
            bool isSelected = (tab.SelectedIndex == e.Index);

            Rectangle bounds = tab.GetTabRect(e.Index);

            // Fond
            Color bgColor = isSelected ? PrimaryBlue : Color.FromArgb(230, 233, 237);
            using (SolidBrush brush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(brush, bounds);
            }

            // Texte
            Color textColor = isSelected ? TextOnDark : TextPrimary;
            TextRenderer.DrawText(e.Graphics, page.Text, FontButton, bounds, textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        public static void StyleMenuStrip(MenuStrip menu)
        {
            menu.BackColor = Color.White;
            menu.ForeColor = TextPrimary;
            menu.Font = FontBody;
            menu.Renderer = new FlatMenuRenderer();
        }

        // ═══════════════════════════════════════════════════════
        // MENU RENDERER
        // ═══════════════════════════════════════════════════════

        private class FlatMenuRenderer : ToolStripProfessionalRenderer
        {
            public FlatMenuRenderer() : base(new FlatColorTable()) { }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);
                Color color = e.Item.Selected ? PrimaryBlueLight : Color.White;

                using (SolidBrush brush = new SolidBrush(color))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = TextPrimary;
                base.OnRenderItemText(e);
            }
        }

        private class FlatColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected => PrimaryBlueLight;
            public override Color MenuItemSelectedGradientBegin => PrimaryBlueLight;
            public override Color MenuItemSelectedGradientEnd => PrimaryBlueLight;
            public override Color MenuItemBorder => Color.Transparent;
            public override Color MenuStripGradientBegin => Color.White;
            public override Color MenuStripGradientEnd => Color.White;
            public override Color MenuItemPressedGradientBegin => PrimaryBlue;
            public override Color MenuItemPressedGradientEnd => PrimaryBlue;
            public override Color ToolStripDropDownBackground => Color.White;
            public override Color ImageMarginGradientBegin => Color.White;
            public override Color ImageMarginGradientMiddle => Color.White;
            public override Color ImageMarginGradientEnd => Color.White;
            public override Color SeparatorDark => BorderLight;
            public override Color SeparatorLight => Color.White;
        }

        // ═══════════════════════════════════════════════════════
        // HELPER CLASSES
        // ═══════════════════════════════════════════════════════

        private class ButtonColors
        {
            public Color Normal { get; set; }
            public Color Hover { get; set; }
        }

        /// <summary>
        /// Applique le thème récursivement à tous les contrôles enfants d'un conteneur
        /// </summary>
        public static void StyleAllControls(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Button btn)
                {
                    // Ne pas re-styler les boutons déjà stylés manuellement
                    if (btn.Tag is ButtonColors) continue;
                    StyleButton(btn, PrimaryBlue, PrimaryBlueDark);
                }
                else if (ctrl is TextBox txt)
                    StyleTextBox(txt);
                else if (ctrl is ComboBox cmb)
                    StyleComboBox(cmb);
                else if (ctrl is NumericUpDown num)
                    StyleNumericUpDown(num);
                else if (ctrl is CheckBox chk)
                    StyleCheckBox(chk);
                else if (ctrl is RadioButton rdo)
                    StyleRadioButton(rdo);
                else if (ctrl is DataGridView dgv)
                    StyleDataGridView(dgv);
                else if (ctrl is LinkLabel lnk)
                    StyleLinkLabel(lnk);
                else if (ctrl is TabControl tab)
                    StyleTabControl(tab);

                // Récursion dans les conteneurs
                if (ctrl.HasChildren)
                    StyleAllControls(ctrl);
            }
        }
    }
}
