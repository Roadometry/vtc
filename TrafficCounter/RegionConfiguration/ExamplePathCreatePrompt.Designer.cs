namespace VTC.RegionConfiguration
{
    partial class ExamplePathCreatePrompt
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExamplePathCreatePrompt));
            this.approachTextbox = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblMessage = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.exitTextbox = new System.Windows.Forms.TextBox();
            this.ignoredCheckbox = new System.Windows.Forms.CheckBox();
            this.pedestrianCheckbox = new System.Windows.Forms.CheckBox();
            this.turnComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // approachTextbox
            // 
            this.approachTextbox.Location = new System.Drawing.Point(20, 42);
            this.approachTextbox.Name = "approachTextbox";
            this.approachTextbox.Size = new System.Drawing.Size(300, 20);
            this.approachTextbox.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(233, 204);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(87, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Enabled = false;
            this.btnOK.Location = new System.Drawing.Point(139, 204);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(87, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Font = new System.Drawing.Font("Raleway", 8.249999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage.Location = new System.Drawing.Point(14, 26);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(57, 13);
            this.lblMessage.TabIndex = 2;
            this.lblMessage.Text = "Approach";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Raleway", 8.249999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(14, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Exit";
            // 
            // exitTextbox
            // 
            this.exitTextbox.Location = new System.Drawing.Point(20, 91);
            this.exitTextbox.Name = "exitTextbox";
            this.exitTextbox.Size = new System.Drawing.Size(300, 20);
            this.exitTextbox.TabIndex = 3;
            // 
            // ignoredCheckbox
            // 
            this.ignoredCheckbox.AutoSize = true;
            this.ignoredCheckbox.Location = new System.Drawing.Point(20, 127);
            this.ignoredCheckbox.Name = "ignoredCheckbox";
            this.ignoredCheckbox.Size = new System.Drawing.Size(66, 17);
            this.ignoredCheckbox.TabIndex = 5;
            this.ignoredCheckbox.Text = "Ignored";
            this.ignoredCheckbox.UseVisualStyleBackColor = true;
            // 
            // pedestrianCheckbox
            // 
            this.pedestrianCheckbox.AutoSize = true;
            this.pedestrianCheckbox.Location = new System.Drawing.Point(20, 150);
            this.pedestrianCheckbox.Name = "pedestrianCheckbox";
            this.pedestrianCheckbox.Size = new System.Drawing.Size(106, 17);
            this.pedestrianCheckbox.TabIndex = 6;
            this.pedestrianCheckbox.Text = "Pedestrian-only";
            this.pedestrianCheckbox.UseVisualStyleBackColor = true;
            // 
            // turnComboBox
            // 
            this.turnComboBox.FormattingEnabled = true;
            this.turnComboBox.Items.AddRange(new object[] {
            "Straight",
            "Left",
            "Right",
            "UTurn",
            "Crossing",
            "Unknown"});
            this.turnComboBox.Location = new System.Drawing.Point(199, 125);
            this.turnComboBox.Name = "turnComboBox";
            this.turnComboBox.Size = new System.Drawing.Size(121, 21);
            this.turnComboBox.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(264, 149);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Turn type";
            // 
            // ExamplePathCreatePrompt
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(335, 239);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.turnComboBox);
            this.Controls.Add(this.pedestrianCheckbox);
            this.Controls.Add(this.ignoredCheckbox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.exitTextbox);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.approachTextbox);
            this.Font = new System.Drawing.Font("Raleway", 8.249999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExamplePathCreatePrompt";
            this.Text = "Create Example Path";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox approachTextbox;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox exitTextbox;
        private System.Windows.Forms.CheckBox ignoredCheckbox;
        private System.Windows.Forms.CheckBox pedestrianCheckbox;
        private System.Windows.Forms.ComboBox turnComboBox;
        private System.Windows.Forms.Label label2;
    }
}