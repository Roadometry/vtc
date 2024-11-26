using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace VTC.Messages
{
    class LogMessage : StringMessage
    {
        public LogMessage(string text, LogLevel level, string actorName) : base(text)
        {
            Level = level;
            ActorName = actorName;
        }

        public LogLevel Level { get; private set; }
        public string ActorName { get; private set; }
    }
}
