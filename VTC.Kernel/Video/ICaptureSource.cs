using System;
using Emgu.CV;
using Emgu.CV.Structure;
using VTC.Common;

namespace VTC.Kernel.Video
{
    /// <summary>
    /// Interface for video stream.
    /// </summary>
    public interface ICaptureSource
    {
        /// <summary>
        /// Camera name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Camera width.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Camera height.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Get next frame from camera.
        /// </summary>
        /// <returns></returns>
        Image<Bgr, byte> QueryFrame();

        /// <summary>
        /// Initialize camera.
        /// </summary>
        void Init();

        /// <summary>
        /// Destroy underlying camera.
        /// </summary>
        void Destroy();

        /// <summary>
        /// Get connectivity status.
        /// </summary>
        bool CaptureComplete();

        bool IsLiveCapture();

        double FPS();

        DateTime StartDateTime();

        double Rotation();

    }
}