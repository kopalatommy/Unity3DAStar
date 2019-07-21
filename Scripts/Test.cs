using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    List<GameObject> fauxUnits = new List<GameObject>();
    GameObject cube;
    public GameObject sUnit;
    public GameObject mUnit;
    public GameObject lUnit;

    // Start is called before the first frame update
    void Start()
    {
        sendMoveLocs(new Vector3(10, 0, 10));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void sendMoveLocs(Vector3 moveLoc)
    {
        List<Unit> units = new List<Unit>();
        MinHeap<Unit> ordered = new MinHeap<Unit>();

        Unit[,] placed = null;

        int layers = 0;

        for (int i = 0; i < 25; i++)
        {
            GameObject g = Instantiate(sUnit);
            g.transform.position = Vector3.zero;
            units.Add(g.GetComponent<Unit>());
        }

        layers = estimateLayers(units);
        ordered.addItems(units);

        placed = new Unit[layers, layers];

        for (int i = 0; i < layers; i++)
        {
            for (int j = 0; j < layers; j++)
            {
                if(ordered.size > 0)
                    placed[i, j] = ordered.getFront();
            }
        }

        Vector3 position = moveLoc;

        position.x -= placed[0, 0].size * (layers / 2);
        position.z -= placed[0, 0].size * (layers / 2);

        float x = position.x;

        for (int i = 0; i < layers; i++)
        {
            position.x = x;
            for (int j = 0; j < layers; j++)
            {
                print(i + ", " + j);
                if (placed[i,j] != null)
                {
                    placed[i, j].RequestPath(position, 0, 0);
                    position.x += placed[i, j].size * 2;
                }
            }
            position.z += placed[i, 0].size * 2;
        }
    }

    void sendNewMoveLocs(Vector3 pos)
    {
        List<Vector2> moveLocs = new List<Vector2>();
        MinHeap<Unit> sorted = new MinHeap<Unit>();
        List<Unit> units = new List<Unit>();
        List<List<Unit>> ordered = new List<List<Unit>>();
        Vector2 max = new Vector2(pos.x, pos.z);
        Vector2 min = new Vector2(pos.x, pos.z);

        int numRows = 1;
        int cRow = 0;
        int numColumns = 0;
        int cColumn = 0;


        for (int i = 0; i < 25; i++)
        {
            GameObject g = Instantiate(sUnit);
            g.transform.position = Vector3.zero;
            units.Add(g.GetComponent<Unit>());
        }

        sorted.addItems(units);

        print(sorted.size);
        int numLayers = 0;
        while (numLayers * numLayers < estimatedSpace(units))
        {
            numLayers++;
        }
        print("Estimating " + numLayers + " layers for " + units.Count + " units");

        //ordered.Add(new List<Unit>());

        /*for (int i = 0; i < estimatedSpace(units); i++)
        {
            for (int j = 0; j < )
            {

            }
        }*/



        while (sorted.size > 0)
        {
            Unit u = sorted.getFront();

            if (cColumn == numColumns)
            {
                cColumn = 0;
                cRow++;

                if (cRow == numRows)
                {
                    cRow = 0;
                    ordered.Add(new List<Unit>());
                    ordered.Insert(0, new List<Unit>());
                    numRows += 2;
                    numColumns += 2;
                }
            }

            if (cColumn == 0 || cColumn == numColumns - 1)
            {
                ordered[cRow].Add(u);
                cColumn++;
            }
            else
            {
                ordered[cRow].Add(u);
                if (units.Count > 0)
                {
                    u = units[0];
                    units.RemoveAt(0);
                    ordered[cRow].Add(u);
                    cRow++;
                }
                else
                {
                    break;
                }
            }

            /*if (cRow == 0 || cRow == numRows - 1)
            {
                print(cRow + "," + cColumn + " : " + numRows + "," + numColumns);
                ordered[cRow][cColumn] = u;
                cColumn++;
            }
            else
            {
                ordered[cRow][cColumn] = u;
                if (units.Count > 0)
                {
                    u = units[0];
                    units.RemoveAt(0);
                    ordered[cRow][cColumn] = u;
                    cRow++;
                }
                else
                {
                    break;
                }
            }*/
        }
        print2D(ordered);
    }

    void print2D(List<List<Unit>> ordered)
    {
        string s = "";
        for (int i = 0; i < ordered.Count; i++)
        {
            for (int j = 0; j < ordered[0].Count; j++)
            {
                s += ordered[i][j].size;
            }
            print(s);
        }
    }

    void buildMoveLocs()
    {
        Vector2 moveLoc = Vector3.zero;
        List<Vector2> moveLocs = new List<Vector2>();
        List<Unit> units = new List<Unit>();
        //Populate

        MinHeap<Unit> sorted = new MinHeap<Unit>();
        sorted.addItems(units);

        Vector2 max = Vector2.zero;
        Vector2 min = Vector2.zero;
        Vector2 current = Vector2.zero;
        Vector2 index = Vector2.zero;

        Unit u = sorted.getFront();
        u.gameObject.transform.position = moveLoc;

        max.x = u.size / 2;
        max.y = u.size / 2;
        min.x = -u.size / 2;
        min.y = -u.size / 2;

        index.x = -1;
        index.y = -1;

        while (sorted.size > 0)
        {
            u = sorted.getFront();

        }
    }

    int estimatedSpace(List<Unit> units)
    {
        int space = 0;

        foreach (Unit u in units)
        {
            space += (u.size * u.size);
        }

        return space;
    }

    int estimateLayers(List<Unit> units)
    {
        int space = estimatedSpace(units);
        int layers = 0;

        while (layers * layers < space)
        {
            layers++;
        }

        return layers;
    }
}
