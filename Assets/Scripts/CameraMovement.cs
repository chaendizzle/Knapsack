using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraMovement : MonoBehaviour
{
    public Vector2Int cameraPos;
    public Vector2Int borderMin;
    public Vector2Int borderMax;
    public HexGridMap combatMap;
    public CombatCursor cursor;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Rect camRect = CameraRect();
        Vector2 v = combatMap.ArrayPosToWorld(cameraPos) + 0f * combatMap.cellSize;
        // even out y positions so camera doesn't jitter when going horizontally
        if (combatMap.parity && cameraPos.y % 2 == 1)
        {
            v += Vector2.up * combatMap.cellSize.x * 0.5f;
        }
        else if (!combatMap.parity && cameraPos.y % 2 == 0)
        {
            v += Vector2.up * combatMap.cellSize.x * 0.5f;
        }
        Vector2 p = transform.position;
        p = Vector2.MoveTowards(p, v, 75 * Time.deltaTime);
        Vector2 min = combatMap.ArrayPosToWorld(new Vector2Int(borderMin.y, borderMin.x)) + 0.5f * camRect.size;
        Vector2 max = combatMap.ArrayPosToWorld(new Vector2Int(combatMap.tiles.GetLength(0) - borderMax.y - 1,
            combatMap.tiles.GetLength(1) - borderMax.x - 1)) - 0.5f * camRect.size;
        // shift bounds by 1/2 cell vertically depending on parity so there isn't empty space
        if (combatMap.parity)
        {
            max += Vector2.down * combatMap.cellSize.x * 0.5f;
        }
        else
        {
            min += Vector2.up * combatMap.cellSize.x * 0.5f;
        }
        // move towards target camera pos
        p = new Vector2(Mathf.Clamp(p.x, min.x, max.x), Mathf.Clamp(p.y, min.y, max.y));
        transform.position = new Vector3(p.x, p.y, transform.position.z);

        // adjust camera pos to include cursor
        int camWidth = (int)(camRect.width / combatMap.cellSize.y);
        int camHeight = (int)(camRect.height / combatMap.cellSize.x);
        int scrollWidth = camWidth / 2 - 2;
        int scrollHeight = camHeight / 2 - 2;
        //if cursor.pos > rightward bound: shows how far right you need to go; if cursor.pos < leftward bound, it'll be negative, showing us how far left we need to go
        int dx = Math.Max(0, cursor.pos.y - (cameraPos.y + scrollWidth)) + Math.Min(0, cursor.pos.y - (cameraPos.y - scrollWidth));
        int dy = Math.Max(0, cursor.pos.x - (cameraPos.x + scrollHeight)) + Math.Min(0, cursor.pos.x - (cameraPos.x - scrollHeight));
        cameraPos += new Vector2Int(dy, dx);
    }

    public static Rect CameraRect()
    {
        Vector2 min = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector2 max = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        return new Rect(min, max - min);
    }

    // focus tile on cameraPosition = (0, 0) is center, (-1, -1) is bottom left, (1, 1) is top right
    public void SelectTile(Vector2Int pos, Vector2 cameraPosition)
    {
        Rect camRect = CameraRect();
        int camWidth = (int)(camRect.width / combatMap.cellSize.x) / 2;
        int camHeight = (int)(camRect.height / combatMap.cellSize.y) / 2;
        cameraPos = pos + new Vector2Int((int)(cameraPosition.x * camWidth), (int)(cameraPosition.y * camHeight));
    }
}
