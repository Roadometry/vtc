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
    public partial class VideoDateTimeForm : Form
    {

        public DateTime OverrideTime;
        public DateTime InitialTime;
        public VideoDateTimeForm(DateTime initialTime)
        {
            InitializeComponent();
            OverrideTime = initialTime;
            InitialTime = initialTime;
            dateTimePicker1.Value = initialTime;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            OverrideTime = dateTimePicker1.Value;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OverrideTime = InitialTime;
            Close();
        }
    }
}
