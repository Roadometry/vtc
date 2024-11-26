using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Common;
using VTC.Common.RegionConfig;
using VTC.Reporting;
using System.Threading;
using System.Data.SQLite;

namespace VTC.Kernel
{
    public class MultipleTrajectorySynthesizer
    {

        public List<Movement> TrajectoryPrototypes = new List<Movement>();

        public void GenerateSyntheticTrajectories(RegionConfig regionConfig)
        {
            TrajectoryPrototypes.Clear();

            var approach1 = regionConfig.Regions.FirstOrDefault(kvp => kvp.Key.Contains("Approach 1")).Value;
            var approach2 = regionConfig.Regions.FirstOrDefault(kvp => kvp.Key.Contains("Approach 2")).Value;
            var approach3 = regionConfig.Regions.FirstOrDefault(kvp => kvp.Key.Contains("Approach 3")).Value;
            var approach4 = regionConfig.Regions.FirstOrDefault(kvp => kvp.Key.Contains("Approach 4")).Value;

            var exit1 = regionConfig.Regions.FirstOrDefault(kvp => kvp.Key.Contains("Exit 1")).Value;
            var exit2 = regionConfig.Regions.FirstOrDefault(kvp => kvp.Key.Contains("Exit 2")).Value;
            var exit3 = regionConfig.Regions.FirstOrDefault(kvp => kvp.Key.Contains("Exit 3")).Value;
            var exit4 = regionConfig.Regions.FirstOrDefault(kvp => kvp.Key.Contains("Exit 4")).Value;

            var sidewalk1 = regionConfig.Regions.FirstOrDefault(kvp => kvp.Key.Contains("Sidewalk 1")).Value;
            var sidewalk2 = regionConfig.Regions.FirstOrDefault(kvp => kvp.Key.Contains("Sidewalk 2")).Value;
            var sidewalk3 = regionConfig.Regions.FirstOrDefault(kvp => kvp.Key.Contains("Sidewalk 3")).Value;
            var sidewalk4 = regionConfig.Regions.FirstOrDefault(kvp => kvp.Key.Contains("Sidewalk 4")).Value;

            //TODO: Name assignment below is kind of a hack, should not be necessary.
            if (approach1 != null)
            {
                approach1.DisplayName = "Approach 1";
            }

            if (approach2 != null)
            {
                approach2.DisplayName = "Approach 2";
            }

            if (approach3 != null)
            {
                approach3.DisplayName = "Approach 3";
            }

            if (approach4 != null)
            {
                approach4.DisplayName = "Approach 4";
            }

            if (exit1 != null)
            {
                exit1.DisplayName = "Exit 1";
            }

            if (exit2 != null)
            {
                exit2.DisplayName = "Exit 2";
            }

            if (exit3 != null)
            {
                exit3.DisplayName = "Exit 3";
            }

            if (exit4 != null)
            {
                exit4.DisplayName = "Exit 4";
            }

            if (sidewalk1 != null)
            {
                sidewalk1.DisplayName = "Sidewalk 1";
            }

            if (sidewalk2 != null)
            {
                sidewalk2.DisplayName = "Sidewalk 2";
            }

            if (sidewalk3 != null)
            {
                sidewalk3.DisplayName = "Sidewalk 3";
            }

            if (sidewalk4 != null)
            {
                sidewalk4.DisplayName = "Sidewalk 4";
            }

            //Generate Straight movements
            var straightMovements = new List<Movement>();

            if (approach1 != null && exit1 != null)
            {
                var road1StraightMovements = ApproachExitPairToMovements(approach1, exit1, Turn.Straight, ObjectType.Car, 40);
                straightMovements.AddRange(road1StraightMovements);
            }

            if (approach2 != null && exit2 != null)
            {
                var road2StraightMovements = ApproachExitPairToMovements(approach2, exit2, Turn.Straight, ObjectType.Car, 40);
                straightMovements.AddRange(road2StraightMovements);
            }

            if (approach3 != null && exit3 != null)
            {
                var road3StraightMovements = ApproachExitPairToMovements(approach3, exit3, Turn.Straight, ObjectType.Car, 40);
                straightMovements.AddRange(road3StraightMovements);
            }

            if (approach4 != null && exit4 != null)
            {
                var road4StraightMovements = ApproachExitPairToMovements(approach4, exit4, Turn.Straight, ObjectType.Car, 40);
                straightMovements.AddRange(road4StraightMovements);
            }

            TrajectoryPrototypes.AddRange(straightMovements);

            //Generate turn movements
            var turnMovements = new List<Movement>();

            if(approach1 != null && approach2 != null && exit1 != null && exit2 != null)
            {
                var road12Movements = RoadPairToTurnMovements(approach1, exit1, approach2, exit2, Turn.Left, ObjectType.Car, 20);
                turnMovements.AddRange(road12Movements);
                var road21Movements = RoadPairToTurnMovements(approach2, exit2, approach1, exit1, Turn.Right, ObjectType.Car, 20);
                turnMovements.AddRange(road21Movements);
            }

            if (approach2 != null && approach3 != null && exit2 != null && exit3 != null)
            {
                var road23Movements = RoadPairToTurnMovements(approach2, exit2, approach3, exit3, Turn.Left, ObjectType.Car, 20);
                turnMovements.AddRange(road23Movements);
                var road32Movements = RoadPairToTurnMovements(approach3, exit3, approach2, exit2, Turn.Right, ObjectType.Car, 20);
                turnMovements.AddRange(road32Movements);
            }

            if (approach3 != null && approach4 != null && exit3 != null && exit4 != null)
            {
                var road34Movements = RoadPairToTurnMovements(approach3, exit3, approach4, exit4, Turn.Left, ObjectType.Car, 20);
                turnMovements.AddRange(road34Movements);
                var road43Movements = RoadPairToTurnMovements(approach4, exit4, approach3, exit3, Turn.Right, ObjectType.Car, 20);
                turnMovements.AddRange(road43Movements);
            }

            if (approach4 != null && approach1 != null && exit4 != null && exit1 != null)
            {
                var road41Movements = RoadPairToTurnMovements(approach4, exit4, approach1, exit1, Turn.Left, ObjectType.Car, 20);
                turnMovements.AddRange(road41Movements);
                var road14Movements = RoadPairToTurnMovements(approach1, exit1, approach4, exit4, Turn.Right, ObjectType.Car, 20);
                turnMovements.AddRange(road14Movements);
            }

            //U-Turns
            if (regionConfig.DisableUTurns == false)
            {
                if (approach1 != null && approach3 != null && exit1 != null && exit3 != null)
                {
                    var road13Movements = RoadPairToTurnMovements(approach1, exit1, approach3, exit3, Turn.UTurn, ObjectType.Car, 20);
                    turnMovements.AddRange(road13Movements);
                    var road31Movements = RoadPairToTurnMovements(approach3, exit3, approach1, exit1, Turn.UTurn, ObjectType.Car, 20);
                    turnMovements.AddRange(road31Movements);
                }

                if (approach2 != null && approach4 != null && exit2 != null && exit4 != null)
                {
                    var road24Movements = RoadPairToTurnMovements(approach2, exit2, approach4, exit4, Turn.UTurn, ObjectType.Car, 20);
                    turnMovements.AddRange(road24Movements);
                    var road42Movements = RoadPairToTurnMovements(approach4, exit4, approach2, exit2, Turn.UTurn, ObjectType.Car, 20);
                    turnMovements.AddRange(road42Movements);
                }
            }

            TrajectoryPrototypes.AddRange(turnMovements);

            //Generate pedestrian Crossing movements
            var pedestrianMovements = new List<Movement>();

            if (sidewalk1 != null && sidewalk2 != null)
            {
                var crossing12Movements = ApproachExitPairToMovements(sidewalk1, sidewalk2, Turn.Crossing, ObjectType.Person, 4);
                pedestrianMovements.AddRange(crossing12Movements);

                var crossing21Movements = ApproachExitPairToMovements(sidewalk2, sidewalk1, Turn.Crossing, ObjectType.Person, 4);
                pedestrianMovements.AddRange(crossing21Movements);
            }

            if (sidewalk3 != null && sidewalk2 != null)
            {
                var crossing23Movements = ApproachExitPairToMovements(sidewalk2, sidewalk3, Turn.Crossing, ObjectType.Person, 4);
                pedestrianMovements.AddRange(crossing23Movements);

                var crossing32Movements = ApproachExitPairToMovements(sidewalk3, sidewalk2, Turn.Crossing, ObjectType.Person, 4);
                pedestrianMovements.AddRange(crossing32Movements);
            }

            if (sidewalk3 != null && sidewalk4 != null)
            {
                var crossing34Movements = ApproachExitPairToMovements(sidewalk3, sidewalk4, Turn.Crossing, ObjectType.Person, 4);
                pedestrianMovements.AddRange(crossing34Movements);

                var crossing43Movements = ApproachExitPairToMovements(sidewalk4, sidewalk3, Turn.Crossing, ObjectType.Person, 4);
                pedestrianMovements.AddRange(crossing43Movements);
            }

            if (sidewalk1 != null && sidewalk4 != null)
            {
                var crossing14Movements = ApproachExitPairToMovements(sidewalk1, sidewalk4, Turn.Crossing, ObjectType.Person, 4);
                pedestrianMovements.AddRange(crossing14Movements);

                var crossing41Movements = ApproachExitPairToMovements(sidewalk4, sidewalk1, Turn.Crossing, ObjectType.Person, 4);
                pedestrianMovements.AddRange(crossing41Movements);
            }

            TrajectoryPrototypes.AddRange(pedestrianMovements);
            
            var examplePaths = GenerateExamplePathTrajectories(regionConfig);
            TrajectoryPrototypes.AddRange(examplePaths);
        }

        public List<Movement> GenerateExamplePathTrajectories(RegionConfig config)
        {
            List<Movement> examplePaths = new List<Movement>();
            foreach (var path in config.ExamplePaths)
            {
                var stateEstimates = new StateEstimateList();
                foreach (var pt in path.Points)
                {
                    var se = new StateEstimate
                    {
                        X = pt.X,
                        Y = pt.Y
                    };
                    stateEstimates.Add(se);
                }

                if (stateEstimates.Count < 2)
                {
                    //I'm not entirely sure why this is necessary, but logs from a user computer where the LoggingActor is crashing
                    //suggest that cases are occuring where the length of the stateEstimates list is less than 2, because accessing it at index 1
                    //causes a crash. 
                    //TODO: Investigate how it's possible for the stateEstimates length to be 2 or less (maybe the path is just drawn with 2 points?).
                    continue;
                }

                for (int i = 1; i < stateEstimates.Count; i++)
                {
                    stateEstimates[i].Vx = stateEstimates[i].X - stateEstimates[i - 1].X;
                    stateEstimates[i].Vy = stateEstimates[i].Y - stateEstimates[i - 1].Y;
                }

                stateEstimates[0].Vx = stateEstimates[1].Vx;
                stateEstimates[0].Vy = stateEstimates[1].Vy;

                var m = new Movement(path.Approach, path.Exit, path.TurnType, ObjectType.Car, stateEstimates, DateTime.Now, path.Ignored);
                examplePaths.Add(m);
            }

            return examplePaths;
        }

        List<Movement> ApproachExitPairToMovements(Polygon approach, Polygon exit, Turn turnType, ObjectType objectType, int max)
        {
            var movements = new ConcurrentBag<Movement>();

            if(approach.Count < 1 || exit.Count < 1)
            {
                return movements.ToList();
            }

            //Build list of possible approach points
            var approachVertexList = approach.ToList();
            var approachCentroid = new System.Drawing.Point();
            approachCentroid.X = approach.Centroid.X;
            approachCentroid.Y = approach.Centroid.Y;
            approachVertexList.Add(approachCentroid);

            //Build list of possible exit points
            var exitVertexList = exit.ToList();
            var exitCentroid = new System.Drawing.Point();
            exitCentroid.X = exit.Centroid.X;
            exitCentroid.Y = exit.Centroid.Y;
            exitVertexList.Add(exitCentroid);

            //Parallel.ForEach(approachVertexList, approachVertex => 
            foreach(var approachVertex in approachVertexList)
            { 
                foreach(var exitVertex in exitVertexList)
                {
                    var roadline = new RoadLine();
                    roadline.ApproachCentroidX = approachVertex.X;
                    roadline.ApproachCentroidY = approachVertex.Y;
                    roadline.ExitCentroidX = exitVertex.X;
                    roadline.ExitCentroidY = exitVertex.Y;

                    var trackedObject = PolygonTrajectorySynthesizer.SyntheticTrajectory(approachVertex, exitVertex, roadline);
                    var movement = new Movement(approach.DisplayName, exit.DisplayName, turnType, objectType, trackedObject.StateHistory, DateTime.Now, false);
                    movements.Add(movement);
                }
            }

            var movementsList = movements.ToList();
            MyExtensions.Shuffle(movementsList);
            var movementsListReduced = movementsList.Take(max).ToList();
            return movementsListReduced;
        }

        List<Movement> RoadPairToTurnMovements(Polygon approachA, Polygon exitA, Polygon approachB, Polygon exitB, Turn turnType, ObjectType objectType, int max)
        { 
             var movements = new ConcurrentBag<Movement>();

            if(approachA.Count < 1 || exitA.Count < 1 || approachB.Count < 1 || exitB.Count < 1)
            {
                return movements.ToList();
            }

            //Build lists of possible approach points
            var approachAVertexList = approachA.ToList();
            var approachACentroid = new System.Drawing.Point();
            approachACentroid.X = approachA.Centroid.X;
            approachACentroid.Y = approachA.Centroid.Y;
            approachAVertexList.Add(approachACentroid);

            var approachBVertexList = approachB.ToList();
            var approachBCentroid = new System.Drawing.Point();
            approachBCentroid.X = approachB.Centroid.X;
            approachBCentroid.Y = approachB.Centroid.Y;
            approachBVertexList.Add(approachBCentroid);

            //Build list of possible exit points
            var exitAVertexList = exitA.ToList();
            var exitACentroid = new System.Drawing.Point();
            exitACentroid.X = exitA.Centroid.X;
            exitACentroid.Y = exitA.Centroid.Y;
            exitAVertexList.Add(exitACentroid);

            var exitBVertexList = exitB.ToList();
            var exitBCentroid = new System.Drawing.Point();
            exitBCentroid.X = exitB.Centroid.X;
            exitBCentroid.Y = exitB.Centroid.Y;
            exitBVertexList.Add(exitBCentroid);

            //Parallel.ForEach(approachAVertexList, approachVertex => 
            foreach(var approachVertex in approachAVertexList)
            { 
                foreach(var exitVertex in exitBVertexList)
                {
                    var approachRoadlines = GenerateRoadlinesFromVertex(approachVertex,exitAVertexList);
                    var exitRoadlines = GenerateRoadlinesToVertex(approachBVertexList, exitVertex);
                    foreach(var approachRoadline in approachRoadlines)
                    { 
                        foreach(var exitRoadline in exitRoadlines)
                        { 
                            var trackedObject = PolygonTrajectorySynthesizer.SyntheticTrajectory(approachVertex, exitVertex, approachRoadline, exitRoadline);
                            var movement = new Movement(approachA.DisplayName, exitB.DisplayName, turnType, objectType, trackedObject.StateHistory, DateTime.Now,false);
                            movements.Add(movement);
                        }
                    }
                }
            }
            
            var movementsList = movements.ToList();
            MyExtensions.Shuffle(movementsList);
            var movementsListReduced = movementsList.Take(max).ToList();
            return movementsListReduced;
        }

        List<RoadLine> GenerateRoadlinesFromVertex(System.Drawing.Point vertex, List<System.Drawing.Point> matchVertices)
        {
            var roadlines = new List<RoadLine>();
            foreach(var mv in matchVertices)
            {
                var roadline = new RoadLine();
                roadline.ApproachCentroidX = vertex.X;
                roadline.ApproachCentroidY = vertex.Y;
                roadline.ExitCentroidX = mv.X;
                roadline.ExitCentroidY = mv.Y;
                roadlines.Add(roadline);
            }
            return roadlines;
        }

        List<RoadLine> GenerateRoadlinesToVertex(List<System.Drawing.Point> matchVertices, System.Drawing.Point vertex)
        {
            var roadlines = new List<RoadLine>();
            foreach(var mv in matchVertices)
            {
                var roadline = new RoadLine();
                roadline.ApproachCentroidX = mv.X;
                roadline.ApproachCentroidY = mv.Y;
                roadline.ExitCentroidX = vertex.X;
                roadline.ExitCentroidY = vertex.Y;
                roadlines.Add(roadline);
            }
            return roadlines;
        }
    }

  public static class ThreadSafeRandom
  {
      [ThreadStatic] private static Random Local;

      public static Random ThisThreadsRandom
      {
        //Note: We explicitly do not want a random seed here. We want a pseudo-random shuffle of the list so that count results are the same
        //from one run to the next, one PC to the next, etc.
          get { return Local ?? (Local = new Random(0)); }
      }
  }

  static class MyExtensions
  {
    public static void Shuffle<T>(this IList<T> list)
    {
      int n = list.Count;
      while (n > 1)
      {
        n--;
        int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
        T value = list[k];
        list[k] = list[n];
        list[n] = value;
      }
    }
  }
}
