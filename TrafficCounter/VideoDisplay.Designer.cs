namespace VTC
{
    partial class VideoDisplay
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VideoDisplay));
            this.xyLabel = new System.Windows.Forms.Label();
            this.imageBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox)).BeginInit();
            this.SuspendLayout();
            // 
            // xyLabel
            // 
            this.xyLabel.AutoSize = true;
            this.xyLabel.BackColor = System.Drawing.Color.Transparent;
            this.xyLabel.Location = new System.Drawing.Point(14, 9);
            this.xyLabel.Name = "xyLabel";
            this.xyLabel.Size = new System.Drawing.Size(28, 13);
            this.xyLabel.TabIndex = 4;
            this.xyLabel.Text = "(X,Y)";
            // 
            // imageBox
            // 
            this.imageBox.Location = new System.Drawing.Point(0, 0);
            this.imageBox.Name = "imageBox";
            this.imageBox.Size = new System.Drawing.Size(752, 453);
            this.imageBox.TabIndex = 5;
            this.imageBox.TabStop = false;
            // 
            // VideoDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(751, 452);
            this.Controls.Add(this.xyLabel);
            this.Controls.Add(this.imageBox);
            this.Cursor = System.Windows.Forms.Cursors.Cross;
            this.Font = new System.Drawing.Font("Raleway", 8.249999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "VideoDisplay";
            this.Text = "VideoDisplay";
            ((System.ComponentModel.ISupportInitialize)(this.imageBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label xyLabel;
        private System.Windows.Forms.PictureBox imageBox;
    }
}