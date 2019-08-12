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

    //private Thread buildMapThread;
    //private Thread getMapThread;

    public float touchedNodes = 0;
    public float totalNodes = 0;

    public bool finsihed = false;

    readonly string dataPath = "";
    public CreateMap(bool makeNewMap, float _length, int _xSize, int _zSize, string _mapName)
    {
        //Debug.Log("Size " + System.Runtime.InteropServices.Marshal.SizeOf(new NodeData2()));
        //Debug.Log("Size " + System.Runtime.InteropServices.Marshal.SizeOf(new NodeData()));

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
            ThreadStart start = new ThreadStart(LoadMap);
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

        ThreadStart start = new ThreadStart(MarkCritical);
        Thread thread = new Thread(start);
        thread.Start();

        /*ThreadStart start = new ThreadStart(MakeCushion);
        Thread thread = new Thread(start);
        thread.Start();
        //makeCushion();*/
        //BuildWithThreads();
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

    /*void MakeCushion()
    {
        totalNodes = (xSize * (1 / length)) * (zSize * (1 / length));

        for (int x = 0; x < xSize * (1 / length); x++)
        {
            for (int z = 0; z < zSize * (1 / length); z++)
            {
                touchedNodes++;
                nodes[x, z].cushion = GetDist(nodes[x, z]);
            }
        }
        mapIsReady = true;

        MarkCritical();

        SaveMap();
    }*/

    List<Node> GetNeighbors(Node n)
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

    public void MarkCritical()
    {
        foreach (Node n in nodes)
        {
            touchedNodes++;
            foreach (Node o in GetNeighbors(n))
            {
                if (!o.walkable)
                {
                    n.critical = true;
                    break;
                }
            }
        }
        mapIsReady = true;
        //SaveMap();
    }

    void SaveMap()
    {
        Debug.Log("Saving map");
        //UnityEngine.Debug.Log("Nodes: " + nodes.Length);
        MapData mData = new MapData()
        {
            mName = mapName,
            //mapNodes = nodes,
            xSize = xSize,
            zSize = zSize,
            nodeLength = length
        };
        /*mData.mName = mapName;
        mData.mapNodes = nodes;
        mData.xSize = xSize;
        mData.zSize = zSize;
        mData.nodeLength = length;*/

        NodeData[,] nData = new NodeData[Mathf.FloorToInt(xSize * (1 / length)), Mathf.FloorToInt(zSize * (1 / length))];
        for (int i = 0; i < xSize * (1 / length); i++)
        {
            for (int j = 0; j < zSize * (1 / length); j++)
            {
                nData[i, j].Populate(nodes[i,j]);
            }
        }
        mData.mapNodes = nData;

        Debug.Log("Build map data");

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
        file = null;
        bf = null;

        //Debug.Log("Walkable: " + walkable + ", non walkable: " + nonWalkable);
        mapIsReady = true;

        Debug.Log("Finished saving map");
        //Debug.Log("Finished making and saving map");
    }

    public float maxNodes = 0;
    public float createdNodes = 0;
    void LoadMap()
    {
        Debug.Log("Loading map");
        MapData mData = DataSaver.instance.GetMap(mapName, dataPath);
        Debug.Log("Got map");

        length = mData.nodeLength;
        xSize = mData.xSize;
        zSize = mData.zSize;

        totalNodes = xSize * (1 / length) * (zSize * (1 / length));
        createdNodes = 0;
        Debug.Log(mData.mapNodes.Length);
        MapManager.nodes = new Node[Mathf.FloorToInt(xSize * (1 / length)), Mathf.FloorToInt(zSize * (1 / length))];
        //Map.nodes = mData.mapNodes;
        //Map.nodes = new Node[Mathf.FloorToInt(xSize * (1 / length)), Mathf.FloorToInt(zSize * (1 / length))];
        for (int i = 0; i < xSize * (1 / length); i++)
        {
            for (int j = 0; j < zSize * (1 / length); j++)
            {
                NodeData n = mData.mapNodes[i, j];
                MapManager.nodes[i, j] = new Node(n.isWalkable, new Vector3(i / 2, n.y, j / 2), i, j, n.moveCost/*, n.cushion*/);
            }
        }
        mapIsReady = true;
        Debug.Log("Nodes: " + MapManager.nodes.Length);
    }

    void BuildWithThreads()
    {
        totalNodes = (xSize * (1 / length)) * (zSize * (1 / length));

        Debug.Log("XSize: " + xSize + ", zSize: " + zSize);
        Debug.Log("Lengths(" + (xSize * (1 / length)) + ", " + (zSize * (1 / length)) + ")");
        Debug.Log("Total nodes: " + ((xSize * (1 / length)) * (zSize * (1 / length)) ));
        Debug.Log("Sections: " + ((xSize * (1 / length)) / 100) + ", left behind: " + ((xSize * (1 / length)) % 100));
        Debug.Log("Length: " + xSize * (1 / length) / 20);

        int sections = 0;

        for (int i = 0; i < (xSize * (1 / length)); i += Mathf.FloorToInt((xSize * (1 / length)) / 100))
        {
            for (int j = 0; j < (zSize * (1 / length)); j += Mathf.FloorToInt((zSize * (1 / length)) / 100))
            {
                //Debug.Log("Starting thread");
                //ThreadPool.QueueUserWorkItem(new WaitCallback(BuildSection(Process, 0, 0, 0, 0)));
                ThreadPool.QueueUserWorkItem(new WaitCallback(BuildSection));
                sections++;
            }
        }

        Debug.Log("Created " + sections + " sections");



        /*for (int x = 0; x < xSize * (1 / length); x++)
        {
            for (int z = 0; z < zSize * (1 / length); z++)
            {
                touchedNodes++;
                nodes[x, z].cushion = GetDist(nodes[x, z]);
            }
        }
        mapIsReady = true;

        MarkCritical();

        SaveMap();*/
    }

    int xStart = 0;
    int xStop = 10000;
    int zStart = 0;
    int zStop = 10000;

    int threadIndex = 0;

    Mutex mutex = new Mutex();
    //Mutex metex = new Mutex();
    void BuildSection(object callback)
    {
        mutex.WaitOne();

        threadIndex += 1;

        //Debug.Log(threadIndex + " has entered the mutex");

        int mXStart = xStart;
        int mXStop = xStop;
        int mZStart = zStart;
        int mZStop = zStop;
        int mTouchedNodes = 0;

        if (xStop == (xSize * (1 / length)))
        {
            xStart = 0;
            xStop = Mathf.FloorToInt((xSize * (1 / length)) / 20);
            zStart += Mathf.FloorToInt((zSize * (1 / length)) / 20);
            zStop += Mathf.FloorToInt((zSize * (1 / length)) / 20);
        }
        else
        {
            xStart += Mathf.FloorToInt((xSize * (1 / length)) / 20);
            xStop += Mathf.FloorToInt((xSize * (1 / length)) / 20);
        }

        //Debug.Log(threadIndex + " is releasing the mutex");


        mutex.ReleaseMutex();

        //Debug.Log("X Range(" + mXStart + ", " + mXStop + ")");
        //Debug.Log("Z Range(" + mZStart + ", " + mZStop + ")");

        for (int i = xStart; i < xStop; i++)
        {
            for (int j = zStart; j < zStop; j++)
            {
                //Debug.Log("("+i+","+j+")");
                /*nodes[i, j].cushion = GetDist(nodes[i, j]);
                foreach (Node n in GetNeighbors(nodes[i,j]))
                {
                    if (!n.walkable)
                    {
                        nodes[i,j].critical = true;
                        break;
                    }
                }*/
                mTouchedNodes++;
                //touchedNodes++;
            }
        }
        Debug.Log("Section nodes: " + mTouchedNodes + ", " + ( (xStop - xStart) * (zStop - zStart) ) );
    }
    /*void BuildSection(int xStart, int xStop, int zStart, int zStop)
    {

    }*/
    /*void BuildSection(object callback, int xStart, int xStop, int zStart, int zStop)
    {
        
    }*/

    /*int testV
    {
        get
        {
            return testV;
        }
        set
        {
            metex.WaitOne();
            testV = value;
            metex.ReleaseMutex();
        }
    }*/

    int testVal = 0;
    void test(object callback)
    {
        testVal += 1;
        Debug.Log(testVal);
    }
}

[Serializable]
public struct NodeData2
{
    public float x, y, z;
    public int xIndex, zIndex;
    //public float cushion;
    public bool isWalkable;
    public float moveCost;
    public bool critical;

    public void Populate(Node n)
    {
        x = n.x;
        y = n.y;
        z = n.z;

        xIndex = n.xIndex;
        zIndex = n.zIndex;

        //cushion = n.cushion;

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
public struct NodeData
{
    public float y;
    //public float cushion;
    public int moveCost;
    public bool isWalkable;

    public void Populate(Node n)
    {
        y = n.y;
        //cushion = n.cushion;
        isWalkable = n.walkable;
        moveCost = n.moveCost;
    }
}

[Serializable]
public struct MapData
{
    public string mName;
    //public Node[,] mapNodes;
    public NodeData[,] mapNodes;
    public int xSize;
    public int zSize;
    public float nodeLength;
}
