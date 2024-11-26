using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTC.Messages
{
    class LogPerformanceMessage
    {
        public LogPerformanceMessage(double fps, double averageProcessingTime)
        {
            FPS = fps;
            AverageProcessingTimeMs = averageProcessingTime;
        }

        public double FPS { get; private set; }
        public double AverageProcessingTimeMs { get; private set; }
    }
}
