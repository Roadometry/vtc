using System;
using System.Windows.Forms;

namespace VTC
{
    public partial class SplashScreen : Form
    {
        private readonly DateTime _startTime;
        private readonly int _delayMs;
        public SplashScreen(int ms)
        {
            InitializeComponent();
            BringToFront();
            _startTime = DateTime.Now;
            _delayMs = ms;
        }

        private void killTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now - _startTime > TimeSpan.FromMilliseconds(_delayMs))
                Close();
            else
                BringToFront();   
        }
    }
}
