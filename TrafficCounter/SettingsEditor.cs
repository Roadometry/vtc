using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VTC.RegionConfiguration;
using VTC.UserConfiguration;

namespace VTC
{
    public partial class SettingsEditor : Form
    {
        private static readonly string UserConfigSavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                                            "\\VTC\\userConfig.xml";

        private readonly IUserConfigDataAccessLayer _userConfigDataAccessLayer = new FileUserConfigDal(UserConfigSavePath);
        

        private VTC.Common.UserConfig _config;

        
        public SettingsEditor()
        {
            InitializeComponent();
            _config = _userConfigDataAccessLayer.LoadUserConfig();
            settingsPropertyGrid.SelectedObject = _config;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            _userConfigDataAccessLayer.SaveUserConfig(_config);
        }

        private void resetToDefaultButton_Click(object sender, EventArgs e)
        {
            _config = new VTC.Common.UserConfig();
            _userConfigDataAccessLayer.SaveUserConfig(_config);
        }
    }
}
