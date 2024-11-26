namespace VTC.RegionConfiguration
{
    partial class RegionConfigSelectorControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pbThumbnail = new System.Windows.Forms.PictureBox();
            this.lblName = new System.Windows.Forms.Label();
            this.lbRegionConfigs = new System.Windows.Forms.ListBox();
            this.btnCreateNewRegionConfig = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbThumbnail)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.Controls.Add(this.pbThumbnail, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblName, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lbRegionConfigs, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnCreateNewRegionConfig, 2, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(758, 225);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // pbThumbnail
            // 
            this.pbThumbnail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbThumbnail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbThumbnail.Location = new System.Drawing.Point(8, 8);
            this.pbThumbnail.Name = "pbThumbnail";
            this.tableLayoutPanel1.SetRowSpan(this.pbThumbnail, 2);
            this.pbThumbnail.Size = new System.Drawing.Size(243, 209);
            this.pbThumbnail.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbThumbnail.TabIndex = 0;
            this.pbThumbnail.TabStop = false;
            // 
            // lblName
            // 
            this.lblName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblName.Font = new System.Drawing.Font("Raleway", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.Location = new System.Drawing.Point(257, 5);
            this.lblName.Name = "lblName";
            this.tableLayoutPanel1.SetRowSpan(this.lblName, 2);
            this.lblName.Size = new System.Drawing.Size(243, 215);
            this.lblName.TabIndex = 1;
            this.lblName.Text = "label1";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbRegionConfigs
            // 
            this.lbRegionConfigs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbRegionConfigs.Font = new System.Drawing.Font("Raleway", 8.249999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbRegionConfigs.FormattingEnabled = true;
            this.lbRegionConfigs.Location = new System.Drawing.Point(506, 8);
            this.lbRegionConfigs.Name = "lbRegionConfigs";
            this.lbRegionConfigs.Size = new System.Drawing.Size(244, 166);
            this.lbRegionConfigs.TabIndex = 2;
            this.lbRegionConfigs.SelectedIndexChanged += new System.EventHandler(this.lbRegionConfigs_SelectedValueChanged);
            // 
            // btnCreateNewRegionConfig
            // 
            this.btnCreateNewRegionConfig.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnCreateNewRegionConfig.Font = new System.Drawing.Font("Raleway", 8.249999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCreateNewRegionConfig.Location = new System.Drawing.Point(544, 187);
            this.btnCreateNewRegionConfig.Name = "btnCreateNewRegionConfig";
            this.btnCreateNewRegionConfig.Size = new System.Drawing.Size(167, 23);
            this.btnCreateNewRegionConfig.TabIndex = 3;
            this.btnCreateNewRegionConfig.Text = "Manage Region Configs...";
            this.btnCreateNewRegionConfig.UseVisualStyleBackColor = true;
            this.btnCreateNewRegionConfig.Click += new System.EventHandler(this.btnCreateNewRegionConfig_Click);
            // 
            // RegionConfigSelectorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "RegionConfigSelectorControl";
            this.Size = new System.Drawing.Size(758, 225);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbThumbnail)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox pbThumbnail;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.ListBox lbRegionConfigs;
        private System.Windows.Forms.Button btnCreateNewRegionConfig;
    }
}
