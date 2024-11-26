using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace VTC.Classifier
{
    public class GPUDetector
    {
        public bool HasGPU = false;
        public int NumGPUs = 0;
        public int MB_VRAM = 0;

        public GPUDetector()
        {
            DetectGPUs();
        }

        public void DetectGPUs()
        {
            Process.Start(".\\deviceQuery.exe");

            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = ".\\deviceQuery.exe";
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.Start();
            string strOutput = pProcess.StandardOutput.ReadToEnd();
            pProcess.WaitForExit();

            var detected_pattern = new Regex(@"Detected \d+ CUDA");
            Match match = detected_pattern.Match(strOutput);
            if(match.Success)
            {
                var detectedXCudaStr = match.Value;
                Regex rgx = new Regex("[^0-9]");
                var numStr = rgx.Replace(detectedXCudaStr, "");
                NumGPUs = int.Parse(numStr);
                HasGPU = NumGPUs >= 0;

                if(HasGPU)
                {
                    var split = strOutput.Split(new char[]{'\r'});
                    var memoryString = split.Where(s => s.Contains("Total amount of global memory:")).FirstOrDefault();
                    var bytesIndex = memoryString.IndexOf("(");
                    var trimmedMemoryString = memoryString.Substring(0,bytesIndex);
                    MB_VRAM = int.Parse(rgx.Replace(trimmedMemoryString, ""));
                }
            }            
        }
    }
}
