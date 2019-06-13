using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager manager;

    Dictionary<int, List<GameObject>> playerUnits = new Dictionary<int, List<GameObject>>();

    public List<Unit> allUnits = new List<Unit>();

    Dictionary<int, Unit> unitCodes = new Dictionary<int, Unit>();
    Dictionary<int, UnitV2> unitV2Codes = new Dictionary<int, UnitV2>();

    int occCode = 0;
    int playerCode = 0;

    private void Awake()
    {
        manager = this;
    }

    public int getPlayerCode()
    {
        int a = playerCode;
        playerCode++;
        return a;
    }

    public int getOccCode(Unit u)
    {
        int a = occCode;
        unitCodes[a] = u;
        occCode++;
        return a;
    }

    public int getOccCode(UnitV2 u)
    {
        int a = occCode;
        unitV2Codes[a] = u;
        occCode++;
        return a;
    }

    public void addUnitToUnitCodes(Unit u)
    {
        unitCodes[u.occCode] = u;
    }

    public Unit getUnitFromUnitCodes(int code)
    {
        return unitCodes[code];
    }

    public void addUnit(int key, Unit u)
    {
        allUnits.Add(u);
        if (playerUnits.ContainsKey(key))
        {
            playerUnits[key].Add(u.gameObject);
        }
        else
        {
            playerUnits[key] = new List<GameObject>();
            playerUnits[key].Add(u.gameObject);
        }
    }

    public void removeUnit(int key, GameObject g)
    {
        playerUnits[key].Remove(g);
    }
}
