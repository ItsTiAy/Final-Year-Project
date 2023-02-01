using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Pathfinding
{
    private const int STRAIGHT_COST = 10;
    private const int DIAGONAL_COST = 14;

    static List<Node> open;
    static List<Node> closed;

    //private int gridHeight = 14;
    //private int gridWidth = 24;

    //public Grid grid;

    private const int maxX = 12;
    private const int minX = -12;
    private const int maxY = 8;
    private const int minY = -7;

    public static Dictionary<Vector2Int, Node> nodes;

    public static void Initialize()
    {
        nodes = new Dictionary<Vector2Int, Node>();

        // Need to change to be dynamic rather than hard coded
        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                Node node = new Node(x, y);
                nodes.Add(new Vector2Int(x, y), node);
            }
        }
    }

    public static List<Node> FindPath(int startX, int startY, int endX, int endY)
    {
        foreach (KeyValuePair <Vector2Int, Node> node in nodes)
        {
            node.Value.ResetNode();
        }

        Node startNode = new Node(startX, startY);
        Node endNode = new Node(endX, endY);

        open = new List<Node> {startNode};
        closed = new List<Node>();

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (open.Count > 0)
        {
            Node currentNode = GetLowestFCostNode(open);
            //Debug.Log("x: " + currentNode.x + " y: " + currentNode.y);

            if (currentNode.x == endNode.x && currentNode.y == endNode.y)
            {
                //Debug.Log("wombat");
                return CalculatePath(currentNode);
            }

            open.Remove(currentNode);
            closed.Add(currentNode);

            foreach (Node neighbourNode in GetNeighbours(currentNode))
            {
                if (closed.Contains(neighbourNode)) continue;

                if (!neighbourNode.IsWalkable())
                {
                    closed.Add(neighbourNode);
                    continue;
                }
                
                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);

                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.previousNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!open.Contains(neighbourNode))
                    {
                        open.Add(neighbourNode);
                    }
                }
                
            }
        }

        Debug.Log("No valid path found");
        return null;
    }

    private static int CalculateDistanceCost(Node a, Node b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);

        return DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + STRAIGHT_COST * remaining;
    }

    private static Node GetLowestFCostNode(List<Node> nodeList)
    {
        Node lowestFCostNode = nodeList[0];

        for (int i = 1; i < nodeList.Count; i++)
        {
            if (nodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = nodeList[i];
            }
        }

        return lowestFCostNode;
    }

    private static List<Node> CalculatePath(Node endNode)
    {
        List<Node> path = new List<Node>();
        path.Add(endNode);
        Node currentNode = endNode;

        //Debug.Log(currentNode.previousNode);

        while (currentNode.previousNode != null)
        {
            path.Add(currentNode.previousNode);
            currentNode = currentNode.previousNode;
        }

        path.Reverse();
        return path;
    }

    private static List<Node> GetNeighbours(Node currentNode)
    {
        List<Node> neighbours = new List<Node>();

        if (currentNode.y - 1 >= minY)
        {
            neighbours.Add(nodes[new Vector2Int(currentNode.x, currentNode.y - 1)]);
        }

        if (currentNode.y + 1 < maxY)
        {
            neighbours.Add(nodes[new Vector2Int(currentNode.x, currentNode.y + 1)]);
        }

        if (currentNode.x - 1 >= minX)
        {
            neighbours.Add(nodes[new Vector2Int(currentNode.x - 1, currentNode.y)]);

            if (nodes[new Vector2Int(currentNode.x - 1, currentNode.y)].IsWalkable())
            {
                if (currentNode.y - 1 >= minY && nodes[new Vector2Int(currentNode.x, currentNode.y - 1)].IsWalkable())
                {
                    neighbours.Add(nodes[new Vector2Int(currentNode.x - 1, currentNode.y - 1)]);
                }

                if (currentNode.y + 1 < maxY && nodes[new Vector2Int(currentNode.x, currentNode.y + 1)].IsWalkable())
                {
                    neighbours.Add(nodes[new Vector2Int(currentNode.x - 1, currentNode.y + 1)]);
                }
            }
        }

        if (currentNode.x + 1 < maxX)
        {
            neighbours.Add(nodes[new Vector2Int(currentNode.x + 1, currentNode.y)]);

            if (nodes[new Vector2Int(currentNode.x + 1, currentNode.y)].IsWalkable())
            {
                if (currentNode.y - 1 >= minY && nodes[new Vector2Int(currentNode.x, currentNode.y - 1)].IsWalkable())
                {
                    neighbours.Add(nodes[new Vector2Int(currentNode.x + 1, currentNode.y - 1)]);
                }

                if (currentNode.y + 1 < maxY && nodes[new Vector2Int(currentNode.x, currentNode.y + 1)].IsWalkable())
                {
                    neighbours.Add(nodes[new Vector2Int(currentNode.x + 1, currentNode.y + 1)]);
                }
            }
        }

        return neighbours;
    }

    public class Node
    {
        public int x;
        public int y;

        public int gCost;
        public int hCost;
        public int fCost;

        public Node previousNode;

        private bool isWalkable;

        public Node(int x, int y)
        {
            this.x = x;
            this.y = y;

            gCost = int.MaxValue;
            CalculateFCost();
            previousNode = null;
            isWalkable = true;
        }

        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }

        public void SetIsWalkable(bool isWalkable)
        {
            this.isWalkable = isWalkable;
        }

        public bool IsWalkable()
        {
            return isWalkable;
        }

        public void ResetNode()
        {
            gCost = int.MaxValue;
            hCost = 0;
            CalculateFCost();
            previousNode = null;
        }
    }
}
