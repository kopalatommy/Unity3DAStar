using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager manager;

    public List<Transform> spawnLocs = new List<Transform>();
    public List<Unit> unitsToSpawn = new List<Unit>();

    public List<Unit> player1Units = new List<Unit>();
    public List<Transform> player1SpawnLocs = new List<Transform>();
    public List<Unit> player2Units = new List<Unit>();
    public List<Unit> player2units = new List<Unit>();
    public List<Transform> player2SpawnLocs = new List<Transform>();

    public List<Color> teamColors = new List<Color>();

    //public int unitCode;
    //public List<int> unitCodes = new List<int>();

    private void Awake()
    {
        teamColors.Add(Color.green);
        teamColors.Add(Color.red);
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
        /*for (int i = 0; i < spawnLocs.Count; i++)
        for (int i = 0; i < 1; i++)
        {
            GameObject g = Instantiate(unitsToSpawn[i].gameObject);
            g.transform.position = spawnLocs[i].position;
            Unit u = g.GetComponent<Unit>();
            UnitManager.manager.addUnit(u.occCode, u);
        }*/

        for (int i = 0; i < player1Units.Count; i++)
        {
            GameObject g = Instantiate(player1Units[i].gameObject);
            g.transform.position = player1SpawnLocs[i].position;
            Unit u = g.GetComponent<Unit>();
            u.playerCode = 1;
            u.teamCode = 1;
            //u.health.fill.color = teamColors[0];
            //u.health.s.value = 1;
            u.health.changeColor(Color.green);
            UnitManager.manager.addUnit(u.occCode, u);
            g.name = "Team1Unit" + i;
            UnitSelection.selection.playerUnits.Add(u);
        }

        for (int i = 0; i < player2Units.Count; i++)
        {
            GameObject g = Instantiate(player2Units[i].gameObject);
            g.transform.position = player2SpawnLocs[i].position;
            Unit u = g.GetComponent<Unit>();
            u.playerCode = 2;
            u.teamCode = 2;
            //u.health.fill.color = teamColors[1];
            u.health.changeColor(Color.red);
            UnitManager.manager.addUnit(u.occCode, u);
            g.name = "Team2Unit" + i;
            //UnitSelection.selection.playerUnits.Add(u);
        }

        //StartCoroutine(testSearch());
        
    }

    IEnumerator testSearch()
    {
        yield return new WaitForSeconds(10);
        print("Finished wait");
        Map.instance.addGroupPathRequest(new Vector3(50, 0, 50), UnitSelection.selection.playerUnits);
    }

    /*public int requestOccCode()
    {
        int t = unitCode;
        unitCode++;
        unitCodes.Add(t);
        return t;
    }*/
}
