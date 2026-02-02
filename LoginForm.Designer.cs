namespace logiciel_d_impression_3d
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageLogin = new System.Windows.Forms.TabPage();
            this.lnkForgotPassword = new System.Windows.Forms.LinkLabel();
            this.btnLogin = new System.Windows.Forms.Button();
            this.txtLoginPassword = new System.Windows.Forms.TextBox();
            this.txtLoginUsername = new System.Windows.Forms.TextBox();
            this.lblLoginPassword = new System.Windows.Forms.Label();
            this.lblLoginUsername = new System.Windows.Forms.Label();
            this.lblLoginTitle = new System.Windows.Forms.Label();
            this.tabPageRegister = new System.Windows.Forms.TabPage();
            this.btnRegister = new System.Windows.Forms.Button();
            this.txtRegisterConfirmPassword = new System.Windows.Forms.TextBox();
            this.lblRegisterConfirmPassword = new System.Windows.Forms.Label();
            this.txtRegisterPassword = new System.Windows.Forms.TextBox();
            this.txtRegisterEmail = new System.Windows.Forms.TextBox();
            this.lblRegisterEmail = new System.Windows.Forms.Label();
            this.txtRegisterUsername = new System.Windows.Forms.TextBox();
            this.lblRegisterPassword = new System.Windows.Forms.Label();
            this.lblRegisterUsername = new System.Windows.Forms.Label();
            this.lblRegisterTitle = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPageLogin.SuspendLayout();
            this.tabPageRegister.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageLogin);
            this.tabControl1.Controls.Add(this.tabPageRegister);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(450, 380);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageLogin
            // 
            this.tabPageLogin.BackColor = System.Drawing.Color.White;
            this.tabPageLogin.Controls.Add(this.lnkForgotPassword);
            this.tabPageLogin.Controls.Add(this.btnLogin);
            this.tabPageLogin.Controls.Add(this.txtLoginPassword);
            this.tabPageLogin.Controls.Add(this.txtLoginUsername);
            this.tabPageLogin.Controls.Add(this.lblLoginPassword);
            this.tabPageLogin.Controls.Add(this.lblLoginUsername);
            this.tabPageLogin.Controls.Add(this.lblLoginTitle);
            this.tabPageLogin.Location = new System.Drawing.Point(4, 24);
            this.tabPageLogin.Name = "tabPageLogin";
            this.tabPageLogin.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLogin.Size = new System.Drawing.Size(442, 352);
            this.tabPageLogin.TabIndex = 0;
            this.tabPageLogin.Text = "Connexion";
            // 
            // lnkForgotPassword
            // 
            this.lnkForgotPassword.AutoSize = true;
            this.lnkForgotPassword.Location = new System.Drawing.Point(220, 200);
            this.lnkForgotPassword.Name = "lnkForgotPassword";
            this.lnkForgotPassword.Size = new System.Drawing.Size(120, 15);
            this.lnkForgotPassword.TabIndex = 6;
            this.lnkForgotPassword.TabStop = true;
            this.lnkForgotPassword.Text = "Mot de passe oublié ?";
            this.lnkForgotPassword.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkForgotPassword_LinkClicked);
            // 
            // btnLogin
            // 
            this.btnLogin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnLogin.ForeColor = System.Drawing.Color.White;
            this.btnLogin.Location = new System.Drawing.Point(100, 240);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(240, 40);
            this.btnLogin.TabIndex = 5;
            this.btnLogin.Text = "Se connecter";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // txtLoginPassword
            // 
            this.txtLoginPassword.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtLoginPassword.Location = new System.Drawing.Point(100, 170);
            this.txtLoginPassword.Name = "txtLoginPassword";
            this.txtLoginPassword.Size = new System.Drawing.Size(240, 25);
            this.txtLoginPassword.TabIndex = 4;
            this.txtLoginPassword.UseSystemPasswordChar = true;
            // 
            // txtLoginUsername
            // 
            this.txtLoginUsername.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtLoginUsername.Location = new System.Drawing.Point(100, 110);
            this.txtLoginUsername.Name = "txtLoginUsername";
            this.txtLoginUsername.Size = new System.Drawing.Size(240, 25);
            this.txtLoginUsername.TabIndex = 3;
            // 
            // lblLoginPassword
            // 
            this.lblLoginPassword.AutoSize = true;
            this.lblLoginPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLoginPassword.Location = new System.Drawing.Point(100, 150);
            this.lblLoginPassword.Name = "lblLoginPassword";
            this.lblLoginPassword.Size = new System.Drawing.Size(77, 15);
            this.lblLoginPassword.TabIndex = 2;
            this.lblLoginPassword.Text = "Mot de passe";
            // 
            // lblLoginUsername
            // 
            this.lblLoginUsername.AutoSize = true;
            this.lblLoginUsername.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLoginUsername.Location = new System.Drawing.Point(100, 90);
            this.lblLoginUsername.Name = "lblLoginUsername";
            this.lblLoginUsername.Size = new System.Drawing.Size(96, 15);
            this.lblLoginUsername.TabIndex = 1;
            this.lblLoginUsername.Text = "Nom d\'utilisateur";
            // 
            // lblLoginTitle
            // 
            this.lblLoginTitle.AutoSize = true;
            this.lblLoginTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblLoginTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.lblLoginTitle.Location = new System.Drawing.Point(150, 30);
            this.lblLoginTitle.Name = "lblLoginTitle";
            this.lblLoginTitle.Size = new System.Drawing.Size(142, 30);
            this.lblLoginTitle.TabIndex = 0;
            this.lblLoginTitle.Text = "Connexion";
            // 
            // tabPageRegister
            // 
            this.tabPageRegister.BackColor = System.Drawing.Color.White;
            this.tabPageRegister.Controls.Add(this.btnRegister);
            this.tabPageRegister.Controls.Add(this.txtRegisterConfirmPassword);
            this.tabPageRegister.Controls.Add(this.lblRegisterConfirmPassword);
            this.tabPageRegister.Controls.Add(this.txtRegisterPassword);
            this.tabPageRegister.Controls.Add(this.txtRegisterEmail);
            this.tabPageRegister.Controls.Add(this.lblRegisterEmail);
            this.tabPageRegister.Controls.Add(this.txtRegisterUsername);
            this.tabPageRegister.Controls.Add(this.lblRegisterPassword);
            this.tabPageRegister.Controls.Add(this.lblRegisterUsername);
            this.tabPageRegister.Controls.Add(this.lblRegisterTitle);
            this.tabPageRegister.Location = new System.Drawing.Point(4, 24);
            this.tabPageRegister.Name = "tabPageRegister";
            this.tabPageRegister.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageRegister.Size = new System.Drawing.Size(442, 352);
            this.tabPageRegister.TabIndex = 1;
            this.tabPageRegister.Text = "Inscription";
            // 
            // btnRegister
            // 
            this.btnRegister.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnRegister.FlatAppearance.BorderSize = 0;
            this.btnRegister.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRegister.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnRegister.ForeColor = System.Drawing.Color.White;
            this.btnRegister.Location = new System.Drawing.Point(100, 290);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(240, 40);
            this.btnRegister.TabIndex = 9;
            this.btnRegister.Text = "S\'inscrire";
            this.btnRegister.UseVisualStyleBackColor = false;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // txtRegisterConfirmPassword
            // 
            this.txtRegisterConfirmPassword.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtRegisterConfirmPassword.Location = new System.Drawing.Point(100, 240);
            this.txtRegisterConfirmPassword.Name = "txtRegisterConfirmPassword";
            this.txtRegisterConfirmPassword.Size = new System.Drawing.Size(240, 25);
            this.txtRegisterConfirmPassword.TabIndex = 8;
            this.txtRegisterConfirmPassword.UseSystemPasswordChar = true;
            // 
            // lblRegisterConfirmPassword
            // 
            this.lblRegisterConfirmPassword.AutoSize = true;
            this.lblRegisterConfirmPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRegisterConfirmPassword.Location = new System.Drawing.Point(100, 220);
            this.lblRegisterConfirmPassword.Name = "lblRegisterConfirmPassword";
            this.lblRegisterConfirmPassword.Size = new System.Drawing.Size(144, 15);
            this.lblRegisterConfirmPassword.TabIndex = 7;
            this.lblRegisterConfirmPassword.Text = "Confirmer le mot de passe";
            // 
            // txtRegisterPassword
            // 
            this.txtRegisterPassword.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtRegisterPassword.Location = new System.Drawing.Point(100, 180);
            this.txtRegisterPassword.Name = "txtRegisterPassword";
            this.txtRegisterPassword.Size = new System.Drawing.Size(240, 25);
            this.txtRegisterPassword.TabIndex = 6;
            this.txtRegisterPassword.UseSystemPasswordChar = true;
            // 
            // txtRegisterEmail
            // 
            this.txtRegisterEmail.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtRegisterEmail.Location = new System.Drawing.Point(100, 120);
            this.txtRegisterEmail.Name = "txtRegisterEmail";
            this.txtRegisterEmail.Size = new System.Drawing.Size(240, 25);
            this.txtRegisterEmail.TabIndex = 5;
            // 
            // lblRegisterEmail
            // 
            this.lblRegisterEmail.AutoSize = true;
            this.lblRegisterEmail.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRegisterEmail.Location = new System.Drawing.Point(100, 100);
            this.lblRegisterEmail.Name = "lblRegisterEmail";
            this.lblRegisterEmail.Size = new System.Drawing.Size(36, 15);
            this.lblRegisterEmail.TabIndex = 4;
            this.lblRegisterEmail.Text = "Email";
            // 
            // txtRegisterUsername
            // 
            this.txtRegisterUsername.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtRegisterUsername.Location = new System.Drawing.Point(100, 70);
            this.txtRegisterUsername.Name = "txtRegisterUsername";
            this.txtRegisterUsername.Size = new System.Drawing.Size(240, 25);
            this.txtRegisterUsername.TabIndex = 3;
            // 
            // lblRegisterPassword
            // 
            this.lblRegisterPassword.AutoSize = true;
            this.lblRegisterPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRegisterPassword.Location = new System.Drawing.Point(100, 160);
            this.lblRegisterPassword.Name = "lblRegisterPassword";
            this.lblRegisterPassword.Size = new System.Drawing.Size(77, 15);
            this.lblRegisterPassword.TabIndex = 2;
            this.lblRegisterPassword.Text = "Mot de passe";
            // 
            // lblRegisterUsername
            // 
            this.lblRegisterUsername.AutoSize = true;
            this.lblRegisterUsername.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRegisterUsername.Location = new System.Drawing.Point(100, 50);
            this.lblRegisterUsername.Name = "lblRegisterUsername";
            this.lblRegisterUsername.Size = new System.Drawing.Size(96, 15);
            this.lblRegisterUsername.TabIndex = 1;
            this.lblRegisterUsername.Text = "Nom d\'utilisateur";
            // 
            // lblRegisterTitle
            // 
            this.lblRegisterTitle.AutoSize = true;
            this.lblRegisterTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblRegisterTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.lblRegisterTitle.Location = new System.Drawing.Point(150, 10);
            this.lblRegisterTitle.Name = "lblRegisterTitle";
            this.lblRegisterTitle.Size = new System.Drawing.Size(132, 30);
            this.lblRegisterTitle.TabIndex = 0;
            this.lblRegisterTitle.Text = "Inscription";
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 380);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Impression 3D - Authentification";
            this.tabControl1.ResumeLayout(false);
            this.tabPageLogin.ResumeLayout(false);
            this.tabPageLogin.PerformLayout();
            this.tabPageRegister.ResumeLayout(false);
            this.tabPageRegister.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageLogin;
        private System.Windows.Forms.TabPage tabPageRegister;
        private System.Windows.Forms.Label lblLoginTitle;
        private System.Windows.Forms.Label lblLoginUsername;
        private System.Windows.Forms.Label lblLoginPassword;
        private System.Windows.Forms.TextBox txtLoginUsername;
        private System.Windows.Forms.TextBox txtLoginPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.LinkLabel lnkForgotPassword;
        private System.Windows.Forms.Label lblRegisterTitle;
        private System.Windows.Forms.Label lblRegisterUsername;
        private System.Windows.Forms.Label lblRegisterPassword;
        private System.Windows.Forms.TextBox txtRegisterUsername;
        private System.Windows.Forms.TextBox txtRegisterPassword;
        private System.Windows.Forms.TextBox txtRegisterConfirmPassword;
        private System.Windows.Forms.Label lblRegisterConfirmPassword;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.TextBox txtRegisterEmail;
        private System.Windows.Forms.Label lblRegisterEmail;
    }
}