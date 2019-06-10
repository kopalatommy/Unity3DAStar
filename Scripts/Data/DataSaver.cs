using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class DataSaver : MonoBehaviour
{
    public static DataSaver instance;

    private void Awake()
    {
        instance = this;
    }

    public void saveMap(mapData mData)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.dataPath + "/mapData/" + mData.mName + ".dat");
        bf.Serialize(file, mData);
    }

    public mapData getMap(string mapName)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.dataPath + "/mapData/" + mapName + ".dat", FileMode.Open);
        mapData temp = (mapData)bf.Deserialize(file);
        file.Close();
        return temp;
    }
}
