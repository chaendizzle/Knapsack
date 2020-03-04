using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{

    public Coroutine Up;
    public Coroutine Down;
    public Coroutine Left;
    public Coroutine Right;
    public Vector2Int pos;
    public bool isEnabled = true;
    public WorldMap worldMap;
    SpriteRenderer sr;

    public Vector2Int border;

    // Start is called before the first frame update
    public void Initialize()
    {
        worldMap = GameObject.FindGameObjectWithTag("WorldMap").GetComponent<WorldMap>();
        pos = new Vector2Int(15, 5);
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isEnabled)
        {
            Vector2 v = worldMap.ArrayPosToWorld(pos) + 0f * worldMap.cellSize;
            Vector2 p = transform.position;
            p = Vector2.MoveTowards(p, v, 20 * Time.deltaTime);
            transform.position = new Vector3(p.x, p.y, transform.position.z);

            HandleInput(KeyCode.UpArrow, ref Up, Vector2Int.right);
            HandleInput(KeyCode.DownArrow, ref Down, Vector2Int.left);
            HandleInput(KeyCode.RightArrow, ref Right, Vector2Int.up);
            HandleInput(KeyCode.LeftArrow, ref Left, Vector2Int.down);
        }
    }

    void HandleInput(KeyCode k, ref Coroutine c, Vector2Int v)
    {
        if(Input.GetKeyDown(k))
        {
            if(c != null)
            {
                StopCoroutine(c);
                
            }
            c = StartCoroutine(RapidKeyPress(k, v));
            
        }
        if(Input.GetKeyUp(k))
        {
            if(c != null)
            {
                StopCoroutine(c);
            }
        }
    }

    IEnumerator RapidKeyPress(KeyCode k, Vector2Int v)
    {
        MovePos(pos, v);
        yield return new WaitForSeconds(0.25f);
        while (true)
        {
            MovePos(pos, v);
            yield return new WaitForSeconds(0.05f);
        }
    }
    private void MovePos(Vector2Int pos, Vector2Int v)
    {
        Vector2Int p = pos + v;
        if (p.x >= border.x && p.y >= border.y &&
            p.x < worldMap.tiles.GetLength(0) - border.y && p.y < worldMap.tiles.GetLength(1) - border.x)
        {
            if (worldMap.tiles[p.x, p.y].terrain != TerrainTileType.NULL)
            {
                this.pos += v;
            }
        }
    }

    public void Enable()
    {
        isEnabled = true;
        sr.color = Color.white;
    }
    public void DisableCursor()
    {
        isEnabled = false;
        sr.color = Color.clear;
    }

    public void SelectTile(Vector2Int pos)
    {
        this.pos = pos;
    }
}
