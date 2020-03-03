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
        public TileBase tile;
    }
    public List<TileGenTerrainPair> terrainTiles;
    Dictionary<TerrainTileType, TileBase> terrainTilesByType = new Dictionary<TerrainTileType, TileBase>();

    [Serializable]
    public class TileGenMovementPair
    {
        public MovementTileType name;
        public TileBase tile;
    }
    public List<TileGenMovementPair> movementTiles;
    Dictionary<MovementTileType, TileBase> movementTilesByType = new Dictionary<MovementTileType, TileBase>();

    OverlappingModel model;

    void Awake()
    {
        foreach (TileGenTerrainPair p in terrainTiles)
        {
            terrainTilesByType[p.name] = p.tile;
        }
        foreach (TileGenMovementPair p in movementTiles)
        {
            movementTilesByType[p.name] = p.tile;
        }
    }

    // this is expensive! Either do this during loading or TODO: a few iterations per tick
    public TerrainTileType[,] Generate(Vector2Int size, bool outputParity, int seed = 0)
    {
        // train and generate
        WorldMap templateMap = Instantiate(templateMapPrefab).GetComponent<WorldMap>();
        templateMap.Initialize();
        byte[,] templateArray = ScanTemplateMap(templateMap);
        Destroy(templateMap.gameObject);
        int len = Mathf.Max(2 * size.x, 2 * size.y);
        // make size odd, since you can only make a rhombus on a hex grid with odd dimensions
        if (len % 2 == 0)
        {
            len++;
        }
        // map enums to (0, ..., n). Can't skip indices or else algorithm bugs out.
        Dictionary<byte, byte> byteMap = new Dictionary<byte, byte>();
        Dictionary<byte, byte> revMap = new Dictionary<byte, byte>();
        for (int i = 0; i < templateArray.GetLength(0); i++)
        {
            for (int j = 0; j < templateArray.GetLength(1); j++)
            {
                if (!byteMap.ContainsKey(templateArray[i, j]))
                {
                    byte idx = (byte)byteMap.Count;
                    byteMap[templateArray[i, j]] = idx;
                    revMap[idx] = templateArray[i, j];
                }
                templateArray[i, j] = byteMap[templateArray[i, j]];
            }
        }
        // run the model
        model = new OverlappingModel(templateArray, 2, len, len, false, false, 8, 0);
        model.Run(seed, 0);
        // process output into a map
        TerrainTileType[,] rawOutput = new TerrainTileType[len, len];
        for (int i = 0; i < len; i++)
        {
            for (int j = 0; j < len; j++)
            {
                byte output = model.Sample(i, j);
                rawOutput[i, j] = TerrainTileType.NULL;
                if (output != 99 && revMap.ContainsKey(output))
                {
                    rawOutput[i, j] = (TerrainTileType)revMap[output];
                }
            }
        }
        // convert to 2D hex grid from rhombus coords
        TerrainTileType[,] rhombusForm = new TerrainTileType[len, len * 2 - 1];
        Vector2Int start = new Vector2Int(len / 2, 0);
        Vector2Int current = start;
        for (int i = 0; i < len; i++)
        {
            current = AdvanceRhombusX(start, i, outputParity);
            for (int j = 0; j < len; j++)
            {
                rhombusForm[current.x, current.y] = rawOutput[i, j];
                current = AdvanceRhombusY(current, 1, outputParity);
            }
        }
        // cut off the edges to create square 2D tilemap
        TerrainTileType[,] terrainMap = new TerrainTileType[size.y, size.x];
        start = new Vector2Int(len - size.x / 2, len / 2 - size.y / 2);
        // make sure starting point is always even
        if (start.x % 2 == 1)
        {
            start.x++;
        }
        if (start.y % 2 == 1)
        {
            start.y++;
        }
        for (int i = 0; i < size.y; i++)
        {
            for (int j = 0; j < size.x; j++)
            {
                terrainMap[i, j] = rhombusForm[start.y + i, start.x + j];
            }
        }
        return terrainMap;
    }

    // template map should be a rhombus.
    byte[,] ScanTemplateMap(WorldMap templateMap)
    {
        MapTile[,] tiles = templateMap.tiles;
        // find the leftmost (min Y) point
        Vector2Int start = FindLeftmostPos(tiles);
        if (start.x < 0)
        {
            throw new InvalidOperationException("Template Map has no terrain tiles.");
        }
        // scan X as far as possible for "rhombus width"
        Vector2Int current = start;
        int width = 0;
        while (current.x < tiles.GetLength(0) && current.y < tiles.GetLength(1) &&
            tiles[current.x, current.y].terrain != TerrainTileType.NULL)
        {
            width++;
            current = AdvanceRhombusX(current, 1, templateMap.parity);
            templateMap.SetDebugTile(current, Color.red);
        }
        // scan Y as far as possible for "rhombus height"
        current = start;
        int height = 0;
        while (current.x >= 0 && current.y < tiles.GetLength(1) &&
            tiles[current.x, current.y].terrain != TerrainTileType.NULL)
        {
            height++;
            current = AdvanceRhombusY(current, 1, templateMap.parity);
            templateMap.SetDebugTile(current, Color.blue);
        }
        // scan entire rhombus
        Debug.Log($"{width}, {height}");
        byte[,] rhombusMap = new byte[width, height];
        for (int i = 0; i < width; i++)
        {
            current = AdvanceRhombusX(start, i, templateMap.parity);
            for (int j = 0; j < height; j++)
            {
                TerrainTileType t = tiles[current.x, current.y].terrain;
                templateMap.SetDebugTile(current, Color.green);
                if (t == TerrainTileType.NULL)
                {
                    throw new InvalidOperationException("Template Map is not a rhombus or contains null tiles.");
                }
                rhombusMap[i, j] = (byte)t;
                current = AdvanceRhombusY(current, 1, templateMap.parity);
            }
        }
        return rhombusMap;
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
            // This occurs only if we are at an EVEN Y coordinate,
            // flipped to odd if the map is aligned with a different parity,
            // and flipped again if we are going left instead of right.
            if ((pos.y % 2 == 0) ^ parity ^ (amount < 0))
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
            // This occurs only if we are at an ODD Y coordinate,
            // flipped to even if the map is aligned with a different parity,
            // and flipped again if we are going left instead of right.
            if ((pos.y % 2 == 1) ^ parity ^ (amount < 0))
            {
                pos.x -= (int)Mathf.Sign(amount);
            }
        }
        return pos;
    }
    Vector2Int FindLeftmostPos(MapTile[,] tiles)
    {
        for (int j = 0; j < tiles.GetLength(1); j++)
        {
            for (int i = 0; i < tiles.GetLength(0); i++)
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
        WorldMap map = test.GetComponent<WorldMap>();
        Vector2Int size = new Vector2Int(27, 27);
        Vector2Int offset = new Vector2Int(-27 / 2, -27 / 2);
        map.Set(Generate(size, offset.y % 2 != 0, 0), offset, terrainTilesByType, movementTilesByType);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
