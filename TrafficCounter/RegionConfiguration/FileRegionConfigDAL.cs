using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NLog;
using VTC.Common.RegionConfig;

namespace VTC.RegionConfiguration
{
    public class FileRegionConfigDal : IRegionConfigDataAccessLayer
    {

        private static readonly Logger Logger = LogManager.GetLogger("vista");

        private readonly string _path;

        public FileRegionConfigDal(string path)
        {
            _path = path;
        }

        public List<RegionConfig> LoadRegionConfigList()
        {
            if (!File.Exists(_path))
            {
                return new List<RegionConfig>();
            }

            using (var file = File.OpenRead(_path))
            {

                DataContractSerializer s = new DataContractSerializer(typeof(IEnumerable<RegionConfig>));
                var regionConfigs = new RegionConfig[0];
                
                try
                {
                    regionConfigs = (RegionConfig[])s.ReadObject(file);
                    int titleNum = 1;
                    foreach (var regionConfig in regionConfigs)
                    {
                        foreach (var region in regionConfig.Regions)
                        {
                            if (region.Value.PolygonClosed)
                                region.Value.UpdateCentroid();
                        }

                        if (string.IsNullOrWhiteSpace(regionConfig.Title))
                        {
                            string title;
                            do
                            {
                                title = "Region Configuration " + titleNum++;
                            } while (regionConfigs.Any(rc => title.ToLowerInvariant().Equals((rc.Title ?? string.Empty).ToLowerInvariant())));
                            regionConfig.Title = title;
                        }

                        regionConfig.RoiMask.UpdateCentroid();
                        regionConfig.SanitizeBadValues();

                        if (regionConfig.ExamplePaths == null)
                        {
                            regionConfig.ExamplePaths = new List<ExamplePath>();
                        }
                    }
                }
                catch (SerializationException ex)
                {
                 Logger.Log(LogLevel.Error, ex, "Serialization exception reading configuration");   
                }
                

                return regionConfigs.ToList();
            }
        }

        public void SaveRegionConfigList(List<RegionConfig> regionConfigs)
        {
            byte[] serialized;

            // Ensure the new items can be serialized successfully before deleting the old file
            using (var stream = new MemoryStream())
            {
                DataContractSerializer s = new DataContractSerializer(typeof(List<RegionConfig>));
                s.WriteObject(stream, regionConfigs);

                serialized = stream.ToArray();
            }

            File.WriteAllBytes(_path, serialized);
        }
    }
}
