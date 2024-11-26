using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;

namespace VTC
{
    public sealed partial class VideoDisplay : Form
    {

        public string LayerName;
		public PictureBox ImageBox => imageBox;

        public void Update(Image<Bgr, byte> frame)
        {
            imageBox.Image = frame.ToBitmap();
        }

        public void Update(Image<Bgr, float> frame)
        {
            imageBox.Image = frame.ToBitmap();
        }
           

        public VideoDisplay(string name, Point initialPosition)
        {
            InitializeComponent();
			LayerName = name;
            Text = "VideoDisplay: " + name;
            StartPosition = FormStartPosition.Manual;
            Location = initialPosition;
        }

        private void imageBox_MouseMove(object sender, MouseEventArgs e)
        {
            var relativePoint = PointToClient(Cursor.Position);
            xyLabel.Text = relativePoint.X + "," + relativePoint.Y;
        }
    }
}
