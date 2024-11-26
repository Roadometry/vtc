using System.Collections.Generic;

namespace VTC.Kernel.Video
{
    /// <summary>
    /// Interface to get video sources.
    /// </summary>
    public interface ICaptureContextProvider
    {
        IEnumerable<CaptureContext> GetCaptures();
    }
}
