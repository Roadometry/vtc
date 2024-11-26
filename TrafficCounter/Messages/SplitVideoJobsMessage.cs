using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Common;

namespace VTC.Messages
{
    class SplitVideoJobsMessage
    {
        public SplitVideoJobsMessage(List<BatchVideoJob> videoJobs)
        {
            VideoJobsList = videoJobs;
        }

        public List<BatchVideoJob> VideoJobsList { get; private set; }
    }
}
