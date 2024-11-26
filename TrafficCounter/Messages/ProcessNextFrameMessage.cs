using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace VTC.Messages
{
    class ProcessNextFrameMessage : FrameMessage
    {
        public Double Timestep;
        public ProcessNextFrameMessage(Image<Bgr, byte> frame, double timestep) : base(frame)
        {
            Timestep = timestep;
        }
    }
}
