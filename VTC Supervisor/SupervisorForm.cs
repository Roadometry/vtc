using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VTC_Supervisor
{
    public partial class SupervisorForm : Form
    {
        int ticks = 0;

        public SupervisorForm()
        {
            InitializeComponent();

            ipcamera1Checkbox.Checked = Properties.Settings.Default.IP1Selected;
            ipcamera2Checkbox.Checked = Properties.Settings.Default.IP2Selected;
            ipcamera3Checkbox.Checked = Properties.Settings.Default.IP3Selected;
            autolaunchCheckbox.Checked = Properties.Settings.Default.autolaunch;
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            autolaunchTimer.Stop();
            SupervisorProcess();
            checkProcessTimer.Start();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            autolaunchTimer.Stop();
            autolaunchLabel.Text = "Auto-launch terminated.";

            checkProcessTimer.Stop();
        }

        private void checkProcessTimer_Tick(object sender, EventArgs e)
        {
            SupervisorProcess();
        }

        private void SupervisorProcess()
        {
            //Count running instances
            var runningInstanceCount = Process.GetProcessesByName("VTC").Length;

            //Count desired instances
            var desiredInstanceCount = 0;
            if (ipcamera1Checkbox.Checked) desiredInstanceCount++;
            if (ipcamera2Checkbox.Checked) desiredInstanceCount++;
            if (ipcamera3Checkbox.Checked) desiredInstanceCount++;

            autolaunchLabel.Text = desiredInstanceCount + " instances of VTC launched.";

            //If number of running instances is correct, do nothing
            if (runningInstanceCount >= desiredInstanceCount)
            {
                return;
            }

            //Terminate all instances
            foreach (var p in Process.GetProcessesByName("VTC"))
            {
                p.CloseMainWindow();
                p.Close();
            }

            //Re-launch instances
            if (ipcamera1Checkbox.Checked)
            {
                Process.Start("VTC.exe","IP1");
                System.Threading.Thread.Sleep(5000);
            }

            if (ipcamera2Checkbox.Checked)
            {
                Process.Start("VTC.exe","IP2");
                System.Threading.Thread.Sleep(5000);
            }

            if (ipcamera3Checkbox.Checked)
            {
                Process.Start("VTC.exe","IP3");
                System.Threading.Thread.Sleep(5000);
            }
        }

        private void ipcamera1Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.IP1Selected = ipcamera1Checkbox.Checked;   
            Properties.Settings.Default.Save();
        }

        private void ipcamera2Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.IP2Selected = ipcamera2Checkbox.Checked;
            Properties.Settings.Default.Save();
        }

        private void ipcamera3Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.IP3Selected = ipcamera3Checkbox.Checked;
            Properties.Settings.Default.Save();
        }

        private void autolaunchCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            autolaunchTimer.Stop();

            Properties.Settings.Default.autolaunch = autolaunchCheckbox.Checked;
            Properties.Settings.Default.Save();

            if(Properties.Settings.Default.autolaunch)
            {     
                autolaunchTimer.Start();
            }
        }

        private void autolaunchTimer_Tick(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.autolaunch == false)
            { 
                autolaunchTimer.Stop();
                return;
            }

            ticks++;

            if(30-ticks > 0)
            {
                autolaunchLabel.Text = "Launching in: " + (30 - ticks) + " seconds";
            }
            else
            {
                autolaunchTimer.Stop();
                autolaunchLabel.Text = "Launching...";
                startButton_Click(null, null);
            }
        }
    }
}
