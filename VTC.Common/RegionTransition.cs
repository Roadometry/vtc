using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTC.Common
{
    public struct RegionTransition
    {
        public string InRegion;
        public string OutRegion;

        public RegionTransition(int inRegion, int outRegion, bool sidewalk)
        {
            if (sidewalk)
            {
                InRegion = "Sidewalk " + inRegion;
                OutRegion = "Sidewalk " + outRegion;
            }
            else
            {
                InRegion = "Approach " + inRegion;
                OutRegion = "Exit " + outRegion;    
            }
        }
    }

}
