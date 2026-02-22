using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public partial class ScratchCardForm : Form
    {
        private readonly string _cle;
        private Bitmap _overlayBitmap;
        private bool _enGrattage;
        private bool _revele;
        private int _fadeAlpha = 255;
        private Timer _timerFade;

        public ScratchCardForm(string cle)
        {
            InitializeComponent();
            _cle = cle;
            this.DoubleBuffered = true;
            AppliquerTheme();
            InitOverlay();
        }

        // ── Thème ─────────────────────────────────────────────────────────────
        private void AppliquerTheme()
        {
            ThemeManager.ApplyThemeToForm(this);
            ThemeManager.StyleAllControls(this);

            lblTitre.ForeColor = ThemeManager.PrimaryBlue;
            lblTitre.Font = ThemeManager.FontTitle;
            lblSousTitre.ForeColor = ThemeManager.TextSecondary;
            lblSousTitre.Font = ThemeManager.FontBody;
            lblProgression.ForeColor = ThemeManager.TextSecondary;
            lblProgression.Font = ThemeManager.FontSmall;

            ThemeManager.StyleButton(btnCopier, ThemeManager.SecondaryGreen, ThemeManager.SecondaryGreenDark);
            ThemeManager.StyleButton(btnFermer, ThemeManager.NeutralGray, ThemeManager.NeutralGrayDark);
        }

        // ── Initialisation de l'overlay gris ─────────────────────────────────
        private void InitOverlay()
        {
            int w = picScratch.Width;
            int h = picScratch.Height;
            _overlayBitmap = new Bitmap(w, h, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(_overlayBitmap))
            {
                // Dégradé gris argent
                using (var grad = new LinearGradientBrush(
                    new Rectangle(0, 0, w, h),
                    Color.FromArgb(255, 155, 155, 168),
                    Color.FromArgb(255, 125, 125, 140),
                    LinearGradientMode.ForwardDiagonal))
                {
                    g.FillRectangle(grad, 0, 0, w, h);
                }

                // Texture de points discrets
                using (var pen = new Pen(Color.FromArgb(25, 255, 255, 255), 1))
                {
                    for (int x = 0; x < w; x += 6)
                        for (int y = 0; y < h; y += 6)
                            g.DrawRectangle(pen, x, y, 1, 1);
                }

                // Texte indicateur centré
                using (var font = new Font("Segoe UI", 15F, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(100, 255, 255, 255)))
                {
                    string texte = "✦   GRATTEZ ICI   ✦";
                    SizeF taille = g.MeasureString(texte, font);
                    g.DrawString(texte, font, brush,
                        (w - taille.Width) / 2f,
                        (h - taille.Height) / 2f);
                }
            }
        }

        // ── Rendu GDI+ ────────────────────────────────────────────────────────
        private void picScratch_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = picScratch.Width;
            int h = picScratch.Height;

            // 1. Fond blanc avec la clé
            using (var brush = new SolidBrush(Color.White))
                g.FillRectangle(brush, 0, 0, w, h);

            using (var font = new Font("Courier New", 22F, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(26, 26, 46)))
            {
                SizeF taille = g.MeasureString(_cle, font);
                g.DrawString(_cle, font, brush,
                    (w - taille.Width) / 2f,
                    (h - taille.Height) / 2f - 10f);
            }

            using (var font = new Font("Segoe UI", 9F))
            using (var brush = new SolidBrush(Color.FromArgb(130, 100, 100, 110)))
            {
                string hint = "Cliquez sur la clé pour la copier";
                SizeF taille = g.MeasureString(hint, font);
                g.DrawString(hint, font, brush,
                    (w - taille.Width) / 2f,
                    h / 2f + 24f);
            }

            // 2. Overlay gris avec transparence progressive
            if (_overlayBitmap != null && _fadeAlpha > 0)
            {
                var rect = new Rectangle(0, 0, w, h);
                var cm = new ColorMatrix { Matrix33 = _fadeAlpha / 255f };
                using (var ia = new ImageAttributes())
                {
                    ia.SetColorMatrix(cm);
                    g.DrawImage(_overlayBitmap, rect, 0, 0, w, h, GraphicsUnit.Pixel, ia);
                }
            }
        }

        // ── Événements souris ─────────────────────────────────────────────────
        private void picScratch_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !_revele)
                _enGrattage = true;
        }

        private void picScratch_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_enGrattage || _revele) return;
            Gratter(e.X, e.Y);
        }

        private void picScratch_MouseUp(object sender, MouseEventArgs e)
        {
            _enGrattage = false;
        }

        private void picScratch_MouseLeave(object sender, EventArgs e)
        {
            _enGrattage = false;
        }

        private void picScratch_Click(object sender, EventArgs e)
        {
            if (_revele) CopierCle();
        }

        // ── Logique de grattage ───────────────────────────────────────────────
        private void Gratter(int x, int y)
        {
            using (Graphics g = Graphics.FromImage(_overlayBitmap))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(Color.FromArgb(0, 0, 0, 0)))
                    g.FillEllipse(b, x - 32, y - 32, 64, 64);
            }
            picScratch.Invalidate();
            CalculerProgression();
        }

        private void CalculerProgression()
        {
            int w = _overlayBitmap.Width;
            int h = _overlayBitmap.Height;
            int transparent = 0, total = 0;

            // Sampling tous les 4px pour la performance
            for (int y = 0; y < h; y += 4)
            {
                for (int x = 0; x < w; x += 4)
                {
                    if (_overlayBitmap.GetPixel(x, y).A < 128)
                        transparent++;
                    total++;
                }
            }

            if (total > 0 && (transparent * 100 / total) >= 60 && !_revele)
                RevelerCle();
        }

        // ── Révélation avec fondu ─────────────────────────────────────────────
        private void RevelerCle()
        {
            _revele = true;
            picScratch.Cursor = Cursors.Hand;
            lblProgression.Visible = false;

            _timerFade = new Timer { Interval = 16 };
            _timerFade.Tick += (s, e) =>
            {
                _fadeAlpha = Math.Max(0, _fadeAlpha - 20);
                picScratch.Invalidate();
                if (_fadeAlpha == 0)
                {
                    _timerFade.Stop();
                    _timerFade.Dispose();
                    _timerFade = null;
                    btnCopier.Visible = true;
                }
            };
            _timerFade.Start();
        }

        // ── Copier dans le presse-papiers ─────────────────────────────────────
        private void CopierCle()
        {
            try
            {
                Clipboard.SetText(_cle);
                string original = btnCopier.Text;
                btnCopier.Text = "✓  Clé copiée !";
                var t = new Timer { Interval = 2000 };
                t.Tick += (s, e) =>
                {
                    btnCopier.Text = original;
                    t.Stop();
                    t.Dispose();
                };
                t.Start();
            }
            catch { }
        }

        private void btnCopier_Click(object sender, EventArgs e) => CopierCle();

        private void btnFermer_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // ── Nettoyage ─────────────────────────────────────────────────────────
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timerFade?.Stop();
            _timerFade?.Dispose();
            _overlayBitmap?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
