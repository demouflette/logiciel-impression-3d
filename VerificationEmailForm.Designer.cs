namespace logiciel_d_impression_3d
{
    partial class VerificationEmailForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitre       = new System.Windows.Forms.Label();
            this.lblEmailInfo   = new System.Windows.Forms.Label();
            this.lblCodeLabel   = new System.Windows.Forms.Label();
            this.txtCode        = new System.Windows.Forms.TextBox();
            this.btnVerifier    = new System.Windows.Forms.Button();
            this.btnRenvoyer    = new System.Windows.Forms.Button();
            this.btnPlusTard    = new System.Windows.Forms.Button();
            this.lnkVerifierWeb = new System.Windows.Forms.LinkLabel();
            this.lblInfo        = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // ── Fenêtre ──────────────────────────────────────────────────
            this.Text = "Vérification de l'adresse email";
            this.Size = new System.Drawing.Size(420, 310);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // ── lblTitre ─────────────────────────────────────────────────
            this.lblTitre.Text = "Vérifiez votre email";
            this.lblTitre.Location = new System.Drawing.Point(20, 20);
            this.lblTitre.Size = new System.Drawing.Size(360, 30);
            this.lblTitre.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);

            // ── lblEmailInfo ─────────────────────────────────────────────
            this.lblEmailInfo.Text = "Un code a été envoyé à votre adresse email.";
            this.lblEmailInfo.Location = new System.Drawing.Point(20, 56);
            this.lblEmailInfo.Size = new System.Drawing.Size(370, 36);
            this.lblEmailInfo.Font = new System.Drawing.Font("Segoe UI", 9F);

            // ── lblCodeLabel ─────────────────────────────────────────────
            this.lblCodeLabel.Text = "Code de vérification";
            this.lblCodeLabel.Location = new System.Drawing.Point(20, 105);
            this.lblCodeLabel.Size = new System.Drawing.Size(160, 20);
            this.lblCodeLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // ── txtCode ───────────────────────────────────────────────────
            this.txtCode.Location = new System.Drawing.Point(20, 128);
            this.txtCode.Size = new System.Drawing.Size(160, 30);
            this.txtCode.Font = new System.Drawing.Font("Courier New", 16F, System.Drawing.FontStyle.Bold);
            this.txtCode.MaxLength = 7; // 123-456
            this.txtCode.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtCode.TextChanged += new System.EventHandler(this.txtCode_TextChanged);

            // ── btnVerifier ───────────────────────────────────────────────
            this.btnVerifier.Text = "Vérifier";
            this.btnVerifier.Location = new System.Drawing.Point(195, 126);
            this.btnVerifier.Size = new System.Drawing.Size(90, 34);
            this.btnVerifier.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnVerifier.Enabled = false;
            this.btnVerifier.Click += new System.EventHandler(this.btnVerifier_Click);

            // ── lblInfo ───────────────────────────────────────────────────
            this.lblInfo.Location = new System.Drawing.Point(20, 168);
            this.lblInfo.Size = new System.Drawing.Size(370, 20);
            this.lblInfo.Font = new System.Drawing.Font("Segoe UI", 8.5F);

            // ── lnkVerifierWeb ───────────────────────────────────────────
            this.lnkVerifierWeb.Text = "J'ai cliqué sur le lien dans l'email →";
            this.lnkVerifierWeb.Location = new System.Drawing.Point(20, 196);
            this.lnkVerifierWeb.Size = new System.Drawing.Size(280, 20);
            this.lnkVerifierWeb.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lnkVerifierWeb.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkVerifierWeb_LinkClicked);

            // ── btnRenvoyer ───────────────────────────────────────────────
            this.btnRenvoyer.Text = "Renvoyer le code";
            this.btnRenvoyer.Location = new System.Drawing.Point(20, 228);
            this.btnRenvoyer.Size = new System.Drawing.Size(150, 32);
            this.btnRenvoyer.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnRenvoyer.Click += new System.EventHandler(this.btnRenvoyer_Click);

            // ── btnPlusTard ───────────────────────────────────────────────
            this.btnPlusTard.Text = "Plus tard";
            this.btnPlusTard.Location = new System.Drawing.Point(295, 228);
            this.btnPlusTard.Size = new System.Drawing.Size(90, 32);
            this.btnPlusTard.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnPlusTard.Click += new System.EventHandler(this.btnPlusTard_Click);

            // ── Ajout contrôles ───────────────────────────────────────────
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblTitre, this.lblEmailInfo, this.lblCodeLabel,
                this.txtCode, this.btnVerifier, this.lblInfo,
                this.lnkVerifierWeb, this.btnRenvoyer, this.btnPlusTard
            });

            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblTitre;
        private System.Windows.Forms.Label lblEmailInfo;
        private System.Windows.Forms.Label lblCodeLabel;
        private System.Windows.Forms.TextBox txtCode;
        private System.Windows.Forms.Button btnVerifier;
        private System.Windows.Forms.Button btnRenvoyer;
        private System.Windows.Forms.Button btnPlusTard;
        private System.Windows.Forms.LinkLabel lnkVerifierWeb;
        private System.Windows.Forms.Label lblInfo;
    }
}
