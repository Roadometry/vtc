using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using VTC.Classifier;

namespace Darknet
{
    public class YoloWrapperNoGPU : IDisposable
    {
        private const string YoloLibraryName = "darknet_cpu.dll";
        private const int MaxObjects = 200;

        [DllImport(YoloLibraryName, EntryPoint = "init")]
        private static extern int InitializeYolo(string configurationFilename, string weightsFilename, int gpu);

        [DllImport(YoloLibraryName, EntryPoint = "detect_image")]
        private static extern int DetectImage(string filename, ref YoloClassifier.BboxContainer container);

        [DllImport(YoloLibraryName, EntryPoint = "detect_mat")]
        private static extern int DetectImage(IntPtr pArray, int nSize, ref YoloClassifier.BboxContainer container);

        [DllImport(YoloLibraryName, EntryPoint = "dispose")]
        private static extern int DisposeYolo();

        public YoloWrapperNoGPU(string configurationFilename, string weightsFilename, int gpu)
        {
            InitializeYolo(configurationFilename, weightsFilename, gpu);
        }

        public void Dispose()
        {
            DisposeYolo();
        }

        public YoloClassifier.bbox_t[] Detect(string filename)
        {
            var container = new YoloClassifier.BboxContainer();
            var count = DetectImage(filename, ref container);

            return container.candidates;
        }

        public YoloClassifier.bbox_t[] Detect(byte[] imageData)
        {
            var container = new YoloClassifier.BboxContainer();
            var size = Marshal.SizeOf(imageData[0]) * imageData.Length;
            var pnt = Marshal.AllocHGlobal(size);

            try
            {
                // Copy the array to unmanaged memory.
                Marshal.Copy(imageData, 0, pnt, imageData.Length);
                var count = DetectImage(pnt, imageData.Length, ref container);
                if (count == -1)
                {
                    throw new NotSupportedException($"{YoloLibraryName} has no OpenCV support");
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Free the unmanaged memory.
                Marshal.FreeHGlobal(pnt);
            }

            return container.candidates;
        }
    }
}
