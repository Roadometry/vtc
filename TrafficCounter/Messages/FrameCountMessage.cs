using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Classifier;

namespace VTC.Messages
{
    class FrameCountMessage
    {
        public FrameCountMessage(UInt64 count)
        {
            Count = count;
        }

        public UInt64 Count { get; private set; }
    }
}
