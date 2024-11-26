using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace VTC.Messages
{
    class LoggingActorMessage : ActorMessage
    {
        public LoggingActorMessage(IActorRef actor) : base(actor)
        {
            
        }
    }
}
