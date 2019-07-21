using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class DataSaver
{
    public static DataSaver instance = new DataSaver();

    /*private void Awake()
    {
        instance = this;
    }*/

    public void SaveMap(MapData mData)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.dataPath + "/mapData/" + mData.mName + ".dat");
        bf.Serialize(file, mData);
    }
    public void SaveMap(Node[,] nodes, string mapName, int length, int xSize, int zSize)
    {
        /*mapData mData = new mapData();
        nodeData[,] nData = new nodeData[nodes.GetLength(0), nodes.GetLength(nodes.Rank - 1)];
        Debug.Log("Rank: " + (nodes.Rank - 1));

        for (int i = 0; i < nodes.GetLength(0); i++)
        {
            for (int j = 0; j < nodes.GetLength(nodes.Rank - 1); j++)
            {
                nodeData n = new nodeData();
                n.populate(nodes[i, j]);
                nData[i, j] = n;
            }
        }
        mData.mName = mapName;
        mData.mapNodes = nData;
        mData.xSize = xSize;
        mData.zSize = zSize;
        mData.nodeLength = length;
        saveMap(mData);*/
    }

    public MapData GetMap(string mapName, string dataPath)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(dataPath + "/mapData/" + mapName + ".dat", FileMode.Open);
        Debug.Log("Opened file");
        //object a = bf.Deserialize(file);
        //Debug.Log("Get object");
        try
        {
            MapData temp = (MapData)bf.Deserialize(file);
            Debug.Log("Rebuilt map data");
            file.Dispose();
            file.Close();
            file = null;
            bf = null;
            return temp;
        }
        catch
        {
            Debug.Log("Failed to deserialize file");
            return new MapData();
        }
    }

    public bool MapExists(string mapName)
    {
        return File.Exists(Application.dataPath + "/mapData/" + mapName + ".dat");
    }
}
