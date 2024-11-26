using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VTC.Common;
using VTC.Common.RegionConfig;
using VTC.Kernel;
using VTC.Reporting;

namespace VTC.KernelTests
{
    [TestClass]
    public class TrajectorySimiliarityUnitTests
    {

        private MultipleTrajectorySynthesizer mts;
        private RegionConfig regionConfig;

        public TrajectorySimiliarityUnitTests()
        {
            //Create regions
            GenerateRegionConfigPolygons();

            //Create roads
            mts = new MultipleTrajectorySynthesizer();
            mts.GenerateSyntheticTrajectories(regionConfig, Path.GetTempPath());
        }

        [TestMethod]
        public void Approach1StraightIdentifiedCorrectly()
        {
            //Create StateHistory
            var d = new TrackedObject();
            d.StateHistory = new StateEstimateList();
            PopulateStateHistory(d, 50.0, 80.0, 0.0, -3.0, 0.0, 0.0, 25);
            var matchedTrajectoryName = TrajectorySimilarity.BestMatchTrajectory(d.StateHistory, mts.TrajectoryPrototypes, "Car");
            var tl = new TrajectoryLogger(matchedTrajectoryName);
            tl.Save(Path.GetTempPath() + "/TestTrajectories");
            Assert.Equals(matchedTrajectoryName.TurnType, Turn.Straight);
        }

        [TestMethod]
        public void Approach1LeftIdentifiedCorrectly()
        {
            //Create StateHistory
            var d = new TrackedObject();
            d.StateHistory = new StateEstimateList();
            PopulateStateHistory(d, 50.0, 80.0, -3.0, -3.0, 0.0, 0.0, 25);

            var matchedTrajectoryName = TrajectorySimilarity.BestMatchTrajectory(d.StateHistory, mts.TrajectoryPrototypes, "Car");
            Assert.Equals(matchedTrajectoryName.TurnType, Turn.Left);
        }

        [TestMethod]
        public void Approach1RightIdentifiedCorrectly()
        {
            //Create StateHistory
            var d = new TrackedObject();
            d.StateHistory = new StateEstimateList();
            PopulateStateHistory(d, 50.0, 80.0, 3.0, 3.0, 0.0, 0.0, 25);

            var matchedTrajectoryName = TrajectorySimilarity.BestMatchTrajectory(d.StateHistory, mts.TrajectoryPrototypes, "Car");
            Assert.Equals(matchedTrajectoryName.TurnType, Turn.Right);
        }

        private void GenerateRegionConfigPolygons()
        {
            regionConfig = new RegionConfig();

            var a1 = GeneratePolygonsAroundPoint(50, 80);
            var a2 = GeneratePolygonsAroundPoint(70, 30);
            var a3 = GeneratePolygonsAroundPoint(30, 10);
            var a4 = GeneratePolygonsAroundPoint(20, 60);

            var e1 = GeneratePolygonsAroundPoint(50, 10);
            var e2 = GeneratePolygonsAroundPoint(20, 30);
            var e3 = GeneratePolygonsAroundPoint(30, 80);
            var e4 = GeneratePolygonsAroundPoint(70, 60);

            regionConfig.Regions.Clear();
            regionConfig.Regions.Add("Approach 1", a1);
            regionConfig.Regions.Add("Approach 2", a2);
            regionConfig.Regions.Add("Approach 3", a3);
            regionConfig.Regions.Add("Approach 4", a4);

            regionConfig.Regions.Add("Exit 1", e1);
            regionConfig.Regions.Add("Exit 2", e2);
            regionConfig.Regions.Add("Exit 3", e3);
            regionConfig.Regions.Add("Exit 4", e4);
        }

        private Polygon GeneratePolygonsAroundPoint(int x, int y)
        {
            var p = new Polygon();

            var ul = new Point(x-1, y-1);
            var ur = new Point(x + 1, y - 1);
            var ll = new Point(x - 1, y + 1);
            var lr = new Point(x+1,y+1);

            p.Add(ul);
            p.Add(ur);
            p.Add(ll);
            p.Add(lr);
            p.Add(ul);

            p.UpdateCentroid();

            return p;
        }

        private void PopulateStateHistory(TrackedObject vehicle, double x, double y, double vx, double vy, double ax, double ay, int num)
        {
            var initial = new StateEstimate();
            initial.X = x;
            initial.Y = y;
            initial.Vx = vx;
            initial.Vy = vy;
            vehicle.StateHistory.Add(initial);

            for (int i = 1; i < num; i++)
            {
                var previous = vehicle.StateHistory.ElementAt(i - 1);
                var nextState = new StateEstimate();
                nextState.X = previous.X + previous.Vx;
                nextState.Vx = previous.Vx + ax;
                nextState.Y = previous.Y + previous.Vy;
                nextState.Vy = previous.Vy + ay;
                vehicle.StateHistory.Add(nextState);
            }
        }
    }
}
