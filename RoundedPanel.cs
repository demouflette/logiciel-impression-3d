using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public class RoundedPanel : Panel
    {
        public int CornerRadius { get; set; } = 12;
        public Color CardBackColor { get; set; } = Color.White;
        public bool ShowShadow { get; set; } = true;
        public int ShadowDepth { get; set; } = 3;
        public string Title { get; set; } = "";
        public Color TitleColor { get; set; } = ThemeManager.PrimaryBlue;

        public RoundedPanel()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Padding = new Padding(ThemeManager.CardPadding,
                string.IsNullOrEmpty(Title) ? ThemeManager.CardPadding : ThemeManager.CardPadding + 25,
                ThemeManager.CardPadding,
                ThemeManager.CardPadding);
        }

        private GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int shadowOffset = ShowShadow ? ShadowDepth : 0;

            // Zone de la carte (réduite pour laisser place à l'ombre)
            Rectangle cardRect = new Rectangle(
                0, 0,
                Width - shadowOffset - 1,
                Height - shadowOffset - 1);

            // Dessiner l'ombre (2 couches pour un effet plus doux)
            if (ShowShadow)
            {
                Rectangle shadowRect1 = cardRect;
                shadowRect1.Offset(ShadowDepth, ShadowDepth);
                using (GraphicsPath shadowPath1 = CreateRoundedRectPath(shadowRect1, CornerRadius))
                using (SolidBrush shadowBrush1 = new SolidBrush(Color.FromArgb(15, 0, 0, 0)))
                {
                    e.Graphics.FillPath(shadowBrush1, shadowPath1);
                }

                Rectangle shadowRect2 = cardRect;
                shadowRect2.Offset(ShadowDepth / 2 + 1, ShadowDepth / 2 + 1);
                using (GraphicsPath shadowPath2 = CreateRoundedRectPath(shadowRect2, CornerRadius))
                using (SolidBrush shadowBrush2 = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
                {
                    e.Graphics.FillPath(shadowBrush2, shadowPath2);
                }
            }

            // Dessiner le fond de la carte
            using (GraphicsPath cardPath = CreateRoundedRectPath(cardRect, CornerRadius))
            using (SolidBrush cardBrush = new SolidBrush(CardBackColor))
            {
                e.Graphics.FillPath(cardBrush, cardPath);
            }

            // Dessiner le titre si présent
            if (!string.IsNullOrEmpty(Title))
            {
                using (SolidBrush titleBrush = new SolidBrush(TitleColor))
                {
                    e.Graphics.DrawString(Title, ThemeManager.FontCardTitle, titleBrush,
                        ThemeManager.CardPadding, ThemeManager.CardPadding - 2);
                }
            }
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            Invalidate();
        }
    }
}
