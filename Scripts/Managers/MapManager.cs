using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    public string mapName = string.Empty;
    public int xSize = 0;
    public int zSize = 0;
    public static float length = 0.5f;
    public static Node[,] nodes = null;

    public bool mapIsReady = false;

    public GameObject nodeMarker1 = null;
    public GameObject nodeMarker2 = null;
    public GameObject nodeMarker3 = null;

    private List<GroupPathRequest> pathRequests = new List<GroupPathRequest>();

    public List<GameObject> printedNodes = new List<GameObject>();
    public bool showOccupied = false;
    public bool clearOccupied = false;

    public static Vector3 vLength = Vector3.one * length;

    public List<Vector3> printLocations = new List<Vector3>();

    Thread buildPathsThread = null;

    private void Awake()
    {
        
    }

    private void Start()
    {
        StartCoroutine(SetUpMap());

        instance = this;

        buildPathsThread = new Thread(new ThreadStart(BuildPaths));
        buildPathsThread.Start();
    }

    private void Update()
    {
        if (showOccupied)
        {
            showOccupied = false;
            foreach (GameObject g in printedNodes)
            {
                Destroy(g);
            }
            printedNodes = new List<GameObject>();
            foreach (Node n in nodes)
            {
                if (n.GetOccCode() != -1)
                {
                    GameObject g = Instantiate(nodeMarker1);
                    g.transform.position = n.Position;
                    g.transform.localScale = vLength;
                    printedNodes.Add(g);
                }
            }
        }
        if (clearOccupied)
        {
            clearOccupied = false;
            foreach (GameObject g in printedNodes)
            {
                Destroy(g);
            }
            printedNodes = new List<GameObject>();
        }
        if (printLocations.Count > 0)
        {
            foreach (Vector3 v in printLocations)
            {
                GameObject g = Instantiate(nodeMarker1);
                Debug.Log("Position: " + v);
                g.transform.position = v;
                g.transform.localPosition = vLength;
            }
            printLocations.Clear();
        }
    }

    public void PrintNode(Vector3 pos)
    {
        GameObject g = Instantiate(nodeMarker1);
        g.transform.position = pos;
        g.transform.localScale = vLength;
    }

    IEnumerator SetUpMap()
    {
        xSize = 1000;
        zSize = 1000;
        CreateMap create = new CreateMap(true, length, xSize, zSize, mapName);
        while (create.totalNodes == 0)
        {
            yield return null;
        }
        //Debug.Log("Total nodes: " + create.totalNodes);
        CameraUI.instance.progressText.text = "Hello";
        while (!create.mapIsReady)
        {
            CameraUI.instance.progressText.text = create.touchedNodes + "/" + create.totalNodes;
            CameraUI.instance.progressBar.value = create.touchedNodes / create.totalNodes;
            yield return null;
        }
        CameraUI.instance.progressText.text = create.touchedNodes + "/" + create.totalNodes;
        CameraUI.instance.progressBar.value = create.touchedNodes / create.totalNodes;
        nodes = create.nodes;
        mapIsReady = true;
        create = null;

        GameObject g = Instantiate(nodeMarker1);
        g.transform.position = nodes[0, 0].Position;
        g.transform.localScale = vLength;

        g = Instantiate(nodeMarker2);
        g.transform.position = nodes[0, nodes.GetLength(0) - 1].Position;
        g.transform.localScale = vLength;

        g = Instantiate(nodeMarker3);
        g.transform.position = nodes[nodes.GetLength(0) - 1, 0].Position;
        g.transform.localScale = vLength;

        g = Instantiate(nodeMarker2);
        g.transform.position = nodes[nodes.GetLength(0) - 1, nodes.GetLength(nodes.Rank - 1) - 1].Position;
        g.transform.localScale = vLength;
    }

    void BuildPaths()
    {
        while (!mapIsReady) ;
        //print("MapHandler: map is ready");
        while (true)
        {
            if (pathRequests.Count > 0)
            {
                List<List<Vector3>> destinations = new List<List<Vector3>>();
                for (int i = 0; i < pathRequests[0].requests.Count; i++)
                {
                    PathRequest req = pathRequests[0].requests[i];

                    if (req.specialCode == 0)
                    {
                        PathFinderV2 aStar = new PathFinderV2(req);
                        while (aStar.status == 0 && aStar.thread.IsAlive) ;
                        if (!aStar.thread.IsAlive)
                        {
                            print("A star thread died");
                        }
                        else
                        {
                            aStar.thread.Abort();
                        }
                        //Debug.Log("A start exitied with code " + aStar.status);
                        if (aStar.status == 1)
                        {
                            //Debug.Log("A star A1 suceeded");
                            destinations.Add(aStar.vPath);
                        }
                        else if (aStar.status == 2)
                        {
                            //Debug.Log("A star failed");
                            pathRequests[0].requests.Remove(req);
                        }
                        else
                        {
                            //Debug.Log("A star exited with status " + aStar.status);
                            pathRequests[0].requests.Remove(req);
                        }
                        aStar = null;
                    }
                    else if (req.specialCode == 1)
                    {
                        PathFinderA1V2 aStar = new PathFinderA1V2(req);
                        while (aStar.status == 0 && aStar.thread.IsAlive) ;
                        if (!aStar.thread.IsAlive)
                        {
                            print("A star a1 thread died");
                        }
                        else
                        {
                            aStar.thread.Abort();
                        }
                        //Debug.Log("A start exitied with code " + aStar.status);
                        if (aStar.status == 1)
                        {
                            //Debug.Log("A star A2 suceeded");
                            destinations.Add(aStar.vPath);
                        }
                        else if (aStar.status == 2)
                        {
                            //Debug.Log("A star A2 failed");
                            pathRequests[0].requests.Remove(req);
                        }
                        else
                        {
                            //Debug.Log("A star A2 exited with status " + aStar.status);
                            pathRequests[0].requests.Remove(req);
                        }
                        aStar = null;
                    }
                }
                for (int i = 0; i < destinations.Count; i++)
                {
                    pathRequests[0].requests[i].requestee.GetPath(destinations[i]);
                }
                pathRequests.RemoveAt(0);
            }
        }
    }

    public Node GetNodeFromLocation(Vector3 loc)
    {
        float x = RoundFloat(loc.x * (1 / length));
        float z = RoundFloat(loc.z * (1 / length));

        //print("X size: " + xSize);
        //print("Z size: " + zSize);

        if (x >= xSize * (1 / length) || z >= zSize * (1 / length) || x < 0 || z < 0)
        {
            print("Bad location " + loc);
            return null;
        }
        return nodes[Mathf.FloorToInt(x), Mathf.FloorToInt(z)];
    }

    private float RoundFloat(float f)
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

    public List<Node> GetNeighbors(Node n)
    {
        List<Node> neighbors = new List<Node>();
        int xIndex = n.xIndex;
        int zIndex = n.zIndex;

        if (xIndex < (xSize * (1 / length)) && xIndex >= 1 && zIndex < (zSize * (1 / length)) && zIndex >= 1)
        {
            neighbors.Add(nodes[xIndex - 1, zIndex - 1]);
        }
        if (xIndex < (xSize * (1 / length)) && xIndex >= 0 && zIndex < (zSize * (1 / length)) - 1 && zIndex >= 0)
        {
            neighbors.Add(nodes[xIndex, zIndex + 1]);
        }
        if (xIndex < (xSize * (1 / length)) && xIndex >= 0 && zIndex < (zSize * (1 / length)) && zIndex >= 1)
        {
            neighbors.Add(nodes[xIndex, zIndex - 1]);
        }
        if (xIndex < (xSize * (1 / length)) - 1 && xIndex >= 1 && zIndex < (zSize * (1 / length)) && zIndex >= 0)
        {
            neighbors.Add(nodes[xIndex + 1, zIndex]);
        }
        if (xIndex < (xSize * (1 / length)) && xIndex >= 1 && zIndex < (zSize * (1 / length)) && zIndex >= 0)
        {
            neighbors.Add(nodes[xIndex - 1, zIndex]);
        }
        if (xIndex < (xSize * (1 / length)) - 1 && xIndex >= 0 && zIndex < (zSize * (1 / length)) - 1 && zIndex >= 0)
        {
            neighbors.Add(nodes[xIndex + 1, zIndex + 1]);
        }
        if (xIndex < (xSize * (1 / length)) - 1 && xIndex >= 0 && zIndex < (zSize * (1 / length)) && zIndex >= 1)
        {
            neighbors.Add(nodes[xIndex + 1, zIndex - 1]);
        }
        if (xIndex < (xSize * (1 / length)) && xIndex >= 1 && zIndex < (zSize * (1 / length)) - 1 && zIndex >= 0)
        {
            neighbors.Add(nodes[xIndex - 1, zIndex + 1]);
        }
        return neighbors;
    }

    public void AddGroupPathRequest(Vector3 _targetPos, List<Unit> _requestUnits, int moveCode)
    {
        Thread buildGroupPathRequests = new Thread(unused => BuildGroupPathRequest(_targetPos, _requestUnits, moveCode));
        buildGroupPathRequests.Start();
    }
    public void AddGroupPathRequest(Vector3 _targetPos, List<Unit> _requestUnits, int moveCode, int occCode)
    {
        Thread buildGroupPathRequests = new Thread(unused => BuildGroupPathRequest(_targetPos, _requestUnits, moveCode));
        buildGroupPathRequests.Start();
    }
    public void AddPathPatchRequest(Vector3 _targetPos, Unit _requestee, int moveCode)
    {
        GroupPathRequest req = new GroupPathRequest()
        {
            requests = new List<PathRequest>(),
        };
        req.requests.Add(_requestee.MakePathRequest(_targetPos, 10, moveCode));
        //ReceiveBatchPathRequest(req);
        pathRequests.Add(req);
        //print("Finished build group path requests");
    }

    public void AddPathRequest(PathRequest pRequest)
    {
        GroupPathRequest req = new GroupPathRequest()
        {
            requests = new List<PathRequest>() { pRequest },
        };
        pathRequests.Add(req);
    }

    private void BuildGroupPathRequest(Vector3 t, List<Unit> lst, int specialCode)
    {
        PlacementSearch search = new PlacementSearch(lst, GetNodeFromLocation(t));
        while (search.status == 0) ;
        if (search.status == 2)
        {
            print("Search failed");
            return;
        }
        //print("Finished search : " + search.status);
        GroupPathRequest req = new GroupPathRequest()
        {
            requests = new List<PathRequest>(),
        };
        if (search.movePos.Count != lst.Count)
        {
            print("Units: " + lst.Count + ", Locs: " + search.movePos.Count);
        }
        else
        {
            for (int i = 0; i < lst.Count; i++)
            {
                PathRequest request = lst[i].MakePathRequest(search.movePos[i], 0, specialCode);

                //PathRequest r = UnitSelection.selection.playerUnits[i].MakePathRequest(search.movePos[i], 0, 0);
                req.requests.Add(request);
            }
            //ReceiveBatchPathRequest(req);
            pathRequests.Add(req);
            //print("Finished build group path requests");
        }
    }

    private void PrintNodes(List<Node> p)
    {
        foreach (Node n in nodes)
        {
            GameObject g = Instantiate(nodeMarker1);
            g.transform.localPosition = n.Position;
            g.transform.localScale = Vector3.one * length;
        }
    }
}

public struct PathRequest
{
    public Node start;
    public Node end;
    public Unit requestee;
    //Defualt to 0
    public int priority;
    public int specialCode;
    public Action<List<Vector3>> callback;
    public int occCode;
}

public struct GroupPathRequest
{
    public List<PathRequest> requests;
    public Vector3 targetPos;
}
