using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Actors;
using VTC.Common.RegionConfig;

namespace VTC.Messages
{
    class UpdateStatsUiHandlerMessage
    {
        public UpdateStatsUiHandlerMessage(TrafficCounter.UpdateStatsUIDelegate uiDelegate)
        {
            StatsUiDelegate = uiDelegate;
        }

        public TrafficCounter.UpdateStatsUIDelegate StatsUiDelegate { get; private set; }
    }
}
