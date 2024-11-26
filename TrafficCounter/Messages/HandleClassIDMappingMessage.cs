using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;

namespace VTC.Messages
{
    public class HandleClassIDMappingMessage
    {
        public HandleClassIDMappingMessage(Dictionary<int,string> classIdMapping)
        {
            ClassIdMapping = classIdMapping;
        }

        public Dictionary<int, string> ClassIdMapping;
    }
}