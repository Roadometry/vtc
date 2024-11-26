using VTC.Common;
using VTC.Common.RegionConfig;

namespace VTC.Kernel.Video
{
    /// <summary>
    /// Holder of video source and corresponding settings.
    /// </summary>
    public class CaptureContext
    {
        public ICaptureSource Capture { get; private set; }
        public RegionConfig Settings { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="capture"></param>
        /// <param name="settings"></param>
        public CaptureContext(ICaptureSource capture, RegionConfig regionConfig)
        {
            Capture = capture;
            Settings = regionConfig;
        }
    }
}
