using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class ParseMap
{
    public GameObject marker;

    private int xCount;
    private int zCount;

    private Node[,] nodes = null;

    struct mapSection
    {
        public int minX;
        public int maxX;
        public int minZ;
        public int maxZ;

        public float[,] cushions;
        public bool walkable;
        public int moveCost;
    }

    ParseMap(int _xCount, int _zCount)
    {
        xCount = _xCount;
        zCount = _zCount;

        ThreadStart start = new ThreadStart(parseMap);
        Thread thread = new Thread(start);
        thread.Start();
    }

    void parseMap()
    {
        mapSection section = new mapSection();
        Vector2Int start;
        Vector2Int end;

        nodes = Map.nodes;
        bool building = false;

        while (!Map.instance.mapIsReady) ;

        /*for (int i = 0; i < xCount; i++)
        {
            for (int j = 0; j < zCount; j++)
            {
                if(!nodes[i, j].touched)
                {
                    section = new mapSection();
                    section.walkable = nodes[i, j].walkable;
                    section.moveCost = nodes[i, j].moveCost;

                    nodes[i, j].touched = true;

                    start = nodes[i, j].index;
                    end = getRange(start, section);
                }
            }
        }*/
    }

    Vector2Int getRange(Vector2Int startPos, mapSection s)
    {
        Vector2Int current = startPos;
        int index = 1;
        int diff = 0;

        while (checkCube(startPos, index, s))
        {
            index++;
        }
        index--;
        current.x += index;
        current.y += index;

        index = 0;
        diff = Mathf.Abs(startPos.y - current.y);
        while (true)
        {
            index++;
            for (int i = 0; i < diff; i++)
            {
                if (!compareNodeToSection(s, nodes[current.x, + current.y + i]))
                {
                    current.y += index;
                    return current;
                }
            }
            current.x++;
        }
    }

    bool checkCube(Vector2Int start, int range, mapSection s)
    {
        for (int i = start.x; i < start.x + range; i++)
        {
            for (int j = start.y; j < start.y + range; j++)
            {
                if (!compareNodeToSection(s, nodes[i,j]))
                {
                    return false;
                }
            }
        }
        return true;
    }

    bool compareNodeToSection(mapSection s,  Node n)
    {
        return n.moveCost == s.moveCost && n.walkable == s.walkable;
    }
}
