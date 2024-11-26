using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Common;

namespace TrajectoryAnalyzer
{
    public struct TrackedObject
    {
        public string ObjectType;
        public List<StateEstimate> StateHistory;
    }
}
