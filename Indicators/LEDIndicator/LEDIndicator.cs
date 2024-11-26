using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Indicators.LEDIndicator
{
    public class LEDIndicator : UserControl
    {
        private bool _on = false;
        private PictureBox pictureBox1;

        [Description("LED Status"), Category("Data"), Browsable(true)]
        public bool On
        {
            get { return _on; }
            set
            {
                _on = value;
                UpdateImage();
            }
        }

        public enum LEDColorOptions
        {
            Green,
            Red
        };

        private LEDColorOptions _ledColor = LEDColorOptions.Green;
        [Description("LED Color"), Category("Data"), Browsable(true)]
        public LEDColorOptions LEDColor
        {
            get { return _ledColor; }
            set
            {
                _ledColor = value;
                UpdateImage();
            }
        }

        public LEDIndicator()
        {
            InitializeComponent();
        }

        private void UpdateImage()
        {
            pictureBox1.Size = new System.Drawing.Size(this.Width, this.Height);

            if (_on)
            {
                switch (_ledColor)
                {
                    case LEDColorOptions.Green:
                        pictureBox1.Image = Properties.Resources.greenledon;
                        break;
                    case LEDColorOptions.Red:
                        pictureBox1.Image = Properties.Resources.redledon;
                        break;
                }
            }
            else
            {
                switch (_ledColor)
                {
                    case LEDColorOptions.Green:
                        pictureBox1.Image = Properties.Resources.greenledoff;
                        break;
                    case LEDColorOptions.Red:
                        pictureBox1.Image = Properties.Resources.redledoff;
                        break;
                }
            }
        }

        private void InitializeComponent()
        {
                this.pictureBox1 = new System.Windows.Forms.PictureBox();
                ((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).BeginInit();
                this.SuspendLayout();
                // 
                // pictureBox1
                // 
                this.pictureBox1.Location = new System.Drawing.Point(0, 0);
                this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
                this.pictureBox1.Name = "pictureBox1";
                this.pictureBox1.Size = new System.Drawing.Size(20, 20);
                this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
                this.pictureBox1.TabIndex = 0;
                this.pictureBox1.TabStop = false;
                this.pictureBox1.Image = Properties.Resources.greenledon;
                // 
                // LEDIndicator
                // 
                this.Controls.Add(this.pictureBox1);
                this.Margin = new System.Windows.Forms.Padding(0);
                this.Name = "LEDIndicator";
                this.Size = new System.Drawing.Size(20, 20);
                ((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).EndInit();
                this.ResumeLayout(false);
        }
    }
}
