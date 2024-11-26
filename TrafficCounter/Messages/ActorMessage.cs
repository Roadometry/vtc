using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;

namespace VTC.Messages
{
    class ActorMessage
    {
        public ActorMessage(IActorRef actorRef)
        {
            ActorRef = actorRef;
        }

        public IActorRef ActorRef { get; private set; }
    }
}
