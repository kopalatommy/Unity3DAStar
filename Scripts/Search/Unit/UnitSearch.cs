using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class UnitSearch
{
    //Search range
    readonly int range = 0;
    //Player code will be used to identify teams
    readonly int teamCode = 0;

    public enum PathStatus { inProcess, succeeded, failed };
    public PathStatus status = PathStatus.inProcess;

    private Node start = null;
    private Thread thread = null;

    public Unit result = null;

    public UnitSearch(int _range, int _teamCode, Node _start)
    {
        start = _start;
        range = _range;
        teamCode = _teamCode;

        ThreadStart startT = new ThreadStart(Search);
        thread = new Thread(startT);
        thread.Start();
    }

    private void Search()
    {
        status = PathStatus.inProcess;
        //Nodes to search
        List<Node> openSet = new List<Node>();
        List<Node> closedSet = new List<Node>();

        //Debug.Log("Starting search");

        openSet.Add(start);
        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            openSet.RemoveAt(0);
            closedSet.Add(current);

            if ( current.GetOccCode() != -1 && UnitManager.manager.getUnitFromUnitCodes(current.GetOccCode()).teamCode != teamCode )
            {
                result = UnitManager.manager.getUnitFromUnitCodes(current.GetOccCode());
                status = PathStatus.succeeded;
                return;
            }

            foreach (Node n in MapManager.instance.GetNeighbors(current))
            {
                if (n != null && !closedSet.Contains(n) && Vector3.Distance(start.Position, n.Position) <= range)
                {
                    if (!openSet.Contains(n))
                    {
                        openSet.Add(n);
                    }
                }
            }
        }
        status = PathStatus.failed;
    }
}