using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VTC
{
    public partial class AboutForm : Form
    {
        public AboutForm(string build, bool isLicensed)
        {
            InitializeComponent();
            PopulateBuildField(build);
            PopulateRegistrationStatusField(isLicensed);
        }

        void PopulateBuildField(string build)
        {
            buildLabel.Text = build;
        }

        void PopulateRegistrationStatusField(bool isLicensed)
        {
            registeredToLabel.Text = isLicensed ? "Registered" : "Unregisted";
        }
    }
}
