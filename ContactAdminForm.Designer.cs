namespace logiciel_d_impression_3d
{
    partial class ContactAdminForm
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
            this.lblTitre   = new System.Windows.Forms.Label();
            this.lblInfo    = new System.Windows.Forms.Label();
            this.lblEmail   = new System.Windows.Forms.Label();
            this.txtEmail   = new System.Windows.Forms.TextBox();
            this.lblSujet   = new System.Windows.Forms.Label();
            this.txtSujet   = new System.Windows.Forms.TextBox();
            this.lblMessage = new System.Windows.Forms.Label();
            this.txtMessage = new System.Windows.Forms.RichTextBox();
            this.btnEnvoyer = new System.Windows.Forms.Button();
            this.btnAnnuler = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // lblTitre
            this.lblTitre.AutoSize  = false;
            this.lblTitre.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblTitre.Height    = 50;
            this.lblTitre.Text      = "Contacter l'administration";
            this.lblTitre.Font      = new System.Drawing.Font("Segoe UI", 13f, System.Drawing.FontStyle.Bold);
            this.lblTitre.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTitre.Padding   = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.lblTitre.Name      = "lblTitre";

            // lblInfo
            this.lblInfo.AutoSize  = false;
            this.lblInfo.Location  = new System.Drawing.Point(16, 58);
            this.lblInfo.Size      = new System.Drawing.Size(448, 36);
            this.lblInfo.Text      = "Votre message sera transmis à l'administration. Indiquez votre email pour recevoir une réponse.";
            this.lblInfo.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
            this.lblInfo.Name      = "lblInfo";

            // lblEmail
            this.lblEmail.AutoSize = true;
            this.lblEmail.Location = new System.Drawing.Point(16, 102);
            this.lblEmail.Text     = "Votre adresse email *";
            this.lblEmail.Font     = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.lblEmail.Name     = "lblEmail";

            // txtEmail
            this.txtEmail.Location = new System.Drawing.Point(16, 122);
            this.txtEmail.Size     = new System.Drawing.Size(448, 24);
            this.txtEmail.Font     = new System.Drawing.Font("Segoe UI", 9.5f);
            this.txtEmail.Name     = "txtEmail";

            // lblSujet
            this.lblSujet.AutoSize = true;
            this.lblSujet.Location = new System.Drawing.Point(16, 158);
            this.lblSujet.Text     = "Sujet *";
            this.lblSujet.Font     = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.lblSujet.Name     = "lblSujet";

            // txtSujet
            this.txtSujet.Location = new System.Drawing.Point(16, 178);
            this.txtSujet.Size     = new System.Drawing.Size(448, 24);
            this.txtSujet.Font     = new System.Drawing.Font("Segoe UI", 9.5f);
            this.txtSujet.Name     = "txtSujet";

            // lblMessage
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point(16, 214);
            this.lblMessage.Text     = "Message *";
            this.lblMessage.Font     = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.lblMessage.Name     = "lblMessage";

            // txtMessage
            this.txtMessage.Location    = new System.Drawing.Point(16, 234);
            this.txtMessage.Size        = new System.Drawing.Size(448, 140);
            this.txtMessage.Font        = new System.Drawing.Font("Segoe UI", 9.5f);
            this.txtMessage.ScrollBars  = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtMessage.AcceptsTab  = false;
            this.txtMessage.Name        = "txtMessage";

            // btnEnvoyer
            this.btnEnvoyer.Location = new System.Drawing.Point(270, 390);
            this.btnEnvoyer.Size     = new System.Drawing.Size(100, 34);
            this.btnEnvoyer.Text     = "Envoyer";
            this.btnEnvoyer.Font     = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            this.btnEnvoyer.Name     = "btnEnvoyer";
            this.btnEnvoyer.Click   += new System.EventHandler(this.btnEnvoyer_Click);

            // btnAnnuler
            this.btnAnnuler.Location = new System.Drawing.Point(380, 390);
            this.btnAnnuler.Size     = new System.Drawing.Size(84, 34);
            this.btnAnnuler.Text     = "Annuler";
            this.btnAnnuler.Font     = new System.Drawing.Font("Segoe UI", 9.5f);
            this.btnAnnuler.Name     = "btnAnnuler";
            this.btnAnnuler.Click   += new System.EventHandler(this.btnAnnuler_Click);

            // ContactAdminForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(480, 440);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox         = false;
            this.MinimizeBox         = false;
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text                = "Contacter l'administration";
            this.Name                = "ContactAdminForm";
            this.AcceptButton        = this.btnEnvoyer;
            this.CancelButton        = this.btnAnnuler;

            this.Controls.Add(this.lblTitre);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lblEmail);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.lblSujet);
            this.Controls.Add(this.txtSujet);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btnEnvoyer);
            this.Controls.Add(this.btnAnnuler);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label     lblTitre;
        private System.Windows.Forms.Label     lblInfo;
        private System.Windows.Forms.Label     lblEmail;
        private System.Windows.Forms.TextBox   txtEmail;
        private System.Windows.Forms.Label     lblSujet;
        private System.Windows.Forms.TextBox   txtSujet;
        private System.Windows.Forms.Label     lblMessage;
        private System.Windows.Forms.RichTextBox txtMessage;
        private System.Windows.Forms.Button    btnEnvoyer;
        private System.Windows.Forms.Button    btnAnnuler;
    }
}
