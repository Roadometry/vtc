using Microsoft.VisualStudio.TestTools.UnitTesting;
using VTC.Kernel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using VTC.Common;
using VTC.Common.RegionConfig;
using SimulatedScene;

namespace VTC.KernelTests
{
    [TestClass()]
    public class MultipleHypothesisTrackerTests
    {
        /// <summary>
        /// Tracking simple linear motion with detections in every frame, and no position-noise.
        /// </summary>
        [TestMethod()]
        public void UpdateTest()
        {
            var vf = new VelocityField(640, 480);
            var mht = new MultipleHypothesisTracker(new RegionConfig(), vf);
            var ct = new CancellationToken();
            var scene = new Scene();
            var history = new List<Measurement[]>();

            var vehicle = new SimulatedObject
            {
                X = 0,
                Y = 0,
                vX = 0.1,
                vY = 0.2,
                Height = 20,
                Width = 40,
                R = 65,
                G = 60,
                B = 55,
                ObjectClass = "car",
                destinationX = 100,
                destinationY = 200
            };

            scene.SceneObjects.Add(vehicle);

            var m1 = scene.GetMeasurements();
            history.Add(m1);

            scene.LinearMotionTimestep();
            var m2 = scene.GetMeasurements();
            history.Add(m2);

            scene.LinearMotionTimestep();
            var m3 = scene.GetMeasurements();
            history.Add(m3);

            scene.LinearMotionTimestep();
            var m4 = scene.GetMeasurements();
            history.Add(m4);

            scene.LinearMotionTimestep();
            var m5 = scene.GetMeasurements();
            history.Add(m5);

        }
    }
}