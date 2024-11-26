using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Common.RegionConfig;

namespace VTC.Messages
{
    class RegionConfigLookupResponseMessage
    {
        public RegionConfigLookupResponseMessage(RegionConfig config, int jobId)
        {
            Configuration = config;
            JobId = jobId;
        }

        public int JobId;
        public RegionConfig Configuration;
    }
}
