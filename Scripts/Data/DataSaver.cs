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

    public void saveMap(mapData mData)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.dataPath + "/mapData/" + mData.mName + ".dat");
        bf.Serialize(file, mData);
    }
    public void saveMap(Node[,] nodes, string mapName, int length, int xSize, int zSize)
    {
        mapData mData = new mapData();
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
        saveMap(mData);
    }

    public mapData getMap(string mapName, string dataPath)
    {
        BinaryFormatter bf = new BinaryFormatter();
        //FileStream file = File.Open(Application.dataPath + "/mapData/" + mapName + ".dat", FileMode.Open);
        //Debug.Log("Data path: " + dataPath);
        FileStream file = File.Open(dataPath + "/mapData/" + mapName + ".dat", FileMode.Open);
        mapData temp = (mapData)bf.Deserialize(file);
        file.Close();
        return temp;
    }

    public bool mapExists(string mapName)
    {
        return File.Exists(Application.dataPath + "/mapData/" + mapName + ".dat");
    }
}
