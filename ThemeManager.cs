using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public enum Theme { Clair, Sombre }

    public static class ThemeManager
    {
        // ═══════════════════════════════════════════════════════
        // THÈME ACTUEL
        // ═══════════════════════════════════════════════════════

        public static Theme ThemeActuel { get; private set; } = Theme.Clair;

        public static event Action ThemeChanged;

        public static void BasculerTheme()
        {
            ThemeActuel = (ThemeActuel == Theme.Clair) ? Theme.Sombre : Theme.Clair;
            ThemeChanged?.Invoke();
        }

        public static void DefinirTheme(Theme theme)
        {
            if (ThemeActuel == theme) return;
            ThemeActuel = theme;
            ThemeChanged?.Invoke();
        }

        private static bool EstSombre => ThemeActuel == Theme.Sombre;

        // ═══════════════════════════════════════════════════════
        // PALETTE DE COULEURS (adaptatives)
        // ═══════════════════════════════════════════════════════

        // Fonds
        public static Color BackgroundMain => EstSombre ? Color.FromArgb(30, 30, 30) : Color.FromArgb(245, 247, 250);
        public static Color BackgroundCard => EstSombre ? Color.FromArgb(45, 45, 45) : Color.White;
        public static Color BackgroundInput => EstSombre ? Color.FromArgb(55, 55, 55) : Color.White;

        // Couleurs d'accentuation (identiques dans les 2 thèmes)
        public static readonly Color PrimaryBlue = Color.FromArgb(52, 152, 219);
        public static readonly Color PrimaryBlueDark = Color.FromArgb(41, 128, 185);
        public static Color PrimaryBlueLight => EstSombre ? Color.FromArgb(30, 60, 90) : Color.FromArgb(232, 240, 254);
        public static readonly Color SecondaryGreen = Color.FromArgb(46, 204, 113);
        public static readonly Color SecondaryGreenDark = Color.FromArgb(39, 174, 96);
        public static readonly Color AccentOrange = Color.FromArgb(243, 156, 18);
        public static readonly Color AccentOrangeDark = Color.FromArgb(211, 132, 0);
        public static readonly Color DangerRed = Color.FromArgb(231, 76, 60);
        public static readonly Color DangerRedDark = Color.FromArgb(192, 57, 43);
        public static readonly Color NeutralGray = Color.FromArgb(149, 165, 166);
        public static readonly Color NeutralGrayDark = Color.FromArgb(127, 140, 141);

        // Textes
        public static Color TextPrimary => EstSombre ? Color.FromArgb(220, 220, 220) : Color.FromArgb(44, 62, 80);
        public static Color TextSecondary => EstSombre ? Color.FromArgb(160, 160, 160) : Color.FromArgb(127, 140, 141);
        public static readonly Color TextOnDark = Color.White;

        // Bordures
        public static Color BorderLight => EstSombre ? Color.FromArgb(65, 65, 65) : Color.FromArgb(220, 225, 230);

        // Grille
        public static Color GridColor => EstSombre ? Color.FromArgb(60, 60, 60) : Color.FromArgb(230, 230, 230);
        public static Color AlternateRowColor => EstSombre ? Color.FromArgb(38, 38, 38) : Color.FromArgb(248, 249, 250);
        public static Color TabInactiveColor => EstSombre ? Color.FromArgb(50, 50, 55) : Color.FromArgb(230, 233, 237);
        public static Color MenuBackground => EstSombre ? Color.FromArgb(40, 40, 40) : Color.White;

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
        // DICTIONNAIRE COULEURS FILAMENT
        // ═══════════════════════════════════════════════════════

        public static readonly System.Collections.Generic.Dictionary<string, Color> CouleursDictionnaire =
            new System.Collections.Generic.Dictionary<string, Color>
        {
            { "Rouge", Color.Red }, { "Bleu", Color.Blue }, { "Vert", Color.Green },
            { "Jaune", Color.Yellow }, { "Orange", Color.Orange }, { "Violet", Color.Purple },
            { "Rose", Color.Pink }, { "Noir", Color.Black }, { "Blanc", Color.White },
            { "Gris", Color.Gray }, { "Marron", Color.Brown }, { "Cyan", Color.Cyan },
            { "Magenta", Color.Magenta }, { "Beige", Color.Beige }, { "Turquoise", Color.Turquoise },
            { "Or", Color.Gold }, { "Argent", Color.Silver }, { "Bronze", Color.FromArgb(205, 127, 50) }
        };

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

            // Stocker la couleur hover dans le Tag
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
            txt.BackColor = BackgroundInput;
            txt.ForeColor = TextPrimary;
            txt.Font = FontBody;
        }

        public static void StyleComboBox(ComboBox cmb)
        {
            cmb.FlatStyle = FlatStyle.Flat;
            cmb.BackColor = BackgroundInput;
            cmb.ForeColor = TextPrimary;
            cmb.Font = FontBody;
        }

        public static void StyleNumericUpDown(NumericUpDown num)
        {
            num.BorderStyle = BorderStyle.FixedSingle;
            num.BackColor = BackgroundInput;
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
            dgv.GridColor = GridColor;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = PrimaryBlue;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = FontButton;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = PrimaryBlue;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersHeight = 36;

            dgv.DefaultCellStyle.BackColor = BackgroundCard;
            dgv.DefaultCellStyle.Font = FontBody;
            dgv.DefaultCellStyle.ForeColor = TextPrimary;
            dgv.DefaultCellStyle.SelectionBackColor = PrimaryBlueLight;
            dgv.DefaultCellStyle.SelectionForeColor = TextPrimary;
            dgv.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = AlternateRowColor;
            dgv.RowTemplate.Height = 30;

            dgv.RowHeadersVisible = false;
        }

        public static void StyleTabControl(TabControl tab)
        {
            tab.DrawMode = TabDrawMode.OwnerDrawFixed;
            tab.SizeMode = TabSizeMode.Fixed;

            // Calculer la largeur nécessaire pour l'onglet le plus long
            int largeurMax = 200;
            using (Graphics g = tab.CreateGraphics())
            {
                foreach (TabPage page in tab.TabPages)
                {
                    int largeur = (int)g.MeasureString(page.Text, FontButton).Width + 20;
                    if (largeur > largeurMax) largeurMax = largeur;
                }
            }
            tab.ItemSize = new Size(largeurMax, 36);

            // Supprimer ancien handler pour éviter doublons
            tab.DrawItem -= TabControl_DrawItem;
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
            Color bgColor = isSelected ? PrimaryBlue : TabInactiveColor;
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
            menu.BackColor = MenuBackground;
            menu.ForeColor = TextPrimary;
            menu.Font = FontBody;
            menu.Renderer = new FlatMenuRenderer();
        }

        public static void StyleGroupBox(GroupBox grp)
        {
            grp.ForeColor = TextPrimary;
            grp.BackColor = BackgroundMain;
        }

        public static void StylePanel(Panel pnl)
        {
            pnl.BackColor = BackgroundMain;
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
                Color color = e.Item.Selected ? PrimaryBlueLight : MenuBackground;

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
            public override Color MenuStripGradientBegin => MenuBackground;
            public override Color MenuStripGradientEnd => MenuBackground;
            public override Color MenuItemPressedGradientBegin => PrimaryBlue;
            public override Color MenuItemPressedGradientEnd => PrimaryBlue;
            public override Color ToolStripDropDownBackground => MenuBackground;
            public override Color ImageMarginGradientBegin => MenuBackground;
            public override Color ImageMarginGradientMiddle => MenuBackground;
            public override Color ImageMarginGradientEnd => MenuBackground;
            public override Color SeparatorDark => BorderLight;
            public override Color SeparatorLight => MenuBackground;
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
                else if (ctrl is GroupBox grp)
                    StyleGroupBox(grp);
                else if (ctrl is Panel pnl && !(ctrl is TabPage))
                    StylePanel(pnl);

                // Appliquer la couleur de fond et texte aux Labels
                if (ctrl is Label lbl && !(ctrl is LinkLabel))
                    lbl.ForeColor = TextPrimary;

                // Récursion dans les conteneurs
                if (ctrl.HasChildren)
                    StyleAllControls(ctrl);
            }
        }
    }
}
