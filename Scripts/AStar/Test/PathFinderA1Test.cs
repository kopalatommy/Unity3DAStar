using System.Collections.Generic;
using UnityEngine;
using System;

public class PathFinderA1Test : MonoBehaviour
{
    //Only set one true
    public bool done = false;
    public bool failed = false;

    private Node start;
    private Node goal;
    private float width;
    private bool pathSuccess = false;
    public List<Node> path = null;
    public List<Node> simplified = null;
    public List<Vector3> vPath = null;
    public int size = 0;
    int code = 0;
    public List<Node> critical = new List<Node>();

    /*public PathFinderA1Test(PathRequest r)
    {
        start = r.start;
        goal = r.end;
        width = r.size;
        size = r.requestee.size;
        code = r.requestee.occCode;
        setUpNodes();
    }*/

    public void RequestPath(Node _start, Node _goal, int _width, int _code)
    {
        start = _start;
        goal = _goal;
        width = _width;
        size = _width;
        code = _code;
        SetUpNodes();
        //print("Created nodes");
        AStar();
    }

    List<Node> alteredNodes = new List<Node>();
    readonly List<GameObject> printedNodes = new List<GameObject>(); 
    private void SetUpNodes()
    {
        int count = 0;
        foreach (Node n in MapManager.nodes)
        {
            n.Revert();
            if (n.GetOccCode() != -1 && n.GetOccCode() != code)
            {
                /*GameObject g = Instantiate(Map.instance.nodeMarker);
                g.transform.position = n.position;
                g.transform.localScale = Vector3.one * Map.length;
                printedNodes.Add(g);*/
                alteredNodes.Add(n);
                n.walkable = false;
                count++;
            }
        }
        /*print("Changed nodes: " + count);
        foreach (Node n in Map.nodes)
        {
            GameObject g = Instantiate(Map.instance.nodeMarker);
            g.transform.position = n.position;
            g.transform.localScale = Vector3.one * Map.length;
        }
        Debug.Log("Count: " + count);*/
    }

    private void ResetNodes()
    {
        //print("Resetting nodes");
        foreach (Node n in MapManager.nodes)
        {
            n.Revert();
            if (alteredNodes.Contains(n))
            {
                n.walkable = true;
            }
        }
    }

    private void AStar()
    {
        Debug.Log("Unit size: " + size * (1 / MapManager.length));
        if (start == null || goal == null)
        {
            Debug.Log("Start or Goal are null, pathfinding cant be completed");
            failed = true;
            return;
        }

        if (!start.walkable || !goal.walkable)
        {
            Debug.Log("Start or Goal are not walkable, pathfinding cant be completed");
            failed = true;
            return;
        }

        MinHeap<Node> openSet = new MinHeap<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.addItem(start);
        long viewed = 0;
        while (openSet.size > 0)
        {
            Node currentNode = openSet.getFront();
            closedSet.Add(currentNode);
            viewed++;
            if (currentNode == goal)
            {
                //Add callback
                print("Path success");
                pathSuccess = true;
                break;
            }

            if (size > GetDist(currentNode))
            {
                Debug.Log("Ignoring start node");
                continue;
            }

            foreach (Node n in MapManager.instance.GetNeighbors(currentNode))
            {
                if (!n.walkable || closedSet.Contains(n))
                {
                    continue;
                }

                if (size * (1 / MapManager.length) > GetDist(n))
                {
                    //Debug.Log("Ignoring node: " + (size * (1 / Map.length)) + " < " + getDist(n));
                    closedSet.Add(n);
                    if (n == goal) print("Ignoring goal");
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, n) + n.moveCost;
                if (newMovementCostToNeighbour <= n.gCost || !openSet.Contains(n))
                {
                    n.gCost = newMovementCostToNeighbour;
                    n.hCost = GetDistance(n, goal);
                    n.parent = currentNode;

                    if (!openSet.Contains(n))
                        openSet.addItem(n);
                    else
                        openSet.UpdateItem(n);
                }
            }
        }
        if (pathSuccess)
        {
            RetracePath();
        }
        else
        {
            print("Failed to build path");
        }
    }

    /*int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.xIndex - nodeB.xIndex);
        int dstZ = Mathf.Abs(nodeA.zIndex - nodeB.zIndex);

        if (dstX > dstZ)
            return (14 * dstZ) + (10 * (dstX - dstZ));
        return (14 * dstX) + (10 * (dstZ - dstX));
    }*/

    int GetDistance(Node nodeA, Node nodeB)
    {
        return Mathf.FloorToInt(Vector3.Distance(nodeA.Position, nodeB.Position));
    }

    private void RetracePath()
    {
        path = new List<Node>();
        Node currentNode = goal;

        while (currentNode != start)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        /*foreach (Node n in path)
        {
            GameObject g = Instantiate(Map.instance.nodeMarker2);
            g.transform.position = n.position;
            g.transform.localScale = Vector3.one * Map.length;
        }*/


        SimplifyPath();
        path = simplified;
        BuildVPath();
        foreach (Node n in path)
        {
            vPath.Add(n.Position);
            GameObject g = Instantiate(MapManager.instance.nodeMarker2);
            g.transform.position = n.Position;
            g.transform.localScale = Vector3.one * MapManager.length;
        }
        ResetNodes();
        done = true;
    }

    private void SimplifyPath()
    {
        simplified = new List<Node>();
        Vector2 directionOld = Vector2.zero;
        simplified.Add(start);
        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].xIndex - path[i].xIndex, path[i - 1].zIndex - path[i].zIndex);
            if (directionNew != directionOld)
            {
                if (!simplified.Contains(path[i - 1]))
                {
                    simplified.Add(path[i - 1]);
                    critical.Add(path[i - 1]);
                }
            }
            if (path[i].critical)
            {
                if (!simplified.Contains(path[i]))
                {
                    simplified.Add(path[i]);
                    critical.Add(path[i - 1]);
                }
            }
            directionOld = directionNew;
        }
        simplified.Add(goal);
        //Debug.Log(simplified.Count);
        path = simplified;
        //return;
        bool changed = false;
        while (true)
        {
            changed = false;
            /*for (int i = simplified.Count - 1; i >= 0; i--)
            {
                if (changed || simplified.Count <= 2)
                {
                    break;
                }
                for (int j = 0; j < i; j++)
                {
                    //bool vb = viableChange(simplified[i], simplified[j]);
                    //bool nn = Math.Abs(i - j) != 1;
                    //bool mc = moveCost(simplified[i].position, simplified[j].position) <= moveCost(simplified.GetRange(j, i - j));
                    if (viableChange(simplified[i], simplified[j]) && Math.Abs(i - j) != 1 && moveCost(simplified[i].position, simplified[j].position) <= moveCost(simplified.GetRange(j, i - j)))
                    //if(vb && nn && mc)
                    {
                        simplified.RemoveRange(j+1, i-j-1);
                        changed = true;
                        break;
                    }
                }
            }*/
            for (int i = 0; i < simplified.Count; i++)
            {
                if (changed || simplified.Count <= 2)
                {
                    break;
                }
                for (int j = simplified.Count - 1; j > i; j--)
                {
                    if (ViableChange(simplified[i], simplified[j]) && Math.Abs(j - i) != 1 && MoveCost(simplified[i].Position, simplified[j].Position) <= MoveCost(simplified.GetRange(i, j - i)))
                    {
                        simplified.RemoveRange(i + 1, j - i - 1);
                        changed = true;
                        break;
                    }
                }
            }
            if (!changed)
            {
                break;
            }
        }
        critical = simplified;
    }

    void BuildVPath()
    {
        vPath = new List<Vector3>();
        for (int i = 0; i < simplified.Count - 1; i++)
        {
            Node s = simplified[i];
            Node e = simplified[i + 1];
            Node last = s;
            Vector3 current = s.Position;
            Vector3 change = (e.Position - s.Position) * .01f;

            float distance = Vector3.Distance(e.Position, current);
            while (distance >= Vector3.Distance(e.Position, current))
            {
                distance = Vector3.Distance(e.Position, current);
                current += change;
                if (last != MapManager.instance.GetNodeFromLocation(current))
                {
                    last = MapManager.instance.GetNodeFromLocation(current);
                    vPath.Add(current);
                }
            }
            vPath.RemoveAt(vPath.Count - 1);
        }
    }

    bool ViableChange(Node s, Node g)
    {
        Vector3 start = s.Position;
        Vector3 end = g.Position;
        Vector3 current = start;
        float distance = Vector3.Distance(end, current);
        Vector3 change = (end - start) * (1 / (distance / .5f));
        List<Node> nodes = new List<Node>();

        //Debug.Log("Start: " + start + " Goal: " + end + " change: " + change + " distance: " + distance);

        while (distance >= Vector3.Distance(end, current) && current != end)
        {
            distance = Vector3.Distance(end, current);
            current += change;
            if (!MapManager.instance.GetNodeFromLocation(current).walkable)
            {
                return false;
            }
        }

        return true;
    }

    int MoveCost(List<Node> n)
    {
        int t = 0;
        for (int i = 0; i < n.Count - 1; i++)
        {
            t += MoveCost(n[i].Position, n[i + 1].Position);
        }
        return t;
    }

    int MoveCost(Vector3 a, Vector3 b)
    {
        int currentCost = 0;

        Vector3 current = a;
        Vector3 change = (b - a) * MapManager.length;
        float dist = Vector3.Distance(current, b);

        while (dist >= Vector3.Distance(current, b))
        {
            currentCost += MapManager.instance.GetNodeFromLocation(current).moveCost;
            dist = Vector3.Distance(b, current);
            current += change;
        }

        return currentCost;
    }

    float GetDist(Node n)
    {
        int num = 1;
        int x = n.xIndex;
        int z = n.zIndex;
        float length = MapManager.length;
        int xSize = MapManager.instance.xSize;
        int zSize = MapManager.instance.zSize;

        while (true)
        {
            for (int i = 0; i < (num * 2) + 1; i++)
            {
                int x2 = x - num + i;
                int z2 = z - num;
                if (x2 >= 0 && x2 < xSize * (1 / length) && z2 >= 0 && z2 < zSize * (1 / length))
                {
                    if (!MapManager.nodes[x2, z2].walkable)
                    {
                        return Vector3.Distance(n.Position, MapManager.nodes[x2, z2].Position);
                    }
                }
                else
                {
                    if (x2 < 0)
                    {
                        x2 = 0;
                    }
                    if (z2 < 0)
                    {
                        z2 = 0;
                    }
                    if (x2 == xSize * (1 / length))
                    {
                        x2 = Mathf.FloorToInt(xSize * (1 / length)) - 1;
                    }
                    if (z2 == zSize * (1 / length))
                    {
                        z2 = Mathf.FloorToInt(zSize * (1 / length)) - 1;
                    }
                    return Vector3.Distance(n.Position, MapManager.nodes[x2, z2].Position);
                }
            }

            for (int i = 0; i < (num * 2) - 1; i++)
            {
                int x2 = x + num;
                int z2 = z - num + i;
                if (x2 >= 0 && x2 < xSize * (1 / length) && z2 >= 0 && z2 < zSize * (1 / length))
                {
                    if (!MapManager.nodes[x2, z2].walkable)
                    {
                        return Vector3.Distance(n.Position, MapManager.nodes[x2, z2].Position);
                    }
                }
                else
                {
                    if (x2 < 0)
                    {
                        x2 = 0;
                    }
                    if (z2 < 0)
                    {
                        z2 = 0;
                    }
                    if (x2 == xSize * (1 / length))
                    {
                        x2 = Mathf.FloorToInt(xSize * (1 / length)) - 1;
                    }
                    if (z2 == zSize * (1 / length))
                    {
                        z2 = Mathf.FloorToInt(zSize * (1 / length)) - 1;
                    }
                    return Vector3.Distance(n.Position, MapManager.nodes[x2, z2].Position);
                }
            }

            for (int i = 0; i < (num * 2) - 1; i++)
            {
                int x2 = x + num - i;
                int z2 = z + num;
                if (x2 >= 0 && x2 < xSize * (1 / length) && z2 >= 0 && z2 < zSize * (1 / length))
                {
                    if (!MapManager.nodes[x2, z2].walkable)
                    {
                        return Vector3.Distance(n.Position, MapManager.nodes[x2, z2].Position);
                    }
                }
                else
                {
                    if (x2 < 0)
                    {
                        x2 = 0;
                    }
                    if (z2 < 0)
                    {
                        z2 = 0;
                    }
                    if (x2 == xSize * (1 / length))
                    {
                        x2 = Mathf.FloorToInt(xSize * (1 / length)) - 1;
                    }
                    if (z2 == zSize * (1 / length))
                    {
                        z2 = Mathf.FloorToInt(zSize * (1 / length)) - 1;
                    }
                    return Vector3.Distance(n.Position, MapManager.nodes[x2, z2].Position);
                }
            }

            for (int i = 0; i < (num * 2) - 1; i++)
            {
                int x2 = x - num;
                int z2 = z + num - i;
                if (x2 >= 0 && x2 < xSize * (1 / length) && z2 >= 0 && z2 < zSize * (1 / length))
                {
                    if (!MapManager.nodes[x2, z2].walkable)
                    {
                        return Vector3.Distance(n.Position, MapManager.nodes[x2, z2].Position);
                    }
                }
                else
                {
                    if (x2 < 0)
                    {
                        x2 = 0;
                    }
                    if (z2 < 0)
                    {
                        z2 = 0;
                    }
                    if (x2 == xSize * (1 / length))
                    {
                        x2 = Mathf.FloorToInt(xSize * (1 / length)) - 1;
                    }
                    if (z2 == zSize * (1 / length))
                    {
                        z2 = Mathf.FloorToInt(zSize * (1 / length)) - 1;
                    }
                    return Vector3.Distance(n.Position, MapManager.nodes[x2, z2].Position);
                }
            }
            num++;
        }
    }
}
