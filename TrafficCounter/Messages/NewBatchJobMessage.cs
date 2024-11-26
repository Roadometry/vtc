using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using VTC.Common;

namespace VTC.Messages
{
    class NewBatchJobMessage
    {
        public NewBatchJobMessage(BatchVideoJob job)
        {
            Job = job;
        }

        public BatchVideoJob Job;
    }
}
