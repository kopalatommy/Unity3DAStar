using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class Unit : MonoBehaviour, IComparable<Unit>
{
    public Transform target;
    public int size;

    private readonly List<GameObject> testing = new List<GameObject>();

    //ForMovement
    public int moveSpeed = 10;

    //For object avoidence
    public int occCode = 0;
    public bool inWay = false;
    public bool moving = false;

    //For user interaction
    public bool selected = false;
    public GameObject selectionMarker = null;

    //Health bar
    public HealthBar health = null;
    public float currentHealth = 100;
    public float maxHealth = 100;


    //For debugging
    public Vector3 moveLoc;
    public Vector3 lastPos;

    public bool testScan = false;
    //1 = 1 sec
    public float timeBetweenScans = .5f;
    public int timeBetweenAttacks = 2000;
    public int maxScoutRange = 2;
    public int maxFireRange = 1;
    public float attackDamage = 10;

    //Identifiers
    public int playerCode = 0;
    public int teamCode = 0;

    //Action
    Action testAction = null;
    //Action<List<Vector3>> receivePath = null;

    public bool testA2Path = false;

    public Vector3 position;

    Thread combatThread;

    private void Start()
    {
        position = transform.position;

        selectionMarker.SetActive(false);
        StartCoroutine(FollowPath8());
        StartCoroutine(CombatHanlder());
        //UnitSelection.selection.playerUnits.Add(this);
        occCode = UnitManager.manager.getOccCode(this);

        testAction = ActionTest;
        //requestPath(target.position);
    }

    public bool test = false;
    private void Update()
    {
        position = transform.position;
        if (test)
        {
            test = false;
            //print("Nodes == null: " + currentNodes == null);
            Node[,] nodes = GetNodesFromLocationV2(transform.position);
            foreach (Node n in nodes)
            {
                GameObject g = Instantiate(MapManager.instance.nodeMarker1);
                g.transform.position = n.Position;
                g.transform.localScale = Vector3.one * MapManager.length;
            }
            print(transform.position + ", " + GetAvgPosition(GetNodesFromLocationV2(transform.position)));
        }
    }

    public void ActionTest()
    {
        print("Action worked");
    }

    public void RequestPath(Vector3 target, int priority, int sCode)
    {
        moveLoc = target;
        //Map.instance.requestPath(Map.instance.getNodeFromLocation(transform.position), Map.instance.getNodeFromLocation(target), this, priority, sCode);
        MapManager.instance.AddGroupPathRequest(target, new List<Unit>() { this }, sCode);
    }
    public void RequestPath(Vector3 target, int priority, int sCode, int occCode)
    {
        moveLoc = target;
        //Map.instance.requestPath(Map.instance.getNodeFromLocation(transform.position), Map.instance.getNodeFromLocation(target), this, priority, sCode);
        MapManager.instance.AddGroupPathRequest(target, new List<Unit>() { this }, sCode, occCode);
    }

    /*public void RequestPath(Vector3 target, int priority, int sCode, Node start)
    {
        moveLoc = target;
        if (!NodesAreOK(GetNodesFromLocationV2(start.Position)))
        {
            print("Pathfinding will fail");
        }
        MapManager.instance.RequestPath(start, MapManager.instance.GetNodeFromLocation(target), this, priority, sCode);
    }*/

    public PathRequest MakePathRequest(Vector3 target, int priority, int sCode)
    {
        PathRequest r = new PathRequest()
        {
            //start = MapManager.instance.GetNodeFromLocation(transform.position),
            start = MapManager.instance.GetNodeFromLocation(position),
            end = MapManager.instance.GetNodeFromLocation(target),
            requestee = this,
            priority = priority,
            specialCode = sCode,
            occCode = this.occCode
        };
        /*r.start = Map.instance.getNodeFromLocation(transform.position);
        r.end = Map.instance.getNodeFromLocation(target);
        r.requestee = this;
        r.size = size;
        r.priority = priority;
        r.specialCode = sCode;*/
        return r;
    }

    public void GetPath(List<Vector3> p)
    {
        foreach (GameObject g in testing)
        {
            Destroy(g);
        }
        /*foreach (Vector3 n in p)
        {
            GameObject g = Instantiate(Map.instance.nodeMarker);
            g.transform.position = n;
            g.transform.localScale = Vector3.one * Map.length;
            testing.Add(g);
        }*/
        lastPos = position;
        nvPath = p;
    }

    List<Vector3> nvPath = new List<Vector3>();
    public Node[,] currentNodes = null;
    public int moveCode = -1;
    //Requests a new path where occupied nodes are unwalkable
    public bool testA1 = false;
    IEnumerator FollowPath8()
    {
        List<Vector3> vPath = new List<Vector3>();
        int pathIndex = 0;

        Renderer mRenderer = gameObject.GetComponent<Renderer>();

        while (MapManager.instance == null || !MapManager.instance.mapIsReady) yield return null;
        vPath.Add(GetAvgPosition(GetNodesFromLocationV2(transform.position)));

        currentNodes = GetNodesFromLocationV2(transform.position);

        while (true)
        {
            if (nvPath != null)
            {
                //print("Received new path");
                //Change color here
                if (nvPath.Count > 0)
                {
                    vPath = nvPath;
                    pathIndex = 0;
                    Node[,] temp = GetNodesFromLocationV2(vPath[pathIndex]);
                    nvPath = null;

                    while (!NodesAreOK(temp))
                    {
                        yield return null;
                        moving = false;
                        if (UnitManager.manager.getUnitFromUnitCodes(GetUnitInWay(temp)).moving)
                        {
                            mRenderer.material.color = Color.magenta;
                        }
                        else
                        {
                            mRenderer.material.color = Color.cyan;
                            PathRequest req = new PathRequest()
                            {
                                start = MapManager.instance.GetNodeFromLocation(transform.position),
                                end = MapManager.instance.GetNodeFromLocation(vPath[vPath.Count - 1]),
                                requestee = this,
                                priority = 10,
                                specialCode = 1,
                                occCode = occCode
                            };
                            MapManager.instance.AddPathRequest(req);
                            //RequestPath(vPath[vPath.Count - 1], 10, 1);
                            /*GameObject o = Instantiate(MapManager.instance.nodeMarker2);
                            o.transform.position = vPath[vPath.Count - 1];
                            o.transform.localScale = MapManager.vLength;*/
                            Debug.Log("Request new path: " + vPath[vPath.Count -1]);
                            while (nvPath == null) yield return null;
                            Debug.Log("Received new path");
                            if (testA1)
                            {
                                GameObject o = Instantiate(MapManager.instance.nodeMarker2);
                                o.transform.position = vPath[vPath.Count - 1];
                                o.transform.localScale = MapManager.vLength;
                                o.transform.name = "GOAL";
                                testA1 = false;
                                foreach (Vector3 v in nvPath)
                                {
                                    if (NodesAreOK(GetNodesFromLocationV2(v)))
                                    {
                                        GameObject g = Instantiate(MapManager.instance.nodeMarker3);
                                        g.transform.position = v;
                                        g.transform.localScale = MapManager.vLength;
                                    }
                                    else
                                    {
                                        GameObject g = Instantiate(MapManager.instance.nodeMarker2);
                                        g.transform.position = v;
                                        g.transform.localScale = MapManager.vLength;
                                    }
                                }
                            }
                            //bool nodesok = true;
                            //foreach (Vector3 v in nvPath) if (!NodesAreOK(GetNodesFromLocationV2(v))) nodesok = false;
                            //print("All nodes are ok: " + nodesok);

                            /*print("Received new path 1");
                            print("New path nodes are ok: " + NodesAreOK(GetNodesFromLocationV2(nvPath[0])));
                            foreach (Vector3 v in nvPath)
                            {
                                GameObject g = Instantiate(MapManager.instance.nodeMarker1);
                                g.transform.position = v;
                                g.transform.localScale = MapManager.vLength;
                            }
                            while (true) yield return null;*/
                            break;
                        }
                    }
                    mRenderer.material.color = Color.green;
                    ResetCurrentNodes(currentNodes);
                    currentNodes = temp;
                    FillCurrentNodes(currentNodes);
                    moving = true;
                    yield return null;
                }
                else
                {
                    //print("Received empty path");
                    nvPath = null;
                }
            }
            else if (transform.position != vPath[pathIndex])
            {
                transform.position = Vector3.MoveTowards(transform.position, vPath[pathIndex], moveSpeed * Time.deltaTime);
                moving = true;
                yield return null;
            }
            else if ((pathIndex + 1) < vPath.Count)
            {
                Node[,] nextNodes = GetNodesFromLocationV2(vPath[pathIndex + 1]);
                while (!NodesAreOK(nextNodes))
                {
                    yield return null;
                    moving = false;
                    Unit u = UnitManager.manager.getUnitFromUnitCodes(GetUnitInWay(nextNodes));
                    //print("U == null: " + u == null);
                    if (u == null)
                    {
                        mRenderer.material.color = Color.black;
                    }
                    else if (u.moving)
                    {
                        mRenderer.material.color = Color.magenta;
                    }
                    else if (nvPath != null)
                    {
                        break;
                    }
                    else
                    {
                        mRenderer.material.color = Color.yellow;
                        //RequestPath(vPath[vPath.Count - 1], 10, 1);
                        MapManager.instance.AddPathPatchRequest(vPath[vPath.Count - 1], this, 1);
                        /*GameObject o = Instantiate(MapManager.instance.nodeMarker2);
                        o.transform.position = vPath[vPath.Count - 1];
                        o.transform.localScale = MapManager.vLength;*/
                        Debug.Log("Requested new path");
                        while (nvPath == null) yield return null;
                        Debug.Log("Received new path");
                        if (testA1)
                        {
                            GameObject o = Instantiate(MapManager.instance.nodeMarker1);
                            o.transform.position = vPath[vPath.Count - 1];
                            o.transform.localScale = MapManager.vLength;
                            o.transform.name = "GOAL";
                            testA1 = false;
                            foreach (Vector3 v in nvPath)
                            {
                                if (NodesAreOK(GetNodesFromLocationV2(v)))
                                {
                                    GameObject g = Instantiate(MapManager.instance.nodeMarker3);
                                    g.transform.position = v;
                                    g.transform.localScale = MapManager.vLength;
                                }
                                else
                                {
                                    GameObject g = Instantiate(MapManager.instance.nodeMarker2);
                                    g.transform.position = v;
                                    g.transform.localScale = MapManager.vLength;
                                }
                            }
                        }
                        /*bool temp = true;
                        foreach (Vector3 v in nvPath) if (!NodesAreOK(GetNodesFromLocationV2(v))) temp = false;
                        print("All nodes are ok: " + temp);*/

                        /*print("Received new path 1");
                        print("New path nodes are ok: " + NodesAreOK(GetNodesFromLocationV2(nvPath[0])));
                        foreach (Vector3 v in nvPath)
                        {
                            GameObject g = Instantiate(MapManager.instance.nodeMarker1);
                            g.transform.position = v;
                            g.transform.localScale = MapManager.vLength;
                        }
                        while (true) yield return null;*/
                        break;
                    }
                }
                mRenderer.material.color = Color.green;
                pathIndex += 1;

                ResetCurrentNodes(currentNodes);

                if (FillCurrentNodes(nextNodes))
                {
                    currentNodes = nextNodes;
                }
                else
                {
                    FillCurrentNodes(currentNodes);
                    pathIndex -= 1;
                }
            }
            else
            {
                mRenderer.material.color = Color.blue;
                moving = false;
                yield return null;
            }
        }
    }

    IEnumerator FollowPath7()
    {
        List<Vector3> vPath = new List<Vector3>();
        int index = 0;
        currentNodes = GetNodesFromLocationV2(transform.position);
        while (MapManager.instance == null || !MapManager.instance.mapIsReady)
        {
            yield return null;
        }

        Node next = MapManager.instance.GetNodeFromLocation(transform.position);
        //vPath.Add(transform.position);
        vPath.Add(GetAvgPosition(GetNodesFromLocationV2(transform.position)));

        while (true)
        {
            if (nvPath != null)
            {
                if (nvPath.Count > 0 && nvPath[0] != null)
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                    if (NodesAreOK(GetNodesFromLocationV2(nvPath[0])))
                    {
                        gameObject.GetComponent<Renderer>().material.color = Color.white;
                        vPath = nvPath;
                        index = 0;
                        ResetCurrentNodes(currentNodes);
                        currentNodes = GetNodesFromLocationV2(vPath[index]);
                        FillCurrentNodes(currentNodes);
                        moving = true;
                        nvPath = null;
                    }
                    else
                    {
                        gameObject.GetComponent<Renderer>().material.color = Color.red;
                        print("Next nodes are not ok");
                        /*foreach (Vector3 v in nvPath)
                        {
                            GameObject g = Instantiate(Map.instance.nodeMarker);
                            g.transform.position = v;
                            g.transform.localScale = Vector3.one * Map.length;
                        }
                        nvPath = null;
                        print("A");*/
                        RequestPath(vPath[vPath.Count - 1], 10, 1);
                        while (nvPath == null) yield return null;
                        print("Got new path");
                    }
                    yield return null;
                    continue;
                }
                else
                {
                    nvPath = null;
                    yield return null;
                    continue;
                }
            }

            if (transform.position != vPath[index])
            {
                transform.position = Vector3.MoveTowards(transform.position, vPath[index], moveSpeed * Time.deltaTime);
                moving = true;
            }
            else if ((index + 1) < vPath.Count)
            {
                Node[,] nextNodes = GetNodesFromLocationV2(vPath[index + 1]);
                moving = true;
                index += 1;//sets it to next index
                if (!NodesAreOK(nextNodes) && nvPath == null)
                {
                    int offending = GetUnitInWay(nextNodes);
                    //print(offending);
                    if (offending != -1)
                    {
                        gameObject.GetComponent<Renderer>().material.color = Color.cyan;

                        Unit offender = UnitManager.manager.getUnitFromUnitCodes(offending);

                        if (!offender.moving)
                        {
                            /*print("Next nodes are occupied");
                            print("Position = walkable: " + nodesAreOK(getNodesFromLocation(transform.position)));*/
                            if (!NodesAreOK(GetNodesFromLocationV2(MapManager.instance.GetNodeFromLocation(transform.position).Position)))
                            {
                                print("Pathfinding is not destined to succeed");
                            }
                            RequestPath(vPath[vPath.Count - 1], 10, 1);
                            while (nvPath == null) yield return null;
                            //if (nvPath == null) yield return new WaitForSeconds(500000);
                            //continue;
                        }
                        else
                        {
                            //Pause Unit
                            print("Hit moving unit");
                            moving = false;
                            while (!NodesAreOK(nextNodes) && nvPath == null && !offender.moving)
                            {
                                yield return null;
                            }
                            /*if (!nodesAreOK(nextNodes))
                            {
                                index--;
                                yield return null;
                                continue;
                            }
                            moving = true;*/
                            //continue;
                        }
                    }

                    index -= 1;//reverts it back to current index
                    if (index - 1 > 0 && NodesAreOK(GetNodesFromLocationV2(vPath[index - 1])) && nvPath == null)
                    {
                        //index -= 1;//sets it to previous index
                        gameObject.GetComponent<Renderer>().material.color = Color.magenta;
                    }
                    else
                    {
                        gameObject.GetComponent<Renderer>().material.color = (index - 1 < 0) ? Color.cyan : Color.blue;
                    }
                    yield return null;
                    continue;
                }
                else
                {
                    if (nvPath != null)
                    {
                        yield return null;
                        continue;
                    }
                    //if (!nodesAreOK(getNodesFromLocationV2(vPath[index])))
                    if(!NodesAreOK(nextNodes))
                    {
                        print("Failed to wait");
                        index--;
                        continue;
                    }
                    else
                    {
                        ResetCurrentNodes(currentNodes);
                        //if (fillCurrentNodes(getNodesFromLocationV2(vPath[index])))
                        if(FillCurrentNodes(nextNodes))
                        {
                            gameObject.GetComponent<Renderer>().material.color = Color.black;
                            //currentNodes = getNodesFromLocationV2(vPath[index]);
                            currentNodes = nextNodes;
                        }
                        else
                        {
                            gameObject.GetComponent<Renderer>().material.color = Color.red;
                            FillCurrentNodes(currentNodes);
                            index -= 1;
                        }
                    }
                }

            }
            else
            {
                gameObject.GetComponent<Renderer>().material.color = Color.grey;
                moving = false;
            }
            yield return null;
        }
    }

    int GetUnitInWay(Node[,] nodes)
    {
        foreach (Node n in nodes)
        {
            if (n.GetOccCode() != occCode && n.GetOccCode() != -1)
            {
                return n.GetOccCode();
            }
        }
        return -1;
    }

    bool NodesAreOK(Node[,] nodes)
    {
        foreach (Node n in nodes)
        {
            /*if(n == null)
            {
                badNode = true;
                return false;
            }*/
            if (n == null)
            {
                return false;
            }
            if (n.GetOccCode() != -1 && n.GetOccCode() != occCode)
            {
                return false;
            }
            if (!n.walkable)
            {
                return false;
            }
        }
        return true;
    }

    bool FillCurrentNodes(Node[,] nodes)
    {
        if (!NodesAreOK(nodes))
        {
            //print("Fill current failed");
            return false;
        }
        foreach (Node n in nodes)
        {
            if (n == null || (n.GetOccCode() != occCode && n.GetOccCode() != -1)) // potential issue with check; if bad get from NodesAreOk()
            {
                return false;
            }
        }
        foreach (Node n in nodes)
        {
            if (n != null)
            {
                //n.occCode = occCode;
                n.SetOccCode(this);
            }
        }
        return true;
    }

    void ResetCurrentNodes(Node[,] nodes)
    {
        foreach (Node n in nodes)
        {
            if (n != null && n.GetOccCode() == occCode)
            {
                n.SetOccCode(-1);
            }
        }
    }

    /*Node[,] GetNodesFromLocation(Vector3 pos , int i)
    {
        float l = MapManager.length;

        Node[,] nodes = new Node[size * 2, size * 2];

        float x = pos.x - size + l;
        float z = pos.z - size + l;

        for (int q = 0; q < size * 2; q++)
        {
            for (int w = 0; w < size * 2; w++)
            {
                if (nodes[q, w] == null)
                {
                    Vector3 nPos = Vector3.zero;
                    nPos.y = pos.y;
                    nPos.x = x + (l * q);
                    nPos.z = z + (w * l);
                    nodes[q, w] = MapManager.instance.GetNodeFromLocation(nPos);
                }
            }
        }
        return nodes;
    }*/

    Node[,] GetNodesFromLocationV2(Vector3 pos)
    {
        float l = MapManager.length;

        Node[,] nodes = new Node[size * 2, size * 2];

        float x = pos.x - (size / 2);
        float z = pos.z - (size / 2);

        for (int q = 0; q < size * 2; q++)
        {
            for (int w = 0; w < size * 2; w++)
            {
                if (nodes[q, w] == null)
                {
                    Vector3 nPos = Vector3.zero;
                    nPos.y = pos.y;
                    nPos.x = x + (l * q);
                    nPos.z = z + (w * l);
                    nodes[q, w] = MapManager.instance.GetNodeFromLocation(nPos);
                }
            }
        }
        return nodes;
    }

    public int CompareTo(Unit u)
    {
        return (size == u.size) ? 1 : (size > u.size) ? 1 : -1;
    }

    public void SetMarker(bool b)
    {
        if (selectionMarker.activeSelf != b)
        {
            //print("Setting marker to " + b);
            selected = b;
            selectionMarker.SetActive(b);
        }
    }

    Vector3 GetAvgPosition(Node[,] n)
    {
        float x = ((n[0, 0].Position.x + n[0, n.GetLength(n.Rank - 1) - 1].Position.x) / 2) + MapManager.length / 2;
        float z = ((n[0, 0].Position.z + n[n.GetLength(n.Rank - 1) - 1, 0].Position.z) / 2) + MapManager.length / 2;
        return new Vector3(x - 0.01f, n[0, 0].Position.y, z - 0.01f);
    }

    Unit SearchForTarget()
    {
        //Nodes to search
        List<Node> openSet = new List<Node>();
        List<Node> closedSet = new List<Node>();

        //Debug.Log("Starting search");

        while (currentNodes == null) ;
        Debug.Log("Starting search");

        Node start = currentNodes[0, 0];

        openSet.Add(start);
        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            openSet.RemoveAt(0);
            closedSet.Add(current);

            if (current.GetOccCode() != -1 && UnitManager.manager.getUnitFromUnitCodes(current.GetOccCode()).teamCode != teamCode)
            {
                /*result = UnitManager.manager.getUnitFromUnitCodes(current.GetOccCode());
                status = PathStatus.succeeded;*/
                Unit u = UnitManager.manager.getUnitFromUnitCodes(current.GetOccCode());
                Debug.Log(name + " is targeting " + u.occCode);
                return u;
            }

            foreach (Node n in MapManager.instance.GetNeighbors(current))
            {
                if (n != null && !closedSet.Contains(n) && Vector3.Distance(start.Position, n.Position) <= maxFireRange)
                {
                    if (!openSet.Contains(n))
                    {
                        openSet.Add(n);
                    }
                }
            }
        }
        return null;
    }

    void HandleCombat()
    {
        Unit target = null;
        while (true)
        {
            if (target)
            {
                target.TakeDamage(attackDamage);
                Thread.Sleep(timeBetweenAttacks);
            }
            else
            {
                target = SearchForTarget();
            }
        }
    }

    IEnumerator CombatHanlder()
    {
        UnitSearch search = null;
        while (true)
        {
            if (search == null)
            {
                search = new UnitSearch(maxScoutRange, teamCode, MapManager.instance.GetNodeFromLocation(transform.position));
                while (search.status == UnitSearch.PathStatus.inProcess) yield return null ;
                if (search.status == UnitSearch.PathStatus.succeeded)
                {
                    if (search.result == null)
                    {
                        print("Search result is null");
                        continue;
                    }
                    search.result.TakeDamage(attackDamage);
                }
                search = null;
            }
            yield return new WaitForSeconds(timeBetweenScans);
        }
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        health.changeHealth(currentHealth / maxHealth);
        if (currentHealth <= 0)
        {
            print("Destroying unit");
            Destroy(gameObject);
        }
    }

    public void OnDestroy()
    {
        ResetCurrentNodes(currentNodes);
        //UnitManager.manager.allUnits.Remove(this);
        //UnitSelection.selection.playerUnits.Remove(this);
    }
}
