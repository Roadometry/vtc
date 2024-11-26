using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;

namespace VTC.Messages
{
    class RequestConfigurationMessage : ActorMessage
    {
        public RequestConfigurationMessage(IActorRef actorRef):base(actorRef)
        {
        }
    }
}
