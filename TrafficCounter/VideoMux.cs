using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using VTC.Common;
using VTC.UserConfiguration;

namespace VTC
{
    public partial class VideoMux : Form
    {
        private UserConfig _userConfig = new UserConfig();

        public VideoMux()
        {
            InitializeComponent();
            LoadUserConfig();
            AddLogo(_userConfig.Logopath);
        }

        public void AddLogo(string logoPath)
        { 
            if(System.IO.File.Exists(logoPath))
            {
                logoPictureBox.Image = Image.FromFile(logoPath);
            }
        }

        public void UpdateImage(Image<Bgr, byte> image)
        {
            videoPictureBox.Image = image.ToBitmap();
        }

        private void LoadUserConfig()
        {
            string UserConfigSavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                        "\\VTC\\userConfig.xml";
            IUserConfigDataAccessLayer _userConfigDataAccessLayer = new FileUserConfigDal(UserConfigSavePath);
            _userConfig = _userConfigDataAccessLayer.LoadUserConfig();
        }
    }
}