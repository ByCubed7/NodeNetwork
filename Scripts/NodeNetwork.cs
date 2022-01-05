// WTFPL license
// ByCubed7 ByCubed7@gmail.com

using System;
using System.Collections;
using System.Collections.Generic;

namespace ByCubed7.NodeNetwork
{
    using ByCubed7.PriorityQueue;

    public class Node : PriorityQueueElement {
        public (int, int) value;

        public Node((int,int) v) {
            value = v;
        }
    }

    public class NodeNetwork
    {
        public static int defaultNodeWeight = 1;
        private static int inf = 10000;

        // TODO: Use a 2D Array of lists of tuples for pro of look up tables
        public Dictionary<(int, int), List<(int, int)>> nodes;
        public int count = 0;
        private Dictionary<(int, int), int> weights;

        public NodeNetwork()
        {
            nodes = new Dictionary<(int, int), List<(int, int)>>();
            weights = new Dictionary<(int, int), int>();
            //path = PathfindAStar(nodes[0,6], nodes[14,0]);
        }

        public void Clear() {
            nodes = new Dictionary<(int, int), List<(int, int)>>();
            weights = new Dictionary<(int, int), int>();
        }

        public List<(int, int)> PathfindAStar((int, int) nodeStart, (int, int) nodeTarget)
        {
            // First, check both the starting position and ending position exist

            // WARNING: nodeStart may not be a valid space,
            // Check that the starting position exists.
            if (!nodes.ContainsKey(nodeStart))
            {
                Log($"Path STARTING POSITION is not accessible.\nNode: {nodeStart}");
                return new List<(int, int)>();
            }

            // WARNING: nodeTarget may not be a valid space,
            // Check that the ending position exists.
            if (!nodes.ContainsKey(nodeTarget))
            {
                Log($"Path TARGET POSITION is not accessible.\nNode: {nodeTarget}");
                return new List<(int, int)>();
            }

            // The set of discovered nodes that may need to be (re-)expanded.
            // Initially, only the start node is known.
            // "This is usually implemented as a min-heap or priority queue rather than a hash-set."
            PriorityQueue<Node> nodesToExpand = new PriorityQueue<Node>(count);

            // cameFrom[node] is the node immediately preceding it on path from start
            Dictionary<(int, int), (int, int)> cameFrom = new Dictionary<(int, int), (int, int)>();

            // gScore[node] is the cost of the cheapest path from start to node
            Dictionary<(int, int), int> gScore = new Dictionary<(int, int), int>();

            // fScore[node] = gScore[node] + Score(node). fScore[node] is the current guess as to
            // how short a path from start to finish can be if it goes through the node.
            Dictionary<(int, int), int> fScore = new Dictionary<(int, int), int>();

            // Add the starting node
            gScore[nodeStart] = 0;
            fScore[nodeStart] = Score(nodeStart, nodeTarget);// + 0 gScore
            nodesToExpand.Enqueue(new Node(nodeStart), fScore[nodeStart]);

            // While nodesToExpand is not empty
            while (nodesToExpand.Count != 0)
            {
                // (Cube) TODO: Change nodesToExpand to a priority list rather
                // repeatedly sorting throught the list each time.
                //nodesToExpand.Sort((nodeA, nodeB) => ((int)fScore[nodeA] - (int)fScore[nodeB]));

                // The node in nodesToExpand having the lowest fScore value
                (int, int) current = nodesToExpand.Dequeue().value;

                if (current == nodeTarget)
                    return MakePath(cameFrom, nodeTarget);

                //nodesToExpand.Remove(current);

                List<(int, int)> neighbours = GetNeighbours(current);

                //NOTE: GetNode(current) may return null.
                if (neighbours == null)
                   continue;

                for (int i = 0; i < neighbours.Count; i++)
                {
                    (int, int) neighbour = neighbours[i];
                    // Distance(current, neighbour) is the weight of the edge from current to neighbour
                    // tempGScore is the distance from start to the neighbor through current
                    int tempGScore = gScore[current] + Distance(current, neighbour);
                    //*
                    if (tempGScore < (gScore.ContainsKey(neighbour) ? gScore[neighbour] : inf))
                    {
                        // If true this path is better than any previous one
                        cameFrom[neighbour] = current;
                        gScore[neighbour] = tempGScore;
                        fScore[neighbour] = gScore[neighbour] + Score(neighbour, nodeTarget);
                        if (!nodesToExpand.Contains(new Node(neighbour)))
                            nodesToExpand.Enqueue(new Node(neighbour), fScore[neighbour]);
                            //nodesToExpand.Add(neighbour);
                    }//*/
                }//*/
            }
            // Open set is empty but goal was never reached
            return null;
        }

        // Converts the dictionary node path to a list of nodes
        private List<(int, int)> MakePath(Dictionary<(int, int), (int, int)> nodes, (int, int) targetNode)
        {
            (int, int) current = targetNode;
            List<(int, int)> path = new List<(int, int)>() {current};

            List<(int, int)> nodeKeys = new List<(int, int)>(nodes.Keys);
            while (nodeKeys.Contains(current))
            {
                current = nodes[current];
                path.Add(current);
            }
            return path;
        }

        // The heuristic function
        private int Score((int, int) node, (int, int) nodeTarget)
        {
            float score = 0f;
            //score += node.distance;
            score += Distance(node, nodeTarget);// * heuristic;
            score += GetWeight(node);
            return (int)score;
        }

        private int Distance((int, int) nodeFrom, (int, int) nodeTo) {
            int changeInX = Math.Abs(nodeTo.Item1 - nodeFrom.Item1);
            int changeInY = Math.Abs(nodeTo.Item2 - nodeFrom.Item2);
            int stability = Math.Abs(changeInX - changeInY);

            return (changeInX + changeInY) + stability / 2;
        }

        public void CreateNode(int x, int y, int weight = -1)
        {
            nodes[(x, y)] = new List<(int, int)>();
            weights[(x, y)] = weight == -1 ? defaultNodeWeight : weight;
            count++;
        }

        public bool IsValidNode((int, int) node)
        {
            return nodes.ContainsKey(node);
        }
        public bool IsValidNode(int x, int y) => IsValidNode((x, y));

        // Node Neighbours
        public List<(int, int)> GetNeighbours((int, int) node)
        {
            if (nodes.ContainsKey(node)) return nodes[node];
            return null;
        }
        public List<(int, int)> GetNeighbours(int x, int y) => GetNeighbours((x, y));

        public void AddNeighbour((int, int) node, (int, int) neigh)
        {
            nodes[node].Add(neigh);
        }
        public void AddNeighbour(int x1, int y1, int x2, int y2) => AddNeighbour((x1, y1), (x2, y2));

        // Node Weight
        public int GetWeight((int, int) node)
        {
            if (weights.ContainsKey(node)) return weights[node];
            Log($"Could not find weight of node, defaulting to 0.\nNode:{node}");
            return 0;
        }
        public void SetWeight((int, int) node, int weight) => weights[node] = weight;

        // Log
        public virtual void Log(string str) => Console.WriteLine("[Node Network] " + str);

    }

}
