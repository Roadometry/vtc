using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTC.Messages
{
    class RegionConfigNameLookupMessage
    {
        public RegionConfigNameLookupMessage(string regionConfigName, int jobId)
        {
            RegionConfigName = regionConfigName;
            JobId = jobId;
        }

        public string RegionConfigName;
        public int JobId;
    }
}
