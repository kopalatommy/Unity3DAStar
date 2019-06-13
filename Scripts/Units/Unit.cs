using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Unit : MonoBehaviour, IComparable<Unit>
{
    public Transform target;
    public int size;

    private List<GameObject> testing = new List<GameObject>();

    //ForMovement
    public int moveSpeed = 10;

    //For object avoidence
    public int occCode = 0;
    public bool inWay = false;
    public bool moving = false;

    //For user interaction
    public bool selected = false;
    public GameObject selectionMarker = null;

    //For debugging
    public Vector3 moveLoc;
    public Vector3 lastPos;

    private void Start()
    {
        StartCoroutine(followPath7());
        UnitSelection.selection.playerUnits.Add(this);
        occCode = UnitManager.manager.getOccCode(this);
        //requestPath(target.position);
    }

    public bool test = false;
    private void Update()
    {
        if (test)
        {
            test = false;
            //print("Nodes == null: " + currentNodes == null);
            Node[,] nodes = getNodesFromLocationV2(transform.position);
            foreach (Node n in nodes)
            {
                GameObject g = Instantiate(Map.instance.nodeMarker);
                g.transform.position = n.position;
                g.transform.localScale = Vector3.one * Map.length;
            }
            print(transform.position + ", " + GetAvgPosition(getNodesFromLocationV2(transform.position)));
        }
    }

    public void requestPath(Vector3 target, int priority, int sCode)
    {
        moveLoc = target;
        Map.instance.requestPath(Map.instance.getNodeFromLocation(transform.position), Map.instance.getNodeFromLocation(target), this, priority, sCode);
    }

    public void getPath(List<Vector3> p)
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

    /*IEnumerator followPath()
    {
        List<Node> path = new List<Node>();
        int index = 0;
        while (Map.instance == null || !Map.instance.mapIsReady)
        {
            yield return null;
        }
        requestPath(target.transform.position, 0, 0);
        currentNode = Map.instance.getNodeFromLocation(transform.position);
        transform.position = currentNode.position;

        newPath.Add(currentNode);
        path.Add(currentNode);
        Node next = path[0];
        while (true)
        {
            if (newPath != null)
            {
                if (newPath.Count > 0 && newPath[0] != null)
                {
                    path = newPath;
                    newPath = null;
                    index = 0;
                    continue;
                }
                else
                {
                    newPath = null;
                    continue;
                }
            }

            if (transform.position != path[index].position)
            {
                transform.position = Vector3.MoveTowards(transform.position, path[index].position, moveSpeed * Time.deltaTime);
            }
            else if ((index + 1) < path.Count)
            {
                index++;
                currentNode = path[index];
                if (newPath != null)
                {
                    yield return null;
                    continue;
                }
            }
            yield return null;
        }
    }*/

    /*IEnumerator followPath2()
    {
        List<Vector3> vPath = new List<Vector3>();
        int index = 0;
        Node[,] currentNodes = getNodesFromLocation(transform.position);
        while (Map.instance == null || !Map.instance.mapIsReady)
        {
            yield return null;
        }

        Node next = Map.instance.getNodeFromLocation(transform.position);
        vPath.Add(transform.position);

        while (true)
        {
            if (nvPath != null)
            {
                if (nvPath.Count > 0 && nvPath[0] != null)
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                    vPath = nvPath;
                    nvPath = null;
                    index = 0;
                    resetCurrentNodes(currentNodes);
                    currentNodes = getNodesFromLocation(vPath[index]);
                    fillCurrentNodes(currentNodes);
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
            }
            else if ((index + 1) < vPath.Count)
            {
                index += 1;//sets it to next index
                if (!nodesAreOK(getNodesFromLocation(vPath[index])) && nvPath == null)
                {
                    int offending = getUnitInWay(getNodesFromLocation(vPath[index]));
                    print(offending);
                    if (offending != -1)
                    {
                        gameObject.GetComponent<Renderer>().material.color = Color.cyan;
                        //print(offending);
                        UnitManager.manager.getUnitFromUnitCodes(offending).inWay = true;
                    }

                    index -= 1;//reverts it back to current index
                    if (index - 1 > 0 && nodesAreOK(getNodesFromLocation(vPath[index - 1])) && nvPath == null)
                    {
                        index -= 1;//sets it to previous index
                        gameObject.GetComponent<Renderer>().material.color = Color.magenta;
                    }
                    else
                    {
                        gameObject.GetComponent<Renderer>().material.color = Color.cyan;
                    }
                    yield return null;
                    continue;
                }

                if (nvPath != null)
                {
                    yield return null;
                    continue;
                }
                resetCurrentNodes(currentNodes);
                if (fillCurrentNodes(getNodesFromLocation(vPath[index])))
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.black;
                    currentNodes = getNodesFromLocation(vPath[index]);
                }
                else
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.red;
                    fillCurrentNodes(currentNodes);
                    index -= 1;
                }
            }
            else
            {
                gameObject.GetComponent<Renderer>().material.color = Color.grey;
                if (inWay)
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.blue;
                }
            }
            yield return null;
        }
    }*/

    //This version adds in the ability to make other units move out of the way
    /*IEnumerator followPath3()
    {
        List<Vector3> vPath = new List<Vector3>();
        int index = 0;
        Node[,] currentNodes = getNodesFromLocation(transform.position);
        while (Map.instance == null || !Map.instance.mapIsReady)
        {
            yield return null;
        }

        Node next = Map.instance.getNodeFromLocation(transform.position);
        vPath.Add(transform.position);

        while (true)
        {
            if (nvPath != null)
            {
                if (nvPath.Count > 0 && nvPath[0] != null)
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                    vPath = nvPath;
                    nvPath = null;
                    index = 0;
                    resetCurrentNodes(currentNodes);
                    currentNodes = getNodesFromLocation(vPath[index]);
                    fillCurrentNodes(currentNodes);
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
            }
            else if ((index + 1) < vPath.Count)
            {
                index += 1;//sets it to next index
                if (!nodesAreOK(getNodesFromLocation(vPath[index])) && nvPath == null)
                {
                    int offending = getUnitInWay(getNodesFromLocation(vPath[index]));
                    print(offending);
                    if (offending != -1)
                    {
                        gameObject.GetComponent<Renderer>().material.color = Color.cyan;
                        //print(offending);
                        Unit offender = UnitManager.manager.getUnitFromUnitCodes(offending);
                        offender.inWay = true;
                        offender.moveCode = sendMoveIndex(vPath[index - 1], vPath[index]);
                    }

                    index -= 1;//reverts it back to current index
                    if (index - 1 > 0 && nodesAreOK(getNodesFromLocation(vPath[index - 1])) && nvPath == null)
                    {
                        index -= 1;//sets it to previous index
                        gameObject.GetComponent<Renderer>().material.color = Color.magenta;
                    }
                    else
                    {
                        gameObject.GetComponent<Renderer>().material.color = (index - 1 < 0) ? Color.cyan : Color.blue;
                    }
                    yield return null;
                    continue;
                }

                if (nvPath != null)
                {
                    yield return null;
                    continue;
                }
                resetCurrentNodes(currentNodes);
                if (fillCurrentNodes(getNodesFromLocation(vPath[index])))
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.black;
                    currentNodes = getNodesFromLocation(vPath[index]);
                }
                else
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.red;
                    fillCurrentNodes(currentNodes);
                    index -= 1;
                }
            }
            else
            {
                gameObject.GetComponent<Renderer>().material.color = Color.grey;
                if (inWay)
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.blue;
                    Vector3 pos = transform.position;
                    print("Move code: " + moveCode);
                    switch (moveCode)
                    {
                        case 0://(+,-)
                            pos.x += size;
                            pos.z -= size;
                            vPath.Insert(index -1, pos);
                            break;
                        case 1://(+,+)
                            pos.x += size;
                            pos.z += size;
                            vPath.Insert(index - 1, transform.position);
                            break;
                        case 2://(+,=)
                            pos.x += size;
                            vPath.Insert(index - 1, transform.position);
                            break;
                        case 3://(-,-)
                            pos.x -= size;
                            pos.z -= size;
                            vPath.Insert(index - 1, transform.position);
                            break;
                        case 4://(-,+)
                            pos.x -= size;
                            pos.z += size;
                            vPath.Insert(index - 1, transform.position);
                            break;
                        case 5://(-,=)
                            pos.x -= size;
                            vPath.Insert(index - 1, transform.position);
                            break;
                        case 6://(=,-)
                            pos.z -= size;
                            vPath.Insert(index - 1, transform.position);
                            break;
                        case 7://(=,+)
                            pos.z += size;
                            vPath.Insert(index - 1, transform.position);
                            break;
                        case 8:
                            vPath.Insert(index - 1, transform.position);
                            break;
                        case 9://Broken
                            print("Move out of way is broken: " + moveCode);
                            break;
                        default:
                            print("Move out of way is broken: " + moveCode);
                            break;
                    }
                    inWay = false;
                    moveCode = -1;
                }
            }
            yield return null;
        }
    }*/

    //This version switches how the object avoidence movement is handles,
    //instead of making the other stopped unit move, the unit that is being 
    //stopped will attempt to move
    //A change would be to handle stopped vs moving offenders
    /*IEnumerator followPath4()
    {
        List<Vector3> vPath = new List<Vector3>();
        int index = 0;
        Node[,] currentNodes = getNodesFromLocation(transform.position);
        while (Map.instance == null || !Map.instance.mapIsReady)
        {
            yield return null;
        }

        Node next = Map.instance.getNodeFromLocation(transform.position);
        vPath.Add(transform.position);

        while (true)
        {
            if (nvPath != null)
            {
                if (nvPath.Count > 0 && nvPath[0] != null)
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                    vPath = nvPath;
                    nvPath = null;
                    index = 0;
                    resetCurrentNodes(currentNodes);
                    currentNodes = getNodesFromLocation(vPath[index]);
                    fillCurrentNodes(currentNodes);
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
            }
            else if ((index + 1) < vPath.Count)
            {
                moving = true;
                index += 1;//sets it to next index
                if (!nodesAreOK(getNodesFromLocation(vPath[index])) && nvPath == null)
                {
                    int offending = getUnitInWay(getNodesFromLocation(vPath[index]));
                    print(offending);
                    if (offending != -1)
                    {
                        gameObject.GetComponent<Renderer>().material.color = Color.cyan;

                        Unit offender = UnitManager.manager.getUnitFromUnitCodes(offending);

                        if (!offender.moving)
                        {
                            Vector3 pos = transform.position;
                            print("Move code: " + sendMoveIndex(vPath[index - 1], vPath[index]));
                            switch (sendMoveIndex(transform.position, vPath[index]))
                            {
                                case 0://(+,-)
                                    pos.x += size;
                                    pos.z -= size;
                                    vPath.Insert(index - 1, pos);
                                    break;
                                case 1://(+,+)
                                    pos.x += size;
                                    pos.z += size;
                                    vPath.Insert(index - 1, transform.position);
                                    break;
                                case 2://(+,=)
                                    pos.x += size;
                                    vPath.Insert(index - 1, transform.position);
                                    break;
                                case 3://(-,-)
                                    pos.x -= size;
                                    pos.z -= size;
                                    vPath.Insert(index - 1, transform.position);
                                    break;
                                case 4://(-,+)
                                    pos.x -= size;
                                    pos.z += size;
                                    vPath.Insert(index - 1, transform.position);
                                    break;
                                case 5://(-,=)
                                    pos.x -= size;
                                    vPath.Insert(index - 1, transform.position);
                                    break;
                                case 6://(=,-)
                                    pos.z -= size;
                                    vPath.Insert(index - 1, transform.position);
                                    break;
                                case 7://(=,+)
                                    pos.z += size;
                                    vPath.Insert(index - 1, transform.position);
                                    break;
                                case 8:
                                    vPath.Insert(index - 1, transform.position);
                                    break;
                                case 9://Broken
                                    print("Move out of way is broken: " + moveCode);
                                    break;
                                default:
                                    print("Move out of way is broken: " + moveCode);
                                    break;
                            }
                        }
                    }

                    index -= 1;//reverts it back to current index
                    if (index - 1 > 0 && nodesAreOK(getNodesFromLocation(vPath[index - 1])) && nvPath == null)
                    {
                        index -= 1;//sets it to previous index
                        gameObject.GetComponent<Renderer>().material.color = Color.magenta;
                    }
                    else
                    {
                        gameObject.GetComponent<Renderer>().material.color = (index - 1 < 0) ? Color.cyan : Color.blue;
                    }
                    yield return null;
                    continue;
                }

                if (nvPath != null)
                {
                    yield return null;
                    continue;
                }
                resetCurrentNodes(currentNodes);
                if (fillCurrentNodes(getNodesFromLocation(vPath[index])))
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.black;
                    currentNodes = getNodesFromLocation(vPath[index]);
                }
                else
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.red;
                    fillCurrentNodes(currentNodes);
                    index -= 1;
                }
            }
            else
            {
                moving = false;
                gameObject.GetComponent<Renderer>().material.color = Color.grey;
            }
            yield return null;
        }
    }*/

    //This one atttempts to move around units by requesting 
    //a new path and setting the nodes of nearby units to null
    /*IEnumerator followPath5()
    {
        List<Vector3> vPath = new List<Vector3>();
        int index = 0;
        Node[,] currentNodes = getNodesFromLocation(transform.position);
        while (Map.instance == null || !Map.instance.mapIsReady)
        {
            yield return null;
        }

        Node next = Map.instance.getNodeFromLocation(transform.position);
        vPath.Add(transform.position);

        while (true)
        {
            if (nvPath != null)
            {
                if (nvPath.Count > 0 && nvPath[0] != null)
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                    if (nodesAreOK(getNodesFromLocation(vPath[index])))
                    {
                        vPath = nvPath;
                        nvPath = null;
                        index = 0;
                        resetCurrentNodes(currentNodes);
                        currentNodes = getNodesFromLocation(vPath[index]);
                        fillCurrentNodes(currentNodes);
                    }
                    else
                    {
                        print("Next nodes are not ok");
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
            }
            else if ((index + 1) < vPath.Count)
            {
                moving = true;
                index += 1;//sets it to next index
                if (!nodesAreOK(getNodesFromLocation(vPath[index])) && nvPath == null)
                {
                    int offending = getUnitInWay(getNodesFromLocation(vPath[index]));
                    //print(offending);
                    if (offending != -1)
                    {
                        gameObject.GetComponent<Renderer>().material.color = Color.cyan;

                        Unit offender = UnitManager.manager.getUnitFromUnitCodes(offending);

                        if (!offender.moving)
                        {
                            int mIndex = sendMoveIndex(transform.position, vPath[index]);
                            requestPath(vPath[vPath.Count-1], 10, 0);

                            Vector3 pos = transform.position;
                            print("Move code: " + sendMoveIndex(vPath[index - 1], vPath[index]));
                            switch (sendMoveIndex(transform.position, vPath[index]))
                            {
                                case 0://(-,-)
                                    pos.x -= size;
                                    vPath.Insert(index - 1, pos);
                                    pos.z -= size;
                                    pos.x -= size;
                                    vPath.Insert(index, pos);
                                    pos.z -= size;
                                    vPath.Insert(index + 1, pos);
                                    vPath.RemoveAt(index + 2);
                                    break;
                                case 1://(-,+)
                                    pos.x -= size;
                                    vPath.Insert(index - 1, pos);
                                    pos.z += size;
                                    vPath.Insert(index, pos);
                                    pos.z += size;
                                    vPath.Insert(index + 1, pos);
                                    vPath.RemoveAt(index + 2);
                                    break;
                                case 2://(-,=)
                                    pos.x -= size;
                                    pos.z += size;
                                    vPath.Insert(index - 1, pos);
                                    pos.x -= size;
                                    pos.z -= size;
                                    vPath.Insert(index, pos);
                                    vPath.RemoveAt(index + 1);
                                    break;
                                case 3://(+,-)
                                    pos.x += size;
                                    vPath.Insert(index - 1, pos);
                                    pos.x += size;
                                    pos.z -= size;
                                    vPath.Insert(index, pos);
                                    pos.z -= size;
                                    vPath.Insert(index + 1, pos);
                                    vPath.RemoveAt(index + 2);
                                    break;
                                case 4://(+,+)
                                    pos.x += size;
                                    vPath.Insert(index - 1, pos);
                                    pos.x += size;
                                    pos.z += size;
                                    vPath.Insert(index, pos);
                                    pos.z += size;
                                    vPath.Insert(index + 1, pos);
                                    vPath.RemoveAt(index + 2);
                                    break;
                                case 5://(+,=)
                                    pos.x += size;
                                    pos.z += size;
                                    vPath.Insert(index - 1, pos);
                                    pos.x += size;
                                    pos.z -= size;
                                    vPath.Insert(index, pos);
                                    vPath.RemoveAt(index + 1);
                                    break;
                                case 6://(=,-)
                                    pos.x += size;
                                    pos.z += size;
                                    vPath.Insert(index - 1, pos);
                                    pos.x += size;
                                    pos.z -= size;
                                    vPath.Insert(index, pos);
                                    vPath.RemoveAt(index + 1);
                                    break;
                                case 7://(=,+)
                                    pos.x -= size;
                                    pos.z += size;
                                    vPath.Insert(index - 1, pos);
                                    pos.x += size;
                                    pos.z -= size;
                                    vPath.Insert(index, pos);
                                    vPath.RemoveAt(index + 1);
                                    break;
                                default:
                                    print("Move out of way is broken: " + moveCode);
                                    break;
                            }
                        }
                        else
                        {
                            //Pause Unit
                            moving = false;
                            while (!nodesAreOK(getNodesFromLocation(vPath[index])) && nvPath == null)
                            {
                                yield return null;
                            }
                            moving = true;
                            continue;
                        }
                    }

                    index -= 1;//reverts it back to current index
                    if (index - 1 > 0 && nodesAreOK(getNodesFromLocation(vPath[index - 1])) && nvPath == null)
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
                    if (!nodesAreOK(getNodesFromLocation(vPath[index])))
                    {
                        print("Failed to wait");
                        continue;
                    }
                    if (nodesAreOK(getNodesFromLocation(vPath[index])))
                    {
                        resetCurrentNodes(currentNodes);
                        if (fillCurrentNodes(getNodesFromLocation(vPath[index])))
                        {
                            gameObject.GetComponent<Renderer>().material.color = Color.black;
                            currentNodes = getNodesFromLocation(vPath[index]);
                        }
                        else
                        {
                            gameObject.GetComponent<Renderer>().material.color = Color.red;
                            fillCurrentNodes(currentNodes);
                            index -= 1;
                        }
                    }
                    else
                    {
                        gameObject.GetComponent<Renderer>().material.color = Color.red;
                        fillCurrentNodes(currentNodes);
                        index -= 1;
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
    }*/

    /*IEnumerator followPath6()
    {
        List<Vector3> vPath = new List<Vector3>();
        int index = 0;
        Node[,] currentNodes = getNodesFromLocation(transform.position);
        while (Map.instance == null || !Map.instance.mapIsReady)
        {
            yield return null;
        }

        Node next = Map.instance.getNodeFromLocation(transform.position);
        vPath.Add(transform.position);

        while (true)
        {
            if (nvPath != null)
            {
                if (nvPath.Count > 0 && nvPath[0] != null)
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                    if (nodesAreOK(getNodesFromLocation(nvPath[0])))
                    {
                        vPath = nvPath;
                        nvPath = null;
                        index = 0;
                        resetCurrentNodes(currentNodes);
                        currentNodes = getNodesFromLocation(vPath[index]);
                        fillCurrentNodes(currentNodes);
                    }
                    else
                    {
                        print("Next nodes are not ok");
                        int mIndex = sendMoveIndex(transform.position, vPath[index]);
                        requestPath(vPath[vPath.Count - 1], 10, 0);

                        Vector3 pos = vPath[index];
                        print("Move code: " + sendMoveIndex(vPath[index], vPath[index + 1]));
                        switch (sendMoveIndex(vPath[index], vPath[index + 1]))
                        {
                            case 0://(-,-)
                                pos.x -= size;
                                vPath.Insert(0, pos);
                                pos.x -= size;
                                vPath.Insert(1, pos);
                                pos.z -= size;
                                vPath.Insert(2, pos);
                                pos.z -= size;
                                vPath.Insert(3, pos);
                                vPath.RemoveAt(4);
                                break;
                            case 1://(-,+)
                                pos.z -= size;
                                vPath.Insert(0, pos);
                                pos.z -= size;
                                vPath.Insert(1, pos);
                                pos.x += size;
                                vPath.Insert(2, pos);
                                pos.x += size;
                                vPath.Insert(3, pos);
                                vPath.RemoveAt(4);
                                break;
                            case 2://(-,=)
                                pos.z += size;
                                vPath.Insert(0, pos);
                                pos.x -= size;
                                vPath.Insert(1, pos);
                                pos.x -= size;
                                vPath.Insert(2, pos);
                                pos.z -= size;
                                vPath.Insert(3, pos);
                                vPath.RemoveAt(4);
                                break;
                            case 3://(+,-)
                                pos.x += size;
                                vPath.Insert(0, pos);
                                pos.x += size;
                                vPath.Insert(1, pos);
                                pos.z -= size;
                                vPath.Insert(2, pos);
                                pos.z -= size;
                                vPath.Insert(3, pos);
                                vPath.RemoveAt(4);
                                break;
                            case 4://(+,+)
                                pos.x += size;
                                vPath.Insert(0, pos);
                                pos.x += size;
                                vPath.Insert(1, pos);
                                pos.z += size;
                                vPath.Insert(2, pos);
                                pos.z += size;
                                vPath.Insert(3, pos);
                                vPath.RemoveAt(4);
                                break;
                            case 5://(+,=)
                                pos.z += size;
                                vPath.Insert(0, pos);
                                pos.x -= size;
                                vPath.Insert(1, pos);
                                pos.z -= size;
                                vPath.Insert(2, pos);
                                pos.z -= size;
                                vPath.Insert(3, pos);
                                vPath.RemoveAt(4);
                                break;
                            case 6://(=,-)
                                pos.x -= size;
                                vPath.Insert(0, pos);
                                pos.z -= size;
                                vPath.Insert(1, pos);
                                pos.z -= size;
                                vPath.Insert(2, pos);
                                pos.x += size;
                                vPath.Insert(3, pos);
                                vPath.RemoveAt(4);
                                break;
                            case 7://(=,+)
                                pos.x -= size;
                                vPath.Insert(0, pos);
                                pos.z += size;
                                vPath.Insert(1, pos);
                                pos.z += size;
                                vPath.Insert(2, pos);
                                pos.x += size;
                                vPath.Insert(3, pos);
                                vPath.RemoveAt(4);
                                break;
                            default:
                                print("Move out of way is broken: " + moveCode);
                                break;
                        }
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
            }
            else if ((index + 1) < vPath.Count)
            {
                Node[,] nextNodes = getNodesFromLocation(vPath[index + 1]);
                moving = true;
                index += 1;//sets it to next index
                if (!nodesAreOK(nextNodes) && nvPath == null)
                {
                    int offending = getUnitInWay(nextNodes);
                    //print(offending);
                    if (offending != -1)
                    {
                        gameObject.GetComponent<Renderer>().material.color = Color.cyan;

                        Unit offender = UnitManager.manager.getUnitFromUnitCodes(offending);

                        if (!offender.moving)
                        {
                            int mIndex = sendMoveIndex(transform.position, vPath[index]);
                            requestPath(vPath[vPath.Count - 1], 10, 0);

                            Vector3 pos = vPath[index - 1];
                            print("Move code: " + sendMoveIndex(vPath[index - 1], vPath[index]));
                            switch (sendMoveIndex(vPath[index - 1], vPath[index]))
                            {
                                case 0://(-,-)
                                    pos.x -= size;
                                    vPath.Insert(index - 1, pos);
                                    pos.x -= size;
                                    vPath.Insert(index, pos);
                                    pos.z -= size;
                                    vPath.Insert(index + 1, pos);
                                    pos.z -= size;
                                    vPath.Insert(index + 2, pos);
                                    vPath.RemoveAt(index + 3);
                                    break;
                                case 1://(-,+)
                                    pos.z -= size;
                                    vPath.Insert(index - 1, pos);
                                    pos.z -= size;
                                    vPath.Insert(index, pos);
                                    pos.x += size;
                                    vPath.Insert(index + 1, pos);
                                    pos.x += size;
                                    vPath.Insert(index + 2, pos);
                                    vPath.RemoveAt(index + 3);
                                    break;
                                case 2://(-,=)
                                    pos.z += size;
                                    vPath.Insert(index - 1, pos);
                                    pos.x -= size;
                                    vPath.Insert(index, pos);
                                    pos.x -= size;
                                    vPath.Insert(index + 1, pos);
                                    pos.z -= size;
                                    vPath.Insert(index + 2, pos);
                                    vPath.RemoveAt(index + 3);
                                    break;
                                case 3://(+,-)
                                    pos.x += size;
                                    vPath.Insert(index - 1, pos);
                                    pos.x += size;
                                    vPath.Insert(index, pos);
                                    pos.z -= size;
                                    vPath.Insert(index + 1, pos);
                                    pos.z -= size;
                                    vPath.Insert(index + 2, pos);
                                    vPath.RemoveAt(4);
                                    break;
                                case 4://(+,+)
                                    pos.x += size;
                                    vPath.Insert(index - 1, pos);
                                    pos.x += size;
                                    vPath.Insert(index, pos);
                                    pos.z += size;
                                    vPath.Insert(index + 1, pos);
                                    pos.z += size;
                                    vPath.Insert(index + 2, pos);
                                    vPath.RemoveAt(index + 3);
                                    break;
                                case 5://(+,=)
                                    pos.z += size;
                                    vPath.Insert(index - 1, pos);
                                    pos.x -= size;
                                    vPath.Insert(index, pos);
                                    pos.z -= size;
                                    vPath.Insert(index + 1, pos);
                                    pos.z -= size;
                                    vPath.Insert(index + 2, pos);
                                    vPath.RemoveAt(index + 3);
                                    break;
                                case 6://(=,-)
                                    pos.x -= size;
                                    vPath.Insert(index - 1, pos);
                                    pos.z -= size;
                                    vPath.Insert(index, pos);
                                    pos.z -= size;
                                    vPath.Insert(index + 1, pos);
                                    pos.x += size;
                                    vPath.Insert(index + 2, pos);
                                    vPath.RemoveAt(index + 3);
                                    break;
                                case 7://(=,+)
                                    pos.x -= size;
                                    vPath.Insert(index - 1, pos);
                                    pos.z += size;
                                    vPath.Insert(index, pos);
                                    pos.z += size;
                                    vPath.Insert(index + 1, pos);
                                    pos.x += size;
                                    vPath.Insert(index + 2, pos);
                                    vPath.RemoveAt(index + 3);
                                    break;
                                default:
                                    print("Move out of way is broken: " + moveCode);
                                    break;
                            }
                            print(vPath.ToString());
                        }
                        else
                        {
                            //Pause Unit
                            moving = false;
                            while (!nodesAreOK(nextNodes) && nvPath == null)
                            {
                                yield return null;
                            }
                            moving = true;
                            continue;
                        }
                    }

                    index -= 1;//reverts it back to current index
                    if (index - 1 > 0 && nodesAreOK(getNodesFromLocation(vPath[index - 1])) && nvPath == null)
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
                    if (!nodesAreOK(getNodesFromLocation(vPath[index])))
                    {
                        print("Failed to wait");
                        continue;
                    }
                    if (nodesAreOK(getNodesFromLocation(vPath[index])))
                    {
                        resetCurrentNodes(currentNodes);
                        if (fillCurrentNodes(getNodesFromLocation(vPath[index])))
                        {
                            gameObject.GetComponent<Renderer>().material.color = Color.black;
                            currentNodes = getNodesFromLocation(vPath[index]);
                        }
                        else
                        {
                            gameObject.GetComponent<Renderer>().material.color = Color.red;
                            fillCurrentNodes(currentNodes);
                            index -= 1;
                        }
                    }
                    else
                    {
                        gameObject.GetComponent<Renderer>().material.color = Color.red;
                        fillCurrentNodes(currentNodes);
                        index -= 1;
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
    }*/

    List<Vector3> nvPath = new List<Vector3>();
    public Node[,] currentNodes = null;
    public int moveCode = -1;
    List<GameObject> markers = new List<GameObject>();
    //Requests a new path where occupied nodes are unwalkable
    IEnumerator followPath7()
    {
        List<Vector3> vPath = new List<Vector3>();
        int index = 0;
        currentNodes = getNodesFromLocationV2(transform.position);
        while (Map.instance == null || !Map.instance.mapIsReady)
        {
            yield return null;
        }

        Node next = Map.instance.getNodeFromLocation(transform.position);
        //vPath.Add(transform.position);
        vPath.Add(GetAvgPosition(getNodesFromLocationV2(transform.position)));

        while (true)
        {
            if (nvPath != null)
            {
                if (nvPath.Count > 0 && nvPath[0] != null)
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                    if (nodesAreOK(getNodesFromLocationV2(nvPath[0])))
                    {
                        vPath = nvPath;
                        nvPath = null;
                        index = 0;
                        resetCurrentNodes(currentNodes);
                        currentNodes = getNodesFromLocationV2(vPath[index]);
                        fillCurrentNodes(currentNodes);
                        moving = true;
                    }
                    else
                    {
                        print("Next nodes are not ok");
                        /*foreach (Vector3 v in nvPath)
                        {
                            GameObject g = Instantiate(Map.instance.nodeMarker);
                            g.transform.position = v;
                            g.transform.localScale = Vector3.one * Map.length;
                        }
                        nvPath = null;
                        print("A");*/
                        while (nvPath == null) yield return null;
                        print("Got new path");
                        requestPath(vPath[vPath.Count - 1], 10, 1);
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

                Node[,] nextNodes = getNodesFromLocationV2(vPath[index + 1]);
                foreach (GameObject g in markers)
                {
                    Destroy(g);
                }
                markers.Clear();
                foreach (Node n in currentNodes)
                {
                    GameObject g = Instantiate(Map.instance.nodeMarker2);
                    g.transform.position = n.position;
                    g.transform.localScale = Map.vLength;
                    markers.Add(g);
                }
                foreach (Node n in nextNodes)
                {
                    GameObject g = Instantiate(Map.instance.nodeMarker);
                    g.transform.position = n.position;
                    g.transform.localScale = Map.vLength;
                    markers.Add(g);
                }


                moving = true;
                index += 1;//sets it to next index
                if (!nodesAreOK(nextNodes) && nvPath == null)
                {
                    int offending = getUnitInWay(nextNodes);
                    //print(offending);
                    if (offending != -1)
                    {
                        gameObject.GetComponent<Renderer>().material.color = Color.cyan;

                        Unit offender = UnitManager.manager.getUnitFromUnitCodes(offending);

                        if (!offender.moving)
                        {
                            /*print("Next nodes are occupied");
                            print("Position = walkable: " + nodesAreOK(getNodesFromLocation(transform.position)));*/
                            requestPath(vPath[vPath.Count - 1], 10, 1);
                            while (nvPath == null) yield return null;
                            if (nvPath == null) yield return new WaitForSeconds(500000);
                            continue;
                        }
                        else
                        {
                            //Pause Unit
                            //print("Hit moving unit");
                            moving = false;
                            while (!nodesAreOK(nextNodes) && nvPath == null && !offender.moving)
                            {
                                yield return null;
                            }
                            if (!nodesAreOK(nextNodes))
                            {
                                index--;
                                yield return null;
                                continue;
                            }
                            moving = true;
                            continue;
                        }
                    }

                    index -= 1;//reverts it back to current index
                    if (index - 1 > 0 && nodesAreOK(getNodesFromLocationV2(vPath[index - 1])) && nvPath == null)
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
                    if(!nodesAreOK(nextNodes))
                    {
                        print("Failed to wait");
                        index--;
                        continue;
                    }
                    else
                    {
                        resetCurrentNodes(currentNodes);
                        //if (fillCurrentNodes(getNodesFromLocationV2(vPath[index])))
                        if(fillCurrentNodes(nextNodes))
                        {
                            gameObject.GetComponent<Renderer>().material.color = Color.black;
                            //currentNodes = getNodesFromLocationV2(vPath[index]);
                            currentNodes = nextNodes;
                        }
                        else
                        {
                            gameObject.GetComponent<Renderer>().material.color = Color.red;
                            fillCurrentNodes(currentNodes);
                            index -= 1;
                        }
                    }
                    /*if (nodesAreOK(getNodesFromLocationV2(vPath[index])))
                    {
                        resetCurrentNodes(currentNodes);
                        if (fillCurrentNodes(getNodesFromLocationV2(vPath[index])))
                        {
                            gameObject.GetComponent<Renderer>().material.color = Color.black;
                            currentNodes = getNodesFromLocationV2(vPath[index]);
                        }
                        else
                        {
                            gameObject.GetComponent<Renderer>().material.color = Color.red;
                            fillCurrentNodes(currentNodes);
                            index -= 1;
                        }
                    }
                    else
                    {
                        gameObject.GetComponent<Renderer>().material.color = Color.red;
                        fillCurrentNodes(currentNodes);
                        index -= 1;
                    }*/
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

    IEnumerator followPath7Debug()
    {
        List<Vector3> vPath = new List<Vector3>();
        int index = 0;
        currentNodes = getNodesFromLocationV2(transform.position);
        while (Map.instance == null || !Map.instance.mapIsReady)
        {
            yield return null;
        }

        Node next = Map.instance.getNodeFromLocation(transform.position);
        vPath.Add(transform.position);

        while (true)
        {
            if (nvPath != null)
            {
                if (nvPath.Count > 0 && nvPath[0] != null)
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                    if (nodesAreOK(getNodesFromLocationV2(nvPath[0])))
                    {
                        vPath = nvPath;
                        nvPath = null;
                        index = 0;
                        resetCurrentNodes(currentNodes);
                        currentNodes = getNodesFromLocationV2(vPath[index]);
                        fillCurrentNodes(currentNodes);
                    }
                    else
                    {
                        print("Next nodes are not ok");
                        foreach (Vector3 v in nvPath)
                        {
                            GameObject g = Instantiate(Map.instance.nodeMarker);
                            g.transform.position = v;
                            g.transform.localScale = Vector3.one * Map.length;
                        }
                        nvPath = null;
                        print("A");
                        requestPath(vPath[vPath.Count - 1], 10, 1);
                        while (nvPath == null) yield return null;
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
                Node[,] nextNodes = getNodesFromLocationV2(vPath[index + 1]);
                moving = true;
                index += 1;//sets it to next index
                if (!nodesAreOK(nextNodes) && nvPath == null)
                {
                    int offending = getUnitInWay(nextNodes);
                    //print(offending);
                    if (offending != -1)
                    {
                        gameObject.GetComponent<Renderer>().material.color = Color.cyan;

                        Unit offender = UnitManager.manager.getUnitFromUnitCodes(offending);

                        if (!offender.moving)
                        {
                            /*print("Next nodes are occupied");
                            print("Position = walkable: " + nodesAreOK(getNodesFromLocation(transform.position)));*/
                            requestPath(vPath[vPath.Count - 1], 10, 1);
                            while (nvPath == null) yield return null;
                            if (nvPath == null) yield return new WaitForSeconds(500000);
                            continue;
                        }
                        else
                        {
                            //Pause Unit
                            //print("Hit moving unit");
                            //moving = false;
                            while (!nodesAreOK(nextNodes) && nvPath == null && !offender.moving)
                            {
                                yield return null;
                            }
                            moving = true;
                            continue;
                        }
                    }

                    index -= 1;//reverts it back to current index
                    if (index - 1 > 0 && nodesAreOK(getNodesFromLocationV2(vPath[index - 1])) && nvPath == null)
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
                    if (!nodesAreOK(getNodesFromLocationV2(vPath[index])))
                    {
                        print("Failed to wait");
                        continue;
                    }
                    if (nodesAreOK(getNodesFromLocationV2(vPath[index])))
                    {
                        resetCurrentNodes(currentNodes);
                        if (fillCurrentNodes(getNodesFromLocationV2(vPath[index])))
                        {
                            gameObject.GetComponent<Renderer>().material.color = Color.black;
                            currentNodes = getNodesFromLocationV2(vPath[index]);
                        }
                        else
                        {
                            gameObject.GetComponent<Renderer>().material.color = Color.red;
                            fillCurrentNodes(currentNodes);
                            index -= 1;
                        }
                    }
                    else
                    {
                        gameObject.GetComponent<Renderer>().material.color = Color.red;
                        fillCurrentNodes(currentNodes);
                        index -= 1;
                    }
                }
            }
            else
            {
                gameObject.GetComponent<Renderer>().material.color = Color.grey;
                //moving = false;
            }
            yield return null;
        }
    }

    int getUnitInWay(Node[,] nodes)
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

    bool nodesAreOK(Node[,] nodes)
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
            if (n.isOccupied && (n.occCode != occCode && n.occCode != -1))
            {
                return false;
            }
        }
        return true;
    }

    bool fillCurrentNodes(Node[,] nodes)
    {
        if (!nodesAreOK(nodes))
        {
            print("Fill current failed");
            return false;
        }
        foreach (Node n in nodes)
        {
            if (n == null || (n.isOccupied && n.occCode != occCode && n.occCode != -1)) // potential issue with check; if bad get from nodesareok
            {
                return false;
            }
        }
        foreach (Node n in nodes)
        {
            if (n != null)
            {
                n.isOccupied = true;
                n.occCode = occCode;
            }
        }
        return true;
    }

    void resetCurrentNodes(Node[,] nodes)
    {
        foreach (Node n in nodes)
        {
            if (n != null && n.occCode == occCode)
            {
                n.isOccupied = false;
                n.occCode = -1;
            }
        }
    }

    Node[,] getNodesFromLocation(Vector3 pos , int i)
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
                    nodes[q, w] = Map.instance.getNodeFromLocation(nPos);
                }
            }
        }
        return nodes;
    }

    Node[,] getNodesFromLocationV2(Vector3 pos)
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
                    nodes[q, w] = Map.instance.getNodeFromLocation(nPos);
                }
            }
        }
        return nodes;
    }

    public int CompareTo(Unit u)
    {
        return (size == u.size) ? 1 : (size > u.size) ? 1 : -1;
    }

    public void setMarker(bool b)
    {
        if (selectionMarker.activeSelf != b)
        {
            selected = b;
            selectionMarker.SetActive(b);
        }
    }

    int sendMoveIndex(Vector3 current, Vector3 target)
    {
        if (current.x < target.x && current.z < target.z)
        {
            return 0;
        }
        else if (current.x < target.x && current.z > target.z)
        {
            return 1;
        }
        else if (current.x < target.x && current.z == target.z)
        {
            return 2;
        }
        else if (current.x > target.x && current.z < target.z)
        {
            return 3;
        }
        else if (current.x > target.x && current.z > target.z)
        {
            return 4;
        }
        else if (current.x > target.x && current.z == target.z)
        {
            return 5;
        }
        else if (current.x == target.x && current.z < target.z)
        {
            return 6;
        }
        else if (current.x == target.x && current.z > target.z)
        {
            return 7;
        }
        else if (current.x == target.x && current.z == target.z)
        {
            return 8;
        }
        else
        {
            return 9;
        }
    }

    //This function causes the unit to move out of the way of another unit
    //The way it moves is based on the current position and the target
    public void moveOutOfTheWay(int mIndex)
    {
        switch (mIndex)
        {
            case 0:
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
        }
    }

    Vector3 GetAvgPosition(Node[,] n)
    {
        float x = ((n[0, 0].position.x + n[0, n.GetLength(n.Rank - 1) - 1].position.x) / 2) + Map.length / 2;
        float z = ((n[0, 0].position.z + n[n.GetLength(n.Rank - 1) - 1, 0].position.z) / 2) + Map.length / 2;
        return new Vector3(x - 0.01f, n[0, 0].position.y, z - 0.01f);
    }
}
