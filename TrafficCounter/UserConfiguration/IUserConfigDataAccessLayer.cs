using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using VTC.Common;

namespace VTC.UserConfiguration
{
    public interface IUserConfigDataAccessLayer
    {
        UserConfig LoadUserConfig();
        void SaveUserConfig(UserConfig userConfig);
    }
}
