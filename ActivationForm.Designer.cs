namespace logiciel_d_impression_3d
{
    partial class ActivationForm
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
            this.lblObtenirCle = new System.Windows.Forms.Label();
            this.txtCle = new System.Windows.Forms.TextBox();
            this.btnActiver = new System.Windows.Forms.Button();
            this.btnEssai = new System.Windows.Forms.Button();
            this.lnkAcheter = new System.Windows.Forms.LinkLabel();
            this.lblInfo = new System.Windows.Forms.Label();
            this.lblCleLicence = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // ── Fenêtre ──────────────────────────────────────────────────
            this.Text = "Activation du logiciel";
            this.Size = new System.Drawing.Size(460, 340);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // ── lblTitre ─────────────────────────────────────────────────
            this.lblTitre.Text = "🖨️ Logiciel d'impression 3D";
            this.lblTitre.Location = new System.Drawing.Point(20, 20);
            this.lblTitre.Size = new System.Drawing.Size(400, 32);
            this.lblTitre.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitre.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ── lblSousTitre ─────────────────────────────────────────────
            this.lblSousTitre.Text = "Entrez votre clé de licence pour activer le logiciel.";
            this.lblSousTitre.Location = new System.Drawing.Point(20, 60);
            this.lblSousTitre.Size = new System.Drawing.Size(410, 40);
            this.lblSousTitre.Font = new System.Drawing.Font("Segoe UI", 9.5F);

            // ── lblCleLicence ─────────────────────────────────────────────
            this.lblCleLicence.Text = "Clé de licence";
            this.lblCleLicence.Location = new System.Drawing.Point(20, 115);
            this.lblCleLicence.Size = new System.Drawing.Size(120, 20);
            this.lblCleLicence.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // ── txtCle ────────────────────────────────────────────────────
            this.txtCle.Location = new System.Drawing.Point(20, 138);
            this.txtCle.Size = new System.Drawing.Size(280, 30);
            this.txtCle.Font = new System.Drawing.Font("Courier New", 13F, System.Drawing.FontStyle.Bold);
            this.txtCle.MaxLength = 19; // XXXX-XXXX-XXXX-XXXX
            this.txtCle.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtCle.TextChanged += new System.EventHandler(this.txtCle_TextChanged);

            // ── btnActiver ────────────────────────────────────────────────
            this.btnActiver.Text = "Activer";
            this.btnActiver.Location = new System.Drawing.Point(315, 135);
            this.btnActiver.Size = new System.Drawing.Size(110, 36);
            this.btnActiver.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnActiver.Enabled = false;
            this.btnActiver.Click += new System.EventHandler(this.btnActiver_Click);

            // ── lblInfo ───────────────────────────────────────────────────
            this.lblInfo.Text = "Format : XXXX-XXXX-XXXX-XXXX";
            this.lblInfo.Location = new System.Drawing.Point(20, 178);
            this.lblInfo.Size = new System.Drawing.Size(405, 20);
            this.lblInfo.Font = new System.Drawing.Font("Segoe UI", 8.5F);

            // ── lblObtenirCle ─────────────────────────────────────────────
            this.lblObtenirCle.Text = "Pas encore de clé ? Contactez l'administration pour en obtenir une.";
            this.lblObtenirCle.Location = new System.Drawing.Point(20, 205);
            this.lblObtenirCle.Size = new System.Drawing.Size(410, 18);
            this.lblObtenirCle.Font = new System.Drawing.Font("Segoe UI", 8.5F);

            // ── lnkAcheter ───────────────────────────────────────────────
            this.lnkAcheter.Text = "Contacter l'administration →";
            this.lnkAcheter.Location = new System.Drawing.Point(20, 225);
            this.lnkAcheter.Size = new System.Drawing.Size(240, 20);
            this.lnkAcheter.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lnkAcheter.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkAcheter_LinkClicked);

            // ── btnEssai ──────────────────────────────────────────────────
            this.btnEssai.Text = "Continuer l'essai";
            this.btnEssai.Location = new System.Drawing.Point(20, 260);
            this.btnEssai.Size = new System.Drawing.Size(200, 36);
            this.btnEssai.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnEssai.Visible = false;
            this.btnEssai.Click += new System.EventHandler(this.btnEssai_Click);

            // ── Ajout des contrôles ───────────────────────────────────────
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblTitre,
                this.lblSousTitre,
                this.lblCleLicence,
                this.txtCle,
                this.btnActiver,
                this.lblInfo,
                this.lblObtenirCle,
                this.lnkAcheter,
                this.btnEssai
            });

            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblTitre;
        private System.Windows.Forms.Label lblSousTitre;
        private System.Windows.Forms.Label lblCleLicence;
        private System.Windows.Forms.TextBox txtCle;
        private System.Windows.Forms.Button btnActiver;
        private System.Windows.Forms.Button btnEssai;
        private System.Windows.Forms.LinkLabel lnkAcheter;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Label lblObtenirCle;
    }
}
