using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VTC.Kernel;

namespace VTC.KernelTests
{
    [TestClass]
    public class PolygonTrajectorySynthesizerUnitTests
    {
        [TestMethod]
        public void IntersectionIsCalculated()
        {
            var road1 = new RoadLine();
            var road2 = new RoadLine();
            road1.ApproachCentroidX = 2;
            road1.ApproachCentroidY = 0;
            road1.ExitCentroidX = 2;
            road1.ExitCentroidY = 1;

            road2.ApproachCentroidX = 0;
            road2.ApproachCentroidY = 2;
            road2.ExitCentroidX = 1;
            road2.ExitCentroidY = 2;

            var intersection = PolygonTrajectorySynthesizer.Intersection(road1, road2);

            Assert.AreEqual(2, intersection.X, "Intersection X-coordinate should be 2.");
            Assert.AreEqual(2, intersection.Y, "Intersection Y-coordinate should be 2.");
        }
    }
}
