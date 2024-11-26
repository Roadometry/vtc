namespace VTC.RegionConfiguration
{
    partial class RegionEditor
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegionEditor));
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.tlpRegionConfigSelector = new System.Windows.Forms.TableLayoutPanel();
            this.lbRegionConfigurations = new System.Windows.Forms.ListBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.tlpRegionConfigEditor = new System.Windows.Forms.TableLayoutPanel();
            this.tbRegionConfigName = new System.Windows.Forms.TextBox();
            this.tlpPolygonToggles = new System.Windows.Forms.TableLayoutPanel();
            this.btnAddExamplePath = new System.Windows.Forms.Button();
            this.btnAddApproachExit = new System.Windows.Forms.Button();
            this.panelImage = new System.Windows.Forms.Panel();
            this.cbCaptureSource = new System.Windows.Forms.ComboBox();
            this.lblCaptureSource = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.importButton = new System.Windows.Forms.Button();
            this.exportButton = new System.Windows.Forms.Button();
            this.importRegionConfigFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.exportRegionConfigFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.tlpMain.SuspendLayout();
            this.tlpRegionConfigSelector.SuspendLayout();
            this.tlpRegionConfigEditor.SuspendLayout();
            this.tlpPolygonToggles.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Font = new System.Drawing.Font("Raleway", 8.249999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(1049, 519);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(87, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "Save";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Font = new System.Drawing.Font("Raleway", 8.249999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(1143, 519);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(87, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tlpMain
            // 
            this.tlpMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMain.ColumnCount = 3;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.tlpRegionConfigSelector, 0, 0);
            this.tlpMain.Controls.Add(this.tlpRegionConfigEditor, 1, 0);
            this.tlpMain.Controls.Add(this.panel1, 2, 0);
            this.tlpMain.Location = new System.Drawing.Point(14, 12);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(3, 3, 3, 50);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Size = new System.Drawing.Size(1217, 499);
            this.tlpMain.TabIndex = 5;
            // 
            // tlpRegionConfigSelector
            // 
            this.tlpRegionConfigSelector.ColumnCount = 1;
            this.tlpRegionConfigSelector.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRegionConfigSelector.Controls.Add(this.lbRegionConfigurations, 0, 0);
            this.tlpRegionConfigSelector.Controls.Add(this.btnAdd, 0, 1);
            this.tlpRegionConfigSelector.Controls.Add(this.btnDelete, 0, 2);
            this.tlpRegionConfigSelector.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRegionConfigSelector.Location = new System.Drawing.Point(3, 3);
            this.tlpRegionConfigSelector.Name = "tlpRegionConfigSelector";
            this.tlpRegionConfigSelector.RowCount = 3;
            this.tlpRegionConfigSelector.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRegionConfigSelector.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRegionConfigSelector.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRegionConfigSelector.Size = new System.Drawing.Size(267, 473);
            this.tlpRegionConfigSelector.TabIndex = 0;
            // 
            // lbRegionConfigurations
            // 
            this.lbRegionConfigurations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbRegionConfigurations.Font = new System.Drawing.Font("Raleway", 8.249999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbRegionConfigurations.FormattingEnabled = true;
            this.lbRegionConfigurations.Location = new System.Drawing.Point(3, 3);
            this.lbRegionConfigurations.Name = "lbRegionConfigurations";
            this.lbRegionConfigurations.Size = new System.Drawing.Size(261, 409);
            this.lbRegionConfigurations.TabIndex = 0;
            this.lbRegionConfigurations.SelectedValueChanged += new System.EventHandler(this.lbRegionConfigurations_SelectedValueChanged);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnAdd.Font = new System.Drawing.Font("Raleway", 8.249999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAdd.Location = new System.Drawing.Point(38, 418);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(23, 3, 23, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(191, 23);
            this.btnAdd.TabIndex = 6;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnDelete.Font = new System.Drawing.Font("Raleway", 8.249999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDelete.Location = new System.Drawing.Point(38, 447);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(23, 3, 23, 3);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(191, 23);
            this.btnDelete.TabIndex = 6;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // tlpRegionConfigEditor
            // 
            this.tlpRegionConfigEditor.ColumnCount = 2;
            this.tlpRegionConfigEditor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpRegionConfigEditor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tlpRegionConfigEditor.Controls.Add(this.tbRegionConfigName, 0, 0);
            this.tlpRegionConfigEditor.Controls.Add(this.tlpPolygonToggles, 0, 1);
            this.tlpRegionConfigEditor.Controls.Add(this.panelImage, 1, 1);
            this.tlpRegionConfigEditor.Controls.Add(this.cbCaptureSource, 1, 2);
            this.tlpRegionConfigEditor.Controls.Add(this.lblCaptureSource, 0, 2);
            this.tlpRegionConfigEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRegionConfigEditor.Location = new System.Drawing.Point(276, 3);
            this.tlpRegionConfigEditor.Name = "tlpRegionConfigEditor";
            this.tlpRegionConfigEditor.RowCount = 4;
            this.tlpRegionConfigEditor.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRegionConfigEditor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRegionConfigEditor.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRegionConfigEditor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpRegionConfigEditor.Size = new System.Drawing.Size(814, 473);
            this.tlpRegionConfigEditor.TabIndex = 4;
            // 
            // tbRegionConfigName
            // 
            this.tlpRegionConfigEditor.SetColumnSpan(this.tbRegionConfigName, 2);
            this.tbRegionConfigName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbRegionConfigName.Location = new System.Drawing.Point(3, 3);
            this.tbRegionConfigName.Name = "tbRegionConfigName";
            this.tbRegionConfigName.Size = new System.Drawing.Size(808, 20);
            this.tbRegionConfigName.TabIndex = 6;
            // 
            // tlpPolygonToggles
            // 
            this.tlpPolygonToggles.AutoSize = true;
            this.tlpPolygonToggles.ColumnCount = 2;
            this.tlpPolygonToggles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPolygonToggles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tlpPolygonToggles.Controls.Add(this.btnAddExamplePath, 0, 2);
            this.tlpPolygonToggles.Controls.Add(this.btnAddApproachExit, 0, 0);
            this.tlpPolygonToggles.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpPolygonToggles.Location = new System.Drawing.Point(3, 36);
            this.tlpPolygonToggles.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.tlpPolygonToggles.Name = "tlpPolygonToggles";
            this.tlpPolygonToggles.RowCount = 3;
            this.tlpPolygonToggles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPolygonToggles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpPolygonToggles.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpPolygonToggles.Size = new System.Drawing.Size(238, 102);
            this.tlpPolygonToggles.TabIndex = 1;
            // 
            // btnAddExamplePath
            // 
            this.btnAddExamplePath.BackColor = System.Drawing.SystemColors.Control;
            this.btnAddExamplePath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddExamplePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddExamplePath.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnAddExamplePath.Location = new System.Drawing.Point(17, 74);
            this.btnAddExamplePath.Margin = new System.Windows.Forms.Padding(17, 3, 17, 3);
            this.btnAddExamplePath.Name = "btnAddExamplePath";
            this.btnAddExamplePath.Size = new System.Drawing.Size(171, 25);
            this.btnAddExamplePath.TabIndex = 2;
            this.btnAddExamplePath.Text = "+ Path";
            this.btnAddExamplePath.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnAddExamplePath.UseVisualStyleBackColor = false;
            this.btnAddExamplePath.Click += new System.EventHandler(this.btnAddExamplePath_Click);
            // 
            // btnAddApproachExit
            // 
            this.btnAddApproachExit.BackColor = System.Drawing.SystemColors.Control;
            this.btnAddApproachExit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddApproachExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddApproachExit.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnAddApproachExit.Location = new System.Drawing.Point(17, 3);
            this.btnAddApproachExit.Margin = new System.Windows.Forms.Padding(17, 3, 17, 3);
            this.btnAddApproachExit.Name = "btnAddApproachExit";
            this.btnAddApproachExit.Size = new System.Drawing.Size(171, 25);
            this.btnAddApproachExit.TabIndex = 1;
            this.btnAddApproachExit.Text = "+ Region";
            this.btnAddApproachExit.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnAddApproachExit.UseVisualStyleBackColor = false;
            this.btnAddApproachExit.Click += new System.EventHandler(this.btnAddApproachExit_Click);
            // 
            // panelImage
            // 
            this.panelImage.AutoScroll = true;
            this.panelImage.AutoSize = true;
            this.panelImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelImage.Location = new System.Drawing.Point(247, 29);
            this.panelImage.Name = "panelImage";
            this.panelImage.Size = new System.Drawing.Size(564, 394);
            this.panelImage.TabIndex = 3;
            // 
            // cbCaptureSource
            // 
            this.cbCaptureSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbCaptureSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCaptureSource.Font = new System.Drawing.Font("Raleway", 8.249999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbCaptureSource.FormattingEnabled = true;
            this.cbCaptureSource.Location = new System.Drawing.Point(247, 429);
            this.cbCaptureSource.Name = "cbCaptureSource";
            this.cbCaptureSource.Size = new System.Drawing.Size(564, 21);
            this.cbCaptureSource.TabIndex = 7;
            this.cbCaptureSource.SelectedValueChanged += new System.EventHandler(this.cbCaptureSource_SelectedValueChanged);
            // 
            // lblCaptureSource
            // 
            this.lblCaptureSource.AutoSize = true;
            this.lblCaptureSource.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblCaptureSource.Font = new System.Drawing.Font("Raleway", 8.249999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCaptureSource.Location = new System.Drawing.Point(144, 426);
            this.lblCaptureSource.Name = "lblCaptureSource";
            this.lblCaptureSource.Size = new System.Drawing.Size(97, 27);
            this.lblCaptureSource.TabIndex = 8;
            this.lblCaptureSource.Text = "Capture Source:  ";
            this.lblCaptureSource.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.AutoScrollMinSize = new System.Drawing.Size(200, 0);
            this.panel1.AutoSize = true;
            this.panel1.Location = new System.Drawing.Point(1096, 3);
            this.panel1.MinimumSize = new System.Drawing.Size(117, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(117, 0);
            this.panel1.TabIndex = 5;
            // 
            // importButton
            // 
            this.importButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.importButton.Font = new System.Drawing.Font("Raleway", 8.249999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.importButton.Location = new System.Drawing.Point(14, 519);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(87, 23);
            this.importButton.TabIndex = 6;
            this.importButton.Text = "Import";
            this.importButton.UseVisualStyleBackColor = true;
            this.importButton.Click += new System.EventHandler(this.importButton_Click);
            // 
            // exportButton
            // 
            this.exportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exportButton.Font = new System.Drawing.Font("Raleway", 8.249999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exportButton.Location = new System.Drawing.Point(108, 519);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(87, 23);
            this.exportButton.TabIndex = 7;
            this.exportButton.Text = "Export";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // importRegionConfigFileDialog
            // 
            this.importRegionConfigFileDialog.FileName = "openFileDialog1";
            // 
            // RegionEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1245, 549);
            this.Controls.Add(this.exportButton);
            this.Controls.Add(this.importButton);
            this.Controls.Add(this.tlpMain);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Font = new System.Drawing.Font("Raleway", 8.249999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(225, 56);
            this.Name = "RegionEditor";
            this.Text = "RegionEditor";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpRegionConfigSelector.ResumeLayout(false);
            this.tlpRegionConfigEditor.ResumeLayout(false);
            this.tlpRegionConfigEditor.PerformLayout();
            this.tlpPolygonToggles.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.ListBox lbRegionConfigurations;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.TableLayoutPanel tlpRegionConfigSelector;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.TableLayoutPanel tlpRegionConfigEditor;
        private System.Windows.Forms.TableLayoutPanel tlpPolygonToggles;
        private System.Windows.Forms.Button btnAddApproachExit;
        private System.Windows.Forms.Panel panelImage;
        private System.Windows.Forms.TextBox tbRegionConfigName;
        private System.Windows.Forms.Label lblCaptureSource;
        private System.Windows.Forms.ComboBox cbCaptureSource;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button importButton;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.OpenFileDialog importRegionConfigFileDialog;
        private System.Windows.Forms.SaveFileDialog exportRegionConfigFileDialog;
        private System.Windows.Forms.Button btnAddExamplePath;
    }
}