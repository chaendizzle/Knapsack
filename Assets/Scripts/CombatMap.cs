﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class CombatMap : MonoBehaviour
{
    public BoundsInt mapBounds;
    public Tilemap tilemap;
    BoundsInt boundsFloor;
    BoundsInt boundsObjects;
    public MapTile[,] tiles;
    public DebugTiles debugTiles;
    Vector2Int lastMouseClick;
    Pathfinder pathfinder;
    bool parity = false;

    // Start is called before the first frame update
    public void /*Fire emblem */Initialize/*ning*/()
    {
        tilemap.color = Color.clear;
        boundsFloor = tilemap.cellBounds;
        TileBase[] tilesWorld = tilemap.GetTilesBlock(boundsFloor);
        
        tiles = new MapTile[boundsFloor.size.x, boundsFloor.size.y];
        for(int x = 0; x < boundsFloor.size.x; x++)
        {
            for(int y = 0; y < boundsFloor.size.y; y++)
            {
                tiles[x, y] = new MapTile();
                tiles[x, y].tile = ParseMapTile(tilesWorld[y * boundsFloor.size.x + x]?.name);
            }
        }

        pathfinder = new Pathfinder(tiles, parity);
        int xMin = tiles.GetLength(0);
        int yMin = tiles.GetLength(1);
        int xMax = -1;
        int yMax = -1;
        for(int x = 0; x < tiles.GetLength(0); x++)
        {
            for(int y = 0; y < tiles.GetLength(1); y++)
            {
                if(tiles[x,y].tile != MapTileType.NULL)
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
            for (int i = 0; i < dests.Count; i++)
            {
                SetDebugTile(dests[i].pos, Color.blue);
            }
        }
    }

    public Vector2 arrayPosToWorld(Vector2Int input)
    {
        Vector2Int floorToBounds = new Vector2Int(boundsFloor.min.x, boundsFloor.min.y);
        return tilemap.CellToWorld((Vector3Int)input + (Vector3Int)(floorToBounds));
    }
    public Vector2Int WorldToArrayPos(Vector2 input)
    {
        Vector2Int floorToBounds = new Vector2Int(boundsFloor.min.x, boundsFloor.min.y);
        Vector3Int v = (tilemap.WorldToCell(input) - (Vector3Int)floorToBounds);
        return new Vector2Int(v.x, v.y);
    }

    MapTileType ParseMapTile(string s)
    {
        if(s == null)
        {
            return MapTileType.NULL;
        }
        int id = int.Parse(s.Replace("CombatPropertyFloorTiles_", ""));
        switch (id)
        {
            case 3:
                return MapTileType.PLAIN;
            case 2:
                return MapTileType.ROUGH;
            case 0:
                return MapTileType.GROUND_WALL;
            case 1:
                return MapTileType.IMPASSABLE;
            default:
                return MapTileType.NULL;
        }
    }

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
}
