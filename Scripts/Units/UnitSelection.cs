using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelection : MonoBehaviour
{
    public static UnitSelection selection;

    public List<Unit> playerUnits = new List<Unit>();
    public List<UnitV2> playerUnitsV2 = new List<UnitV2>();
    public List<Unit> selected = new List<Unit>();
    public List<UnitV2> selectedV2 = new List<UnitV2>();
    bool isSelecting = false;

    Vector3 mousePos;
    Vector3 mousePos2;

    private void Start()
    {
        selection = this;
    }

    private void Update()
    {
        mousePos = Input.mousePosition;
        if (Input.GetMouseButtonDown(1))
        {
            mousePos2 = Input.mousePosition;

            for (int i = 0; i < selected.Count; i++)
            {
                if (selected[i] == null)
                {
                    selected.RemoveAt(i);
                    i = 1000;
                    continue;
                }
                selected[i].setMarker(false);
            }
            isSelecting = true;
            selected = new List<Unit>();
        }

        if (Input.GetMouseButtonUp(1))
        {
            foreach (Unit u in playerUnits)
            {
                if (u == null) continue;
                if (inBounds(u.gameObject))
                {
                    selected.Add(u);
                    u.selected = true;
                }
            }
            isSelecting = false;
        }

        if (isSelecting)
        {
            foreach (Unit u in playerUnits)
            {
                if (u == null) continue;
                if (inBounds(u.gameObject))
                {
                    u.setMarker(true);
                }
                else
                {
                    u.setMarker(false);
                }
            }
        }

        if (Input.GetMouseButtonDown(2))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit))
            {
                //sendMoveLoc(hit.point);
                Map.instance.addGroupPathRequest(hit.point, selected);
            }
        }
    }

    void sendMoveLoc(Vector3 pos)
    {
        if (selected.Count == 0)
        {
            return;
        }

        MinHeap<Unit> ordered = new MinHeap<Unit>(selected);
        Unit[,] placed = null;
        int layers = estimateLayers();

        placed = new Unit[layers, layers];

        for (int i = 0; i < layers; i++)
        {
            for (int j = 0; j < layers; j++)
            {
                if (ordered.size > 0)
                    placed[i, j] = ordered.getFront();
            }
        }

        Vector3 position = pos;

        position.x -= placed[0, 0].size * (layers / 2);
        position.z -= placed[0, 0].size * (layers / 2);

        float x = position.x;
        float increase = 0;

        for (int i = 0; i < layers; i++)
        {
            position.x = x;
            for (int j = 0; j < layers; j++)
            {
                if (placed[i, j] != null)
                {
                    placed[i, j].requestPath(position, 0, 0);
                    position.x += placed[i, j].size * 2;
                    increase = placed[i, j].size;
                }
            }
            position.z += increase * 2;
        }
    }

    int estimateLayers()
    {
        int space = 0;
        int layers = 0;

        foreach (Unit u in selected)
        {
            space += u.size * u.size;
        }

        while (layers * layers < space)
        {
            layers++;
        }

        return layers;
    }

    bool inBounds(GameObject g)
    {
        Camera camera = Camera.main;

        Bounds viewportBounds = Utils.GetViewportBounds(camera, mousePos, mousePos2);
        return viewportBounds.Contains(camera.WorldToViewportPoint(g.transform.position));
    }

    void OnGUI()
    {
        if (isSelecting)
        {
            var rect = Utils.GetScreenRect(mousePos2, Input.mousePosition);
            Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }
}
