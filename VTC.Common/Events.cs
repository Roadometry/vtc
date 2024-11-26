using System;
using System.Collections.Generic;

namespace VTC.Common
{
    public static class TrackingEvents
    {
        public class TrajectoryListEventArgs : EventArgs
        {
            public List<TrackedObject> TrackedObjects { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public class TrackedObjectsEventArgs : EventArgs
        {
            public List<TrackedObject> Measurements { get; set; }
            public DateTime Timestamp { get; set; }
        }
   
    }
    
}

