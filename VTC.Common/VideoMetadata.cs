using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Shell32;

namespace VTC.Common
{
    [DataContract]  
    public class VideoMetadata
    {
        [DataMember]  
        public string VideoPath;

        [DataMember] 
        public string FileDate;

        public static VideoMetadata ExtractFromVideo(string path)
        {
            var vm = new VideoMetadata();
            vm.VideoPath = path;
            var creationTime = File.GetCreationTime(path);
            vm.FileDate = creationTime.ToLongDateString() + " " + creationTime.ToLongTimeString();
            return vm;
        }
    }
}
