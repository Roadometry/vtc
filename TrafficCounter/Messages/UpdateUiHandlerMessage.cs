using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Actors;
using VTC.Common.RegionConfig;

namespace VTC.Messages
{
    class UpdateUiHandlerMessage
    {
        public UpdateUiHandlerMessage(ProcessingActor.UpdateUIDelegate uiDelegate)
        {
            UiDelegate = uiDelegate;
        }

        public ProcessingActor.UpdateUIDelegate UiDelegate { get; private set; }
    }
}
