using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Common;
using VTC.Common.RegionConfig;

namespace VTC.Messages
{
    class UpdateRegionConfigurationMessage : RegionConfigurationMessage
    {
        public UpdateRegionConfigurationMessage(RegionConfig config) : base(config)
        {
        }
    }
}
