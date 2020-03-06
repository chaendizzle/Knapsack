using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DebugTiles : MonoBehaviour
{
    Tilemap tilemap;
    public TileBase debugTile;
    public GameObject debugLinePrefab;
    public GameObject debugArrowPrefab;

    List<DebugLine> lines = new List<DebugLine>();
    DebugArrow arrow;
    
    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    DebugLine AddLine()
    {
        DebugLine line = Instantiate(debugLinePrefab, transform).GetComponent<DebugLine>();
        lines.Add(line);
        return line;
    }
    void SetLine(int i, Vector2 start, Vector2 end)
    {
        DebugLine line = lines[i].GetComponent<DebugLine>();
        line.SetLine(start, end);
        lines.Add(line);
    }
    void RemoveLine(int i)
    {
        Destroy(lines[i].gameObject);
        lines.RemoveAt(i);
    }
    DebugLine GetLine(int i, int pathSize)
    {
        // destroy unused line
        if (i >= pathSize)
        {
            RemoveLine(i);
            return null;
        }
        // move existing line
        if (i < lines.Count)
        {
            return lines[i];
        }
        // create new line
        if (i >= lines.Count)
        {
            return AddLine();
        }
        return null;
    }

    public void SetPath(List<Vector2> path)
    {
        for (int i = 0; i < Mathf.Max(path.Count - 1, lines.Count); i++)
        {
            DebugLine line = GetLine(i, path.Count - 1);
            if (i < path.Count - 2)
            {
                line.SetLine(path[i], path[i + 1]);
            }
            // last one
            else if (i == path.Count - 2)
            {
                line.SetLine(path[i], path[i] * 0.5f + path[i + 1] * 0.5f);
            }
            else
            {
                i--;
            }
        }
    }
    public void SetArrow(Vector2 start, Vector2 end)
    {
        if (arrow == null)
        {
            arrow = Instantiate(debugArrowPrefab, transform).GetComponent<DebugArrow>();
        }
        arrow.SetLine(start, end);
    }
    public void ClearLines()
    {
        foreach (DebugLine line in lines)
        {
            if (line != null)
            {
                Destroy(line.gameObject);
            }
        }
        lines.Clear();
        if (arrow != null)
        {
            Destroy(arrow.gameObject);
        }
        arrow = null;
    }

    public void SetTile(Vector2 pos, Color c)
    {
        Vector3Int v = tilemap.WorldToCell(pos);
        tilemap.SetTile(v, debugTile);
        tilemap.SetTileFlags(v, TileFlags.None);
        tilemap.SetColor(v, c);
    }
    public void ClearTile(Vector2 pos)
    {
        Vector3Int v = tilemap.WorldToCell(pos);
        tilemap.SetTile(v, null);
        tilemap.SetTileFlags(v, TileFlags.None);
        tilemap.SetColor(v, Color.white);
    }
    public void ClearTiles()
    {
        tilemap.ClearAllTiles();
    }
}
