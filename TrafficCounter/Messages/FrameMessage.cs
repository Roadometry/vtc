using Emgu.CV;
using Emgu.CV.Structure;

namespace VTC.Messages
{
    public class FrameMessage
    {
        public FrameMessage(Image<Bgr, byte> frame)
        {
            Frame = frame;
        }

        public Image<Bgr, byte> Frame { get; private set; }
    }
}