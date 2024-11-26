using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTC.Messages
{
    class ActorStatusMessage
    {
        public ActorStatusMessage(Dictionary<string, bool> statuses)
        {
            Statuses = statuses;
        }

        public Dictionary<string, bool> Statuses { get; private set; }
    }
}
