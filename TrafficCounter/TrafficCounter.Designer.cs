using System.Windows.Forms;
using VTC.UI;

namespace VTC
{
   partial class TrafficCounter
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TrafficCounter));
            this.timeActiveTextBox = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.tbVistaStats = new System.Windows.Forms.TextBox();
            this.CameraComboBox = new System.Windows.Forms.ComboBox();
            this.btnConfigureRegions = new System.Windows.Forms.Button();
            this.heartbeatTimer = new System.Windows.Forms.Timer(this.components);
            this.SelectVideosButton = new System.Windows.Forms.Button();
            this.selectVideoFilesDialog = new System.Windows.Forms.OpenFileDialog();
            this.infoBox = new System.Windows.Forms.TextBox();
            this.generateReportButton = new System.Windows.Forms.Button();
            this.trackedObjectsTextbox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.loggingIndicator = new Indicators.LEDIndicator.LEDIndicator();
            this.framegrabbingIndicator = new Indicators.LEDIndicator.LEDIndicator();
            this.processingIndicator = new Indicators.LEDIndicator.LEDIndicator();
            this.sequencingIndicator = new Indicators.LEDIndicator.LEDIndicator();
            this.configurationIndicator = new Indicators.LEDIndicator.LEDIndicator();
            this.remainingTimeBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.frameTextbox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.fpsTextbox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.debugTextbox = new System.Windows.Forms.TextBox();
            this.userSettingsButton = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.aboutButton = new System.Windows.Forms.Button();
            this.resetCountsButton = new System.Windows.Forms.Button();
            this.resetDatabaseButton = new System.Windows.Forms.Button();
            this.loadSplitVideoButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // timeActiveTextBox
            // 
            this.timeActiveTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.timeActiveTextBox.Location = new System.Drawing.Point(537, 357);
            this.timeActiveTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.timeActiveTextBox.Name = "timeActiveTextBox";
            this.timeActiveTextBox.ReadOnly = true;
            this.timeActiveTextBox.Size = new System.Drawing.Size(142, 28);
            this.timeActiveTextBox.TabIndex = 8;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.label11.Location = new System.Drawing.Point(424, 362);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(102, 22);
            this.label11.TabIndex = 64;
            this.label11.Text = "Time active";
            // 
            // tbVistaStats
            // 
            this.tbVistaStats.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tbVistaStats.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbVistaStats.Location = new System.Drawing.Point(18, 627);
            this.tbVistaStats.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbVistaStats.Multiline = true;
            this.tbVistaStats.Name = "tbVistaStats";
            this.tbVistaStats.ReadOnly = true;
            this.tbVistaStats.Size = new System.Drawing.Size(661, 460);
            this.tbVistaStats.TabIndex = 6;
            // 
            // CameraComboBox
            // 
            this.CameraComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CameraComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.CameraComboBox.FormattingEnabled = true;
            this.CameraComboBox.Location = new System.Drawing.Point(18, 33);
            this.CameraComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.CameraComboBox.Name = "CameraComboBox";
            this.CameraComboBox.Size = new System.Drawing.Size(358, 30);
            this.CameraComboBox.TabIndex = 1;
            this.CameraComboBox.SelectedIndexChanged += new System.EventHandler(this.CameraComboBox_SelectedIndexChanged);
            // 
            // btnConfigureRegions
            // 
            this.btnConfigureRegions.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnConfigureRegions.Location = new System.Drawing.Point(18, 279);
            this.btnConfigureRegions.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnConfigureRegions.Name = "btnConfigureRegions";
            this.btnConfigureRegions.Size = new System.Drawing.Size(172, 36);
            this.btnConfigureRegions.TabIndex = 4;
            this.btnConfigureRegions.Text = "Configure regions";
            this.btnConfigureRegions.UseVisualStyleBackColor = true;
            this.btnConfigureRegions.Click += new System.EventHandler(this.btnConfigureRegions_Click);
            // 
            // heartbeatTimer
            // 
            this.heartbeatTimer.Enabled = true;
            this.heartbeatTimer.Interval = 5000;
            this.heartbeatTimer.Tick += new System.EventHandler(this.heartbeatTimer_Tick);
            // 
            // SelectVideosButton
            // 
            this.SelectVideosButton.BackColor = System.Drawing.Color.DodgerBlue;
            this.SelectVideosButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SelectVideosButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SelectVideosButton.ForeColor = System.Drawing.Color.White;
            this.SelectVideosButton.Location = new System.Drawing.Point(18, 76);
            this.SelectVideosButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.SelectVideosButton.Name = "SelectVideosButton";
            this.SelectVideosButton.Size = new System.Drawing.Size(360, 66);
            this.SelectVideosButton.TabIndex = 2;
            this.SelectVideosButton.Text = "Load video";
            this.SelectVideosButton.UseVisualStyleBackColor = false;
            this.SelectVideosButton.Click += new System.EventHandler(this.SelectVideosButton_Click);
            // 
            // selectVideoFilesDialog
            // 
            this.selectVideoFilesDialog.Filter = "Video files|*.mp4;*.avi;*.wmv;*.3gp;*.asf;*.h264;*.mkv;*.ts;*.MOV;*.AVI;*.3GP;*.H" +
    "264;*.TS;*.ASF;*.MP4;*.WMV;*.m4v;|All files|*.*";
            this.selectVideoFilesDialog.Multiselect = true;
            this.selectVideoFilesDialog.Title = "Select videos to process";
            // 
            // infoBox
            // 
            this.infoBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.infoBox.Location = new System.Drawing.Point(18, 435);
            this.infoBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.infoBox.Multiline = true;
            this.infoBox.Name = "infoBox";
            this.infoBox.ReadOnly = true;
            this.infoBox.Size = new System.Drawing.Size(352, 85);
            this.infoBox.TabIndex = 7;
            // 
            // generateReportButton
            // 
            this.generateReportButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.generateReportButton.Location = new System.Drawing.Point(200, 279);
            this.generateReportButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.generateReportButton.Name = "generateReportButton";
            this.generateReportButton.Size = new System.Drawing.Size(172, 36);
            this.generateReportButton.TabIndex = 65;
            this.generateReportButton.Text = "Generate Report";
            this.generateReportButton.UseVisualStyleBackColor = true;
            this.generateReportButton.Click += new System.EventHandler(this.generateReportButton_Click);
            // 
            // trackedObjectsTextbox
            // 
            this.trackedObjectsTextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.trackedObjectsTextbox.Location = new System.Drawing.Point(18, 369);
            this.trackedObjectsTextbox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.trackedObjectsTextbox.Multiline = true;
            this.trackedObjectsTextbox.Name = "trackedObjectsTextbox";
            this.trackedObjectsTextbox.ReadOnly = true;
            this.trackedObjectsTextbox.Size = new System.Drawing.Size(352, 55);
            this.trackedObjectsTextbox.TabIndex = 67;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.label1.Location = new System.Drawing.Point(507, 48);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 20);
            this.label1.TabIndex = 74;
            this.label1.Text = "Logging";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.label2.Location = new System.Drawing.Point(507, 87);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 20);
            this.label2.TabIndex = 75;
            this.label2.Text = "Frame Grabbing";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.label3.Location = new System.Drawing.Point(507, 126);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 20);
            this.label3.TabIndex = 76;
            this.label3.Text = "Processing";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.label4.Location = new System.Drawing.Point(507, 165);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 20);
            this.label4.TabIndex = 77;
            this.label4.Text = "Sequencing";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.label5.Location = new System.Drawing.Point(507, 204);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(104, 20);
            this.label5.TabIndex = 78;
            this.label5.Text = "Configuration";
            // 
            // loggingIndicator
            // 
            this.loggingIndicator.LEDColor = Indicators.LEDIndicator.LEDIndicator.LEDColorOptions.Green;
            this.loggingIndicator.Location = new System.Drawing.Point(472, 40);
            this.loggingIndicator.Margin = new System.Windows.Forms.Padding(0);
            this.loggingIndicator.Name = "loggingIndicator";
            this.loggingIndicator.On = false;
            this.loggingIndicator.Size = new System.Drawing.Size(30, 30);
            this.loggingIndicator.TabIndex = 80;
            // 
            // framegrabbingIndicator
            // 
            this.framegrabbingIndicator.LEDColor = Indicators.LEDIndicator.LEDIndicator.LEDColorOptions.Green;
            this.framegrabbingIndicator.Location = new System.Drawing.Point(472, 76);
            this.framegrabbingIndicator.Margin = new System.Windows.Forms.Padding(0);
            this.framegrabbingIndicator.Name = "framegrabbingIndicator";
            this.framegrabbingIndicator.On = false;
            this.framegrabbingIndicator.Size = new System.Drawing.Size(30, 30);
            this.framegrabbingIndicator.TabIndex = 81;
            // 
            // processingIndicator
            // 
            this.processingIndicator.LEDColor = Indicators.LEDIndicator.LEDIndicator.LEDColorOptions.Green;
            this.processingIndicator.Location = new System.Drawing.Point(472, 116);
            this.processingIndicator.Margin = new System.Windows.Forms.Padding(0);
            this.processingIndicator.Name = "processingIndicator";
            this.processingIndicator.On = false;
            this.processingIndicator.Size = new System.Drawing.Size(30, 30);
            this.processingIndicator.TabIndex = 82;
            // 
            // sequencingIndicator
            // 
            this.sequencingIndicator.LEDColor = Indicators.LEDIndicator.LEDIndicator.LEDColorOptions.Green;
            this.sequencingIndicator.Location = new System.Drawing.Point(472, 154);
            this.sequencingIndicator.Margin = new System.Windows.Forms.Padding(0);
            this.sequencingIndicator.Name = "sequencingIndicator";
            this.sequencingIndicator.On = false;
            this.sequencingIndicator.Size = new System.Drawing.Size(30, 30);
            this.sequencingIndicator.TabIndex = 83;
            // 
            // configurationIndicator
            // 
            this.configurationIndicator.LEDColor = Indicators.LEDIndicator.LEDIndicator.LEDColorOptions.Green;
            this.configurationIndicator.Location = new System.Drawing.Point(472, 194);
            this.configurationIndicator.Margin = new System.Windows.Forms.Padding(0);
            this.configurationIndicator.Name = "configurationIndicator";
            this.configurationIndicator.On = false;
            this.configurationIndicator.Size = new System.Drawing.Size(30, 30);
            this.configurationIndicator.TabIndex = 84;
            // 
            // remainingTimeBox
            // 
            this.remainingTimeBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.remainingTimeBox.Location = new System.Drawing.Point(537, 316);
            this.remainingTimeBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.remainingTimeBox.Name = "remainingTimeBox";
            this.remainingTimeBox.ReadOnly = true;
            this.remainingTimeBox.Size = new System.Drawing.Size(142, 28);
            this.remainingTimeBox.TabIndex = 85;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.label6.Location = new System.Drawing.Point(380, 321);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(140, 22);
            this.label6.TabIndex = 86;
            this.label6.Text = "Remaining Time";
            // 
            // frameTextbox
            // 
            this.frameTextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.frameTextbox.Location = new System.Drawing.Point(537, 276);
            this.frameTextbox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.frameTextbox.Name = "frameTextbox";
            this.frameTextbox.ReadOnly = true;
            this.frameTextbox.Size = new System.Drawing.Size(142, 28);
            this.frameTextbox.TabIndex = 87;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.label7.Location = new System.Drawing.Point(464, 280);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(61, 22);
            this.label7.TabIndex = 88;
            this.label7.Text = "Frame";
            // 
            // fpsTextbox
            // 
            this.fpsTextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.fpsTextbox.Location = new System.Drawing.Point(537, 398);
            this.fpsTextbox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.fpsTextbox.Name = "fpsTextbox";
            this.fpsTextbox.ReadOnly = true;
            this.fpsTextbox.Size = new System.Drawing.Size(142, 28);
            this.fpsTextbox.TabIndex = 89;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.label8.Location = new System.Drawing.Point(483, 402);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(45, 22);
            this.label8.TabIndex = 90;
            this.label8.Text = "FPS";
            // 
            // debugTextbox
            // 
            this.debugTextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.debugTextbox.Location = new System.Drawing.Point(18, 531);
            this.debugTextbox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.debugTextbox.Multiline = true;
            this.debugTextbox.Name = "debugTextbox";
            this.debugTextbox.ReadOnly = true;
            this.debugTextbox.Size = new System.Drawing.Size(661, 85);
            this.debugTextbox.TabIndex = 91;
            // 
            // userSettingsButton
            // 
            this.userSettingsButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.userSettingsButton.Location = new System.Drawing.Point(18, 234);
            this.userSettingsButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.userSettingsButton.Name = "userSettingsButton";
            this.userSettingsButton.Size = new System.Drawing.Size(172, 36);
            this.userSettingsButton.TabIndex = 92;
            this.userSettingsButton.Text = "User settings";
            this.userSettingsButton.UseVisualStyleBackColor = true;
            this.userSettingsButton.Click += new System.EventHandler(this.userSettingsButton_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.label9.Location = new System.Drawing.Point(255, 9);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(114, 20);
            this.label9.TabIndex = 93;
            this.label9.Text = "Select Camera";
            // 
            // aboutButton
            // 
            this.aboutButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.aboutButton.Location = new System.Drawing.Point(200, 234);
            this.aboutButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.aboutButton.Name = "aboutButton";
            this.aboutButton.Size = new System.Drawing.Size(172, 36);
            this.aboutButton.TabIndex = 94;
            this.aboutButton.Text = "About";
            this.aboutButton.UseVisualStyleBackColor = true;
            this.aboutButton.Click += new System.EventHandler(this.aboutButton_Click);
            // 
            // resetCountsButton
            // 
            this.resetCountsButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.resetCountsButton.Location = new System.Drawing.Point(18, 324);
            this.resetCountsButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.resetCountsButton.Name = "resetCountsButton";
            this.resetCountsButton.Size = new System.Drawing.Size(172, 36);
            this.resetCountsButton.TabIndex = 95;
            this.resetCountsButton.Text = "Reset counts";
            this.resetCountsButton.UseVisualStyleBackColor = true;
            this.resetCountsButton.Click += new System.EventHandler(this.ResetCountsButton_Click);
            // 
            // resetDatabaseButton
            // 
            this.resetDatabaseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.resetDatabaseButton.Location = new System.Drawing.Point(200, 324);
            this.resetDatabaseButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.resetDatabaseButton.Name = "resetDatabaseButton";
            this.resetDatabaseButton.Size = new System.Drawing.Size(172, 36);
            this.resetDatabaseButton.TabIndex = 96;
            this.resetDatabaseButton.Text = "Reset Database";
            this.resetDatabaseButton.UseVisualStyleBackColor = true;
            this.resetDatabaseButton.Click += new System.EventHandler(this.resetDatabaseButton_Click);
            // 
            // loadSplitVideoButton
            // 
            this.loadSplitVideoButton.BackColor = System.Drawing.Color.DodgerBlue;
            this.loadSplitVideoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.loadSplitVideoButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loadSplitVideoButton.ForeColor = System.Drawing.Color.White;
            this.loadSplitVideoButton.Location = new System.Drawing.Point(18, 152);
            this.loadSplitVideoButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.loadSplitVideoButton.Name = "loadSplitVideoButton";
            this.loadSplitVideoButton.Size = new System.Drawing.Size(360, 66);
            this.loadSplitVideoButton.TabIndex = 97;
            this.loadSplitVideoButton.Text = "Load split video";
            this.loadSplitVideoButton.UseVisualStyleBackColor = false;
            this.loadSplitVideoButton.Click += new System.EventHandler(this.loadSplitVideoButton_Click);
            // 
            // TrafficCounter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(702, 1107);
            this.Controls.Add(this.loadSplitVideoButton);
            this.Controls.Add(this.resetDatabaseButton);
            this.Controls.Add(this.resetCountsButton);
            this.Controls.Add(this.aboutButton);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.userSettingsButton);
            this.Controls.Add(this.debugTextbox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.fpsTextbox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.frameTextbox);
            this.Controls.Add(this.remainingTimeBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.configurationIndicator);
            this.Controls.Add(this.sequencingIndicator);
            this.Controls.Add(this.processingIndicator);
            this.Controls.Add(this.framegrabbingIndicator);
            this.Controls.Add(this.loggingIndicator);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.trackedObjectsTextbox);
            this.Controls.Add(this.generateReportButton);
            this.Controls.Add(this.infoBox);
            this.Controls.Add(this.SelectVideosButton);
            this.Controls.Add(this.timeActiveTextBox);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.tbVistaStats);
            this.Controls.Add(this.CameraComboBox);
            this.Controls.Add(this.btnConfigureRegions);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(426, 786);
            this.Name = "TrafficCounter";
            this.Text = "VTC";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TrafficCounter_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

      }

      #endregion
      private TextBox timeActiveTextBox;
      private Label label11;
      private TextBox tbVistaStats;
      private ComboBox CameraComboBox;
      private Button btnConfigureRegions;
      private Timer heartbeatTimer;
        private Button SelectVideosButton;
        private OpenFileDialog selectVideoFilesDialog;
        private TextBox infoBox;
        private Button generateReportButton;
        private TextBox trackedObjectsTextbox;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Indicators.LEDIndicator.LEDIndicator loggingIndicator;
        private Indicators.LEDIndicator.LEDIndicator framegrabbingIndicator;
        private Indicators.LEDIndicator.LEDIndicator processingIndicator;
        private Indicators.LEDIndicator.LEDIndicator sequencingIndicator;
        private Indicators.LEDIndicator.LEDIndicator configurationIndicator;
        private TextBox remainingTimeBox;
        private Label label6;
        private TextBox frameTextbox;
        private Label label7;
        private TextBox fpsTextbox;
        private Label label8;
        private TextBox debugTextbox;
        private Button userSettingsButton;
        private Label label9;
        private Button aboutButton;
        private Button resetCountsButton;
        private Button resetDatabaseButton;
        private Button loadSplitVideoButton;
    }
}