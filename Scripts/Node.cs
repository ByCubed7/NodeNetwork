using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ByCubed7.Pathfinding
{
    public class Node
    {
        public Vector2 position;
        public List<Node> neighbour;

        public bool hasValue;

        private Vector2Int coord;

        public Node(Vector2Int coord, Vector2 position)
        {
            this.coord = coord;
            this.position = position;

            this.neighbour = new List<Node>();
            this.hasValue = true;
        }

        public void AddNeighbour(Node nodeNeighbour)
        {
            if (nodeNeighbour == null) return;
            neighbour.Add(nodeNeighbour);
        }


        public float Distance(Vector2 positionFrom)
        {
            // Calculate using Manhattan distance
            float total = 0f;
            total += Mathf.Abs(position.x - positionFrom.x);
            total += Mathf.Abs(position.y - positionFrom.y);
            return total;
        }
        public float Distance(Node nodeB) => Distance(nodeB.position);

        public void DebugDraw(Color colour)
        {
            Debug.DrawLine(position + new Vector2(0.1f, 0.1f), position + new Vector2(-0.1f, -0.1f), colour);
            Debug.DrawLine(position + new Vector2(0.1f, -0.1f), position + new Vector2(-0.1f, 0.1f), colour);
        }
        public void DebugDraw() => DebugDraw(Color.red);

        public void DebugDrawNeighbours()
        {
            foreach (Node nodeNeighbour in neighbour)
                Debug.DrawLine(position, nodeNeighbour.position, Color.gray);
        }

        public void DebugDrawAll()
        {
            DebugDraw();
            DebugDrawNeighbours();
        }

        public override string ToString()
        {
            return base.ToString() + ": " + coord.ToString();
        }
    }
}
