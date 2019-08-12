using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Node : IComparable<Node>
{
    //public bool initialized;

    //public float cushion;

    /*public int xIndex;
    public int zIndex;*/
    public int xIndex;
    public int zIndex;

    public bool walkable = false;
    public int moveCost = 0;
    //public Vector3 position = Vector3.zero;
    public float x, y, z;

    Action<Unit> onOccupy = null;

    public Vector3 Position
    {
        get
        {
            return new Vector3(x, y, z);
        }
        set
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }
    }

    //A star calculation variables
    public int gCost = 0;
    public int hCost = 0;
    public Node parent = null;
    //public int heapIndex = 0;

    //Unit avoidence
    private int occCode = -1;

    public void SetOccCode(Unit u)
    {
        occCode = u.occCode;
        onOccupy?.Invoke(u);
    }
    public void SetOccCode(int val)
    {
        occCode = val;
    }

    public int GetOccCode()
    {
        return occCode;
    }

    //PathFinding
    public bool critical = false;

    //Search algorithm
    public bool claimed = false;

    //For debugging
    public bool touched = false;

    public Node(bool _walkable, Vector3 pos, int xIn, int zIn, int mCost/*, float _cushion*/)
    {
        //position = pos;
        x = pos.x;
        y = pos.y;
        z = pos.z;
        moveCost = mCost;
        walkable = _walkable;
        xIndex = xIn;
        zIndex = zIn;
        //cushion = _cushion;
        /*xIndex = xIn;
        zIndex = zIn;*/
    }

    public void Revert()
    {
        //parent = null;
        gCost = 0;
    }

    // AStar methods
    //Full cost
    public int FCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = FCost.CompareTo(nodeToCompare.FCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return compare;
    }
}
