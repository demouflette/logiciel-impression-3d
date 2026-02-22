namespace logiciel_d_impression_3d
{
    partial class ScratchCardForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitre = new System.Windows.Forms.Label();
            this.lblSousTitre = new System.Windows.Forms.Label();
            this.picScratch = new System.Windows.Forms.PictureBox();
            this.lblProgression = new System.Windows.Forms.Label();
            this.btnCopier = new System.Windows.Forms.Button();
            this.btnFermer = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picScratch)).BeginInit();
            this.SuspendLayout();

            // lblTitre
            this.lblTitre.Location = new System.Drawing.Point(20, 18);
            this.lblTitre.Size = new System.Drawing.Size(460, 42);
            this.lblTitre.Text = "Félicitations !";
            this.lblTitre.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // lblSousTitre
            this.lblSousTitre.Location = new System.Drawing.Point(20, 62);
            this.lblSousTitre.Size = new System.Drawing.Size(460, 26);
            this.lblSousTitre.Text = "Grattez la zone grise pour révéler votre clé de licence";
            this.lblSousTitre.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // picScratch
            this.picScratch.Location = new System.Drawing.Point(30, 98);
            this.picScratch.Size = new System.Drawing.Size(440, 160);
            this.picScratch.TabStop = false;
            this.picScratch.Cursor = System.Windows.Forms.Cursors.Cross;
            this.picScratch.BackColor = System.Drawing.Color.White;
            this.picScratch.Paint += new System.Windows.Forms.PaintEventHandler(this.picScratch_Paint);
            this.picScratch.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picScratch_MouseDown);
            this.picScratch.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picScratch_MouseMove);
            this.picScratch.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picScratch_MouseUp);
            this.picScratch.MouseLeave += new System.EventHandler(this.picScratch_MouseLeave);
            this.picScratch.Click += new System.EventHandler(this.picScratch_Click);

            // lblProgression
            this.lblProgression.Location = new System.Drawing.Point(20, 266);
            this.lblProgression.Size = new System.Drawing.Size(460, 24);
            this.lblProgression.Text = "Grattez la zone grise ✦";
            this.lblProgression.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // btnCopier
            this.btnCopier.Location = new System.Drawing.Point(30, 298);
            this.btnCopier.Size = new System.Drawing.Size(440, 42);
            this.btnCopier.Text = "📋  Copier la clé";
            this.btnCopier.Visible = false;
            this.btnCopier.Click += new System.EventHandler(this.btnCopier_Click);

            // btnFermer
            this.btnFermer.Location = new System.Drawing.Point(160, 352);
            this.btnFermer.Size = new System.Drawing.Size(180, 42);
            this.btnFermer.Text = "Fermer";
            this.btnFermer.Click += new System.EventHandler(this.btnFermer_Click);

            // ScratchCardForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 410);
            this.Controls.Add(this.lblTitre);
            this.Controls.Add(this.lblSousTitre);
            this.Controls.Add(this.picScratch);
            this.Controls.Add(this.lblProgression);
            this.Controls.Add(this.btnCopier);
            this.Controls.Add(this.btnFermer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Votre clé de licence";
            this.CancelButton = this.btnFermer;

            ((System.ComponentModel.ISupportInitialize)(this.picScratch)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblTitre;
        private System.Windows.Forms.Label lblSousTitre;
        private System.Windows.Forms.PictureBox picScratch;
        private System.Windows.Forms.Label lblProgression;
        private System.Windows.Forms.Button btnCopier;
        private System.Windows.Forms.Button btnFermer;
    }
}
