using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using static MapTile;

public class CombatMap : MonoBehaviour
{
    // set by unity inspector
    public DebugTiles debugTiles;
    public Tilemap terrain;
    public Tilemap mapObjects;
    public Tilemap properties;
    public bool parity { get; private set; }

    // accessed by other components
    [NonSerialized]
    public BoundsInt mapBounds;
    public MapTile[,] tiles { get; private set; }
    public Vector2Int offset => (Vector2Int)boundsFloor.min;

    BoundsInt boundsFloor;
    BoundsInt boundsObjects;
    Vector2Int lastMouseClick;
    Pathfinder pathfinder;

    // Start is called before the first frame update
    public void /*Fire emblem */Initialize/*ning*/()
    {
        properties.color = Color.clear;
        boundsFloor = terrain.cellBounds;
        TileBase[] propertiesTiles = properties.GetTilesBlock(boundsFloor);
        TileBase[] terrainTiles = terrain.GetTilesBlock(boundsFloor);

        tiles = new MapTile[boundsFloor.size.x, boundsFloor.size.y];
        for(int x = 0; x < boundsFloor.size.x; x++)
        {
            for(int y = 0; y < boundsFloor.size.y; y++)
            {
                tiles[x, y] = new MapTile();
                tiles[x, y].movement = ParseMovementTileType(propertiesTiles[y * boundsFloor.size.x + x]?.name);
                tiles[x, y].terrain = ParseTerrainTileType(terrainTiles[y * boundsFloor.size.x + x]?.name);
            }
        }
        // if we start at an odd position, set to odd parity
        parity = offset.y % 2 != 0;

        pathfinder = new Pathfinder(tiles, false);
        int xMin = tiles.GetLength(0);
        int yMin = tiles.GetLength(1);
        int xMax = -1;
        int yMax = -1;
        for(int x = 0; x < tiles.GetLength(0); x++)
        {
            for(int y = 0; y < tiles.GetLength(1); y++)
            {
                if(tiles[x,y].movement != MovementTileType.NULL)
                {
                    xMin = Math.Min(xMin, x);
                    yMin = Math.Min(yMin, y);
                    xMax = Math.Max(xMax, x);
                    yMax = Math.Max(yMax, y);
                }
            }
        }
        xMax += 1;
        yMax += 1;
        mapBounds.xMax = xMax;
        mapBounds.xMin = xMin;
        mapBounds.yMax = yMax;
        mapBounds.yMin = yMin;
    }

    // Update is called once per frame---------------------------------------------------------------------------------------------------------------------
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int arrayMousePos = WorldToArrayPos(mousePos);
            lastMouseClick = arrayMousePos;
            SetDebugTile(arrayMousePos, Color.blue);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ClearDebugLines();
            ClearDebugTiles();
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int arrayMousePos = WorldToArrayPos(mousePos);
            List<PathResultNode> path = pathfinder.FindPath(new MapUnit() { movementType = MovementType.GROUND }, lastMouseClick, arrayMousePos);
            if (path != null)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    SetDebugLine(path[i].pos, path[i + 1].pos);
                }
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            ClearDebugLines();
            ClearDebugTiles();
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int arrayMousePos = WorldToArrayPos(mousePos);
            List<PathResultNode> dests = pathfinder.FindDestinations(new MapUnit() { movementType = MovementType.GROUND }, arrayMousePos, 4);
            if (dests != null)
            {
                for (int i = 0; i < dests.Count; i++)
                {
                    SetDebugTile(dests[i].pos, Color.blue);
                }
            }
            Debug.Log(arrayMousePos);
        }
    }

    public Vector2 arrayPosToWorld(Vector2Int input)
    {
        Vector2Int floorToBounds = new Vector2Int(boundsFloor.min.x, boundsFloor.min.y);
        return properties.CellToWorld((Vector3Int)input + (Vector3Int)(floorToBounds));
    }
    public Vector2Int WorldToArrayPos(Vector2 input)
    {
        Vector2Int floorToBounds = new Vector2Int(boundsFloor.min.x, boundsFloor.min.y);
        Vector3Int v = (properties.WorldToCell(input) - (Vector3Int)floorToBounds);
        return new Vector2Int(v.x, v.y);
    }

    // pathfinding debug
    public void SetDebugTile(Vector2Int arrayPos, Color c)
    {
        debugTiles.SetTile(arrayPosToWorld(arrayPos), c);
    }
    public void ClearDebugTiles()
    {
        debugTiles.ClearTiles();
    }
    public void SetDebugLine(Vector2Int arrayPosA, Vector2Int arrayPosB)
    {
        debugTiles.SetLine(arrayPosToWorld(arrayPosA), arrayPosToWorld(arrayPosB));
    }
    public void ClearDebugLines()
    {
        debugTiles.ClearLines();
    }

    public static CombatMap GetInstance()
    {
        return GameObject.FindGameObjectWithTag("WorldMap").GetComponent<CombatMap>();
    }

    // set the map tiles
    public void Set(TerrainTileType[,] tiles, Vector2Int offset,
        Dictionary<TerrainTileType, Tile> terrainTiles, Dictionary<MovementTileType, Tile> movementTiles)
    {
        terrain.ClearAllTiles();
        properties.ClearAllTiles();
        // set up new tiles
        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                Tile terrainTile = null;
                if (terrainTiles.ContainsKey(tiles[i, j]))
                {
                    terrainTile = terrainTiles[tiles[i, j]];
                }
                terrain.SetTile(new Vector3Int(i, j, 0) + (Vector3Int)offset, terrainTile);
                MovementTileType mvt = FromTerrainTileType(tiles[i, j]);
                Tile mvtTile = null;
                if (movementTiles.ContainsKey(mvt))
                {
                    mvtTile = movementTiles[mvt];
                }
                properties.SetTile(new Vector3Int(i, j, 0) + (Vector3Int)offset, mvtTile);
            }
        }
        Initialize();
    }
}
