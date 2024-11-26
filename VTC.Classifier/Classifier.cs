using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using VTC.Common;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Darknet;
using NLog;

namespace VTC.Classifier
{
    //Below structs just copied for reference from darknet code
    //internal struct bbox_t
    //{
    //    public int x, y, w, h;    // (x,y) - top-left corner, (w, h) - width & height of bounded box
    //    public float prob;                 // confidence - probability that the object was found correctly
    //    public int obj_id;        // class of object - from range [0, classes-1]
    //    public int track_id;      // tracking id for video (0 - untracked, 1 - inf - tracked object)
    //};

    //internal struct image_t
    //{
    //    public int h;                      // height
    //    public int w;                      // width
    //    public int c;                      // number of chanels (3 - for RGB)
    //    public float* data;                // pointer to the image data
    //};

    [DataContract]
    public class EventConfig
    {
        [DataMember] public Dictionary<RegionTransition, string> Events;

        public EventConfig()
        {
            Events = new Dictionary<RegionTransition, string>();
        }
    }

    public static class TrajectoryClassifier
    {
        public static KeyValuePair<RegionTransition, string>? ClassifyRegionTransition(KeyValuePair<string, Common.RegionConfig.Polygon> start, KeyValuePair<string, Common.RegionConfig.Polygon> end, EventConfig eventConfiguration)
        {
            var transitionEvent = eventConfiguration.Events.FirstOrDefault(kvp => kvp.Key.InRegion == start.Key && kvp.Key.OutRegion == end.Key);
            return transitionEvent;
        }
    }

    public class YoloClassifier
    {
        private const int MaxObjects = 1000;
        [StructLayout(LayoutKind.Sequential)]
        public struct bbox_t
        {
            public UInt32 x, y, w, h;    // (x,y) - top-left corner, (w, h) - width & height of bounded box
            public float prob;           // confidence - probability that the object was found correctly
            public UInt32 obj_id;        // class of object - from range [0, classes-1]
            public UInt32 track_id;      // tracking id for video (0 - untracked, 1 - inf - tracked object)
            public UInt32 frames_counter;
            public float x_3d, y_3d, z_3d;  // 3-D coordinates, if there is used 3D-stereo camera
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct BboxContainer
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxObjects)]
            public bbox_t[] candidates;
        }

        private YoloWrapper Detector;
        private YoloWrapperNoGPU DetectorNoGPU;

        public bool cpuMode = true;

        private static readonly Logger Logger = LogManager.GetLogger("main.form");

        public YoloClassifier()
        {
            var gpuDetector = new GPUDetector();
            if(gpuDetector.HasGPU && gpuDetector.MB_VRAM > 3000)
            {
                cpuMode = false;
            }

            try
            {
                var defaultYoloCfgFilepath = "yolo.cfg";
                var defaultYoloWeightsFilepath = "yolo.weights";

                var selectedYoloCfgFilepath = defaultYoloCfgFilepath;
                var selectedYoloWeightsFilepath = defaultYoloWeightsFilepath;

                var YoloCfgDropinFilepaths = Directory.EnumerateFiles("./DetectionNetwork/");
                foreach (var p in YoloCfgDropinFilepaths)
                {
                    var ext = Path.GetExtension(p);
                    if (ext == null) continue;
                    if (ext.Contains("cfg"))
                    {
                        selectedYoloCfgFilepath = p;
                        Logger.Debug("Selected YoloCfg:" + selectedYoloCfgFilepath);
                        Console.WriteLine("Selected YoloCfg:" + selectedYoloCfgFilepath);
                    }

                    if (ext.Contains("weights"))
                    {
                        selectedYoloWeightsFilepath = p;
                        Logger.Debug("Selected YoloWeights:" + selectedYoloWeightsFilepath);
                        Console.WriteLine("Selected YoloWeights:" + selectedYoloWeightsFilepath);
                    }
                }

                if (cpuMode)
                {
                    Logger.Debug("Calling detector constructor (no GPU)...");
                    DetectorNoGPU = new Darknet.YoloWrapperNoGPU(selectedYoloCfgFilepath, selectedYoloWeightsFilepath, 0);    
                }
                else
                {
                    Logger.Debug("Calling detector constructor (with GPU)...");
                    Detector = new Darknet.YoloWrapper(selectedYoloCfgFilepath, selectedYoloWeightsFilepath, 0);
                }

                Logger.Debug("Classifier initialized.");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Fatal, ex.Message);
            }
        }

        ~YoloClassifier()
        {
            if (Detector == null)
            {
                return;
            }

            if (cpuMode)
            {
                DetectorNoGPU.Dispose();
            }
            else
            {
                Detector.Dispose();
            }
        }

        private byte[] GetRGBValues(Bitmap bmp)
        {
            // Lock the bitmap's bits. 
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);bmp.UnlockBits(bmpData);

            return rgbValues;
        }

        public List<Measurement> DetectFrameYolo(Image<Bgr, byte> frame)
        {
            List<Measurement> measurements = new List<Measurement>();
            var converter = new ImageConverter();
            var bytes = (byte[]) converter.ConvertTo(frame.ToBitmap(), typeof(byte[]));

            bbox_t[] detectionArray;
            if(!cpuMode && Detector != null)
            {
                detectionArray = Detector.Detect(bytes);
            }
            else
            {
                return new List<Measurement>();
            }

            for (int i = 0; i < detectionArray.Length; i++)
            {
                if (detectionArray[i].x == 0 && detectionArray[i].y == 0) {break;}

                var m = new Measurement
                {
                    Height = detectionArray[i].h,
                    Width = detectionArray[i].w,
                    X = detectionArray[i].x + detectionArray[i].w / 2,
                    Y = detectionArray[i].y + detectionArray[i].h / 2
                };

                if (m.Height > frame.Height | m.Width > frame.Width)
                {
                    Debug.WriteLine("Detection rectangle too large.");
                    continue;
                }

                if ( (m.X > frame.Width) | (m.Y > frame.Height) | (m.X < 0) | (m.Y < 0))
                {
                    Debug.WriteLine("Detection coordinates out-of-bounds.");
                    continue;
                }

                //Select a region smaller than the entire detection rectangle. Divide height and width by 2.
                // 

                int xColor1 = (int) m.X - Convert.ToInt32(detectionArray[i].w / 2);
                int yColor1 = (int) m.Y - Convert.ToInt32(detectionArray[i].h / 2);

                int xColor2 = (int) m.X + Convert.ToInt32(detectionArray[i].w/2);
                int yColor2 = (int) m.Y + Convert.ToInt32(detectionArray[i].h/2);

                var color = SampleColorAtRectangle(frame, xColor1, yColor1,
                    xColor2,
                    yColor2);

                m.Blue = color.Blue;
                m.Green = color.Green;
                m.Red = color.Red;
                m.Size = m.Width * m.Height;
                m.ObjectClass = (int) detectionArray[i].obj_id;
                measurements.Add(m);
            }
            

            return measurements;
        }

        private Bgr SampleColorAtRectangle(Image<Bgr, byte> frame, int x1, int y1, int x2, int y2)
        {
            int width_original = frame.Width;
            int height_original = frame.Height;
            Bgr avg;
            MCvScalar MVcavg;
            int width = x2 - x1;
            int height = y2 - y1;
            frame.ROI = new Rectangle(x1, y1, width, height);
            frame.AvgSdv(out avg, out MVcavg);
            frame.ROI = new Rectangle(0,0, width_original, height_original); //Reset ROI
            return avg;
        }

        unsafe static byte[] GetBytes(float value) {
            var bytes = new byte[4];
            fixed (byte* b = bytes)
                *((int*)b) = *(int*)&value;

            return bytes;
        }

        private UnmanagedImageMemory MarshalEmguImageToimage_t(Image<Bgr, float> frame)
        {
            if(frame == null)
                return new UnmanagedImageMemory();

            unsafe
            {
                //Calculate sizes for allocation
                int struct_size = sizeof(float*) + 3 * sizeof(int) + 2;
                int data_size = frame.Bytes.Length;

                //Allocate
                IntPtr pNativeDataStruct = Marshal.AllocHGlobal(struct_size);
                IntPtr pNativeDataFrame = Marshal.AllocHGlobal(data_size);

                //Copy data
                Marshal.WriteInt32(pNativeDataStruct, frame.Height);
                Marshal.WriteInt32(pNativeDataStruct + 1 * sizeof(int), frame.Width);
                Marshal.WriteInt32(pNativeDataStruct + 2 * sizeof(int), frame.NumberOfChannels);
                Int64 fullPtr = pNativeDataFrame.ToInt64();
                Marshal.WriteInt64(pNativeDataStruct + 4 * sizeof(int), fullPtr);

                int frameWidth = frame.Width;
                int frameHeight = frame.Height;
                //int nChannels = frame.NumberOfChannels;
                //int[] destinationChannelIndex = { 2, 1, 0 }; //Swap R and B channels: OpenCV is Bgr, Yolo expects Rgb.

                Parallel.For(0, frameWidth, i =>
                    {
                        //Parallel.For(0, frameHeight, j =>
                        for(int j=0;j<frameHeight;j++)
                        {
                            int dst_index =
                                (i + frameWidth * j + frameWidth * frameHeight * 2) *
                                sizeof(float);
                            byte[] bytes = BitConverter.GetBytes(frame.Data[j, i, 0]);
                            IntPtr newptr = new IntPtr(pNativeDataFrame.ToInt64() + dst_index);
                            Marshal.Copy(bytes, 0, newptr, 4);

                            dst_index =
                                (i + frameWidth * j + frameWidth * frameHeight) *
                                sizeof(float);
                            bytes = BitConverter.GetBytes(frame.Data[j, i, 1]);
                            newptr = new IntPtr(pNativeDataFrame.ToInt64() + dst_index);
                            Marshal.Copy(bytes, 0, newptr, 4);

                            dst_index =
                                (i + frameWidth * j) *
                                sizeof(float);
                            bytes = BitConverter.GetBytes(frame.Data[j, i, 2]);
                            newptr = new IntPtr(pNativeDataFrame.ToInt64() + dst_index);
                            Marshal.Copy(bytes, 0, newptr, 4);
                        }
                    }
                );

                var memory = new UnmanagedImageMemory();
                memory.pNativeDataFrame = pNativeDataFrame;
                memory.pNativeDataStruct = pNativeDataStruct;
                return memory;
            }
        }
    }

    public class YoloIntegerNameMapping
    {
        private String IntegerToObjectClassFilepath = "coco.names";
        public Dictionary<int, ObjectType> IntegerToObjectType = new Dictionary<int, ObjectType>();
        public Dictionary<int, string> IntegerToObjectName = new Dictionary<int, string>();
        private static readonly Logger Logger = LogManager.GetLogger("main.form");

        public static string GetObjectNameFromClassInteger(int classId, Dictionary<int,string> idClassMapping)
        {
            return idClassMapping.ContainsKey(classId) ? idClassMapping[classId] : "Unknown";
        }

        public static ObjectType GetObjectTypeFromClassInteger(int classId, Dictionary<int,ObjectType> idClassMapping)
        {
            return idClassMapping.ContainsKey(classId) ? idClassMapping[classId] : ObjectType.Unknown;
        }

        public ObjectType YoloIntegerToObjectType(int ObjectID)
        {
            if (IntegerToObjectType.ContainsKey(ObjectID))
            {
                return IntegerToObjectType[ObjectID];
            }
            
            return ObjectType.Unknown;
        }

        public YoloIntegerNameMapping()
        {
            //Look for network definition files
            var YoloCfgDropinFilepaths = Directory.EnumerateFiles("./DetectionNetwork/");

            //Find the integer->string mapping defition file
            foreach (var p in YoloCfgDropinFilepaths)
            {
                var ext = Path.GetExtension(p);
                if (ext == null) continue;
                if (ext.Contains("names"))
                {
                    IntegerToObjectClassFilepath = p;
                    Logger.Debug("Selected YoloNames:" + IntegerToObjectClassFilepath);
                    Console.WriteLine("Selected YoloNames:" + IntegerToObjectClassFilepath);
                }
            }

            var lines = File.ReadAllLines(IntegerToObjectClassFilepath);
            for(int i=0; i < lines.Length; i++)
            {
                foreach(var c in DetectionClasses.ClassDetectionWhitelist)
                {
                    if(c.ToString().ToLower() == lines[i])
                    {
                        IntegerToObjectName.Add(i,lines[i]);       
                        IntegerToObjectType.Add(i,c);       
                    }
                }
            }
        }
    }

    class UnmanagedImageMemory
    {
        public IntPtr pNativeDataStruct;
        public IntPtr pNativeDataFrame;

        public UnmanagedImageMemory()
        {
            pNativeDataFrame = IntPtr.Zero;
            pNativeDataStruct = IntPtr.Zero;
        }
    }

}
