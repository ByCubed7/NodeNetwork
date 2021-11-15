using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ByCubed7.Pathfinding
{
    public class NodeNetwork
    {
        public static int defaultNodeWeight = 1;

        // TODO: Use a 2D Array of lists of tuples for pro of look up tables
        public Dictionary<(int, int), List<(int, int)>> nodes;
        public Dictionary<(int, int), int> weights;

        public NodeNetwork()
        {
            nodes = new Dictionary<(int, int), List<(int, int)>>();
            weights = new Dictionary<(int, int), int>();
            //path = PathfindAStar(nodes[0,6], nodes[14,0]);
        }

        public void LoadFromTilemap(Tilemap tilemap)
        {
            BoundsInt bounds = tilemap.cellBounds;
            TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

            nodes = new Dictionary<(int, int), List<(int, int)>>();

            for (int x = 0; x < bounds.size.x; x++) {
                for (int y = 0; y < bounds.size.y; y++) {
                    Tile tile = (Tile)allTiles[x + y * bounds.size.x];
                    if (tile == null) {
                        AddNode(x, y);
                    }
                }
            }

            // link neighbours
            for (int x = 0; x < bounds.size.x; x++) {//nodes.GetLength(0)
                for (int y = 0; y < bounds.size.y; y++) {
                    List<(int, int)> currentNode = GetNode(x, y);

                    if (currentNode == null) continue;

                    if (x + 1 < bounds.size.x) AddNeighbour((x, y), (x + 1, y));
                    if (y + 1 < bounds.size.y) AddNeighbour((x, y), (x, y + 1));
                    if (x > 0) AddNeighbour((x, y), (x - 1, y));
                    if (y > 0) AddNeighbour((x, y), (x, y - 1));
                }
            }
        }

        public List<(int, int)> PathfindAStar((int, int) nodeStart, (int, int) nodeTarget)
        {
            // First, check both the starting position and ending position exist

            // WARNING: nodeStart may not be a valid space,
            // Check that the starting position exists.
            if (!nodes.ContainsKey(nodeStart))
            {
                Debug.LogWarning($"[Node Network] Path STARTING POSITION is not accessible.\nNode:{nodeStart}");
                return new List<(int, int)>();
            }

            // WARNING: nodeTarget may not be a valid space,
            // Check that the ending position exists.
            if (!nodes.ContainsKey(nodeTarget))
            {
                Debug.LogWarning($"[Node Network] Path TARGET POSITION is not accessible.\nNode:{nodeTarget}");
                return new List<(int, int)>();
            }

            // The set of discovered nodes that may need to be (re-)expanded.
            // Initially, only the start node is known.
            // "This is usually implemented as a min-heap or priority queue rather than a hash-set."
            List<(int, int)> nodesToExpand = new List<(int, int)>();

            // cameFrom[node] is the node immediately preceding it on path from start
            Dictionary<(int, int), (int, int)> cameFrom = new Dictionary<(int, int), (int, int)>();

            // gScore[node] is the cost of the cheapest path from start to node
            Dictionary<(int, int), int> gScore = new Dictionary<(int, int), int>();

            // fScore[node] = gScore[node] + Score(node). fScore[node] is the current guess as to
            // how short a path from start to finish can be if it goes through the node.
            Dictionary<(int, int), int> fScore = new Dictionary<(int, int), int>();

            // Add the starting node
            nodesToExpand.Add(nodeStart);
            gScore[nodeStart] = 0;
            fScore[nodeStart] = Score(nodeStart, nodeTarget);// + 0 gScore

            // While nodesToExpand is not empty
            while (nodesToExpand.Count != 0)
            {
                // (Cube) TODO: Change nodesToExpand to a priority list rather
                // repeatedly sorting throught the list each time.
                nodesToExpand.Sort((nodeA, nodeB) => ((int)fScore[nodeA] - (int)fScore[nodeB]));

                // The node in nodesToExpand having the lowest fScore value
                (int, int) current = nodesToExpand[0];

                if (current == nodeTarget)
                    return MakePath(cameFrom, nodeTarget);

                nodesToExpand.Remove(current);

                List<(int, int)> neighbours = GetNode(current);

                // NOTE: GetNode(current) may return null.
                if (neighbours == null) {
                    Debug.LogWarning($"[Node Network] Can not find accessible path.\nNode:{current}");
                    return new List<(int, int)>();
                }

                for (int i = 0; i < neighbours.Count; i++)
                {
                    (int, int) neighbour = neighbours[i];
                    // Distance(current, neighbour) is the weight of the edge from current to neighbour
                    // tempGScore is the distance from start to the neighbor through current
                    int tempGScore = gScore[current] + Distance(current, neighbour);
                    //*
                    if (tempGScore < (gScore.ContainsKey(neighbour) ? gScore[neighbour] : Mathf.Infinity))
                    {
                        // If true this path is better than any previous one
                        cameFrom[neighbour] = current;
                        gScore[neighbour] = tempGScore;
                        fScore[neighbour] = gScore[neighbour] + Score(neighbour, nodeTarget);
                        if (!nodesToExpand.Contains(neighbour))
                            nodesToExpand.Add(neighbour);
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
        public int Score((int, int) node, (int, int) nodeTarget)
        {
            float score = 0f;
            //score += node.distance;
            score += Distance(node, nodeTarget);// * heuristic;
            score += Weight(node);
            return (int)score;
        }

        public int Distance((int, int) nodeFrom, (int, int) nodeTo) {
            int changeInX = Mathf.Abs(nodeTo.Item1 - nodeFrom.Item1);
            int changeInY = Mathf.Abs(nodeTo.Item2 - nodeFrom.Item2);
            int stability = Mathf.Abs(changeInX - changeInY);

            return (changeInX + changeInY) + stability / 2;
        }

        public void AddNode(int x, int y, int weight = -1)
        {
            nodes[(x, y)] = new List<(int, int)>();
            weights[(x, y)] = weight == -1 ? defaultNodeWeight : weight;
        }

        // Node Neighbours
        public List<(int, int)> GetNode((int, int) node)
        {
            if (nodes.ContainsKey(node)) return nodes[node];
            return null;
        }
        public List<(int, int)> GetNode(int x, int y) => GetNode((x, y));

        public void AddNeighbour((int, int) node, (int, int) neigh)
        {
            nodes[node].Add(neigh);
        }
        public void AddNeighbour(int x1, int y1, int x2, int y2) => AddNeighbour((x1, y1), (x2, y2));

        // Node Weight
        public int Weight((int, int) node)
        {
            if (weights.ContainsKey(node)) return weights[node];
            Debug.LogWarning($"[Node Network] Could not find weight of node, defaulting to 0.\nNode:{node}");
            return 0;
        }
        public void SetWeight((int, int) node, int weight) => weights[node] = weight;

    }

}
