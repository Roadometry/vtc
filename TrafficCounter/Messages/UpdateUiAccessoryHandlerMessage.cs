using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Actors;
using VTC.Common.RegionConfig;

namespace VTC.Messages
{
    class UpdateUiAccessoryHandlerMessage
    {
        public UpdateUiAccessoryHandlerMessage(FrameGrabActor.UpdateUIDelegate uiDelegate)
        {
            UiDelegate = uiDelegate;
        }

        public FrameGrabActor.UpdateUIDelegate UiDelegate { get; private set; }
    }
}
