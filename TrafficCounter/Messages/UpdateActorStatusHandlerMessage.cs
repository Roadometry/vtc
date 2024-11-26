using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Actors;

namespace VTC.Messages
{
    class UpdateActorStatusHandlerMessage
    {
        public UpdateActorStatusHandlerMessage(SupervisorActor.UpdateActorStatusDelegate updatedelegate)
        {
            UpdateStatusDelegate = updatedelegate;
        }

        public SupervisorActor.UpdateActorStatusDelegate UpdateStatusDelegate { get; private set; }
    }
}
