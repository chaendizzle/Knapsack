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

    List<GameObject> lines = new List<GameObject>();
    
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

    public void SetLine(Vector2 start, Vector2 end)
    {
        DebugLine line = Instantiate(debugLinePrefab).GetComponent<DebugLine>();
        line.SetLine(start, end);
        lines.Add(line.gameObject);
    }
    public void SetArrow(Vector2 start, Vector2 end)
    {
        DebugArrow line = Instantiate(debugArrowPrefab).GetComponent<DebugArrow>();
        line.SetLine(start, end);
        lines.Add(line.gameObject);
    }
    public void ClearLines()
    {
        foreach (GameObject go in lines)
        {
            if (go != null)
            {
                Destroy(go);
            }
        }
        lines.Clear();
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
