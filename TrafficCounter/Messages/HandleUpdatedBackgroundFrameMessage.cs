using Emgu.CV;
using Emgu.CV.Structure;

namespace VTC.Messages
{
    public class HandleUpdatedBackgroundFrameMessage:FrameMessage
    {
        public HandleUpdatedBackgroundFrameMessage(Image<Bgr, byte> frame) : base(frame)
        {
        }
    }
}