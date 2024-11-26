using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Actors;
using VTC.Common.RegionConfig;

namespace VTC.Messages
{
    class UpdateInfoUiHandlerMessage
    {
        public UpdateInfoUiHandlerMessage(TrafficCounter.UpdateInfoUIDelegate uiDelegate)
        {
            InfoUiDelegate = uiDelegate;
        }

        public TrafficCounter.UpdateInfoUIDelegate InfoUiDelegate { get; private set; }
    }
}
