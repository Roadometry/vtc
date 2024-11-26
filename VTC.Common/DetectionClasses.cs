using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTC.Common
{
    public static class DetectionClasses
    {
        public static readonly ObjectType[] ClassDetectionWhitelist = { ObjectType.Car, ObjectType.Person, ObjectType.Bicycle, ObjectType.Bus, ObjectType.Motorcycle, ObjectType.Truck };
    }
}
