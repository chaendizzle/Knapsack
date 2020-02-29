using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileGenerator : MonoBehaviour
{
    public GameObject templateMapPrefab;
    public GameObject test;

    [Serializable]
    public class TileGenTerrainPair
    {
        public TerrainTileType name;
        public Tile tile;
    }
    public List<TileGenTerrainPair> terrainTiles;

    [Serializable]
    public class TileGenMovementPair
    {
        public MovementTileType name;
        public Tile tile;
    }
    public List<TileGenMovementPair> movementTiles;

    OverlappingModel model;

    void Awake()
    {

    }

    // this is expensive! Either do this during loading or TODO: a few iterations per tick
    public TerrainTileType[,] Generate(int width, int height, int seed = 0)
    {
        ScanTemplateMap(test.GetComponent<CombatMap>());
        return null;
        // train and generate
        CombatMap templateMap = Instantiate(templateMapPrefab).GetComponent<CombatMap>();
        byte[,] templateArray = ScanTemplateMap(templateMap);
        Destroy(templateMap.gameObject);
        model = new OverlappingModel(templateArray, 2, 2 * width, 2 * height, false, false, 4, 0);
        model.Run(seed, 1000);
        // create terrain array
        TerrainTileType[,] rawOutput = new TerrainTileType[2 * width, 2 * height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                rawOutput[i, j] = (TerrainTileType)model.Sample(i, j);
            }
        }
        return null;
    }

    // template map should be a rhombus.
    byte[,] ScanTemplateMap(CombatMap templateMap)
    {
        MapTile[,] tiles = templateMap.tiles;
        // find the leftmost (min Y) point
        Vector2Int start = FindLeftmostPos(tiles);
        if (start.x < 0)
        {
            throw new InvalidOperationException("Template Map has no terrain tiles.");
        }
        start.x += 7;
        // scan X as far as possible for "rhombus width"
        Vector2Int current = start;
        int width = 0;
        while (tiles[current.x, current.y].terrain != TerrainTileType.NULL)
        {
            width++;
            current = AdvanceRhombusX(current, 1, templateMap.parity);
        }
        // scan Y as far as possible for "rhombus height"
        current = start;
        int height = 0;
        while (tiles[current.x, current.y].terrain != TerrainTileType.NULL)
        {
            height++;
            current = AdvanceRhombusY(current, 1, templateMap.parity);
        }
        // scan entire rhombus
        byte[,] rhombusMap = new byte[width, height];
        for (int i = 0; i < width; i++)
        {
            current = AdvanceRhombusX(start, i, templateMap.parity);
            for (int j = 0; j < height; j++)
            {
                templateMap.SetDebugTile(current, Color.red);
                TerrainTileType t = tiles[current.x, current.y].terrain;
                if (t == TerrainTileType.NULL)
                {
                    throw new InvalidOperationException("Template Map is not a rhombus or contains null tiles.");
                }
                rhombusMap[i, j] = (byte)t;
                current = AdvanceRhombusY(current, 1, templateMap.parity);
            }
        }
        return null;
    }
    // Used to navigate rhombus coordinates.
    // We convert to "rhombus coordinates" so that the hex grid
    // gets accurately represented in a 2D grid.
    // In "rhombus coordinates", moving up and to the right is the X direction,
    // and moving down and to the right is the Y direction.
    // Also note that Flat-top hex grids have switched X and Y coordinates,
    // so you move up by increasing the X value.
    Vector2Int AdvanceRhombusX(Vector2Int pos, int amount, bool parity)
    {
        for (int i = 0; i < Mathf.Abs(amount); i++)
        {
            pos.y += (int)Mathf.Sign(amount);
            // Check if moving up and to the right, increasing Y,
            // will cause our X coordinate to change on the hex grid.
            // This occurs only if we are at an ODD Y coordinate,
            // flipped to odd if the map is aligned with a different parity,
            // and flipped again if we are going left instead of right.
            if ((pos.y % 2 == 1) ^ parity ^ (amount < 0))
            {
                pos.x += (int)Mathf.Sign(amount);
            }
        }
        return pos;
    }
    Vector2Int AdvanceRhombusY(Vector2Int pos, int amount, bool parity)
    {
        for (int i = 0; i < Mathf.Abs(amount); i++)
        {
            pos.y += (int)Mathf.Sign(amount);
            // Check if moving down and to the right, increasing Y,
            // will cause our X coordinate to change on the hex grid.
            // This occurs only if we are at an EVEN Y coordinate,
            // flipped to even if the map is aligned with a different parity,
            // and flipped again if we are going left instead of right.
            if ((pos.y % 2 == 0) ^ parity ^ (amount < 0))
            {
                pos.x -= (int)Mathf.Sign(amount);
            }
        }
        return pos;
    }
    Vector2Int FindLeftmostPos(MapTile[,] tiles)
    {
        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                if (tiles[i, j].terrain != TerrainTileType.NULL)
                {
                    return new Vector2Int(i, j);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    // Start is called before the first frame update
    void Start()
    {
        Generate(1, 1, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
