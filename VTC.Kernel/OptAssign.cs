using System.Collections.Generic;
using System.Threading;

namespace VTC.Kernel
{

    public struct MurtyNode
    {
        //Each int[] element is an assignment pair - [ detection, assigment ]
        public List<int[]> InclusionList; //This node ensures every assignment in the inclusion list
        public List<int[]> ExclusionList; //This node excludes every assignment in the exclusion list

        public MurtyNode(List<int[]> inclusions, List<int[]> exclusions)
        {
            InclusionList = inclusions;
            ExclusionList = exclusions;
        }
    }

    public class OptAssign
    {
        public static List<int[]> FindKBestAssignments(double[,] costs, int k)
        {
            double[,] tempCosts = (double[,]) costs.Clone();
            int numTargets = tempCosts.GetLength(0);
            if (k > numTargets)
                k = numTargets;

            List<int[]> kBestAssignments = new List<int[]>();
            for (int i = 0; i < k; i++)
            {
                int[] thisAssignment = BestAssignment(tempCosts);
                kBestAssignments.Add(thisAssignment);
                if (k > (i + 1))
                {
                    List<MurtyNode> partition = partition_assignment(thisAssignment);
                    MurtyNode bestNode = FindBestNode(tempCosts, partition);
                    tempCosts = adjusted_cost(tempCosts, bestNode);
                }
            }

            return kBestAssignments;
        }

        public static int[] BestAssignment(int[,] costs)
        { 
            int[,] tempCosts = (int[,]) costs.Clone();
            int[] assignment =  tempCosts.FindAssignments();
            return assignment;
        }

        public static int[] BestAssignment(double[,] costs)
        {
            double[,] tempCosts = (double[,])costs.Clone();
            int[] assignment = tempCosts.FindAssignments();
            return assignment;
        }

        public static List<MurtyNode> partition_assignment(int[] assignment)
        {
            List<MurtyNode> partition = new List<MurtyNode>();
            for (int i = 0; i < assignment.Length - 1; i++)
            {
                //Form inclusion list: fix all assignments where j < i
                List<int[]> inclusionList = new List<int[]>();
                for (int j = 0; j < i; j++)
                {
                    int[] inclusion = { j, assignment[j] };
                    inclusionList.Add(inclusion);
                }

                //Form exclusion list: exclude assignment i
                List<int[]> exclusionList = new List<int[]>();
                int[] exclusion = { i, assignment[i] };
                exclusionList.Add(exclusion);

                MurtyNode node = new MurtyNode(inclusionList, exclusionList);
                partition.Add(node);
            }

            return partition;
        }

        public static int[,] adjusted_cost(int[,] cost, MurtyNode node)
        {
            int[,] adjustedCosts = cost;
            node.ExclusionList.ForEach(delegate(int[] exclusion)
            {
                adjustedCosts[exclusion[0], exclusion[1]] = int.MaxValue;
            });

            node.InclusionList.ForEach(delegate(int[] inclusion)
            {
                adjustedCosts[inclusion[0], inclusion[1]] = int.MinValue;
            });

            return adjustedCosts;
        }

        public static double[,] adjusted_cost(double[,] cost, MurtyNode node)
        {
            double[,] adjustedCosts = cost;
            node.ExclusionList.ForEach(delegate(int[] exclusion)
            {
                adjustedCosts[exclusion[0], exclusion[1]] = double.MaxValue;
            });

            node.InclusionList.ForEach(delegate(int[] inclusion)
            {
                adjustedCosts[inclusion[0], inclusion[1]] = double.MinValue;
            });

            return adjustedCosts;
        }

        public static MurtyNode FindBestNode(int[,] costs, List<MurtyNode> partition)
        {
            MurtyNode optimalNode = new MurtyNode();
            int leastCost = int.MaxValue;

            partition.ForEach(delegate(MurtyNode node)
            {
                int[,] adjustedCosts = adjusted_cost(costs, node);
                int[] thisNodeBestAssignment = BestAssignment(adjustedCosts);
                int thisAssignmentCost = AssignmentCost(adjustedCosts, thisNodeBestAssignment);
                if (thisAssignmentCost < leastCost)
                {
                    leastCost = thisAssignmentCost;
                    optimalNode = node;
                }
            });

            return optimalNode;
        }

        public static MurtyNode FindBestNode(double[,] costs, List<MurtyNode> partition)
        {
            MurtyNode optimalNode = new MurtyNode();
            double leastCost = double.MaxValue;

            partition.ForEach(delegate(MurtyNode node)
            {
                double[,] adjustedCosts = adjusted_cost(costs, node);
                int[] thisNodeBestAssignment = BestAssignment(adjustedCosts);
                double thisAssignmentCost = AssignmentCost(adjustedCosts, thisNodeBestAssignment);
                if (thisAssignmentCost < leastCost)
                {
                    leastCost = thisAssignmentCost;
                    optimalNode = node;
                }
            });

            return optimalNode;
        }

        public static int AssignmentCost(int[,] cost, int[] assignment)
        {
            int assignmentCost = 0;
            for (int i = 0; i < cost.GetLength(0); i++)
                    assignmentCost += cost[i, assignment[i]];

            return assignmentCost;
        }

        public static double AssignmentCost(double[,] cost, int[] assignment)
        {
            double assignmentCost = 0;
            for (int i = 0; i < cost.GetLength(0); i++)
                assignmentCost += cost[i, assignment[i]];

            return assignmentCost;
        }

    }
}
