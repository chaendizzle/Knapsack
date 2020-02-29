using System;
using UnityEngine;

// PLAIN: Any unit can pass this tile.
// ROUGH: Ground units expend double movement.
// GROUND_WALL: Ground units cannot cross this.
// IMPASSABLE: All units cannot cross this.
public enum MapTileType
{
    PLAIN, ROUGH, GROUND_WALL, IMPASSABLE, NULL
}

public class MapTile
{
    public MapTileType tile;
    public GameObject modifier;

    public override string ToString()
    {
        return tile.ToString();
    }
    public bool GetPassable(MapUnit c)
    {
        if (tile == MapTileType.IMPASSABLE || tile == MapTileType.NULL)
        {
            return false;
        }
        if (c.movementType == MovementType.AIR)
        {
            return tile != MapTileType.IMPASSABLE;
        }
        return tile != MapTileType.GROUND_WALL;
    }
    public float GetCost(MapUnit c)
    {
        if (c.movementType == MovementType.AIR)
        {
            return 1;
        }
        if (c.movementType == MovementType.GROUND)
        {
            switch (tile)
            {
                case MapTileType.PLAIN:
                    return 1;
                case MapTileType.ROUGH:
                    return 2;
                default:
                    return 1;
            }
        }
        return 1;
    }
}