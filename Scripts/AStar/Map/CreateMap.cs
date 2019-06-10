using UnityEngine;
using System.Threading;
using System;

//If a bunch of nodes are null, check the position of the map, all areas of the map should be above 0. IE, if length = 100x100 than positon should = 50,0,50 

public class CreateMap
{
    public static CreateMap instance;

    public Node[,] nodes;
    public static Vector2 nLength;

    public bool mapIsReady = false;
    public GameObject map;

    public string mapName;

    public int xSize;
    public int zSize;

    public float length;

    private Thread buildMapThread;
    private Thread getMapThread;

    public CreateMap(bool makeNewMap, float len, int xs, int zs, string mName)
    {
        mapName = mName;
        xSize = xs;
        zSize = zs;
        length = len;
        nLength = new Vector2(1 / len, 1 / len);
        instance = this;

        if (makeNewMap)
        {
            buildMap();
        }
        else
        {

        }
    }

    private void buildMap()
    {
        int num = Mathf.FloorToInt(1 / length);
        nodes = new Node[xSize * num, zSize * num];

        int xIndex = 0;
        int zIndex = 0;
        int count = 0;

        for (float x = 0; x < xSize; x += length)
        {
            zIndex = 0;
            for (float z = 0; z < zSize; z += length)
            {
                count++;
                bool walkable = false;
                bool hit = false;
                Vector3 position = new Vector3(0,0,0);
                int moveCost = 0;
                for (float y = 5; y >= -1; y -= length)
                {
                    position = new Vector3(x,y,z);
                    Collider[] hits = Physics.OverlapSphere(position, length + 0.01f);
                    if (hits.Length > 0)
                    {
                        hit = true;
                        position = new Vector3(x,y - length,z);
                        foreach (Collider c in hits)
                        {
                            if (c.gameObject.tag == "Ground")
                            {
                                walkable = true;
                                moveCost = (10 > moveCost) ? 10 : moveCost;
                            }
                            else if (c.gameObject.tag == "Road")
                            {
                                walkable = true;
                                //No move cost
                            }
                            else if (c.gameObject.tag == "Mud")
                            {
                                walkable = true;
                                moveCost = (15 > moveCost) ? 15 : moveCost;
                            }
                            else if (c.gameObject.tag == "Obstacle")
                            {
                                walkable = false;
                                break;
                            }
                        }
                        nodes[xIndex, zIndex] = new Node(walkable, position, xIndex, zIndex, moveCost);
                        break;
                    }
                }
                if (!hit)
                {
                    //Debug.Log("Did not hit");
                    position.y = 0;
                    nodes[xIndex, zIndex] = new Node(false, position, xIndex, zIndex, moveCost);
                }
                zIndex++;
            }
            //Debug.Log("X index = " + xIndex);
            xIndex++;
        }
        //Debug.Log("Size = " + count + " : " + nodes.Length);
        makeCushion();
    }

    public void markCritical()
    {
        foreach (Node n in nodes)
        {
            foreach (Node o in Map.getNeighbors(n))
            {
                if (o.walkable)
                {
                    n.critical = true;
                    break;
                }
            }
        }
    }

    void makeCushion()
    {
        for (int x = 0; x < xSize * (1 / length); x++)
        {
            for (int z = 0; z < zSize * (1 / length); z++)
            {
                nodes[x, z].cushion = getDist(nodes[x, z]);
            }
        }
        mapIsReady = true;
    }

    float getDist(Node n)
    {
        int num = 1;
        int x = n.xIndex;
        int z = n.zIndex;

        while (true)
        {
            for (int i = 0; i < (num * 2) + 1; i++)
            {
                int x2 = x - num + i;
                int z2 = z - num;
                if (x2 >= 0 && x2 < xSize * (1 / length) && z2 >= 0 && z2 < zSize * (1 / length))
                {
                    if(!nodes[x2, z2].walkable)
                    {
                        return Vector3.Distance(n.position, nodes[x2, z2].position);
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
                    return Vector3.Distance(n.position, nodes[x2, z2].position);
                }
            }

            for (int i = 0; i < (num * 2) - 1; i++)
            {
                int x2 = x + num;
                int z2 = z - num + i;
                if (x2 >= 0 && x2 < xSize * (1 / length) && z2 >= 0 && z2 < zSize * (1 / length))
                {
                    if (!nodes[x2, z2].walkable)
                    {
                        return Vector3.Distance(n.position, nodes[x2, z2].position);
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
                    return Vector3.Distance(n.position, nodes[x2, z2].position);
                }
            }

            for (int i = 0; i < (num * 2) - 1; i++)
            {
                int x2 = x + num - i;
                int z2 = z + num;
                if (x2 >= 0 && x2 < xSize * (1 / length) && z2 >= 0 && z2 < zSize * (1 / length))
                {
                    if (!nodes[x2, z2].walkable)
                    {
                        return Vector3.Distance(n.position, nodes[x2, z2].position);
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
                    return Vector3.Distance(n.position, nodes[x2, z2].position);
                }
            }

            for (int i = 0; i < (num * 2) - 1; i++)
            {
                int x2 = x - num;
                int z2 = z + num - i;
                if (x2 >= 0 && x2 < xSize * (1 / length) && z2 >= 0 && z2 < zSize * (1 / length))
                {
                    if (!nodes[x2, z2].walkable)
                    {
                        return Vector3.Distance(n.position, nodes[x2, z2].position);
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
                    return Vector3.Distance(n.position, nodes[x2, z2].position);
                }
            }
            num++;
        }
    }

    public void saveMap()
    {
        int num = Mathf.FloorToInt(1 / length);
        nodeData[,] nData = new nodeData[xSize * num, zSize * num];
        for (int x = 0; x < xSize * num; x++)
        {
            for (int z = 0; z < zSize * num; z++)
            {
                nData[x, z] = new nodeData();
                nData[x, z].populate(nodes[x,z]);
            }
        }
        mapData data = new mapData();
        data.xSize = xSize;
        data.zSize = zSize;
        data.nodeLength = length;
        data.mapNodes = nData;
        data.mName = mapName;
        DataSaver.instance.saveMap(data);
    }
}

[Serializable]
public struct nodeData
{
    public float x, y, z;
    public int xIndex, zIndex;
    public float cushion;
    public bool isWalkable;
    public int moveCost;

    public void populate(Node n)
    {
        x = n.position.x;
        y = n.position.y;
        z = n.position.z;

        xIndex = n.xIndex;
        zIndex = n.zIndex;

        cushion = n.cushion;

        isWalkable = n.walkable;

        moveCost = n.moveCost;
    }

    public override string ToString()
    {
        return "Node data located at " + x + ", " + y + ", " + z + " is walkable: " + isWalkable;
    }
}

[Serializable]
public struct mapData
{
    public string mName;
    public nodeData[,] mapNodes;
    public int xSize;
    public int zSize;
    public float nodeLength;
}
