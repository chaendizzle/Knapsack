using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CombatActionType
{
    Move, Attack, Trade, Heal, Buff, Spell, Null
}

public interface ICombatAction
{
    CombatActionType type { get; }

    // initialize to use this unit
    void Reset(CombatUnit unit);

    // find a list of target tiles given a unit, given that it can move
    // to any of the tiles in 'from'
    void FindTargets(CombatUnit unit, List<Vector2Int> from);

    // list of targets at a position, used to choose target when action is chosen
    List<Vector2Int> GetTargets(Vector2Int at);

    // highlight and draw markers for player if necessary
    void Highlight();

    // check if the target is valid for this action
    bool CheckAction(Vector2Int target);

    // perform the given action
    IEnumerator Run(CombatUnit unit, Vector2Int moveTo, Vector2Int target);
}
