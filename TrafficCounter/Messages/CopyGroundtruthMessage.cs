using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;

namespace VTC.Messages
{
    class CopyGroundtruthMessage
    {
        public CopyGroundtruthMessage(string groundTruthPath)
        {
            GroundTruthPath = groundTruthPath;
        }

        public string GroundTruthPath;
    }
}
