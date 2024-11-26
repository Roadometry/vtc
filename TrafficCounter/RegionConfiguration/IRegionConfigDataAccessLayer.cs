using System.Collections.Generic;
using VTC.Common.RegionConfig;

namespace VTC.RegionConfiguration
{
    public interface IRegionConfigDataAccessLayer
    {
        List<RegionConfig> LoadRegionConfigList();
        void SaveRegionConfigList(List<RegionConfig> regionConfigs);
    }
}
