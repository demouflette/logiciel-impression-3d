namespace logiciel_d_impression_3d
{
    partial class Form1
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fichierToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.profilToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.paramètresToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.déconnexionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.quitterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.vérifierMiseÀJourToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rafraîchirSpécsImprimantesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.àProposToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblWelcomeUser = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmbPrinter = new System.Windows.Forms.ComboBox();
            this.lblPrinter = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dgvCouleurs = new System.Windows.Forms.DataGridView();
            this.lblNombreCouleurs = new System.Windows.Forms.Label();
            this.numNombreCouleurs = new System.Windows.Forms.NumericUpDown();
            this.lblConfigCouleurs = new System.Windows.Forms.Label();
            this.numNombrePlateaux = new System.Windows.Forms.NumericUpDown();
            this.lblNombrePlateaux = new System.Windows.Forms.Label();
            this.lblAMSStatus = new System.Windows.Forms.Label();
            this.numNombreAMS = new System.Windows.Forms.NumericUpDown();
            this.lblNombreAMS = new System.Windows.Forms.Label();
            this.chkAMS = new System.Windows.Forms.CheckBox();
            this.rdoMultiCouleur = new System.Windows.Forms.RadioButton();
            this.rdoMonoCouleur = new System.Windows.Forms.RadioButton();
            this.numNombreObjets = new System.Windows.Forms.NumericUpDown();
            this.lblNombreObjets = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.panelTemps = new System.Windows.Forms.Panel();
            this.btnCalculerDevis = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCouleurs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNombreCouleurs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNombrePlateaux)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNombreAMS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNombreObjets)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fichierToolStripMenuItem,
            this.aideToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1000, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fichierToolStripMenuItem
            // 
            this.fichierToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.profilToolStripMenuItem,
            this.paramètresToolStripMenuItem,
            this.déconnexionToolStripMenuItem,
            this.toolStripSeparator1,
            this.quitterToolStripMenuItem});
            this.fichierToolStripMenuItem.Name = "fichierToolStripMenuItem";
            this.fichierToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.fichierToolStripMenuItem.Text = "Fichier";
            // 
            // profilToolStripMenuItem
            // 
            this.profilToolStripMenuItem.Name = "profilToolStripMenuItem";
            this.profilToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.profilToolStripMenuItem.Text = "Profil";
            this.profilToolStripMenuItem.Click += new System.EventHandler(this.profilToolStripMenuItem_Click);
            // 
            // paramètresToolStripMenuItem
            // 
            this.paramètresToolStripMenuItem.Name = "paramètresToolStripMenuItem";
            this.paramètresToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.paramètresToolStripMenuItem.Text = "Paramètres impression";
            this.paramètresToolStripMenuItem.Click += new System.EventHandler(this.paramètresToolStripMenuItem_Click);
            // 
            // déconnexionToolStripMenuItem
            // 
            this.déconnexionToolStripMenuItem.Name = "déconnexionToolStripMenuItem";
            this.déconnexionToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.déconnexionToolStripMenuItem.Text = "Déconnexion";
            this.déconnexionToolStripMenuItem.Click += new System.EventHandler(this.déconnexionToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(194, 6);
            // 
            // quitterToolStripMenuItem
            // 
            this.quitterToolStripMenuItem.Name = "quitterToolStripMenuItem";
            this.quitterToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.quitterToolStripMenuItem.Text = "Quitter";
            this.quitterToolStripMenuItem.Click += new System.EventHandler(this.quitterToolStripMenuItem_Click);
            // 
            // aideToolStripMenuItem
            // 
            this.aideToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.vérifierMiseÀJourToolStripMenuItem,
            this.rafraîchirSpécsImprimantesToolStripMenuItem,
            this.toolStripSeparator2,
            this.àProposToolStripMenuItem});
            this.aideToolStripMenuItem.Name = "aideToolStripMenuItem";
            this.aideToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.aideToolStripMenuItem.Text = "Aide";
            // 
            // vérifierMiseÀJourToolStripMenuItem
            // 
            this.vérifierMiseÀJourToolStripMenuItem.Name = "vérifierMiseÀJourToolStripMenuItem";
            this.vérifierMiseÀJourToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.vérifierMiseÀJourToolStripMenuItem.Text = "Vérifier les mises à jour";
            this.vérifierMiseÀJourToolStripMenuItem.Click += new System.EventHandler(this.vérifierMiseÀJourToolStripMenuItem_Click);
            // 
            // rafraîchirSpécsImprimantesToolStripMenuItem
            // 
            this.rafraîchirSpécsImprimantesToolStripMenuItem.Name = "rafraîchirSpécsImprimantesToolStripMenuItem";
            this.rafraîchirSpécsImprimantesToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.rafraîchirSpécsImprimantesToolStripMenuItem.Text = "Rafraîchir specs imprimantes";
            this.rafraîchirSpécsImprimantesToolStripMenuItem.Click += new System.EventHandler(this.rafraîchirSpécsImprimantesToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(233, 6);
            // 
            // àProposToolStripMenuItem
            // 
            this.àProposToolStripMenuItem.Name = "àProposToolStripMenuItem";
            this.àProposToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.àProposToolStripMenuItem.Text = "À propos";
            this.àProposToolStripMenuItem.Click += new System.EventHandler(this.àProposToolStripMenuItem_Click);
            // 
            // lblWelcomeUser
            // 
            this.lblWelcomeUser.AutoSize = true;
            this.lblWelcomeUser.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblWelcomeUser.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.lblWelcomeUser.Location = new System.Drawing.Point(12, 35);
            this.lblWelcomeUser.Name = "lblWelcomeUser";
            this.lblWelcomeUser.Size = new System.Drawing.Size(100, 25);
            this.lblWelcomeUser.TabIndex = 1;
            this.lblWelcomeUser.Text = "Bienvenue";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmbPrinter);
            this.groupBox1.Controls.Add(this.lblPrinter);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupBox1.Location = new System.Drawing.Point(17, 75);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(960, 80);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Imprimante";
            // 
            // cmbPrinter
            // 
            this.cmbPrinter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPrinter.FormattingEnabled = true;
            this.cmbPrinter.Location = new System.Drawing.Point(100, 35);
            this.cmbPrinter.Name = "cmbPrinter";
            this.cmbPrinter.Size = new System.Drawing.Size(240, 25);
            this.cmbPrinter.TabIndex = 1;
            // 
            // lblPrinter
            // 
            this.lblPrinter.AutoSize = true;
            this.lblPrinter.Location = new System.Drawing.Point(15, 38);
            this.lblPrinter.Name = "lblPrinter";
            this.lblPrinter.Size = new System.Drawing.Size(79, 19);
            this.lblPrinter.TabIndex = 0;
            this.lblPrinter.Text = "Imprimante:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dgvCouleurs);
            this.groupBox2.Controls.Add(this.lblNombreCouleurs);
            this.groupBox2.Controls.Add(this.numNombreCouleurs);
            this.groupBox2.Controls.Add(this.lblConfigCouleurs);
            this.groupBox2.Controls.Add(this.numNombrePlateaux);
            this.groupBox2.Controls.Add(this.lblNombrePlateaux);
            this.groupBox2.Controls.Add(this.lblAMSStatus);
            this.groupBox2.Controls.Add(this.numNombreAMS);
            this.groupBox2.Controls.Add(this.lblNombreAMS);
            this.groupBox2.Controls.Add(this.chkAMS);
            this.groupBox2.Controls.Add(this.rdoMultiCouleur);
            this.groupBox2.Controls.Add(this.rdoMonoCouleur);
            this.groupBox2.Controls.Add(this.numNombreObjets);
            this.groupBox2.Controls.Add(this.lblNombreObjets);
            this.groupBox2.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupBox2.Location = new System.Drawing.Point(17, 170);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(650, 530);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Configuration couleurs et filaments";
            // 
            // dgvCouleurs
            // 
            this.dgvCouleurs.AllowUserToAddRows = true;
            this.dgvCouleurs.AllowUserToDeleteRows = true;
            this.dgvCouleurs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCouleurs.Location = new System.Drawing.Point(19, 250);
            this.dgvCouleurs.Name = "dgvCouleurs";
            this.dgvCouleurs.Size = new System.Drawing.Size(610, 260);
            this.dgvCouleurs.TabIndex = 11;
            // 
            // lblNombreCouleurs
            // 
            this.lblNombreCouleurs.AutoSize = true;
            this.lblNombreCouleurs.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblNombreCouleurs.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.lblNombreCouleurs.Location = new System.Drawing.Point(19, 220);
            this.lblNombreCouleurs.Name = "lblNombreCouleurs";
            this.lblNombreCouleurs.Size = new System.Drawing.Size(130, 15);
            this.lblNombreCouleurs.TabIndex = 10;
            this.lblNombreCouleurs.Text = "Couleurs: 1 | Plateaux: 1";
            // 
            // numNombreCouleurs
            // 
            this.numNombreCouleurs.Enabled = false;
            this.numNombreCouleurs.Location = new System.Drawing.Point(490, 175);
            this.numNombreCouleurs.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.numNombreCouleurs.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numNombreCouleurs.Name = "numNombreCouleurs";
            this.numNombreCouleurs.Size = new System.Drawing.Size(140, 25);
            this.numNombreCouleurs.TabIndex = 9;
            this.numNombreCouleurs.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numNombreCouleurs.ValueChanged += new System.EventHandler(this.numNombreCouleurs_ValueChanged);
            // 
            // lblConfigCouleurs
            // 
            this.lblConfigCouleurs.AutoSize = true;
            this.lblConfigCouleurs.Location = new System.Drawing.Point(360, 177);
            this.lblConfigCouleurs.Name = "lblConfigCouleurs";
            this.lblConfigCouleurs.Size = new System.Drawing.Size(124, 19);
            this.lblConfigCouleurs.TabIndex = 8;
            this.lblConfigCouleurs.Text = "Nombre couleurs:";
            // 
            // numNombrePlateaux
            // 
            this.numNombrePlateaux.Enabled = false;
            this.numNombrePlateaux.Location = new System.Drawing.Point(200, 175);
            this.numNombrePlateaux.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.numNombrePlateaux.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numNombrePlateaux.Name = "numNombrePlateaux";
            this.numNombrePlateaux.Size = new System.Drawing.Size(140, 25);
            this.numNombrePlateaux.TabIndex = 7;
            this.numNombrePlateaux.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numNombrePlateaux.ValueChanged += new System.EventHandler(this.numNombrePlateaux_ValueChanged);
            // 
            // lblNombrePlateaux
            // 
            this.lblNombrePlateaux.AutoSize = true;
            this.lblNombrePlateaux.Location = new System.Drawing.Point(15, 177);
            this.lblNombrePlateaux.Name = "lblNombrePlateaux";
            this.lblNombrePlateaux.Size = new System.Drawing.Size(140, 19);
            this.lblNombrePlateaux.TabIndex = 6;
            this.lblNombrePlateaux.Text = "Nombre de plateaux:";
            // 
            // lblAMSStatus
            // 
            this.lblAMSStatus.AutoSize = true;
            this.lblAMSStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblAMSStatus.Location = new System.Drawing.Point(200, 135);
            this.lblAMSStatus.Name = "lblAMSStatus";
            this.lblAMSStatus.Size = new System.Drawing.Size(96, 19);
            this.lblAMSStatus.TabIndex = 5;
            this.lblAMSStatus.Text = "AMS désactivé";
            // 
            // numNombreAMS
            // 
            this.numNombreAMS.Enabled = false;
            this.numNombreAMS.Location = new System.Drawing.Point(200, 95);
            this.numNombreAMS.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.numNombreAMS.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numNombreAMS.Name = "numNombreAMS";
            this.numNombreAMS.Size = new System.Drawing.Size(140, 25);
            this.numNombreAMS.TabIndex = 4;
            this.numNombreAMS.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numNombreAMS.ValueChanged += new System.EventHandler(this.numNombreAMS_ValueChanged);
            // 
            // lblNombreAMS
            // 
            this.lblNombreAMS.AutoSize = true;
            this.lblNombreAMS.Location = new System.Drawing.Point(15, 97);
            this.lblNombreAMS.Name = "lblNombreAMS";
            this.lblNombreAMS.Size = new System.Drawing.Size(176, 19);
            this.lblNombreAMS.TabIndex = 3;
            this.lblNombreAMS.Text = "Nombre d\'AMS (max 6 × 4):";
            // 
            // chkAMS
            // 
            this.chkAMS.AutoSize = true;
            this.chkAMS.Enabled = false;
            this.chkAMS.Location = new System.Drawing.Point(19, 134);
            this.chkAMS.Name = "chkAMS";
            this.chkAMS.Size = new System.Drawing.Size(151, 23);
            this.chkAMS.TabIndex = 2;
            this.chkAMS.Text = "Utiliser AMS (Bambu)";
            this.chkAMS.UseVisualStyleBackColor = true;
            this.chkAMS.CheckedChanged += new System.EventHandler(this.chkAMS_CheckedChanged);
            // 
            // rdoMultiCouleur
            // 
            this.rdoMultiCouleur.AutoSize = true;
            this.rdoMultiCouleur.Location = new System.Drawing.Point(19, 65);
            this.rdoMultiCouleur.Name = "rdoMultiCouleur";
            this.rdoMultiCouleur.Size = new System.Drawing.Size(115, 23);
            this.rdoMultiCouleur.TabIndex = 1;
            this.rdoMultiCouleur.Text = "Multi-couleur";
            this.rdoMultiCouleur.UseVisualStyleBackColor = true;
            this.rdoMultiCouleur.CheckedChanged += new System.EventHandler(this.rdoMultiCouleur_CheckedChanged);
            // 
            // rdoMonoCouleur
            // 
            this.rdoMonoCouleur.AutoSize = true;
            this.rdoMonoCouleur.Checked = true;
            this.rdoMonoCouleur.Location = new System.Drawing.Point(19, 35);
            this.rdoMonoCouleur.Name = "rdoMonoCouleur";
            this.rdoMonoCouleur.Size = new System.Drawing.Size(120, 23);
            this.rdoMonoCouleur.TabIndex = 0;
            this.rdoMonoCouleur.TabStop = true;
            this.rdoMonoCouleur.Text = "Mono-couleur";
            this.rdoMonoCouleur.UseVisualStyleBackColor = true;
            this.rdoMonoCouleur.CheckedChanged += new System.EventHandler(this.rdoMonoCouleur_CheckedChanged);
            // 
            // numNombreObjets
            // 
            this.numNombreObjets.Location = new System.Drawing.Point(490, 35);
            this.numNombreObjets.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numNombreObjets.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numNombreObjets.Name = "numNombreObjets";
            this.numNombreObjets.Size = new System.Drawing.Size(140, 25);
            this.numNombreObjets.TabIndex = 12;
            this.numNombreObjets.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblNombreObjets
            // 
            this.lblNombreObjets.AutoSize = true;
            this.lblNombreObjets.Location = new System.Drawing.Point(360, 37);
            this.lblNombreObjets.Name = "lblNombreObjets";
            this.lblNombreObjets.Size = new System.Drawing.Size(115, 19);
            this.lblNombreObjets.TabIndex = 13;
            this.lblNombreObjets.Text = "Nombre d\'objets:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.panelTemps);
            this.groupBox3.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupBox3.Location = new System.Drawing.Point(687, 170);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(290, 440);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Temps d\'impression par plateau";
            // 
            // panelTemps
            // 
            this.panelTemps.AutoScroll = true;
            this.panelTemps.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTemps.Location = new System.Drawing.Point(15, 30);
            this.panelTemps.Name = "panelTemps";
            this.panelTemps.Size = new System.Drawing.Size(260, 390);
            this.panelTemps.TabIndex = 0;
            // 
            // btnCalculerDevis
            // 
            this.btnCalculerDevis.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnCalculerDevis.FlatAppearance.BorderSize = 0;
            this.btnCalculerDevis.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCalculerDevis.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnCalculerDevis.ForeColor = System.Drawing.Color.White;
            this.btnCalculerDevis.Location = new System.Drawing.Point(687, 630);
            this.btnCalculerDevis.Name = "btnCalculerDevis";
            this.btnCalculerDevis.Size = new System.Drawing.Size(290, 60);
            this.btnCalculerDevis.TabIndex = 5;
            this.btnCalculerDevis.Text = "📊 Calculer le devis";
            this.btnCalculerDevis.UseVisualStyleBackColor = false;
            this.btnCalculerDevis.Click += new System.EventHandler(this.btnCalculerDevis_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 720);
            this.Controls.Add(this.btnCalculerDevis);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblWelcomeUser);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Logiciel d\'Impression 3D - Calcul de Devis";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCouleurs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNombreCouleurs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNombrePlateaux)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNombreAMS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNombreObjets)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fichierToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem profilToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem paramètresToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem déconnexionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem quitterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aideToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem vérifierMiseÀJourToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rafraîchirSpécsImprimantesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem àProposToolStripMenuItem;
        private System.Windows.Forms.Label lblWelcomeUser;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cmbPrinter;
        private System.Windows.Forms.Label lblPrinter;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dgvCouleurs;
        private System.Windows.Forms.Label lblNombreCouleurs;
        private System.Windows.Forms.NumericUpDown numNombreCouleurs;
        private System.Windows.Forms.Label lblConfigCouleurs;
        private System.Windows.Forms.NumericUpDown numNombrePlateaux;
        private System.Windows.Forms.Label lblNombrePlateaux;
        private System.Windows.Forms.Label lblAMSStatus;
        private System.Windows.Forms.NumericUpDown numNombreAMS;
        private System.Windows.Forms.Label lblNombreAMS;
        private System.Windows.Forms.CheckBox chkAMS;
        private System.Windows.Forms.RadioButton rdoMultiCouleur;
        private System.Windows.Forms.RadioButton rdoMonoCouleur;
        private System.Windows.Forms.NumericUpDown numNombreObjets;
        private System.Windows.Forms.Label lblNombreObjets;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Panel panelTemps;
        private System.Windows.Forms.Button btnCalculerDevis;
    }
}

