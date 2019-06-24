using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class PlacementSearch
{
    public List<Unit> unitsToPlace = null;
    public List<Node[,]> placements = new List<Node[,]>();
    public List<Vector3> movePos = new List<Vector3>();
    //0 = in process
    //1 = succeeded
    //2 = failed
    public int status = 0;

    Thread thread = null;
    Node start = null;

    public PlacementSearch(List<Unit> _unitsToPlace, Node _start)
    {
        unitsToPlace = _unitsToPlace;
        start = _start;

        ThreadStart startT = new ThreadStart(Search);
        thread = new Thread(startT);
        thread.Start();
    }

    void Search()
    {
        long touchedNodes = 0;
        bool found = false;

        ResetNodes();
        Debug.Log("Units to place: " + unitsToPlace.Count);
        for (int i = 0; i < unitsToPlace.Count; i++)
        {
            List<Node> openSet = new List<Node>();
            List<Node> closedSet = new List<Node>();

            openSet.Add(start);
            while (openSet.Count > 0)
            {
                touchedNodes++;
                Node current = openSet[0];
                openSet.RemoveAt(0);
                closedSet.Add(current);

                Node[,] nodesToTest = GetNodesFromLocationV2(current.position, (int)( 1.5 *(unitsToPlace[i].size * (1 / Map.length))));

                if (NodesAreOK(nodesToTest))
                {
                    //Debug.Log("Nodes are ok");
                    placements.Add(nodesToTest);
                    foreach (Node n in nodesToTest)
                    {
                        n.claimed = true;
                    }
                    movePos.Add(GetAvgPosition(nodesToTest));
                    found = true;
                    break;
                }

                foreach (Node n in Map.getNeighbors(current))
                {
                    if (!closedSet.Contains(n))
                    {
                        if (!openSet.Contains(n))
                        {
                            //Debug.Log("Added node");
                            openSet.Add(n);
                        }
                    }
                }
            }
            if (!found)
            {
                //Debug.Log("Open set: " + openSet.Count + ", Closed set: " + closedSet.Count);
                status = 2;
                return;
            }
            //Debug.Log("Found: " + i);
        }
        //Debug.Log("Touched: " + touchedNodes);
        status = 1;
    }

    void ResetNodes()
    {
        foreach (Node n in Map.nodes)
        {
            n.claimed = false;
        }
    }

    Node[,] GetNodesFromLocationV2(Vector3 pos, int size)
    {
        float l = Map.length;

        Node[,] nodes = new Node[size * 2, size * 2];

        float x = pos.x - (size / 2);
        float z = pos.z - (size / 2);

        for (int q = 0; q < size * 2; q++)
        {
            for (int w = 0; w < size * 2; w++)
            {
                if (nodes[q, w] == null)
                {
                    Vector3 nPos = Vector3.zero;
                    nPos.y = pos.y;
                    nPos.x = x + (l * q);
                    nPos.z = z + (w * l);
                    nodes[q, w] = Map.instance.getNodeFromLocation(nPos);
                }
            }
        }
        return nodes;
    }

    bool NodesAreOK(Node[,] nodes)
    {
        foreach (Node n in nodes)
        {
            if (n == null)
            {
                Debug.Log("NULL");
                return false;
            }
            if (n.claimed || !n.walkable)
            {
                return false;
            }
        }
        return true;
    }

    Vector3 GetAvgPosition(Node[,] n)
    {
        float x = ((n[0, 0].position.x + n[0, n.GetLength(n.Rank - 1) - 1].position.x) / 2) + Map.length / 2;
        float z = ((n[0, 0].position.z + n[n.GetLength(n.Rank - 1) - 1, 0].position.z) / 2) + Map.length / 2;
        return new Vector3(x - 0.01f, n[0, 0].position.y, z - 0.01f);
    }
}
