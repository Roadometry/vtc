using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Common.RegionConfig;

namespace VTC.Messages
{
    class UpdateVideoPropertiesMessage
    {
        public UpdateVideoPropertiesMessage(double fps, double totalFrames)
        {
            Fps = fps;
            TotalFrames = totalFrames;
        }

        public double Fps { get; private set; }
        public double TotalFrames { get; private set; }
    }
}
