using System;
using System.Windows.Forms;

namespace VTC.RegionConfiguration
{
    public sealed partial class InputPrompt : Form
    {
        public string InputString => tbInput.Text;

        public InputPrompt(string caption, string message)
        {
            InitializeComponent();

            Text = caption;
            lblMessage.Text = message;
            tbInput.TextChanged += (sender, args) =>
            {
                btnOK.Enabled = !string.IsNullOrWhiteSpace(tbInput.Text);
            };
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
