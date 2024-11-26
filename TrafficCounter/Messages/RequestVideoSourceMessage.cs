using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace VTC.Messages
{
    class RequestVideoSourceMessage : ActorMessage
    {
        public RequestVideoSourceMessage(IActorRef actorRef) : base(actorRef)
        {
        }
    }
}
