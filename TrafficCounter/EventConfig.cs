using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace VTC
{ 
    [DataContract]
    public class RegionConfig
    {
        [DataMember]
        public Polygon RoiMask;
        [DataMember]
        public Dictionary<string, Polygon> Regions;
        [DataMember]
        private int ConfigVersion = 0;

        // Required to update this class.  See "Load"
        private static readonly int CurrentConfigVersion = 1;

        public RegionConfig()
        {
            RoiMask = new Polygon();
            Regions = new Dictionary<string, Polygon>();
        }

        public RegionConfig DeepCopy()
        {
            var copy = new RegionConfig();

            foreach (var pt in RoiMask)
            {
                copy.RoiMask.Add(new Point(pt.X, pt.Y));
            }

            foreach (var kvp in Regions)
            {
                copy.Regions[kvp.Key] = new Polygon();
                foreach (var pt in kvp.Value)
                {
                    copy.Regions[kvp.Key].Add(new Point(pt.X, pt.Y));
                }
            }

            return copy;
        }

        public void Save(string path)
        {
            using (var f = System.IO.File.Create(path))
            {
                this.ConfigVersion = CurrentConfigVersion;
                DataContractSerializer s = new DataContractSerializer(this.GetType());
                s.WriteObject(f, this);
            }
        }

        public static RegionConfig Load(string path)
        {
            try
            {
                using (var f = System.IO.File.OpenRead(path))
                {
                    DataContractSerializer s = new DataContractSerializer(typeof(RegionConfig));
                    var pgr = (RegionConfig)s.ReadObject(f);

                    if (pgr.ConfigVersion < CurrentConfigVersion)
                    {
                        return null;
                    }

                    return pgr;
                }
            }
            catch
            {
                return null;
            }
        }

    }
}
