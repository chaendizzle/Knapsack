using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CursorInputType
{
    MOUSE, KEYS
}

public class CombatCursor : MonoBehaviour
{
    public Coroutine Up;
    public Coroutine Down;
    public Coroutine Left;
    public Coroutine Right;
    public Vector2Int pos;
    public bool isEnabled = true;
    public HexGridMap combatMap;
    SpriteRenderer sr;

    public Vector2Int border;
    public CursorInputType inputType;
    Vector2 oldMousePos;

    // Start is called before the first frame update
    public void Initialize()
    {
        pos = combatMap.WorldToArrayPos(transform.position);
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isEnabled)
        {
            Vector2 v = combatMap.ArrayPosToWorld(pos) + 0f * combatMap.cellSize;
            Vector2 p = transform.position;
            p = Vector2.MoveTowards(p, v, (inputType == CursorInputType.KEYS ? 20 : Mathf.Infinity) * Time.deltaTime);
            transform.position = new Vector3(p.x, p.y, transform.position.z);

            // choose input type
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
                Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                inputType = CursorInputType.KEYS;
            }
            else if (((Vector2)Input.mousePosition - oldMousePos).magnitude > 1f ||
                Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                StopKeypressCoroutine(ref Up);
                StopKeypressCoroutine(ref Down);
                StopKeypressCoroutine(ref Right);
                StopKeypressCoroutine(ref Left);
                inputType = CursorInputType.MOUSE;
            }
            oldMousePos = Input.mousePosition;

            // keyboard input
            if (inputType == CursorInputType.KEYS)
            {
                // remember hex grid is XY switched, so going right is actually going up
                HandleInput(KeyCode.UpArrow, ref Up, Vector2Int.right);
                HandleInput(KeyCode.DownArrow, ref Down, Vector2Int.left);
                HandleInput(KeyCode.RightArrow, ref Right, Vector2Int.up);
                HandleInput(KeyCode.LeftArrow, ref Left, Vector2Int.down);
            }
            else
            {
                // set cursor pos to the tile targeted by the mouse
                pos = combatMap.WorldToArrayPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
        }
    }

    void StopKeypressCoroutine(ref Coroutine c)
    {
        if (c != null)
        {
            StopCoroutine(c);
        }
        c = null;
    }

    void HandleInput(KeyCode k, ref Coroutine c, Vector2Int v)
    {
        if(Input.GetKeyDown(k))
        {
            StopKeypressCoroutine(ref c);
            c = StartCoroutine(RapidKeyPress(k, v));
        }
        if(Input.GetKeyUp(k))
        {
            StopKeypressCoroutine(ref c);
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
            p.x < combatMap.tiles.GetLength(0) - border.y && p.y < combatMap.tiles.GetLength(1) - border.x)
        {
            if (combatMap.tiles[p.x, p.y].terrain != TerrainTileType.NULL)
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
