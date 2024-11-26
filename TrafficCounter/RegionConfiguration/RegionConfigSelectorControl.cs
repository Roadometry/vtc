using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Emgu.CV;
using VTC.Common.RegionConfig;

namespace VTC.RegionConfiguration
{
    public partial class RegionConfigSelectorControl : UserControl
    {
        public event EventHandler CreateNewRegionConfigClicked;

        public Emgu.CV.Image<Emgu.CV.Structure.Bgr, float> BaseThumbnail
        {
            get; }

        public RegionConfigSelectorControl(List<RegionConfig> regionConfigs, Emgu.CV.Image<Emgu.CV.Structure.Bgr, float> baseThumbnail, string name)
        {
            InitializeComponent();

            BaseThumbnail = baseThumbnail;
            pbThumbnail.Image = BaseThumbnail.ToBitmap();

            lbRegionConfigs.DataSource = regionConfigs;
            lbRegionConfigs.DisplayMember = "Title";
            lblName.Text = name;
        }

        public void UpdateRegionConfigs(List<RegionConfig> newRegionConfigs)
        {
            lbRegionConfigs.DataSource = newRegionConfigs;

            if (SelectedRegionConfig != null)
            {
                var selectedTitle = SelectedRegionConfig.Title;
                if (selectedTitle != null)
                {
                    var selected = newRegionConfigs.FirstOrDefault(r => r.Title.Equals(selectedTitle));
                    if (null != selected)
                    {
                        lbRegionConfigs.SelectedItem = selected;
                    }
                }
            }
            else
            {
                var selected = newRegionConfigs.First();
                if (null != selected)
                {
                    lbRegionConfigs.SelectedItem = selected;
                }
            }
        }

        public RegionConfig SelectedRegionConfig
        {
            get => lbRegionConfigs.SelectedItem as RegionConfig;
            set => lbRegionConfigs.SelectedItem = value;
        }

        private void lbRegionConfigs_SelectedValueChanged(object sender, EventArgs e)
        {
            var regionConfig = lbRegionConfigs.SelectedItem as RegionConfig;

            if (regionConfig == null) return;
            var maskedThumbnail = regionConfig.RoiMask.GetMask(BaseThumbnail.Width, BaseThumbnail.Height, new Emgu.CV.Structure.Bgr(Color.Blue));

            pbThumbnail.Image = BaseThumbnail.Add(maskedThumbnail).ToBitmap();
        }

        private void btnCreateNewRegionConfig_Click(object sender, EventArgs e)
        {
            CreateNewRegionConfigClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
