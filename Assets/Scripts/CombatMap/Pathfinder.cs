using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct PathResultNode
{
    public Vector2Int pos;
    public float cost;
    public override string ToString()
    {
        return $"({pos}: {cost})";
    }
}

public class Pathfinder
{
    MapTile[,] tiles;
    int parity;
    public Pathfinder(MapTile[,] tiles, bool parity)
    {
        this.tiles = tiles;
        this.parity = parity ? 0 : 1;
    }

    public List<PathResultNode> FindPath(CombatUnit unit, Vector2Int start, Vector2Int goal, float maxCost = Mathf.Infinity)
    {
        return AStar(unit, start, goal, maxCost, false);
    }
    public List<PathResultNode> FindDestinations(CombatUnit unit, Vector2Int start, float maxCost)
    {
        return AStar(unit, start, new Vector2Int(int.MinValue, int.MinValue), maxCost, true);
    }

    List<PathResultNode> AStar(CombatUnit unit, Vector2Int start, Vector2Int end, float maxCost, bool allDestinations)
    {
        // create AStarNode grid
        var grid = new AStarNode[tiles.GetLength(0), tiles.GetLength(1)];
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = new AStarNode()
                {
                    pos = new Vector2Int(i, j),
                    tile = tiles[i, j]
                };
                grid[i, j].Reset(unit);
            }
        }
        // check start node
        AStarNode startNode = GetNode(grid, start);
        if (startNode == null)
        {
            return null;
        }
        startNode.g = 0;
        // don't use heuristics if we are getting all destinations
        startNode.h = allDestinations ? 0 : Heuristic(startNode.pos, end);
        // check end node
        if (!allDestinations && GetNode(grid, end) == null)
        {
            return null;
        }
        // add start node to open set
        var openSet = new FastPriorityQueue<AStarNode>(grid.GetLength(0) * grid.GetLength(1));
        openSet.Enqueue(startNode, startNode.f);
        startNode.open = true;
        while (openSet.Count > 0)
        {
            // pop off most promising node
            AStarNode current = openSet.Dequeue();
            // check if we are done
            if (!allDestinations && current.pos == end)
            {
                openSet.Clear();
                return GetPath(grid, end);
            }
            if (allDestinations && current.g > maxCost)
            {
                return GetDestinations(grid, maxCost);
            }
            current.closed = true;
            // for each neighbor
            foreach (AStarNode neighbor in GetNeighbors(grid, current.pos))
            {
                if (neighbor == null || neighbor.closed)
                {
                    continue;
                }
                // check for better path
                float tentativeG = current.g + Cost(unit, current.pos, neighbor.pos);
                if (!neighbor.open)
                {
                    neighbor.previous = current;
                    neighbor.g = tentativeG;
                    // don't use heuristics if we are getting all destinations
                    neighbor.h = allDestinations ? 0 : Heuristic(neighbor.pos, end);
                    openSet.Enqueue(neighbor, neighbor.f);
                    neighbor.open = true;
                }
                else if (tentativeG < neighbor.g)
                {
                    neighbor.previous = current;
                    neighbor.g = tentativeG;
                    openSet.UpdatePriority(neighbor, neighbor.f);
                }
            }
        }
        // return null if not looking for all destinations
        return allDestinations ? GetDestinations(grid, maxCost) : null;
    }

    List<PathResultNode> GetPath(AStarNode[,] grid, Vector2Int end)
    {
        List<PathResultNode> path = new List<PathResultNode>();
        AStarNode current = GetNode(grid, end);
        while (current != null)
        {
            path.Add(current.ToResultNode());
            current = current.previous;
        }
        path.Reverse();
        return path;
    }
    List<PathResultNode> GetDestinations(AStarNode[,] grid, float maxCost)
    {
        List<PathResultNode> destinations = new List<PathResultNode>();
        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                if (grid[i, j].g <= maxCost)
                {
                    destinations.Add(grid[i, j].ToResultNode());
                }
            }
        }
        return destinations;
    }

    AStarNode[] GetNeighbors(AStarNode[,] grid, Vector2Int pos)
    {
        if (pos.y % 2 == parity)
        {
            return new AStarNode[]{
                GetNode(grid, pos + Vector2Int.right),
                GetNode(grid, pos + Vector2Int.down),
                GetNode(grid, pos + Vector2Int.up),
                GetNode(grid, pos + Vector2Int.left + Vector2Int.up),
                GetNode(grid, pos + Vector2Int.left),
                GetNode(grid, pos + Vector2Int.left + Vector2Int.down),
            };
        }
        else
        {
            return new AStarNode[]{
                GetNode(grid, pos + Vector2Int.left),
                GetNode(grid, pos + Vector2Int.down),
                GetNode(grid, pos + Vector2Int.up),
                GetNode(grid, pos + Vector2Int.right + Vector2Int.up),
                GetNode(grid, pos + Vector2Int.right),
                GetNode(grid, pos + Vector2Int.right + Vector2Int.down),
            };
        }
    }

    AStarNode GetNode(AStarNode[,] grid, Vector2Int pos)
    {
        if (BoundsCheck(pos))
        {
            return grid[pos.x, pos.y].wall ? null : grid[pos.x, pos.y];
        }
        return null;
    }

    bool BoundsCheck(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < tiles.GetLength(0) && pos.y < tiles.GetLength(1);
    }

    float Heuristic(Vector2Int start, Vector2Int end)
    {
        Vector2 startPos = new Vector2(start.y, start.y % 2 == parity ? start.x : start.x + 0.5f);
        Vector2 endPos = new Vector2(end.y, end.y % 2 == parity ? end.x : end.x + 0.5f);
        return Vector2.Distance(startPos, endPos);
    }
    float Cost(CombatUnit unit, Vector2Int start, Vector2Int neighbor)
    {
        return tiles[neighbor.x, neighbor.y].GetCost(unit);
    }

    class AStarNode : FastPriorityQueueNode
    {
        public Vector2Int pos;
        public AStarNode previous;
        public bool open = false;
        public bool closed = false;
        public float f => g + h;
        public float g = Mathf.Infinity;
        public float h = 0;
        public MapTile tile;
        public bool wall;
        public void Reset(CombatUnit unit)
        {
            wall = true;
            if (tile != null)
            {
                wall = !tile.GetPassable(unit);
            }
            previous = null;
            open = false;
            closed = false;
            g = Mathf.Infinity;
            h = 0;
        }
        public PathResultNode ToResultNode()
        {
            return new PathResultNode() { pos = pos, cost = g };
        }
    }
}