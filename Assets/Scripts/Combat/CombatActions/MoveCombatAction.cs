using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveCombatAction : MonoBehaviour, ICombatAction
{
    List<Vector2Int> movable;
    Dictionary<Vector2Int, bool> movableDict;
    HexGridMap map;

    Vector2Int oldPos;
    float oldMovement;

    public CombatActionType type { get; private set; }

    void Awake()
    {
        type = CombatActionType.Move;
        map = HexGridMap.GetCombatMap();
    }

    public void FindTargets(CombatUnit unit, List<Vector2Int> from)
    {
        movable = map.pathfinder.FindDestinations(unit, from[0], unit.currentMovement).Select(x => x.pos).ToList();
        movableDict = new Dictionary<Vector2Int, bool>();
        foreach (Vector2Int pos in movable)
        {
            if (map.units[pos.x, pos.y] == null)
            {

                movableDict[pos] = true;
            }
        }
    }
    public List<Vector2Int> GetTargets()
    {
        return movable;
    }

    public void Highlight()
    {
        foreach (Vector2Int p in movable)
        {
            map.SetDebugTile(p, Color.blue);
        }
    }

    public bool CheckAction(Vector2Int target)
    {
        return movable.Contains(target);
    }

    public IEnumerator Run(CombatUnit unit, Vector2Int moveTo, Vector2Int target)
    {
        oldPos = unit.pos;
        oldMovement = unit.currentMovement;
        yield return unit.Move(map.pathfinder.FindPath(unit, unit.pos, target));
    }

    public void Undo(CombatUnit unit)
    {
        unit.SetPos(oldPos);
        unit.currentMovement = oldMovement;
    }
}