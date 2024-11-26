using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Classifier;

namespace VTC.Messages
{
    class EventConfigMessage
    {
        public EventConfigMessage(EventConfig config)
        {
            Config = config;
        }

        public EventConfig Config { get; private set; }
    }
}
