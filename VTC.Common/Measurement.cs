
using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace VTC.Common
{
    [DataContract]
    public class Measurement
    {
        [DataMember] public double X;
        [DataMember] public double Y;
        [DataMember] public double Red;
        [DataMember] public double Green;
        [DataMember] public double Blue;
        [DataMember] public double Size;
        [DataMember] public double Width;
        [DataMember] public double Height;
        [DataMember] public int ObjectClass;
    }
}
