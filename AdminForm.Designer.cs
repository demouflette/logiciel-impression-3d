namespace logiciel_d_impression_3d
{
    partial class AdminForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitre          = new System.Windows.Forms.Label();
            this.lblStatut         = new System.Windows.Forms.Label();
            this.btnActualiser     = new System.Windows.Forms.Button();
            this.btnOuvrirAdmin    = new System.Windows.Forms.Button();
            this.tabControl1       = new System.Windows.Forms.TabControl();
            this.tabUtilisateurs   = new System.Windows.Forms.TabPage();
            this.lblNbUtilisateurs = new System.Windows.Forms.Label();
            this.dgvUtilisateurs   = new System.Windows.Forms.DataGridView();
            this.tabLicences       = new System.Windows.Forms.TabPage();
            this.lblNbLicences     = new System.Windows.Forms.Label();
            this.dgvLicences       = new System.Windows.Forms.DataGridView();
            this.tabControl1.SuspendLayout();
            this.tabUtilisateurs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.dgvUtilisateurs).BeginInit();
            this.tabLicences.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.dgvLicences).BeginInit();
            this.SuspendLayout();

            // ── Fenêtre ──────────────────────────────────────────────────
            this.Text = "Administration";
            this.Size = new System.Drawing.Size(960, 640);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(800, 500);

            // ── lblTitre ─────────────────────────────────────────────────
            this.lblTitre.Text = "🔧 Administration";
            this.lblTitre.Location = new System.Drawing.Point(16, 14);
            this.lblTitre.Size = new System.Drawing.Size(350, 32);
            this.lblTitre.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);

            // ── btnActualiser ─────────────────────────────────────────────
            this.btnActualiser.Text = "↻ Actualiser";
            this.btnActualiser.Size = new System.Drawing.Size(110, 32);
            this.btnActualiser.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnActualiser.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnActualiser.Click += new System.EventHandler(this.btnActualiser_Click);

            // ── btnOuvrirAdmin ─────────────────────────────────────────────
            this.btnOuvrirAdmin.Text = "🌐 Dashboard web";
            this.btnOuvrirAdmin.Size = new System.Drawing.Size(130, 32);
            this.btnOuvrirAdmin.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnOuvrirAdmin.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnOuvrirAdmin.Click += new System.EventHandler(this.btnOuvrirAdmin_Click);

            // ── lblStatut ─────────────────────────────────────────────────
            this.lblStatut.Text = "Chargement...";
            this.lblStatut.Location = new System.Drawing.Point(16, 52);
            this.lblStatut.Size = new System.Drawing.Size(300, 20);
            this.lblStatut.Font = new System.Drawing.Font("Segoe UI", 8.5F);

            // ── TabControl ────────────────────────────────────────────────
            this.tabControl1.Location = new System.Drawing.Point(16, 78);
            this.tabControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
                                    | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.tabControl1.Size = new System.Drawing.Size(910, 520);
            this.tabControl1.Controls.Add(this.tabUtilisateurs);
            this.tabControl1.Controls.Add(this.tabLicences);

            // ── Tab Utilisateurs ──────────────────────────────────────────
            this.tabUtilisateurs.Text = "👤 Utilisateurs";
            this.tabUtilisateurs.Padding = new System.Windows.Forms.Padding(8);
            this.tabUtilisateurs.Controls.Add(this.lblNbUtilisateurs);
            this.tabUtilisateurs.Controls.Add(this.dgvUtilisateurs);

            this.lblNbUtilisateurs.Location = new System.Drawing.Point(8, 8);
            this.lblNbUtilisateurs.Size = new System.Drawing.Size(300, 20);
            this.lblNbUtilisateurs.Font = new System.Drawing.Font("Segoe UI", 8.5F);

            // DataGridView Utilisateurs
            ConfigurerGrille(this.dgvUtilisateurs);
            this.dgvUtilisateurs.Location = new System.Drawing.Point(8, 32);
            this.dgvUtilisateurs.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
                                        | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.dgvUtilisateurs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                ColTexte("Nom d'utilisateur", 160),
                ColTexte("Email", 220),
                ColTexte("Rôle", 80),
                ColTexte("Inscrit le", 100),
                ColTexte("Email vérifié", 110),
            });
            this.dgvUtilisateurs.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvUtilisateurs_CellDoubleClick);

            // ── Tab Licences ──────────────────────────────────────────────
            this.tabLicences.Text = "🔑 Licences";
            this.tabLicences.Padding = new System.Windows.Forms.Padding(8);
            this.tabLicences.Controls.Add(this.lblNbLicences);
            this.tabLicences.Controls.Add(this.dgvLicences);

            this.lblNbLicences.Location = new System.Drawing.Point(8, 8);
            this.lblNbLicences.Size = new System.Drawing.Size(300, 20);
            this.lblNbLicences.Font = new System.Drawing.Font("Segoe UI", 8.5F);

            // DataGridView Licences
            ConfigurerGrille(this.dgvLicences);
            this.dgvLicences.Location = new System.Drawing.Point(8, 32);
            this.dgvLicences.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
                                    | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.dgvLicences.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                ColTexte("Clé", 170),
                ColTexte("Email", 200),
                ColTexte("Type", 80),
                ColTexte("Expiration", 95),
                ColTexte("Machine", 130),
                ColTexte("Statut", 80),
            });
            this.dgvLicences.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvLicences_CellDoubleClick);

            // ── Positionnement boutons (après calcul taille) ──────────────
            this.Load += (s, e) => {
                int r = this.ClientSize.Width - 16;
                btnOuvrirAdmin.Location = new System.Drawing.Point(r - 130, 14);
                btnActualiser.Location  = new System.Drawing.Point(r - 130 - 110 - 8, 14);
            };
            this.Resize += (s, e) => {
                int r = this.ClientSize.Width - 16;
                btnOuvrirAdmin.Location = new System.Drawing.Point(r - 130, 14);
                btnActualiser.Location  = new System.Drawing.Point(r - 130 - 110 - 8, 14);
            };

            // ── Ajout contrôles ───────────────────────────────────────────
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblTitre, this.btnActualiser, this.btnOuvrirAdmin,
                this.lblStatut, this.tabControl1
            });

            this.tabControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.dgvUtilisateurs).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.dgvLicences).EndInit();
            this.ResumeLayout(false);
        }

        private static void ConfigurerGrille(System.Windows.Forms.DataGridView dgv)
        {
            dgv.Size = new System.Drawing.Size(870, 450);
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false;
            dgv.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dgv.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            dgv.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
            dgv.RowTemplate.Height = 28;
        }

        private static System.Windows.Forms.DataGridViewTextBoxColumn ColTexte(string header, int width)
        {
            return new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                HeaderText = header,
                MinimumWidth = width,
                FillWeight = width,
                ReadOnly = true,
                SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
            };
        }

        private System.Windows.Forms.Label lblTitre;
        private System.Windows.Forms.Label lblStatut;
        private System.Windows.Forms.Button btnActualiser;
        private System.Windows.Forms.Button btnOuvrirAdmin;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabUtilisateurs;
        private System.Windows.Forms.Label lblNbUtilisateurs;
        private System.Windows.Forms.DataGridView dgvUtilisateurs;
        private System.Windows.Forms.TabPage tabLicences;
        private System.Windows.Forms.Label lblNbLicences;
        private System.Windows.Forms.DataGridView dgvLicences;
    }
}
