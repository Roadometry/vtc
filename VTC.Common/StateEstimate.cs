using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra.Double;
using Npgsql;
using System.Data.SQLite;

namespace VTC.Common
{
    /// <summary>
    /// Holds 2D position and velocity estimates for Kalman filtering
    /// </summary>
    [DataContract]
    public class StateEstimate
    {
        //public Measurement measurements;
        public double X;
        public double Y;
        public double CovX;          //Location covariances
        public double CovY;

        public double Vx;             //Velocity estimates
        public double Vy;
        public double CovVx;         //Velocity covariances
        public double CovVy;

        public double Red;
        public double Green;
        public double Blue;

        public double CovRed;
        public double CovGreen;
        public double CovBlue;

        public double Size;
        public double CovSize;

        public double VSize;
        public double CovVSize;

        public double PathLength;    //Total path length traveled so far

        public int TotalMissedDetections; //Total number of times this object has not been detected during its lifetime
        public int SuccessiveMissedDetections; //Successive number of times this object has not been detected since the last detection

        public long MovementId;

        public Dictionary<int, int> ClassDetectionCounts = new Dictionary<int, int>();

        public StateEstimate PropagateStateNoMeasurement(double timestep, DenseMatrix H, DenseMatrix R, DenseMatrix F, DenseMatrix Q, double compensationGain)
        {
            var updatedState = new StateEstimate
            {
                TotalMissedDetections = TotalMissedDetections + 1,
                SuccessiveMissedDetections = SuccessiveMissedDetections + 1,
                PathLength =
                    PathLength + Math.Sqrt(Math.Pow((timestep * Vx), 2) + Math.Pow((timestep * Vy), 2))
            };

            var zEst = new DenseMatrix(9, 1)
            {
                [0, 0] = X,
                [1, 0] = Vx,
                [2, 0] = Y,
                [3, 0] = Vy,
                [4, 0] = Red,
                [5, 0] = Green,
                [6, 0] = Blue,
                [7, 0] = Size,
                [8,0] = VSize
            }; //9-Row state vector: x, vx, y, vy, R, G, B, s

            var pBar = new DenseMatrix(9, 9)
            {
                [0, 0] = CovX,
                [1, 1] = CovVx,
                [2, 2] = CovY,
                [3, 3] = CovVy,
                [4, 4] = CovRed,
                [5, 5] = CovGreen,
                [6, 6] = CovBlue,
                [7, 7] = CovSize,
                [8, 8] = CovVSize
            };

            //DenseMatrix B = H * P_bar * H;
            var zNext = F * zEst;
            var fTranspose = (DenseMatrix)F.Transpose();
            var pNext = (F * pBar * fTranspose) + compensationGain * Q;

            //Move values from matrix form into object properties
            updatedState.X = zNext[0, 0];
            updatedState.Y = zNext[2, 0];
            updatedState.Vx = zNext[1, 0];
            updatedState.Vy = zNext[3, 0];
            updatedState.Red = Math.Abs(zNext[4, 0]);
            updatedState.Green = Math.Abs(zNext[5, 0]);
            updatedState.Blue = Math.Abs(zNext[6, 0]);
            updatedState.Size = Math.Abs(zNext[7, 0]);
            updatedState.VSize = zNext[8, 0];

            updatedState.CovX = pNext[0, 0];
            updatedState.CovVx = pNext[1, 1];
            updatedState.CovY = pNext[2, 2];
            updatedState.CovVy = pNext[3, 3];
            updatedState.CovRed = pNext[4, 4];
            updatedState.CovGreen = pNext[5, 5];
            updatedState.CovBlue = pNext[6, 6];
            updatedState.CovSize = pNext[7, 7];
            updatedState.CovVSize = pNext[8, 8];

            updatedState.ClassDetectionCounts = ClassDetectionCounts;

            return updatedState;
        }

        public StateEstimate PropagateState(double timestep, DenseMatrix H, DenseMatrix R, DenseMatrix F, DenseMatrix Q, Measurement measurements)
        {
            var updatedState = new StateEstimate
            {
                PathLength =
                    PathLength + Math.Sqrt(Math.Pow((timestep * Vx), 2) + Math.Pow((timestep * Vy), 2))
            };

            var zEst = new DenseMatrix(9, 1)
            {
                [0, 0] = X,
                [1, 0] = Vx,
                [2, 0] = Y,
                [3, 0] = Vy,
                [4, 0] = Red,
                [5, 0] = Green,
                [6, 0] = Blue,
                [7, 0] = Size,
                [8,0] = VSize
            }; //9-Row state vector: x, vx, y, vy, r, g, b, Size, VSize

            var zMeas = new DenseMatrix(6, 1)
            {
                [0, 0] = measurements.X,
                [1, 0] = measurements.Y,
                [2, 0] = measurements.Red,
                [3, 0] = measurements.Green,
                [4, 0] = measurements.Blue,
                [5, 0] = measurements.Size
            }; //6-Row measurement vector: x,y,r,g,b, size

            var pBar = new DenseMatrix(9, 9)
            {
                [0, 0] = CovX,
                [1, 1] = CovVx,
                [2, 2] = CovY,
                [3, 3] = CovVy,
                [4, 4] = CovRed,
                [5, 5] = CovGreen,
                [6, 6] = CovBlue,
                [7, 7] = CovSize,
                [8,8] = CovVSize
            };

            //DenseMatrix B = H * P_bar * H;
            var zNext = F * zEst;
            var fTranspose = (DenseMatrix)F.Transpose();
            var pNext = (F * pBar * fTranspose) + Q;
            var yResidual = zMeas - H * zNext;
            var hTranspose = (DenseMatrix)H.Transpose();
            var s = H * pNext * hTranspose + R;
            var sInv = (DenseMatrix)s.Inverse();
            var k = pNext * hTranspose * sInv;
            var zPost = zNext + k * yResidual;
            var pPost = (DenseMatrix.CreateIdentity(9) - k * H) * pNext;

            //Move values from matrix form into object properties
            updatedState.CovX = pPost[0, 0];
            updatedState.CovVx = pPost[1, 1];
            updatedState.CovY = pPost[2, 2];
            updatedState.CovVy = pPost[3, 3];
            updatedState.CovRed = pPost[4, 4];
            updatedState.CovGreen = pPost[5, 5];
            updatedState.CovBlue = pPost[6, 6];
            updatedState.CovSize = pPost[7, 7];
            updatedState.CovVSize = pPost[8, 8];

            updatedState.X = zPost[0, 0];
            updatedState.Vx = zPost[1, 0];
            updatedState.Y = zPost[2, 0];
            updatedState.Vy = zPost[3, 0];
            updatedState.Red = zPost[4, 0];
            updatedState.Green = zPost[5, 0];
            updatedState.Blue = zPost[6, 0];
            updatedState.Size = zPost[7, 0];
            updatedState.VSize = zPost[8, 0];

            updatedState.ClassDetectionCounts = ClassDetectionCounts;
            if (updatedState.ClassDetectionCounts.ContainsKey(measurements.ObjectClass))
            {
                updatedState.ClassDetectionCounts[measurements.ObjectClass]++;
            }
            else
            {
                updatedState.ClassDetectionCounts.Add(measurements.ObjectClass, 1);
            }

            return updatedState;
        }

        public static DenseMatrix Residual(DenseMatrix zEst, DenseMatrix zMeas)
        {
            var residual = zMeas - zEst;
            return residual;
        }

        public int MostFrequentClassId()
        {
            if (ClassDetectionCounts.Count > 0)
            {
                var sortedCounts = ClassDetectionCounts.ToList();
                sortedCounts.Sort((x, y) => (x.Value.CompareTo(y.Value)));
                KeyValuePair<int, int> mostCommon = sortedCounts.Last();
                return mostCommon.Key;
            }
            
            return -1;
        }

        public void Save(DbConnection dbConnection)
        {
            try
            {
                var cmd = dbConnection.CreateCommand();
                cmd.CommandText =
                    $"INSERT INTO stateestimate(movementid,x,y,vx,vy,red,blue,green,size,vsize,pathlength) VALUES({MovementId},{X},{Y},{Vx},{Vy},{Red},{Blue},{Green},{Size},{VSize},{PathLength})";
                var result = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Debug.WriteLine("SaveDetection:" + e.Message);
            }
        }

    }

    public class StateEstimateList : List<StateEstimate>
    {
        public double Smoothness()
        {
            var xPositions = this.Select(se => se.Vx).ToArray();
            var xAutocorrelation = MathNet.Numerics.Statistics.Correlation.Auto(xPositions, 1, 1).Sum();

            var yPositions = this.Select(se => se.Vy).ToArray();
            var yAutocorrelation = MathNet.Numerics.Statistics.Correlation.Auto(yPositions, 1, 1).Sum();

            var totalAutocorrelation = xAutocorrelation + yAutocorrelation;

            return totalAutocorrelation;
        }
    }



}
