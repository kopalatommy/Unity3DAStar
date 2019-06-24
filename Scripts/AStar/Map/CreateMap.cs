using UnityEngine;
using System.Threading;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using System.Collections;

//If a bunch of nodes are null, check the position of the map, all areas of the map should be above 0. IE, if length = 100x100 than positon should = 50,0,50 

public class CreateMap
{
    public static CreateMap instance;

    
    public static Vector2 nLength;

    public bool mapIsReady = false;
    public GameObject map;

    public string mapName;

    public int xSize;
    public int zSize;

    public float length;

    private Thread buildMapThread;
    private Thread getMapThread;

    public float touchedNodes = 0;
    public float totalNodes = 0;

    public bool finsihed = false;

    string dataPath = "";
    public CreateMap(bool makeNewMap, float _length, int _xSize, int _zSize, string _mapName)
    {
        length = _length;
        xSize = _xSize;
        zSize = _zSize;
        mapName = _mapName;

        dataPath = Application.dataPath;
        if (makeNewMap)
        {
            /*ThreadStart start = new ThreadStart(buildMap);
            Thread thread = new Thread(start);
            thread.Start();*/
            buildMap();
        }
        else
        {
            ThreadStart start = new ThreadStart(loadMap);
            Thread thread = new Thread(start);
            thread.Start();
        }
    }

    public Node[,] nodes;
    void buildMap()
    {
        int zIndex = 0;
        int xIndex = 0;

        //Debug.Log("Building path");
        //UnityEngine.Debug.Log("X size: " + xSize + ", Z size: " + zSize);
        //UnityEngine.Debug.Log("Length: " + length);

        totalNodes = xSize * (1 / length) * (zSize * (1 / length));
        //UnityEngine.Debug.Log("Total nodes: " + totalNodes);

        int num = Mathf.FloorToInt(1 / length);
        nodes = new Node[xSize * num, zSize * num];

        for (float x = 0; x < xSize; x += length)
        {
            zIndex = 0;
            for (float z = 0; z < zSize; z += length)
            {
                createdNodes++;
                bool walkable = false;
                bool hit = false;
                Vector3 position = new Vector3(0, 0, 0);
                int moveCost = 0;
                for (float y = 5; y >= -1; y -= length)
                {
                    position = new Vector3(x, y, z);
                    Collider[] hits = Physics.OverlapSphere(position, length + 0.01f);
                    if (hits.Length > 0)
                    {
                        hit = true;
                        position = new Vector3(x, y - length, z);
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
                    position.y = 0;
                    nodes[xIndex, zIndex] = new Node(false, position, xIndex, zIndex, moveCost);
                }
                zIndex++;
            }
            xIndex++;
        }
        ThreadStart start = new ThreadStart(makeCushion);
        Thread thread = new Thread(start);
        thread.Start();
        //makeCushion();
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
                    float dist = Vector3.Distance(n.position, nodes[x2, z2].position);
                    if (!nodes[x2, z2].walkable)
                    {
                        return dist;
                    }
                    else if (dist > 20)
                    {
                        return dist;
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
                /*if (Vector3.Distance(n.position, nodes[x2, z2].position) >= maxSearchSize)
                {
                    return maxSearchSize;
                }*/
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

    void makeCushion()
    {
        totalNodes = (xSize * (1 / length)) * (zSize * (1 / length));

        for (int x = 0; x < xSize * (1 / length); x++)
        {
            for (int z = 0; z < zSize * (1 / length); z++)
            {
                touchedNodes++;
                nodes[x, z].cushion = getDist(nodes[x, z]);
            }
        }
        mapIsReady = true;

        markCritical();

        Debug.Log("Saving map");
        IEnumerator save = saveMap();
        while (save.MoveNext()) ;
        Debug.Log("Finished saving");
    }

    List<Node> getNeighbors(Node n)
    {
        List<Node> neighbors = new List<Node>();
        int xIndex = n.xIndex;
        int zIndex = n.zIndex;

        if (xIndex < 200 && xIndex >= 1 && zIndex < 200 && zIndex >= 1)
        {
            neighbors.Add(nodes[xIndex - 1, zIndex - 1]);
        }

        if (xIndex < 200 && xIndex >= 0 && zIndex < 199 && zIndex >= 0)
        {
            neighbors.Add(nodes[xIndex, zIndex + 1]);
        }


        if (xIndex < 200 && xIndex >= 0 && zIndex < 200 && zIndex >= 1)
        {
            neighbors.Add(nodes[xIndex, zIndex - 1]);
        }


        if (xIndex < 199 && xIndex >= 1 && zIndex < 200 && zIndex >= 0)
        {
            neighbors.Add(nodes[xIndex + 1, zIndex]);
        }


        if (xIndex < 200 && xIndex >= 1 && zIndex < 200 && zIndex >= 0)
        {
            neighbors.Add(nodes[xIndex - 1, zIndex]);
        }


        if (xIndex < 199 && xIndex >= 0 && zIndex < 199 && zIndex >= 0)
        {
            neighbors.Add(nodes[xIndex + 1, zIndex + 1]);
        }


        if (xIndex < 199 && xIndex >= 0 && zIndex < 200 && zIndex >= 1)
        {
            neighbors.Add(nodes[xIndex + 1, zIndex - 1]);
        }


        if (xIndex < 200 && xIndex >= 1 && zIndex < 199 && zIndex >= 0)
        {
            neighbors.Add(nodes[xIndex - 1, zIndex + 1]);
        }

        return neighbors;
    }

    public void markCritical()
    {
        foreach (Node n in nodes)
        {
            foreach (Node o in getNeighbors(n))
            {
                if (o.walkable)
                {
                    n.critical = true;
                    break;
                }
            }
        }
    }

    IEnumerator saveMap()
    {
        int walkable = 0;
        int nonWalkable = 0;

        Debug.Log("Saving map");
        //UnityEngine.Debug.Log("Nodes: " + nodes.Length);
        mapData mData = new mapData();
        nodeData[,] data = new nodeData[(int)(xSize * (1 / length)), (int)(zSize * (1 / length))];
        for (int i = 0; i < (xSize * (1 / length)); i++)
        {
            for (int j = 0; j < (zSize * (1 / length)); j++)
            {
                nodeData n = new nodeData();
                if (nodes[i,j].walkable)
                {
                    walkable++;
                }
                else
                {
                    nonWalkable++;
                }
                n.populate(nodes[i, j]);
                data[i, j] = n;
            }
            yield return null;
        }
        mData.mName = mapName;
        mData.mapNodes = data;
        mData.xSize = xSize;
        mData.zSize = zSize;
        mData.nodeLength = length;

        //DataSaver.instance.saveMap(mData);
        if (File.Exists(dataPath + "/mapData/" + mData.mName + ".dat"))
        {
            File.Delete(dataPath + "/mapData/" + mData.mName + ".dat");
            Debug.Log("Deleted file");
        }
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(dataPath + "/mapData/" + mData.mName + ".dat");
        bf.Serialize(file, mData);
        file.Close();

        //Debug.Log("Walkable: " + walkable + ", non walkable: " + nonWalkable);
        mapIsReady = true;
        //Debug.Log("Finished making and saving map");
    }

    public float maxNodes = 0;
    public float createdNodes = 0;
    void loadMap()
    {
        //Debug.Log("Load map");
        mapData mData = DataSaver.instance.getMap(mapName, dataPath);

        length = mData.nodeLength;
        xSize = mData.xSize;
        zSize = mData.zSize;

        totalNodes = xSize * (1 / length) * (zSize * (1 / length));
        createdNodes = 0;

        nodes = new Node[(int)(xSize * (1 / length)), (int)(zSize * (1 / length))];
        for (int i = 0; i < xSize * (1 / length); i++)
        {
            for (int j = 0; j < zSize * (1 / length); j++)
            {
                createdNodes++;
                nodes[i, j] = new Node(mData.mapNodes[i, j]);
            }
        }
        mapIsReady = true;
        //Debug.Log("Nodes: " + nodes.Length);
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
    public bool critical;

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

        critical = n.critical;
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
