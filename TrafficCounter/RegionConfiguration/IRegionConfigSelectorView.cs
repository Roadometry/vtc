using System.Collections.Generic;
using VTC.Common.RegionConfig;
using VTC.Kernel.Video;

namespace VTC.RegionConfiguration
{
    public interface IRegionConfigSelectorView
    {
        void SetModel(RegionConfigSelectorModel model);
        RegionConfigSelectorModel GetModel();
        Dictionary<ICaptureSource, RegionConfig> GetRegionConfigSelections();
    }
}
