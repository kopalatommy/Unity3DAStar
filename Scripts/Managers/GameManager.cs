using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager manager;

    public List<Transform> spawnLocs = new List<Transform>();
    public List<Unit> unitsToSpawn = new List<Unit>();
    //public int unitCode;
    //public List<int> unitCodes = new List<int>();

    private void Awake()
    {
        manager = this;
        StartCoroutine(waitForMap());
    }

    //This function waits for the map to be created and ready
    //and then perfroms actions dependent on the map being created
    IEnumerator waitForMap()
    {
        while (Map.instance == null || Map.instance.mapIsReady == false)
        {
            yield return null;
        }

        spawnUnits();
    }

    void spawnUnits()
    {
        for (int i = 0; i < spawnLocs.Count; i++)
        {
            GameObject g = Instantiate(unitsToSpawn[i].gameObject);
            g.transform.position = spawnLocs[i].position;
            Unit u = g.GetComponent<Unit>();
            UnitManager.manager.addUnit(u.occCode, u);
        }
    }

    /*public int requestOccCode()
    {
        int t = unitCode;
        unitCode++;
        unitCodes.Add(t);
        return t;
    }*/
}
