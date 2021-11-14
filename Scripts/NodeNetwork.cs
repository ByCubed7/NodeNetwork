using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ByCubed7.Pathfinding
{
    public class NodeNetwork
    {
        public Dictionary<(int, int), Node> nodes;

        public NodeNetwork()
        {
            nodes = new Dictionary<(int, int), Node>();
            //path = PathfindAStar(nodes[0,6], nodes[14,0]);
        }

        public void LoadFromTilemap(Tilemap tilemap)
        {
            BoundsInt bounds = tilemap.cellBounds;
            TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

            nodes = new Dictionary<(int, int), Node>();
            //nodes = new Node[bounds.size.x, bounds.size.y)];

            Vector3 offset = (Vector3)tilemap.origin * tilemap.cellSize.x;
            Vector3 anchor = (Vector3)tilemap.tileAnchor * tilemap.cellSize.x;

            for (int x = 0; x < bounds.size.x; x++) {
                for (int y = 0; y < bounds.size.y; y++) {
                    Tile tile = (Tile)allTiles[x + y * bounds.size.x];
                    if (tile != null) {
                        //Debug.Log("x:" + x + " y:" + y + " tile: " + tile);
                    } else {
                        //Debug.Log("x:" + x + " y:" + y + " tile: (null)");
                        Vector3 worldPosition = tilemap.CellToWorld(new Vector3Int(x, y, 0));
                        Vector3 tilePosition = worldPosition + offset + anchor;
                        nodes[(x, y)] = new Node(
                            new Vector2Int(x, y),
                            new Vector2(tilePosition.x, tilePosition.y)
                        );
                    }
                }
            }

            // link neighbours
            for (int x = 0; x < bounds.size.x; x++) {//nodes.GetLength(0)
                for (int y = 0; y < bounds.size.y; y++) {
                    Node currentNode = GetNodeAt(x, y);

                    if (currentNode == null) continue;

                    if (x + 1 < bounds.size.x) currentNode.AddNeighbour(GetNodeAt(x + 1, y));
                    if (y + 1 < bounds.size.y) currentNode.AddNeighbour(GetNodeAt(x, y + 1));
                    if (x > 0) currentNode.AddNeighbour(GetNodeAt(x - 1, y));
                    if (y > 0) currentNode.AddNeighbour(GetNodeAt(x, y - 1));
                }
            }
        }

        public Node GetNodeAt(int x, int y)
        {
            if (nodes.ContainsKey((x, y)))
                return nodes[(x, y)];
            return null;
        }

        // Converts the dictionary node path to a list of nodes
        private List<Node> MakePath(Dictionary<Node, Node> nodes, Node targetNode)
        {
            Node current = targetNode;
            List<Node> path = new List<Node>() {current};

            List<Node> nodeKeys = new List<Node>(nodes.Keys);
            while (nodeKeys.Contains(current))
            {
                current = nodes[current];
                path.Add(current);
            }
            return path;
        }

        public List<Node> PathfindAStar(Node nodeStart, Node nodeTarget)
        {
            // The set of discovered nodes that may need to be (re-)expanded.
            // Initially, only the start node is known.
            // "This is usually implemented as a min-heap or priority queue rather than a hash-set."
            List<Node> nodesToExpand = new List<Node>();

            // cameFrom[node] is the node immediately preceding it on path from start
            Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();

            // gScore[node] is the cost of the cheapest path from start to node
            Dictionary<Node, int> gScore = new Dictionary<Node, int>();

            // fScore[node] = gScore[node] + Score(node). fScore[node] is the current guess as to
            // how short a path from start to finish can be if it goes through the node.
            Dictionary<Node, int> fScore = new Dictionary<Node, int>();

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
                Node current = nodesToExpand[0];

                if (current == nodeTarget)
                    return MakePath(cameFrom, nodeTarget);

                nodesToExpand.Remove(current);

                List<Node> currentNeighbours = current.neighbour;
                for (int i = 0; i < current.neighbour.Count; i++)
                {
                    Node neighbor = current.neighbour[i];
                    // Distance(current, neighbor) is the weight of the edge from current to neighbor
                    // tempGScore is the distance from start to the neighbor through current
                    int tempGScore = gScore[current] + (int)current.Distance(neighbor);
                    //*
                    if (tempGScore < (gScore.ContainsKey(neighbor) ? gScore[neighbor] : Mathf.Infinity))
                    {
                        // If true this path is better than any previous one
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tempGScore;
                        fScore[neighbor] = gScore[neighbor] + Score(neighbor, nodeTarget);
                        if (!nodesToExpand.Contains(neighbor))
                            nodesToExpand.Add(neighbor);
                    }//*/
                }//*/
            }
            // Open set is empty but goal was never reached
            return null;
        }

        // The heuristic function
        public static int Score(Node node, Node nodeTarget)
        {
            float score = 0f;
            //score += node.distance;
            score += node.Distance(nodeTarget);// * heuristic;
            return (int)score;
        }

        public Node NodeAtWorldPosition(Vector2 position, float dec = 0.5f)
        {
            foreach (KeyValuePair<(int, int), Node> entry in nodes)
            {
                Vector2 difference = position - entry.Value.position;
                if (difference.x + difference.y < dec) return entry.Value;
            }
            return null;
        }

        public Node NearestNodeAtWorldPosition(Vector2 position)
        {
            float nearestDistance = 0f;
            Node nearestNode = null;
            foreach (KeyValuePair<(int, int), Node> entry in nodes)
            {
                float valueDistance = entry.Value.Distance(position);
                if (valueDistance < nearestDistance || nearestNode == null) {
                    nearestDistance = valueDistance;
                    nearestNode = entry.Value;
                }
            }
            return nearestNode;
        }

        public void DebugDraw()
        {
            foreach (KeyValuePair<(int, int), Node> entry in nodes)
                entry.Value.DebugDraw();

        }

        public void DebugDrawPath(List<Node> path)
        {
            foreach (Node node in path)
                node.DebugDraw(Color.green);

            Node lastNode = null;
            foreach (Node node in path)
            {
                if (lastNode != null)
                    Debug.DrawLine(node.position, lastNode.position, Color.blue);
                lastNode = node;
            }
        }

    }
}
