using UnityEngine;
using System.Threading;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class CreateMapV2
{
    public static CreateMap instance;


    public static Vector2 nLength;

    public bool mapIsReady = false;
    public GameObject map;

    public string mapName;

    public int xSize;
    public int zSize;

    public float length;

    //private Thread buildMapThread;
    //private Thread getMapThread;

    public float touchedNodes = 0;
    public float totalNodes = 0;

    public bool finsihed = false;

    readonly string dataPath = "";
    public CreateMapV2(bool makeNewMap, float _length, int _xSize, int _zSize, string _mapName)
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
            BuildMap();
        }
        else
        {
            ThreadStart start = new ThreadStart(loadMap);
            Thread thread = new Thread(start);
            thread.Start();
        }
    }

    public Node[,] nodes;
    void BuildMap()
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
                        nodes[xIndex, zIndex] = new Node(walkable, position, xIndex, zIndex, moveCost/*, 0*/);
                        break;
                    }
                }
                if (!hit)
                {
                    position.y = 0;
                    nodes[xIndex, zIndex] = new Node(false, position, xIndex, zIndex, moveCost/*, 0*/);
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

    float GetDist(Node n)
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
                    float dist = Vector3.Distance(n.Position, nodes[x2, z2].Position);
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
                    return Vector3.Distance(n.Position, nodes[x2, z2].Position);
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
                        return Vector3.Distance(n.Position, nodes[x2, z2].Position);
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
                    return Vector3.Distance(n.Position, nodes[x2, z2].Position);
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
                        return Vector3.Distance(n.Position, nodes[x2, z2].Position);
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
                    return Vector3.Distance(n.Position, nodes[x2, z2].Position);
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
                        return Vector3.Distance(n.Position, nodes[x2, z2].Position);
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
                    return Vector3.Distance(n.Position, nodes[x2, z2].Position);
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
                //nodes[x, z].cushion = GetDist(nodes[x, z]);
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

    //////
    //Steps
    //1.Build Map
    //2.Convert map nodes into nde data
    //3.Split up map into sections in structs
    //4.Save sections in mapData/mapName
    //  save structs as mapName#.dat in folder
    IEnumerator saveMap()
    {
        yield return null;
        //int walkable = 0;
        //int nonWalkable = 0;

        //Step one is already done


        //Step 2 convert into mapData
        /*Debug.Log("Saving map");
        nodeData[,] data = new nodeData[(int)(xSize * (1 / length)), (int)(zSize * (1 / length))];
        for (int i = 0; i < (xSize * (1 / length)); i++)
        {
            for (int j = 0; j < (zSize * (1 / length)); j++)
            {
                nodeData n = new nodeData();
                n.populate(nodes[i, j]);
                data[i, j] = n;
            }
            yield return null;
        }

        //Step 3 split up map into sections
        int xSections = Mathf.FloorToInt((xSize * (1/length)) / 100000);
        int zSections = Mathf.FloorToInt((xSize * (1 / length)) / 100000);
        mapData[,] split = new mapData[xSections, zSections];

        for (int x = 0; x < xSections; x++)
        {
            for (int z = 0; z < zSections; z++)
            {
                split[x,z] = new
            }
        }


        mapData mData = new mapData();
        mData.mName = mapName;
        mData.mapNodes = data;
        mData.xSize = xSize;
        mData.zSize = zSize;
        mData.nodeLength = length;

        if (Directory.Exists("/mapData/" + mData.mName))
        {
            Directory.Delete("/mapData/" + mData.mName);
        }

        //DataSaver.instance.saveMap(mData);
        /*if (File.Exists(dataPath + "/mapData/" + mData.mName + ".dat"))
        {
            File.Delete(dataPath + "/mapData/" + mData.mName + ".dat");
            Debug.Log("Deleted file");
        }*/

        /*int numberOfSections = Mathf.FloorToInt(createdNodes / 100000);
        for (int i = 0; i < numberOfSections / 2; i++)
        {
            for (int j = 0; j < numberOfSections / 2; j++)
            {
                mapSection s = new mapSection();
                s.
            }
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(dataPath + "/mapData/" + mData.mName + ".dat");
        bf.Serialize(file, mData);
        file.Close();
        file = null;
        bf = null;

        //Debug.Log("Walkable: " + walkable + ", non walkable: " + nonWalkable);
        mapIsReady = true;*/
        //Debug.Log("Finished making and saving map");
    }

    /*mapData getMapSection(int x, int z)
    {
        mapData m = new mapData();
        m.mName = "1";
    }*/

    public float maxNodes = 0;
    public float createdNodes = 0;
    void loadMap()
    {
        //Debug.Log("Load map");
        /*mapData mData = DataSaver.instance.getMap(mapName, dataPath);

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
        mapIsReady = true;*/
        //Debug.Log("Nodes: " + nodes.Length);
    }
}

[Serializable]
struct mapSection
{
    int sideCount;
    Node[,] section;
    Vector2Int index;
}