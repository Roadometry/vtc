using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Common;

namespace VTC.Messages
{
    class TrackingEventMessage
    {
        public TrackingEventMessage(TrackingEvents.TrajectoryListEventArgs args)
        {
            EventArgs = args;
        }

        public TrackingEvents.TrajectoryListEventArgs EventArgs { get; private set; }
    }
}
