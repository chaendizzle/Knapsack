using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackCombatAction : MonoBehaviour, ICombatAction
{
    List<CombatUnit> enemies;
    Dictionary<CombatUnit, List<Vector2Int>> attackable;
    Dictionary<Vector2Int, List<CombatUnit>> attackableUnits;
    HexGridMap map;
    CombatFlow combat;

    public CombatActionType type { get; private set; }

    void Awake()
    {
        type = CombatActionType.Attack;
        map = HexGridMap.GetCombatMap();
        combat = CombatFlow.GetInstance();
    }

    public void FindTargets(CombatUnit unit, List<Vector2Int> from)
    {
        enemies = combat.GetOpponents(unit.combatSide);
        attackable = new Dictionary<CombatUnit, List<Vector2Int>>();
        attackableUnits = new Dictionary<Vector2Int, List<CombatUnit>>();
        foreach (Vector2Int pos in from)
        {
            foreach (CombatUnit enemy in enemies)
            {
                if (unit.CanPerformAction(CombatActionType.Attack, pos, enemy))
                {
                    if (!attackable.ContainsKey(enemy))
                    {
                        attackable[enemy] = new List<Vector2Int>();
                    }
                    if (!attackableUnits.ContainsKey(pos))
                    {
                        attackableUnits[pos] = new List<CombatUnit>();
                    }
                    attackable[enemy].Add(pos);
                    attackableUnits[pos].Add(enemy);
                }
            }
        }
    }

    public void Highlight()
    {
        foreach (CombatUnit enemy in attackable.Keys)
        {
            map.SetDebugTile(enemy.pos, Color.red);
        }
    }

    public bool CheckAction(Vector2Int target)
    {
        return attackable.Keys.Any(x => x.pos == target);
    }

    public IEnumerator Run(CombatUnit unit, Vector2Int moveTo, Vector2Int target)
    {
        yield return unit.Move(map.pathfinder.FindPath(unit, unit.pos, target));
    }
}