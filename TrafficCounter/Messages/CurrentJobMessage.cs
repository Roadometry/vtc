using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Common;

namespace VTC.Messages
{
    class CurrentJobMessage
    {
        public CurrentJobMessage(BatchVideoJob job)
        {
            CurrentJob = job;
        }

        public BatchVideoJob CurrentJob { get; private set; }
    }
}
