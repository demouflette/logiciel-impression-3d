namespace logiciel_d_impression_3d
{
    partial class PromoForm
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
            this.lblMessage = new System.Windows.Forms.Label();
            this.panelCode = new System.Windows.Forms.Panel();
            this.lblJours = new System.Windows.Forms.Label();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.lblCodeLabel = new System.Windows.Forms.Label();
            this.btnValider = new System.Windows.Forms.Button();
            this.btnFermer = new System.Windows.Forms.Button();
            this.lblInfo = new System.Windows.Forms.Label();
            this.panelCode.SuspendLayout();
            this.SuspendLayout();

            // lblTitre
            this.lblTitre.AutoSize = false;
            this.lblTitre.Dock = System.Windows.Forms.DockStyle.None;
            this.lblTitre.Location = new System.Drawing.Point(24, 20);
            this.lblTitre.Size = new System.Drawing.Size(432, 36);
            this.lblTitre.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTitre.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitre.Text = "Promotion";

            // lblMessage
            this.lblMessage.AutoSize = false;
            this.lblMessage.Location = new System.Drawing.Point(24, 62);
            this.lblMessage.Size = new System.Drawing.Size(432, 80);
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.lblMessage.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblMessage.Text = "";

            // panelCode
            this.panelCode.Location = new System.Drawing.Point(24, 150);
            this.panelCode.Size = new System.Drawing.Size(432, 100);
            this.panelCode.Visible = false;
            this.panelCode.Controls.Add(this.lblJours);
            this.panelCode.Controls.Add(this.lblCodeLabel);
            this.panelCode.Controls.Add(this.txtCode);
            this.panelCode.Controls.Add(this.btnValider);

            // lblJours
            this.lblJours.AutoSize = false;
            this.lblJours.Location = new System.Drawing.Point(0, 0);
            this.lblJours.Size = new System.Drawing.Size(432, 28);
            this.lblJours.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblJours.Text = "";

            // lblCodeLabel
            this.lblCodeLabel.AutoSize = true;
            this.lblCodeLabel.Location = new System.Drawing.Point(0, 32);
            this.lblCodeLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCodeLabel.Text = "Code promotionnel :";

            // txtCode
            this.txtCode.Location = new System.Drawing.Point(0, 52);
            this.txtCode.Size = new System.Drawing.Size(280, 30);
            this.txtCode.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold);
            this.txtCode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtCode.MaxLength = 30;
            this.txtCode.TabIndex = 0;

            // btnValider
            this.btnValider.Location = new System.Drawing.Point(290, 50);
            this.btnValider.Size = new System.Drawing.Size(140, 34);
            this.btnValider.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnValider.Text = "Valider le code";
            this.btnValider.TabIndex = 1;
            this.btnValider.Click += new System.EventHandler(this.btnValider_Click);

            // lblInfo
            this.lblInfo.AutoSize = false;
            this.lblInfo.Location = new System.Drawing.Point(24, 258);
            this.lblInfo.Size = new System.Drawing.Size(432, 24);
            this.lblInfo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblInfo.Text = "";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // btnFermer
            this.btnFermer.Location = new System.Drawing.Point(340, 290);
            this.btnFermer.Size = new System.Drawing.Size(116, 34);
            this.btnFermer.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnFermer.Text = "Fermer";
            this.btnFermer.TabIndex = 2;
            this.btnFermer.Click += new System.EventHandler(this.btnFermer_Click);

            // PromoForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 340);
            this.Controls.Add(this.lblTitre);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.panelCode);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.btnFermer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Promotion exclusive";
            this.panelCode.ResumeLayout(false);
            this.panelCode.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblTitre;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Panel panelCode;
        private System.Windows.Forms.Label lblJours;
        private System.Windows.Forms.Label lblCodeLabel;
        private System.Windows.Forms.TextBox txtCode;
        private System.Windows.Forms.Button btnValider;
        private System.Windows.Forms.Button btnFermer;
        private System.Windows.Forms.Label lblInfo;
    }
}
