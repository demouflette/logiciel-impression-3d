namespace logiciel_d_impression_3d
{
    partial class ParametresImpressionForm
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
            this.tabPageGeneral = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.numMarge = new System.Windows.Forms.NumericUpDown();
            this.lblMarge = new System.Windows.Forms.Label();
            this.numTVA = new System.Windows.Forms.NumericUpDown();
            this.lblTVA = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.numPourcentagePurge = new System.Windows.Forms.NumericUpDown();
            this.lblPourcentagePurge = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.numCoutElectricite = new System.Windows.Forms.NumericUpDown();
            this.lblCoutElectricite = new System.Windows.Forms.Label();
            this.tabPageBobines = new System.Windows.Forms.TabPage();
            this.btnSupprimerBobine = new System.Windows.Forms.Button();
            this.btnAjouterBobine = new System.Windows.Forms.Button();
            this.dgvBobines = new System.Windows.Forms.DataGridView();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.numAmortissement = new System.Windows.Forms.NumericUpDown();
            this.lblAmortissement = new System.Windows.Forms.Label();
            this.numCoutMainOeuvre = new System.Windows.Forms.NumericUpDown();
            this.lblCoutMainOeuvre = new System.Windows.Forms.Label();
            this.groupBoxGithub = new System.Windows.Forms.GroupBox();
            this.txtTokenGithub = new System.Windows.Forms.TextBox();
            this.groupBoxSlicer = new System.Windows.Forms.GroupBox();
            this.txtCheminSlicer = new System.Windows.Forms.TextBox();
            this.lblCheminSlicer = new System.Windows.Forms.Label();
            this.btnParcourirSlicer = new System.Windows.Forms.Button();
            this.lblStatutSlicerParam = new System.Windows.Forms.Label();
            this.lblTokenGithub = new System.Windows.Forms.Label();
            this.tabPageImprimantes = new System.Windows.Forms.TabPage();
            this.dgvImprimantesCustom = new System.Windows.Forms.DataGridView();
            this.btnAjouterImprimante = new System.Windows.Forms.Button();
            this.btnSupprimerImprimante = new System.Windows.Forms.Button();
            this.lblInfoImprimantes = new System.Windows.Forms.Label();
            this.btnEnregistrer = new System.Windows.Forms.Button();
            this.btnAnnuler = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPageGeneral.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMarge)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTVA)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPourcentagePurge)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCoutElectricite)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCoutMainOeuvre)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAmortissement)).BeginInit();
            this.tabPageBobines.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBobines)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageGeneral);
            this.tabControl1.Controls.Add(this.tabPageBobines);
            this.tabControl1.Controls.Add(this.tabPageImprimantes);
            this.tabControl1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(760, 680);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageGeneral
            // 
            this.tabPageGeneral.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.tabPageGeneral.Controls.Add(this.groupBoxSlicer);
            this.tabPageGeneral.Controls.Add(this.groupBoxGithub);
            this.tabPageGeneral.Controls.Add(this.groupBox4);
            this.tabPageGeneral.Controls.Add(this.groupBox3);
            this.tabPageGeneral.Controls.Add(this.groupBox2);
            this.tabPageGeneral.Controls.Add(this.groupBox1);
            this.tabPageGeneral.Location = new System.Drawing.Point(4, 26);
            this.tabPageGeneral.Name = "tabPageGeneral";
            this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGeneral.Size = new System.Drawing.Size(752, 560);
            this.tabPageGeneral.TabIndex = 0;
            this.tabPageGeneral.Text = "Paramètres généraux";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.numMarge);
            this.groupBox3.Controls.Add(this.lblMarge);
            this.groupBox3.Controls.Add(this.numTVA);
            this.groupBox3.Controls.Add(this.lblTVA);
            this.groupBox3.Location = new System.Drawing.Point(20, 230);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(710, 120);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Facturation";
            // 
            // numMarge
            // 
            this.numMarge.DecimalPlaces = 2;
            this.numMarge.Location = new System.Drawing.Point(300, 70);
            this.numMarge.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numMarge.Name = "numMarge";
            this.numMarge.Size = new System.Drawing.Size(150, 25);
            this.numMarge.TabIndex = 3;
            this.numMarge.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // lblMarge
            // 
            this.lblMarge.AutoSize = true;
            this.lblMarge.Location = new System.Drawing.Point(20, 72);
            this.lblMarge.Name = "lblMarge";
            this.lblMarge.Size = new System.Drawing.Size(185, 19);
            this.lblMarge.TabIndex = 2;
            this.lblMarge.Text = "Marge par objet (pourcentage):";
            // 
            // numTVA
            // 
            this.numTVA.DecimalPlaces = 2;
            this.numTVA.Location = new System.Drawing.Point(300, 35);
            this.numTVA.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numTVA.Name = "numTVA";
            this.numTVA.Size = new System.Drawing.Size(150, 25);
            this.numTVA.TabIndex = 1;
            this.numTVA.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // lblTVA
            // 
            this.lblTVA.AutoSize = true;
            this.lblTVA.Location = new System.Drawing.Point(20, 37);
            this.lblTVA.Name = "lblTVA";
            this.lblTVA.Size = new System.Drawing.Size(108, 19);
            this.lblTVA.TabIndex = 0;
            this.lblTVA.Text = "TVA (pourcentage):";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.numPourcentagePurge);
            this.groupBox2.Controls.Add(this.lblPourcentagePurge);
            this.groupBox2.Location = new System.Drawing.Point(20, 130);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(710, 80);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "AMS (Automatic Material System)";
            // 
            // numPourcentagePurge
            // 
            this.numPourcentagePurge.DecimalPlaces = 2;
            this.numPourcentagePurge.Location = new System.Drawing.Point(300, 35);
            this.numPourcentagePurge.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numPourcentagePurge.Name = "numPourcentagePurge";
            this.numPourcentagePurge.Size = new System.Drawing.Size(150, 25);
            this.numPourcentagePurge.TabIndex = 1;
            this.numPourcentagePurge.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // lblPourcentagePurge
            // 
            this.lblPourcentagePurge.AutoSize = true;
            this.lblPourcentagePurge.Location = new System.Drawing.Point(20, 37);
            this.lblPourcentagePurge.Name = "lblPourcentagePurge";
            this.lblPourcentagePurge.Size = new System.Drawing.Size(239, 19);
            this.lblPourcentagePurge.TabIndex = 0;
            this.lblPourcentagePurge.Text = "Pourcentage de perte dû à la purge (%):";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.numCoutElectricite);
            this.groupBox1.Controls.Add(this.lblCoutElectricite);
            this.groupBox1.Location = new System.Drawing.Point(20, 20);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(710, 90);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Électricité";
            // 
            // numCoutElectricite
            // 
            this.numCoutElectricite.DecimalPlaces = 4;
            this.numCoutElectricite.Increment = new decimal(new int[] {
            1,
            0,
            0,
            262144});
            this.numCoutElectricite.Location = new System.Drawing.Point(300, 40);
            this.numCoutElectricite.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numCoutElectricite.Name = "numCoutElectricite";
            this.numCoutElectricite.Size = new System.Drawing.Size(150, 25);
            this.numCoutElectricite.TabIndex = 1;
            this.numCoutElectricite.Value = new decimal(new int[] {
            15,
            0,
            0,
            131072});
            // 
            // lblCoutElectricite
            // 
            this.lblCoutElectricite.AutoSize = true;
            this.lblCoutElectricite.Location = new System.Drawing.Point(20, 42);
            this.lblCoutElectricite.Name = "lblCoutElectricite";
            this.lblCoutElectricite.Size = new System.Drawing.Size(177, 19);
            this.lblCoutElectricite.TabIndex = 0;
            this.lblCoutElectricite.Text = "Coût de l\'électricité (€/kWh):";
            // 
            // tabPageBobines
            // 
            this.tabPageBobines.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.tabPageBobines.Controls.Add(this.btnSupprimerBobine);
            this.tabPageBobines.Controls.Add(this.btnAjouterBobine);
            this.tabPageBobines.Controls.Add(this.dgvBobines);
            this.tabPageBobines.Location = new System.Drawing.Point(4, 26);
            this.tabPageBobines.Name = "tabPageBobines";
            this.tabPageBobines.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageBobines.Size = new System.Drawing.Size(752, 560);
            this.tabPageBobines.TabIndex = 1;
            this.tabPageBobines.Text = "Gestion des bobines";
            // 
            // btnSupprimerBobine
            // 
            this.btnSupprimerBobine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnSupprimerBobine.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSupprimerBobine.ForeColor = System.Drawing.Color.White;
            this.btnSupprimerBobine.Location = new System.Drawing.Point(150, 400);
            this.btnSupprimerBobine.Name = "btnSupprimerBobine";
            this.btnSupprimerBobine.Size = new System.Drawing.Size(120, 35);
            this.btnSupprimerBobine.TabIndex = 2;
            this.btnSupprimerBobine.Text = "Supprimer";
            this.btnSupprimerBobine.UseVisualStyleBackColor = false;
            this.btnSupprimerBobine.Click += new System.EventHandler(this.btnSupprimerBobine_Click);
            // 
            // btnAjouterBobine
            // 
            this.btnAjouterBobine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnAjouterBobine.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAjouterBobine.ForeColor = System.Drawing.Color.White;
            this.btnAjouterBobine.Location = new System.Drawing.Point(20, 400);
            this.btnAjouterBobine.Name = "btnAjouterBobine";
            this.btnAjouterBobine.Size = new System.Drawing.Size(120, 35);
            this.btnAjouterBobine.TabIndex = 1;
            this.btnAjouterBobine.Text = "Ajouter";
            this.btnAjouterBobine.UseVisualStyleBackColor = false;
            this.btnAjouterBobine.Click += new System.EventHandler(this.btnAjouterBobine_Click);
            // 
            // dgvBobines
            // 
            this.dgvBobines.AllowUserToAddRows = false;
            this.dgvBobines.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBobines.Location = new System.Drawing.Point(20, 20);
            this.dgvBobines.Name = "dgvBobines";
            this.dgvBobines.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBobines.Size = new System.Drawing.Size(710, 365);
            this.dgvBobines.TabIndex = 0;
            //
            // groupBox4
            //
            this.groupBox4.Controls.Add(this.numAmortissement);
            this.groupBox4.Controls.Add(this.lblAmortissement);
            this.groupBox4.Controls.Add(this.numCoutMainOeuvre);
            this.groupBox4.Controls.Add(this.lblCoutMainOeuvre);
            this.groupBox4.Location = new System.Drawing.Point(20, 360);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(710, 100);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Main-d\u0027\u0153uvre et amortissement";
            //
            // numCoutMainOeuvre
            //
            this.numCoutMainOeuvre.DecimalPlaces = 2;
            this.numCoutMainOeuvre.Location = new System.Drawing.Point(300, 30);
            this.numCoutMainOeuvre.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numCoutMainOeuvre.Name = "numCoutMainOeuvre";
            this.numCoutMainOeuvre.Size = new System.Drawing.Size(150, 25);
            this.numCoutMainOeuvre.TabIndex = 1;
            //
            // lblCoutMainOeuvre
            //
            this.lblCoutMainOeuvre.AutoSize = true;
            this.lblCoutMainOeuvre.Location = new System.Drawing.Point(20, 32);
            this.lblCoutMainOeuvre.Name = "lblCoutMainOeuvre";
            this.lblCoutMainOeuvre.Size = new System.Drawing.Size(200, 19);
            this.lblCoutMainOeuvre.TabIndex = 0;
            this.lblCoutMainOeuvre.Text = "Co\u00fbt main-d\u0027\u0153uvre (\u20ac/heure) :";
            //
            // numAmortissement
            //
            this.numAmortissement.DecimalPlaces = 2;
            this.numAmortissement.Location = new System.Drawing.Point(300, 65);
            this.numAmortissement.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numAmortissement.Name = "numAmortissement";
            this.numAmortissement.Size = new System.Drawing.Size(150, 25);
            this.numAmortissement.TabIndex = 3;
            //
            // lblAmortissement
            //
            this.lblAmortissement.AutoSize = true;
            this.lblAmortissement.Location = new System.Drawing.Point(20, 67);
            this.lblAmortissement.Name = "lblAmortissement";
            this.lblAmortissement.Size = new System.Drawing.Size(250, 19);
            this.lblAmortissement.TabIndex = 2;
            this.lblAmortissement.Text = "Amortissement machine (\u20ac/heure) :";
            //
            // groupBoxGithub
            //
            this.groupBoxGithub.Controls.Add(this.txtTokenGithub);
            this.groupBoxGithub.Controls.Add(this.lblTokenGithub);
            this.groupBoxGithub.Location = new System.Drawing.Point(20, 470);
            this.groupBoxGithub.Name = "groupBoxGithub";
            this.groupBoxGithub.Size = new System.Drawing.Size(710, 80);
            this.groupBoxGithub.TabIndex = 3;
            this.groupBoxGithub.TabStop = false;
            this.groupBoxGithub.Text = "Partage des donn\u00e9es de calibration";
            //
            // txtTokenGithub
            //
            this.txtTokenGithub.Location = new System.Drawing.Point(300, 35);
            this.txtTokenGithub.Name = "txtTokenGithub";
            this.txtTokenGithub.Size = new System.Drawing.Size(390, 25);
            this.txtTokenGithub.TabIndex = 1;
            this.txtTokenGithub.UseSystemPasswordChar = true;
            //
            // lblTokenGithub
            //
            this.lblTokenGithub.AutoSize = true;
            this.lblTokenGithub.Location = new System.Drawing.Point(20, 38);
            this.lblTokenGithub.Name = "lblTokenGithub";
            this.lblTokenGithub.Size = new System.Drawing.Size(240, 19);
            this.lblTokenGithub.TabIndex = 0;
            this.lblTokenGithub.Text = "Token GitHub (pour partager les donn\u00e9es):";
            //
            // groupBoxSlicer
            //
            this.groupBoxSlicer.Controls.Add(this.lblStatutSlicerParam);
            this.groupBoxSlicer.Controls.Add(this.btnParcourirSlicer);
            this.groupBoxSlicer.Controls.Add(this.txtCheminSlicer);
            this.groupBoxSlicer.Controls.Add(this.lblCheminSlicer);
            this.groupBoxSlicer.Location = new System.Drawing.Point(20, 560);
            this.groupBoxSlicer.Name = "groupBoxSlicer";
            this.groupBoxSlicer.Size = new System.Drawing.Size(710, 80);
            this.groupBoxSlicer.TabIndex = 5;
            this.groupBoxSlicer.TabStop = false;
            this.groupBoxSlicer.Text = "Slicer Bambu Studio";
            //
            // lblCheminSlicer
            //
            this.lblCheminSlicer.AutoSize = true;
            this.lblCheminSlicer.Location = new System.Drawing.Point(15, 35);
            this.lblCheminSlicer.Name = "lblCheminSlicer";
            this.lblCheminSlicer.Size = new System.Drawing.Size(130, 15);
            this.lblCheminSlicer.TabIndex = 0;
            this.lblCheminSlicer.Text = "Chemin de l\'ex\u00e9cutable :";
            //
            // txtCheminSlicer
            //
            this.txtCheminSlicer.Location = new System.Drawing.Point(170, 32);
            this.txtCheminSlicer.Name = "txtCheminSlicer";
            this.txtCheminSlicer.Size = new System.Drawing.Size(350, 25);
            this.txtCheminSlicer.TabIndex = 1;
            //
            // btnParcourirSlicer
            //
            this.btnParcourirSlicer.Location = new System.Drawing.Point(530, 30);
            this.btnParcourirSlicer.Name = "btnParcourirSlicer";
            this.btnParcourirSlicer.Size = new System.Drawing.Size(80, 28);
            this.btnParcourirSlicer.TabIndex = 2;
            this.btnParcourirSlicer.Text = "Parcourir...";
            this.btnParcourirSlicer.Click += new System.EventHandler(this.btnParcourirSlicer_Click);
            //
            // lblStatutSlicerParam
            //
            this.lblStatutSlicerParam.AutoSize = true;
            this.lblStatutSlicerParam.Location = new System.Drawing.Point(170, 60);
            this.lblStatutSlicerParam.Name = "lblStatutSlicerParam";
            this.lblStatutSlicerParam.Size = new System.Drawing.Size(100, 15);
            this.lblStatutSlicerParam.TabIndex = 3;
            this.lblStatutSlicerParam.Text = "";
            //
            // tabPageImprimantes
            //
            this.tabPageImprimantes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.tabPageImprimantes.Controls.Add(this.lblInfoImprimantes);
            this.tabPageImprimantes.Controls.Add(this.dgvImprimantesCustom);
            this.tabPageImprimantes.Controls.Add(this.btnAjouterImprimante);
            this.tabPageImprimantes.Controls.Add(this.btnSupprimerImprimante);
            this.tabPageImprimantes.Location = new System.Drawing.Point(4, 26);
            this.tabPageImprimantes.Name = "tabPageImprimantes";
            this.tabPageImprimantes.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageImprimantes.Size = new System.Drawing.Size(752, 650);
            this.tabPageImprimantes.TabIndex = 2;
            this.tabPageImprimantes.Text = "Imprimantes personnalisées";
            //
            // lblInfoImprimantes
            //
            this.lblInfoImprimantes.AutoSize = true;
            this.lblInfoImprimantes.Location = new System.Drawing.Point(20, 15);
            this.lblInfoImprimantes.Name = "lblInfoImprimantes";
            this.lblInfoImprimantes.Size = new System.Drawing.Size(400, 19);
            this.lblInfoImprimantes.Text = "Ajoutez des imprimantes non listées avec leurs caractéristiques :";
            //
            // dgvImprimantesCustom
            //
            this.dgvImprimantesCustom.Location = new System.Drawing.Point(20, 45);
            this.dgvImprimantesCustom.Name = "dgvImprimantesCustom";
            this.dgvImprimantesCustom.Size = new System.Drawing.Size(710, 400);
            this.dgvImprimantesCustom.AllowUserToAddRows = false;
            this.dgvImprimantesCustom.AllowUserToDeleteRows = false;
            this.dgvImprimantesCustom.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvImprimantesCustom.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvImprimantesCustom.TabIndex = 0;
            //
            // btnAjouterImprimante
            //
            this.btnAjouterImprimante.Location = new System.Drawing.Point(20, 460);
            this.btnAjouterImprimante.Name = "btnAjouterImprimante";
            this.btnAjouterImprimante.Size = new System.Drawing.Size(150, 35);
            this.btnAjouterImprimante.Text = "Ajouter";
            this.btnAjouterImprimante.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            //
            // btnSupprimerImprimante
            //
            this.btnSupprimerImprimante.Location = new System.Drawing.Point(180, 460);
            this.btnSupprimerImprimante.Name = "btnSupprimerImprimante";
            this.btnSupprimerImprimante.Size = new System.Drawing.Size(150, 35);
            this.btnSupprimerImprimante.Text = "Supprimer";
            this.btnSupprimerImprimante.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            //
            // btnEnregistrer
            //
            this.btnEnregistrer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnEnregistrer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnregistrer.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnEnregistrer.ForeColor = System.Drawing.Color.White;
            this.btnEnregistrer.Location = new System.Drawing.Point(530, 710);
            this.btnEnregistrer.Name = "btnEnregistrer";
            this.btnEnregistrer.Size = new System.Drawing.Size(120, 40);
            this.btnEnregistrer.TabIndex = 1;
            this.btnEnregistrer.Text = "Enregistrer";
            this.btnEnregistrer.UseVisualStyleBackColor = false;
            this.btnEnregistrer.Click += new System.EventHandler(this.btnEnregistrer_Click);
            // 
            // btnAnnuler
            // 
            this.btnAnnuler.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.btnAnnuler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAnnuler.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnAnnuler.ForeColor = System.Drawing.Color.White;
            this.btnAnnuler.Location = new System.Drawing.Point(660, 710);
            this.btnAnnuler.Name = "btnAnnuler";
            this.btnAnnuler.Size = new System.Drawing.Size(120, 40);
            this.btnAnnuler.TabIndex = 2;
            this.btnAnnuler.Text = "Annuler";
            this.btnAnnuler.UseVisualStyleBackColor = false;
            this.btnAnnuler.Click += new System.EventHandler(this.btnAnnuler_Click);
            // 
            // ParametresImpressionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 760);
            this.Controls.Add(this.btnAnnuler);
            this.Controls.Add(this.btnEnregistrer);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ParametresImpressionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Paramètres d\'impression";
            this.tabControl1.ResumeLayout(false);
            this.tabPageGeneral.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMarge)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTVA)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPourcentagePurge)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCoutElectricite)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBoxSlicer.ResumeLayout(false);
            this.groupBoxSlicer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCoutMainOeuvre)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAmortissement)).EndInit();
            this.tabPageBobines.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvBobines)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageGeneral;
        private System.Windows.Forms.TabPage tabPageBobines;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown numCoutElectricite;
        private System.Windows.Forms.Label lblCoutElectricite;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown numPourcentagePurge;
        private System.Windows.Forms.Label lblPourcentagePurge;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.NumericUpDown numMarge;
        private System.Windows.Forms.Label lblMarge;
        private System.Windows.Forms.NumericUpDown numTVA;
        private System.Windows.Forms.Label lblTVA;
        private System.Windows.Forms.DataGridView dgvBobines;
        private System.Windows.Forms.Button btnAjouterBobine;
        private System.Windows.Forms.Button btnSupprimerBobine;
        private System.Windows.Forms.GroupBox groupBoxGithub;
        private System.Windows.Forms.TextBox txtTokenGithub;
        private System.Windows.Forms.Label lblTokenGithub;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.NumericUpDown numCoutMainOeuvre;
        private System.Windows.Forms.Label lblCoutMainOeuvre;
        private System.Windows.Forms.NumericUpDown numAmortissement;
        private System.Windows.Forms.Label lblAmortissement;
        private System.Windows.Forms.GroupBox groupBoxSlicer;
        private System.Windows.Forms.TabPage tabPageImprimantes;
        private System.Windows.Forms.DataGridView dgvImprimantesCustom;
        private System.Windows.Forms.Button btnAjouterImprimante;
        private System.Windows.Forms.Button btnSupprimerImprimante;
        private System.Windows.Forms.Label lblInfoImprimantes;
        private System.Windows.Forms.TextBox txtCheminSlicer;
        private System.Windows.Forms.Label lblCheminSlicer;
        private System.Windows.Forms.Button btnParcourirSlicer;
        private System.Windows.Forms.Label lblStatutSlicerParam;
        private System.Windows.Forms.Button btnEnregistrer;
        private System.Windows.Forms.Button btnAnnuler;
    }
}