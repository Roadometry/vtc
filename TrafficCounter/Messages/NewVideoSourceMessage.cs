using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Kernel.Video;

namespace VTC.Messages
{
    class NewVideoSourceMessage
    {
        public NewVideoSourceMessage(ICaptureSource captureSource)
        {
            CaptureSource = captureSource;
        }

        public ICaptureSource CaptureSource { get; private set; }
    }
}
