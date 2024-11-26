using System;
using System.Collections.Generic;
using System.Windows.Forms;
using VTC.Common.RegionConfig;

namespace VTC.RegionConfiguration
{
    using Kernel.Video;
    using System.Linq;

    public partial class RegionConfigSelectorView : Form, IRegionConfigSelectorView
    {
        private List<RegionConfig> _regionConfigs;
        private readonly Dictionary<RegionConfigSelectorControl, ICaptureSource> _captureSourceLookup = new Dictionary<RegionConfigSelectorControl, ICaptureSource>();
        private readonly IRegionConfigDataAccessLayer _regionConfigDal;

        public RegionConfigSelectorView(IRegionConfigDataAccessLayer regionConfigDal) 
        {
            InitializeComponent();

            _regionConfigDal = regionConfigDal;
        }

        private RegionConfigSelectorControl CreateRegionConfigSelectorControl(List<RegionConfig> regionConfigs, ICaptureSource captureSource)
        {   
            var frame = captureSource.QueryFrame();
            var thumbnail = frame.Convert<Emgu.CV.Structure.Bgr, float>();
            for (int i = 0; i < 100; i++)
            {
                var tempFrame = captureSource.QueryFrame();
                if(tempFrame != null)
                { 
                    var tempFrameConverted = tempFrame.Convert<Emgu.CV.Structure.Bgr, float>();
                    thumbnail.AccumulateWeighted(tempFrameConverted, 0.01);
                }
                else
                {
                   System.Diagnostics.Debug.WriteLine("Frame " + i + " was null in RegionConfigSelectorView");
                }
            }

            var control = new RegionConfigSelectorControl(regionConfigs, thumbnail, captureSource.Name)
            {
                BorderStyle = BorderStyle.FixedSingle
            };


            control.Width =
                tlpControls.Width - tlpControls.Padding.Left - tlpControls.Padding.Right
                - control.Margin.Left - control.Margin.Right
                - 100;

            control.Anchor =  AnchorStyles.Left | AnchorStyles.Right;

            control.CreateNewRegionConfigClicked += OnCreateNewRegionConfigClicked;

            return control;
        }

        private void OnCreateNewRegionConfigClicked(object sender, EventArgs e)
        {
            var control = sender as RegionConfigSelectorControl;
            if (control == null)
            {
                MessageBox.Show("Choose a video source before configuring new regions.");
            }
            else
            {
                var captureSourceList = new List<ICaptureSource> {_captureSourceLookup[control]};

                var createRegionConfigForm = new RegionEditor(captureSourceList, _regionConfigDal, control.SelectedRegionConfig);
                if (createRegionConfigForm.ShowDialog() == DialogResult.OK)
                {
                    _regionConfigs = _regionConfigDal.LoadRegionConfigList();
                    foreach (var c in _captureSourceLookup.Keys)
                    {
                        c.UpdateRegionConfigs(_regionConfigs);
                    }

                    control.SelectedRegionConfig = createRegionConfigForm.SelectedRegionConfig;
                }
            }
        }

        public void SetModel(RegionConfigSelectorModel model)
        {
            _regionConfigs = model.RegionConfigs;

            _captureSourceLookup.Clear();
            tlpControls.Controls.Clear();
            tlpControls.RowCount = 0;

            foreach (var captureSource in model.CaptureSources)
            {
                var control = CreateRegionConfigSelectorControl(_regionConfigs, captureSource);
                _captureSourceLookup[control] = captureSource;
                tlpControls.RowCount++;
                tlpControls.Controls.Add(control);
            }
        }

        public Dictionary<ICaptureSource, RegionConfig> GetRegionConfigSelections()
        {
            var result = new Dictionary<ICaptureSource, RegionConfig>();

            foreach(var kvp in _captureSourceLookup)
            {
                var captureSource = kvp.Value;
                var regionConfig = kvp.Key.SelectedRegionConfig;

                result[captureSource] = regionConfig;
            }

            return result;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        public RegionConfigSelectorModel GetModel()
        {
            return new RegionConfigSelectorModel(_captureSourceLookup.Values.ToList(), _regionConfigs.ToList());
        }
    }
}
