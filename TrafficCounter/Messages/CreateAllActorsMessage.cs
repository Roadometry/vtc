using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Actors;

namespace VTC.Messages
{
    class CreateAllActorsMessage
    {
        public CreateAllActorsMessage(ProcessingActor.UpdateUIDelegate updateUiDelegate, TrafficCounter.UpdateStatsUIDelegate statsUiDelegate, TrafficCounter.UpdateInfoUIDelegate infoUiDelegate, FrameGrabActor.UpdateUIDelegate framegrabUiDelegate, TrafficCounter.UpdateDebugDelegate debugDelegate)
        {
            UpdateUiDelegate = updateUiDelegate;
            UpdateStatsUiDelegate = statsUiDelegate;
            UpdateInfoUiDelegate = infoUiDelegate;
            UpdateFrameGrabUiDelegate = framegrabUiDelegate;
            UpdateDebugDelegate = debugDelegate;
        }

        public ProcessingActor.UpdateUIDelegate UpdateUiDelegate { get; private set; }
        public TrafficCounter.UpdateStatsUIDelegate UpdateStatsUiDelegate { get; private set; }
        public TrafficCounter.UpdateInfoUIDelegate UpdateInfoUiDelegate { get; private set; }
        public FrameGrabActor.UpdateUIDelegate UpdateFrameGrabUiDelegate { get; private set; }
        public TrafficCounter.UpdateDebugDelegate UpdateDebugDelegate { get; private set;}
    }
}
