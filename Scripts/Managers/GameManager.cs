using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager manager;

    public bool gameInProgress = true;

    public List<Transform> spawnLocs = new List<Transform>();
    public List<Unit> unitsToSpawn = new List<Unit>();

    public List<Unit> player1Units = new List<Unit>();
    public List<Transform> player1SpawnLocs = new List<Transform>();
    public List<Unit> player2Units = new List<Unit>();
    public List<Unit> player2units = new List<Unit>();
    public List<Transform> player2SpawnLocs = new List<Transform>();

    public List<Color> teamColors = new List<Color>();

    //////////////////////
    public bool nextWave = false;
    public int currentWave = 0;
    public int currentPlayableSize = 10;

    //public int unitCode;
    //public List<int> unitCodes = new List<int>();

    public GameObject holder;

    private void Awake()
    {
        teamColors.Add(Color.green);
        teamColors.Add(Color.red);
        manager = this;
        StartCoroutine(testWaves());
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

    IEnumerable handleWaves()
    {
        List<GameObject> printedNodes = new List<GameObject>();
        while (!Map.instance.mapIsReady)
        {
            yield return null;
        }

        List<Node> temp = getWaveBorder(50);
        foreach (Node n in temp)
        {
            GameObject g = Instantiate(Map.instance.nodeMarker);
            g.transform.position = n.position;
            g.transform.localScale = Map.vLength;
        }
        /*while (gameInProgress)
        {


            yield return null;
        }*/
    }

    public int waveNum = 0;
    public List<GameObject> printedNodes = new List<GameObject>();
    IEnumerator testWaves()
    {
        while (Map.instance == null || !Map.instance.mapIsReady)
        {
            yield return null;
        }
        waveNum = 0;
        while (gameInProgress)
        {
            if (printedNodes.Count > 0)
            {
                foreach (GameObject g in printedNodes)
                {
                    Destroy(g);
                }
            }
            printedNodes = new List<GameObject>();
            List<Node> temp = getWaveBorder(200 + (waveNum * 10));
//            print("Wave: " + waveNum);
            foreach (Node n in temp)
            {
                GameObject g = Instantiate(Map.instance.nodeMarker);
                g.transform.position = n.position;
                g.transform.localScale = Map.vLength;
                printedNodes.Add(g);
            }
            yield return new WaitForSeconds(1);
            waveNum++;
            if (waveNum > Map.xSize * (1/Map.length))
            {
                waveNum = 0;
            }
//            print("Increased wave num");
        }
    }

    List<Node> getWaveBorder(int range)
    {
        List<Node> toReturn = new List<Node>();

        int x = Map.xSize - (int)(1/Map.length);
        int z = Map.xSize - (int)(1 / Map.length);

//        Debug.Log("(" + x + "," + z + ")");

        x -= ((int)(1 / Map.length) / 2) + (range / 2);
        z += ((int)(1 / Map.length) / 2) + (range / 2);
//        Debug.Log("(" + x + "," + z + ")");
        for (int i = 0; i < range; i++)
        {
            toReturn.Add(Map.nodes[x,z]);
            x++;
        }
        for (int i = 0; i < range; i++)
        {
            toReturn.Add(Map.nodes[x, z]);
            z--;
        }
        for (int i = 0; i < range; i++)
        {
            toReturn.Add(Map.nodes[x, z]);
            x--;
        }
        for (int i = 0; i < range; i++)
        {
            toReturn.Add(Map.nodes[x, z]);
            z++;
        }
        return toReturn;
    }
}
