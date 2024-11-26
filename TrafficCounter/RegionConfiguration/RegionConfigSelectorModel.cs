using VTC.Common.RegionConfig;

namespace VTC.RegionConfiguration
{
    using Kernel.Video;
    using System.Collections.Generic;

    public class RegionConfigSelectorModel
    {
        public readonly List<ICaptureSource> CaptureSources;
        public readonly List<RegionConfig> RegionConfigs;

        public RegionConfigSelectorModel(List<ICaptureSource> captureSources, List<RegionConfig> regionConfigs)
        {
            CaptureSources = captureSources;
            RegionConfigs = regionConfigs;
        }
    }
}
