using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using VTC.Common.RegionConfig;

namespace VTC.Messages
{
    class RegionConfigurationMessage
    {
        public RegionConfigurationMessage(RegionConfig config)
        {
            Config = config;
        }

        public RegionConfig Config { get; private set; }
    }
}
