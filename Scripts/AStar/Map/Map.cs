using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Map : MonoBehaviour
{
    public static Map instance;

    public bool makeNewMap = false;
    public string mapName;
    public int xSize = 0;
    public int zSize = 0;
    public static float length = .5f;
    public static Node[,] nodes;

    public bool mapIsReady = false;
    public bool cushionMade = false;

    public GameObject nodeMarker;
    public GameObject nodeMarker2;
    public GameObject nodeMarker3;

    private List<PathRequest> pathRequestQueue = new List<PathRequest>();
    PathFinderV2 aStar;
    PathFinderA1 aStarA1;
    PathFinderA2 aStarA2;
    List<Node> path = null;
    List<Vector3> vPath = null;

    public Vector3 testPos;
    public bool create;

    public List<Node> testList = new List<Node>();

    public List<GameObject> printedNodes = new List<GameObject>();
    public bool showOccupied = false;
    public bool revertOccupied = false;
    public int testCode = 0;
    public bool buildTestPath = false;
    public int x1 = 0;
    public int z1 = 0;
    public int x2 = 0;
    public int z2 = 0;

    private void Start()
    {
        instance = this;
        StartCoroutine(setUpMap());
        StartCoroutine(buildPaths());
    }

    private void Update()
    {
        if (create)
        {
            create = !create;
            GameObject g = Instantiate(nodeMarker);
            g.transform.position = testPos;
            g.transform.localScale = Vector3.one * length;
            testPos = Vector3.zero;
        }

        if (showOccupied)
        {
            showOccupied = false;
            foreach (Node n in nodes)
            {
                if (n.isOccupied)
                {
                    GameObject g = Instantiate(nodeMarker);
                    g.transform.position = n.position;
                    g.transform.localScale = Vector3.one * length;
                    printedNodes.Add(g);
                }
            }
            //testPathA1();
        }
        if (revertOccupied)
        {
            revertOccupied = false;
            foreach (GameObject g in printedNodes)
            {
                Destroy(g);
            }
            printedNodes = new List<GameObject>();
        }
        if (buildTestPath)
        {
            buildTestPath = false;
            //testPathA1(nodes[x1,z1], nodes[x2,z2], testCode);
            GetComponent<PathFinderA1Test>().requestPath(nodes[x1, z1], nodes[x2, z2], 1, testCode);
        }
    }

    List<Node> alteredNodes = new List<Node>();
    private void setUpNodes()
    {
        int count = 0;
        foreach (Node n in Map.nodes)
        {
            n.revert();
            if (n.isOccupied)
            {
                alteredNodes.Add(n);
                n.walkable = false;
                count++;
            }
        }
        Debug.Log("Count: " + count);
    }

    IEnumerator setUpMap()
    {
        if (makeNewMap)
        {
            CreateMap m = new CreateMap(makeNewMap,length, xSize, zSize, mapName);
            while (!m.mapIsReady)
            {
                yield return null;
            }
            nodes = m.nodes;
            foreach (Node n in nodes)
            {
                foreach (Node o in Map.getNeighbors(n))
                {
                    if (!o.walkable)
                    {
                        n.critical = true;
                        break;
                    }
                }
            }
            mapIsReady = true;
            //showCost();
        }
        else
        {
            CreateMap m = new CreateMap(makeNewMap, length, xSize, zSize, mapName);
            while (!m.mapIsReady)
            {
                yield return null;
            }
            nodes = m.nodes;
            mapIsReady = true;
            GameObject g = Instantiate((GameObject)Resources.Load(mapName));
            g.transform.position = new Vector3(xSize / 2, 0, zSize / 2);
            g = Instantiate(nodeMarker);
            g.transform.position = nodes[0, 0].position;
            g.transform.localScale = Vector3.one * length;
        }
    }

    void showCost()
    {
        foreach (Node n in nodes)
        {
            if (!n.walkable)
                continue;
            GameObject g = (n.moveCost == 0) ? Instantiate(nodeMarker) : Instantiate(nodeMarker2);
            g.transform.position = n.position;
            g.transform.localScale = Vector3.one * length;
        }
    }

    void showNodes()
    {
        foreach (Node n in nodes)
        {
            if (n == null || !n.walkable)
            {
                continue;
            }
            GameObject g = Instantiate(nodeMarker);
            g.transform.localScale = new Vector3(.5f, .5f * n.cushion, .5f);
            g.transform.position = n.position;
        }
    }

    void showWalkable()
    {
        foreach (Node n in nodes)
        {
            if (n == null)
            {
                continue;
            }
            if (n.walkable)
            {
                GameObject g = Instantiate(nodeMarker);
                g.transform.localScale = Vector3.one * length;
                g.transform.position = n.position;
            }
            else
            {
                GameObject g = Instantiate(nodeMarker2);
                g.transform.localScale = Vector3.one * length;
                g.transform.position = n.position;
            }
        }
    }

    void showCritical()
    {
        foreach (Node n in nodes)
        {
            if (n.walkable && n.critical)
            {
                GameObject g = Instantiate(nodeMarker2);
                g.transform.localScale = Vector3.one * length;
                g.transform.position = n.position;
            }
        }
    }

    public void requestPath(Node start, Node goal, Unit u, int p, int sc)
    {
        PathRequest r = new PathRequest();
        r.start = start;
        r.end = goal;
        r.requestee = u;
        r.size = u.size;
        r.priority = p;
        r.specialCode = sc;
        if (pathRequestQueue.Count > 0)
        {
            for (int i = pathRequestQueue.Count - 1; i >= 0; i--)
            {
                if (pathRequestQueue[i].priority < p)
                {
                    pathRequestQueue.Insert(i, r);
                    return;
                }
            }
            pathRequestQueue.Add(r);
        }
        else
        {
            pathRequestQueue.Add(r);
        }
    }

    IEnumerator buildPaths()
    {
        while (!mapIsReady)
        {
            yield return null;
        }
        Unit requestee = null;
        bool building = false;
        while (true)
        {
            if (vPath == null && !building && pathRequestQueue.Count > 0)
            {
                building = true;
                PathRequest req = pathRequestQueue[0];
                requestee = req.requestee;
                pathRequestQueue.RemoveAt(0);
                //aStar = new PathFinder(req.start, req.end, req.occupied, req.size);

                if (req.specialCode == 0)
                {
                    //print("Starting regular");
                    aStarA2 = null;
                    aStar = new PathFinderV2(req);
                    StartCoroutine(RunAStar());
                }
                else if(req.specialCode == 1)
                {
                    //print("Starting A1");
                    aStar = null;
                    aStarA2 = new PathFinderA2(req);
                    //aStarA1.requestPath(req.start, req.end, req.requestee.size, req.requestee.occCode);
                    StartCoroutine(RunAStarA1());
                }
                else
                {
                    print("Req attribute code: " + req.specialCode);
                }
                yield return null;
            }
            else if (building && vPath != null)
            {
                building = false;
                requestee.getPath(vPath);
                /*foreach (Node n in testList)
                {
                    GameObject g = Instantiate(nodeMarker);
                    g.transform.position = n.position;
                    g.transform.localScale = Vector3.one * length;
                }*/
                vPath = null;
                aStar = null;
                aStarA2 = null;
                yield return null;
            }
            else if (aStar != null && aStar.failed && building)
            {
                //print("Failed to find path");
                vPath = null;
                building = false;
            }
            yield return null;
        }
    }

    IEnumerator RunAStar()
    {
        while (!aStar.done && !aStar.failed)
        {
            yield return null;
        }
        if (aStar.failed)
        {
            print("Pathfinding failed");
        }
        //print("Finished regular");
        vPath = aStar.vPath;
        testList = aStar.critical;
        StopCoroutine(RunAStar());
    }

    //This is the generic A star where 
    //all nodes occupied by stopped units
    //are set to unwalkable
    IEnumerator RunAStarA1()
    {
        while (!aStarA2.done && !aStarA2.failed)
        {
            yield return null;
        }
        if (aStarA2.failed)
        {
            print("Pathfinding failed");
        }
        print("Finished A2");
        vPath = aStarA2.vPath;
        testList = aStarA2.critical;
        StopCoroutine(RunAStarA1());
    }

    public Node getNodeFromLocation(Vector3 location)
    {
        float x = getClosestFloat(location.x * (1 / length));
        float z = getClosestFloat(location.z * (1 / length));

        if (x >= xSize * (1 / length) || z >= zSize * (1 / length) || x < 0 || z < 0)
        {
            print("Bad location " + location);
        }
        return nodes[Mathf.FloorToInt(x), Mathf.FloorToInt(z)];
    }

    private float getClosestFloat(float f)
    {
        if (f % 1 == 0)
        {
            return f;
        }
        else
        {
            float t = f % 1f;
            if (t >= .75f)
            {
                return (float)Math.Truncate(f) + 1;
            }
            else if (t >= .25f)
            {
                return (float)Math.Truncate(f) + .5f;
            }
            else
            {
                return (float)Math.Truncate(f);
            }
        }
    }

    //Review, replace try catch
    public static List<Node> getNeighbors(Node n)
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

    private void printNodes(List<Node> p)
    {
        foreach (Node n in nodes)
        {
            GameObject g = Instantiate(nodeMarker);
            g.transform.localPosition = n.position;
            g.transform.localScale = Vector3.one * length;
        }
    }

    public void sendMoveLocations(List<Unit> units)
    {
        MinHeap<Unit> sorted = new MinHeap<Unit>();
        sorted.addItems(units);
        for (int i = 0; i < sorted.size; i++)
        {
            Debug.Log(sorted.getFront().size);
        }
    }

    private void testPathA1(Node start, Node goal, int code)
    {
        int size = 2;
        int count = 0;
        bool pathSuccess = false;

        foreach (Node n in Map.nodes)
        {
            n.revert();
            if (n.isOccupied && n.occCode != code)
            {
                alteredNodes.Add(n);
                n.walkable = false;
                count++;
            }
        }
        Debug.Log("Count: " + count);

        Debug.Log("Unit size: " + size * (1 / Map.length));
        if (start == null || goal == null)
        {
            Debug.Log("Start or Goal are null, pathfinding cant be completed");
            return;
        }

        if (!start.walkable || !goal.walkable)
        {
            Debug.Log("Start or Goal are not walkable, pathfinding cant be completed");
            return;
        }

        MinHeap<Node> openSet = new MinHeap<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.addItem(start);
        long viewed = 0;
        while (openSet.size > 0)
        {
            Node currentNode = openSet.getFront();
            closedSet.Add(currentNode);
            viewed++;
            if (currentNode == goal)
            {
                //Add callback
                pathSuccess = true;
                break;
            }

            if (size > getDist(currentNode) && (currentNode.occCode == -1 || currentNode.occCode == code))
            {
                Debug.Log("Ignoring start node");
                continue;
            }

            foreach (Node n in Map.getNeighbors(currentNode))
            {
                if (!n.walkable || closedSet.Contains(n))
                {
                    continue;
                }

                if (size * (1 / Map.length) > getDist(n))
                {
                    Debug.Log("Ignoring node");
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, n) + n.moveCost;
                if (newMovementCostToNeighbour <= n.gCost || !openSet.Contains(n))
                {
                    n.gCost = newMovementCostToNeighbour;
                    n.hCost = GetDistance(n, goal);
                    n.parent = currentNode;

                    if (!openSet.Contains(n))
                        openSet.addItem(n);
                    else
                        openSet.UpdateItem(n);
                }
            }
        }
        if (pathSuccess)
        {
            path = new List<Node>();
            Node currentNode = goal;

            while (currentNode != start)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            path.Reverse();
            //simplifyPath();
            //path = simplified;
            //buildVPath();
            foreach (Node n in path)
            {
                GameObject g = Instantiate(nodeMarker2);
                g.transform.position = n.position;
                g.transform.localScale = Vector3.one * length;
            }
        }
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.xIndex - nodeB.xIndex);
        int dstZ = Mathf.Abs(nodeA.zIndex - nodeB.zIndex);

        if (dstX > dstZ)
            return (14 * dstZ) + (10 * (dstX - dstZ));
        return (14 * dstX) + (10 * (dstZ - dstX));
    }

    float getDist(Node n)
    {
        int num = 1;
        int x = n.xIndex;
        int z = n.zIndex;
        float length = Map.length;
        int xSize = Map.instance.xSize;
        int zSize = Map.instance.zSize;

        while (true)
        {
            for (int i = 0; i < (num * 2) + 1; i++)
            {
                int x2 = x - num + i;
                int z2 = z - num;
                if (x2 >= 0 && x2 < xSize * (1 / length) && z2 >= 0 && z2 < zSize * (1 / length))
                {
                    if (!Map.nodes[x2, z2].walkable)
                    {
                        return Vector3.Distance(n.position, Map.nodes[x2, z2].position);
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
                    return Vector3.Distance(n.position, Map.nodes[x2, z2].position);
                }
            }

            for (int i = 0; i < (num * 2) - 1; i++)
            {
                int x2 = x + num;
                int z2 = z - num + i;
                if (x2 >= 0 && x2 < xSize * (1 / length) && z2 >= 0 && z2 < zSize * (1 / length))
                {
                    if (!Map.nodes[x2, z2].walkable)
                    {
                        return Vector3.Distance(n.position, Map.nodes[x2, z2].position);
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
                    return Vector3.Distance(n.position, Map.nodes[x2, z2].position);
                }
            }

            for (int i = 0; i < (num * 2) - 1; i++)
            {
                int x2 = x + num - i;
                int z2 = z + num;
                if (x2 >= 0 && x2 < xSize * (1 / length) && z2 >= 0 && z2 < zSize * (1 / length))
                {
                    if (!Map.nodes[x2, z2].walkable)
                    {
                        return Vector3.Distance(n.position, Map.nodes[x2, z2].position);
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
                    return Vector3.Distance(n.position, Map.nodes[x2, z2].position);
                }
            }

            for (int i = 0; i < (num * 2) - 1; i++)
            {
                int x2 = x - num;
                int z2 = z + num - i;
                if (x2 >= 0 && x2 < xSize * (1 / length) && z2 >= 0 && z2 < zSize * (1 / length))
                {
                    if (!Map.nodes[x2, z2].walkable)
                    {
                        return Vector3.Distance(n.position, Map.nodes[x2, z2].position);
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
                    return Vector3.Distance(n.position, Map.nodes[x2, z2].position);
                }
            }
            num++;
        }
    }
}

public struct PathRequest
{
    public Node start;
    public Node end;
    public Unit requestee;
    public bool occupied;
    public int size;
    //Defualt to 0
    public int priority;
    public int specialCode;
}

