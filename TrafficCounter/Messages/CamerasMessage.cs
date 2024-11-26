using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Kernel.Video;

namespace VTC.Messages
{
    class CamerasMessage
    {
        public CamerasMessage(List<ICaptureSource> cameras)
        {
            Cameras = cameras;
        }

        public List<ICaptureSource> Cameras { get; private set; }
    }
}
