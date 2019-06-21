using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Node : IComparable<Node>
{
    public float cushion = -1;

    public int xIndex = 0;
    public int zIndex = 0;
    public bool walkable = false;
    public int moveCost = 0;
    public Vector3 position;

    //A star calculation variables
    public int gCost;
    public int hCost;
    public Node parent;
    public int heapIndex = 0;

    //Unit avoidence
    public int occCode = -1;
    public bool isOccupied = false;

    //PathFinding
    public bool critical = false;

    //Search algorithm
    public bool claimed = false;

    public Node(bool _walkable, Vector3 pos, int xIn, int zIn, int mCost)
    {
        position = pos;
        moveCost = mCost;
        walkable = _walkable;
        xIndex = xIn;
        zIndex = zIn;
    }

    public void revert()
    {

    }

    // AStar methods
    //Full cost
    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return compare;
    }
}
