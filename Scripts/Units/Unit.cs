using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    private void Start()
    {
        selectionMarker.SetActive(false);
        StartCoroutine(FollowPath7());
        StartCoroutine(CombatHanlder());
        //UnitSelection.selection.playerUnits.Add(this);
        occCode = UnitManager.manager.getOccCode(this);

        testAction = ActionTest;
        //requestPath(target.position);
    }

    public bool test = false;
    private void Update()
    {
        if (test)
        {
            test = false;
            //print("Nodes == null: " + currentNodes == null);
            Node[,] nodes = GetNodesFromLocationV2(transform.position);
            foreach (Node n in nodes)
            {
                GameObject g = Instantiate(Map.instance.nodeMarker);
                g.transform.position = n.Position;
                g.transform.localScale = Vector3.one * Map.length;
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
        Map.instance.AddGroupPathRequest(target, new List<Unit>() { this });
    }

    public void RequestPath(Vector3 target, int priority, int sCode, Node start)
    {
        moveLoc = target;
        if (!NodesAreOK(GetNodesFromLocationV2(start.Position)))
        {
            print("Pathfinding will fail");
        }
        Map.instance.RequestPath(start, Map.instance.GetNodeFromLocation(target), this, priority, sCode);
    }

    public PathRequest MakePathRequest(Vector3 target, int priority, int sCode)
    {
        PathRequest r = new PathRequest()
        {
            start = Map.instance.GetNodeFromLocation(transform.position),
            end = Map.instance.GetNodeFromLocation(target),
            requestee = this,
            size = size,
            priority = priority,
            specialCode = sCode
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
        lastPos = transform.position;
        nvPath = p;
    }

    List<Vector3> nvPath = new List<Vector3>();
    public Node[,] currentNodes = null;
    public int moveCode = -1;
    //Requests a new path where occupied nodes are unwalkable
    IEnumerator FollowPath7()
    {
        List<Vector3> vPath = new List<Vector3>();
        int index = 0;
        currentNodes = GetNodesFromLocationV2(transform.position);
        while (Map.instance == null || !Map.instance.mapIsReady)
        {
            yield return null;
        }

        Node next = Map.instance.GetNodeFromLocation(transform.position);
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
                            if (!NodesAreOK(GetNodesFromLocationV2(Map.instance.GetNodeFromLocation(transform.position).Position)))
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
            if (n.occCode != occCode && n.occCode != -1)
            {
                return n.occCode;
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
            if (n.occCode != -1 && n.occCode != occCode)
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
            print("Fill current failed");
            return false;
        }
        foreach (Node n in nodes)
        {
            if (n == null || (n.occCode != occCode && n.occCode != -1)) // potential issue with check; if bad get from nodesareok
            {
                return false;
            }
        }
        foreach (Node n in nodes)
        {
            if (n != null)
            {
                n.occCode = occCode;
            }
        }
        return true;
    }

    void ResetCurrentNodes(Node[,] nodes)
    {
        foreach (Node n in nodes)
        {
            if (n != null && n.occCode == occCode)
            {
                n.occCode = -1;
            }
        }
    }

    Node[,] GetNodesFromLocation(Vector3 pos , int i)
    {
        float l = Map.length;

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
                    nodes[q, w] = Map.instance.GetNodeFromLocation(nPos);
                }
            }
        }
        return nodes;
    }

    Node[,] GetNodesFromLocationV2(Vector3 pos)
    {
        float l = Map.length;

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
                    nodes[q, w] = Map.instance.GetNodeFromLocation(nPos);
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
        float x = ((n[0, 0].Position.x + n[0, n.GetLength(n.Rank - 1) - 1].Position.x) / 2) + Map.length / 2;
        float z = ((n[0, 0].Position.z + n[n.GetLength(n.Rank - 1) - 1, 0].Position.z) / 2) + Map.length / 2;
        return new Vector3(x - 0.01f, n[0, 0].Position.y, z - 0.01f);
    }

    IEnumerator CombatHanlder()
    {
        UnitSearch search = null;
        while (true)
        {
            if (search == null)
            {
                search = new UnitSearch(maxScoutRange, teamCode, Map.instance.GetNodeFromLocation(transform.position));
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
