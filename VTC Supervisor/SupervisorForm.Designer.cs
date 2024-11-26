namespace VTC_Supervisor
{
    partial class SupervisorForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SupervisorForm));
            this.ipcamera1Checkbox = new System.Windows.Forms.CheckBox();
            this.ipcamera2Checkbox = new System.Windows.Forms.CheckBox();
            this.ipcamera3Checkbox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.checkProcessTimer = new System.Windows.Forms.Timer(this.components);
            this.autolaunchLabel = new System.Windows.Forms.Label();
            this.autolaunchCheckbox = new System.Windows.Forms.CheckBox();
            this.autolaunchTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // ipcamera1Checkbox
            // 
            this.ipcamera1Checkbox.AutoSize = true;
            this.ipcamera1Checkbox.Location = new System.Drawing.Point(92, 63);
            this.ipcamera1Checkbox.Name = "ipcamera1Checkbox";
            this.ipcamera1Checkbox.Size = new System.Drawing.Size(84, 17);
            this.ipcamera1Checkbox.TabIndex = 0;
            this.ipcamera1Checkbox.Text = "IP Camera 1";
            this.ipcamera1Checkbox.UseVisualStyleBackColor = true;
            this.ipcamera1Checkbox.CheckedChanged += new System.EventHandler(this.ipcamera1Checkbox_CheckedChanged);
            // 
            // ipcamera2Checkbox
            // 
            this.ipcamera2Checkbox.AutoSize = true;
            this.ipcamera2Checkbox.Location = new System.Drawing.Point(92, 87);
            this.ipcamera2Checkbox.Name = "ipcamera2Checkbox";
            this.ipcamera2Checkbox.Size = new System.Drawing.Size(84, 17);
            this.ipcamera2Checkbox.TabIndex = 1;
            this.ipcamera2Checkbox.Text = "IP Camera 2";
            this.ipcamera2Checkbox.UseVisualStyleBackColor = true;
            this.ipcamera2Checkbox.CheckedChanged += new System.EventHandler(this.ipcamera2Checkbox_CheckedChanged);
            // 
            // ipcamera3Checkbox
            // 
            this.ipcamera3Checkbox.AutoSize = true;
            this.ipcamera3Checkbox.Location = new System.Drawing.Point(92, 110);
            this.ipcamera3Checkbox.Name = "ipcamera3Checkbox";
            this.ipcamera3Checkbox.Size = new System.Drawing.Size(84, 17);
            this.ipcamera3Checkbox.TabIndex = 2;
            this.ipcamera3Checkbox.Text = "IP Camera 3";
            this.ipcamera3Checkbox.UseVisualStyleBackColor = true;
            this.ipcamera3Checkbox.CheckedChanged += new System.EventHandler(this.ipcamera3Checkbox_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(251, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Check the cameras to be launched and supervised.";
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(63, 177);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(153, 60);
            this.startButton.TabIndex = 4;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(63, 243);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(153, 60);
            this.stopButton.TabIndex = 5;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // checkProcessTimer
            // 
            this.checkProcessTimer.Interval = 10000;
            this.checkProcessTimer.Tick += new System.EventHandler(this.checkProcessTimer_Tick);
            // 
            // autolaunchLabel
            // 
            this.autolaunchLabel.AutoSize = true;
            this.autolaunchLabel.Location = new System.Drawing.Point(24, 342);
            this.autolaunchLabel.Name = "autolaunchLabel";
            this.autolaunchLabel.Size = new System.Drawing.Size(71, 13);
            this.autolaunchLabel.TabIndex = 6;
            this.autolaunchLabel.Text = "Launching in:";
            // 
            // autolaunchCheckbox
            // 
            this.autolaunchCheckbox.AutoSize = true;
            this.autolaunchCheckbox.Location = new System.Drawing.Point(92, 376);
            this.autolaunchCheckbox.Name = "autolaunchCheckbox";
            this.autolaunchCheckbox.Size = new System.Drawing.Size(83, 17);
            this.autolaunchCheckbox.TabIndex = 7;
            this.autolaunchCheckbox.Text = "Auto-launch";
            this.autolaunchCheckbox.UseVisualStyleBackColor = true;
            this.autolaunchCheckbox.CheckedChanged += new System.EventHandler(this.autolaunchCheckbox_CheckedChanged);
            // 
            // autolaunchTimer
            // 
            this.autolaunchTimer.Enabled = true;
            this.autolaunchTimer.Interval = 1000;
            this.autolaunchTimer.Tick += new System.EventHandler(this.autolaunchTimer_Tick);
            // 
            // SupervisorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(295, 491);
            this.Controls.Add(this.autolaunchCheckbox);
            this.Controls.Add(this.autolaunchLabel);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ipcamera3Checkbox);
            this.Controls.Add(this.ipcamera2Checkbox);
            this.Controls.Add(this.ipcamera1Checkbox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SupervisorForm";
            this.Text = "VTC Supervisor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox ipcamera1Checkbox;
        private System.Windows.Forms.CheckBox ipcamera2Checkbox;
        private System.Windows.Forms.CheckBox ipcamera3Checkbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Timer checkProcessTimer;
        private System.Windows.Forms.Label autolaunchLabel;
        private System.Windows.Forms.CheckBox autolaunchCheckbox;
        private System.Windows.Forms.Timer autolaunchTimer;
    }
}

