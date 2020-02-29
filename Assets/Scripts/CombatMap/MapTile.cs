using System;
using UnityEngine;

// PLAIN: Any unit can pass this tile.
// ROUGH: Ground units expend double movement.
// GROUND_WALL: Ground units cannot cross this.
// IMPASSABLE: All units cannot cross this.
public enum MovementTileType
{
    PLAIN, ROUGH, GROUND_WALL, IMPASSABLE, NULL
}
public enum TerrainTileType
{
    DIRT, GRASS, SAND, SWAMP, MOUNTAIN, SHALLOW_WATER, DEEP_WATER, NULL
}

public class MapTile
{
    public MovementTileType movement;
    public TerrainTileType terrain;
    public GameObject modifier;

    public override string ToString()
    {
        return movement.ToString();
    }
    public bool GetPassable(MapUnit c)
    {
        if (movement == MovementTileType.IMPASSABLE || movement == MovementTileType.NULL)
        {
            return false;
        }
        if (c.movementType == MovementType.AIR)
        {
            return movement != MovementTileType.IMPASSABLE;
        }
        return movement != MovementTileType.GROUND_WALL;
    }
    public float GetCost(MapUnit c)
    {
        if (c.movementType == MovementType.AIR)
        {
            return 1;
        }
        if (c.movementType == MovementType.GROUND)
        {
            switch (movement)
            {
                case MovementTileType.PLAIN:
                    return 1;
                case MovementTileType.ROUGH:
                    return 2;
                default:
                    return 1;
            }
        }
        return 1;
    }

    public static MovementTileType ParseMovementTileType(string s)
    {
        if (s == null)
        {
            return MovementTileType.NULL;
        }
        int id = int.Parse(s.Replace("CombatPropertyFloorTiles_", ""));
        switch (id)
        {
            case 3:
                return MovementTileType.PLAIN;
            case 2:
                return MovementTileType.ROUGH;
            case 0:
                return MovementTileType.GROUND_WALL;
            case 1:
                return MovementTileType.IMPASSABLE;
            default:
                return MovementTileType.NULL;
        }
    }
    public static TerrainTileType ParseTerrainTileType(string s)
    {
        if (s == null)
        {
            return TerrainTileType.NULL;
        }
        int id = int.Parse(s.Replace("FloorGraphics_", ""));
        switch (id)
        {
            case 4:
                return TerrainTileType.DIRT;
            case 0:
            case 1:
                return TerrainTileType.GRASS;
            case 8:
                return TerrainTileType.SAND;
            case 5:
                return TerrainTileType.MOUNTAIN;
            case 7:
                return TerrainTileType.SWAMP;
            case 2:
                return TerrainTileType.SHALLOW_WATER;
            case 9:
                return TerrainTileType.DEEP_WATER;
            default:
                return TerrainTileType.NULL;
        }
    }
    public static MovementTileType FromTerrainTileType(TerrainTileType t)
    {
        switch (t)
        {
            case TerrainTileType.DIRT:
            case TerrainTileType.GRASS:
            case TerrainTileType.SAND:
                return MovementTileType.PLAIN;
            case TerrainTileType.SWAMP:
                return MovementTileType.ROUGH;
            case TerrainTileType.MOUNTAIN:
            case TerrainTileType.SHALLOW_WATER:
                return MovementTileType.GROUND_WALL;
            case TerrainTileType.DEEP_WATER:
                return MovementTileType.IMPASSABLE;
            default:
                return MovementTileType.NULL;
        }
    }
}