﻿using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class PathFinder
{
    //Only set one true
    public bool done = false;
    public bool failed = false;

    private Thread thread = null;
    private Node start = null;
    private Node goal = null;
    //width * width = number of nodes a unit takes up
    private int width = 0;
    private bool pathSuccess = false;
    public List<Node> path = null;
    public List<Node> simplified = null;
    public List<Vector3> vPath = null;

    public List<Node> critical = new List<Node>();

    public PathFinder(Node _start, Node _goal)
    {
        start = _start;
        goal = _goal;
        startThread();
    }

    public PathFinder(PathRequest r)
    {
        start = r.start;
        goal = r.end;
        width = r.size;
        startThread();
    }

    public void startThread()
    {
        Debug.Log("Unit size = " + width);
        resetNodes();//reset all nodes to ensure correct path has been made
        ThreadStart startT = new ThreadStart(AStar);
        thread = new Thread(startT);
        thread.Start();
    }

    private void resetNodes()
    {
        foreach (Node n in Map.nodes)
        {
            n.revert();
        }
    }

    private void AStar()
    {
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

        /*if (start.cushion < width || goal.cushion < width)
        {
            Debug.Log("Start or Goal is to close to obstacle, pathfinding cant be completed");
            failed = true;
            return;
        }*/

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
                pathSuccess = true;
                break;
            }

            foreach (Node n in Map.getNeighbors(currentNode))
            {
                if (!n.walkable || closedSet.Contains(n))
                {
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
            retracePath();
        }
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.xIndex - nodeB.xIndex);
        int dstZ = Mathf.Abs(nodeA.zIndex - nodeB.zIndex);

        if (dstX > dstZ)
            return (14 * dstZ) + (10 * (dstX - dstZ));
        return (14 * dstX) + (10 * (dstZ - dstX));
    }

    private void retracePath()
    {
        path = new List<Node>();
        Node currentNode = goal;

        while (currentNode != start)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        simplifyPath();
        path = simplified;
        buildVPath();
        done = true;
    }

    private void simplifyPath()
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
                    if (viableChange(simplified[i], simplified[j]) && Math.Abs(j - i) != 1 && moveCost(simplified[i].position, simplified[j].position) <= moveCost(simplified.GetRange(i, j - i)))
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

    /*void buildVPath()
    {
        vPath = new List<Vector3>();
        for (int i = 0; i < simplified.Count - 1; i++)
        {
            Node s = simplified[i];
            Node e = simplified[i + 1];
            Node last = s;
            Vector3 current = s.position;
            Vector3 change = (e.position - s.position) * .01f;

            float distance = Vector3.Distance(getAvgNodePosition(getNodesFromLocation(e.position)[0,0]), getAvgNodePosition(getNodesFromLocation(current)[0,0]));
            while (distance >= Vector3.Distance(e.position, current))
            {
                distance = Vector3.Distance(getAvgNodePosition(getNodesFromLocation(e.position)[0, 0]), current);
                current += change;
                if (last != Map.instance.getNodeFromLocation(current))
                {
                    last = Map.instance.getNodeFromLocation(current);
                    vPath.Add(current);
                }
            }
            //vPath.RemoveAt(vPath.Count-1);
        }
        Vector3 temp = getAvgNodePosition(getNodesFromLocation(vPath[vPath.Count - 1])[0, 0]);
        vPath.RemoveAt(vPath.Count - 1);
        vPath.Add(temp);
    }*/

    void buildVPath()
    {
        List<Vector3> newNodes = new List<Vector3>();
        vPath = new List<Vector3>();
        for (int i = 0; i < simplified.Count - 1; i++)
        {
            Node s = simplified[i];
            Node e = simplified[i + 1];
            Node last = s;
            Vector3 current = s.position;
            Vector3 change = (e.position - s.position) * .01f;

            float distance = Vector3.Distance(e.position, current);
            while (distance >= Vector3.Distance(e.position, current))
            {
                distance = Vector3.Distance(e.position, current);
                current += change;
                if (last != Map.instance.getNodeFromLocation(current))
                {
                    last = Map.instance.getNodeFromLocation(current);
                    vPath.Add(current);
                }
            }
            vPath.RemoveAt(vPath.Count - 1);
        }
        /*Vector3 temp = getAvgNodePosition(getNodesFromLocation(vPath[vPath.Count - 1])[0, 0]);
        vPath.RemoveAt(vPath.Count - 1);
        vPath.Add(temp);*/
    }

    bool viableChange(Node s, Node g)
    {
        Vector3 start = s.position;
        Vector3 end = g.position;
        Vector3 current = start;
        float distance = Vector3.Distance(end, current);
        Vector3 change = (end - start) * (1 / (distance / .5f));
        List<Node> nodes = new List<Node>();

        //Debug.Log("Start: " + start + " Goal: " + end + " change: " + change + " distance: " + distance);

        while (distance >= Vector3.Distance(end, current) && current != end)
        {
            distance = Vector3.Distance(end, current);
            current += change;
            if (!Map.instance.getNodeFromLocation(current).walkable)
            {
                return false;
            }
        }

        return true;
    }

    int moveCost(List<Node> n)
    {
        int t = 0;
        for (int i = 0; i < n.Count - 1; i++)
        {
            t += moveCost(n[i].position, n[i + 1].position);
        }
        return t;
    }

    int moveCost(Vector3 a, Vector3 b)
    {
        int currentCost = 0;

        Vector3 current = a;
        Vector3 change = (b - a) * Map.length;
        float dist = Vector3.Distance(current, b);

        while (dist >= Vector3.Distance(current, b))
        {
            currentCost += Map.instance.getNodeFromLocation(current).moveCost;
            dist = Vector3.Distance(b, current);
            current += change;
        }

        return currentCost;
    }
}

public struct PathResult
{
    public Vector3[] path;
    public bool success;
    public Action<Vector3[], bool> callback;

    public PathResult(Vector3[] path, bool success, Action<Vector3[], bool> callback)
    {
        this.path = path;
        this.success = success;
        this.callback = callback;
    }
}
