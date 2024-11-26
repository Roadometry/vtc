using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NLog;
using VTC.Common;
using VTC.Common.RegionConfig;

namespace VTC.UserConfiguration
{
    public class FileUserConfigDal : IUserConfigDataAccessLayer
    {

        private static readonly Logger Logger = LogManager.GetLogger("vista");

        private readonly string _path;

        public FileUserConfigDal(string path)
        {
            _path = path;
        }

        public UserConfig LoadUserConfig()
        {
            if (!File.Exists(_path))
            {
                return new UserConfig();
            }

            using (var file = File.OpenRead(_path))
            {
                DataContractSerializer s = new DataContractSerializer(typeof(UserConfig));
                var userConfig = new UserConfig();
                
                try
                {
                    userConfig = (UserConfig)s.ReadObject(file);
                }
                catch (SerializationException ex)
                {
                 Logger.Log(LogLevel.Error, ex, "Serialization exception reading configuration");   
                }
                
                return userConfig;
            }
        }

        public void SaveUserConfig(UserConfig userConfig)
        {
            byte[] serialized;

            // Ensure the new items can be serialized successfully before deleting the old file
            using (var stream = new MemoryStream())
            {
                DataContractSerializer s = new DataContractSerializer(typeof(UserConfig));
                s.WriteObject(stream, userConfig);

                serialized = stream.ToArray();
            }

            File.WriteAllBytes(_path, serialized);
        }
    }
}
