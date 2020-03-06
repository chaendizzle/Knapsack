using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerTurnHandler : MonoBehaviour, ITurnHandler
{
    public bool isFinished { get; private set; }

    CombatCursor cursor;
    HexGridMap map;
    CombatFlow combat;

    public List<MonoBehaviour> combatActionList;
    Dictionary<CombatActionType, ICombatAction> combatActions = new Dictionary<CombatActionType, ICombatAction>();

    void Awake()
    {
        combat = CombatFlow.GetInstance();
        cursor = CombatCursor.GetInstance();
        map = HexGridMap.GetCombatMap();
        foreach (MonoBehaviour c in combatActionList)
        {
            ICombatAction combatAction = c as ICombatAction;
            if (c == null)
            {
                throw new InvalidOperationException("Invalid CombatAction that does not inherit from ICombatAction in Combat Actions list.");
            }
            combatActions[combatAction.type] = combatAction;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    Coroutine playerUI;
    public void Turn(List<CombatUnit> units)
    {
        isFinished = false;
        // activate player UI, then let the player do their work
        playerUI = StartCoroutine(PlayerUI(units));
        // finish when all players have no actions
        StartCoroutine(FinishPlayerTurn(units));
    }

    int scrollIndex = 0;
    IEnumerator PlayerUI(List<CombatUnit> units)
    {
        while (true)
        {
            // No player selected
            CombatUnit selected = null;
            while (selected == null)
            {
                // scroll wheel or A key cycles through players
                if (Input.GetKeyDown(KeyCode.A) || Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    cursor.pos = units[scrollIndex].pos;
                    scrollIndex = (scrollIndex + 1) % units.Count;
                }
                if (Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    cursor.pos = units[scrollIndex].pos;
                    scrollIndex = (scrollIndex - 1 + units.Count) % units.Count;
                }
                // TODO: go to combat menu
                if (Input.GetButtonDown("Cancel"))
                {

                }
                // check for selecting a player
                if (Input.GetButtonDown("Submit"))
                {
                    if (map.units[cursor.pos.x, cursor.pos.y] != null)
                    {
                        CombatUnit unit = map.units[cursor.pos.x, cursor.pos.y];
                        if (units.Contains(unit) && unit.hasTurn)
                        {
                            selected = map.units[cursor.pos.x, cursor.pos.y];
                        }
                    }
                }
                yield return null;
            }

            // Select player by highlighting all targets
            MoveCombatAction moveAction = (MoveCombatAction)combatActions[CombatActionType.Move];
            moveAction.FindTargets(selected, new List<Vector2Int>() { selected.pos });
            moveAction.Highlight();
            foreach (CombatActionType action in combatActions.Keys)
            {
                if (action != CombatActionType.Move)
                {
                    combatActions[action].FindTargets(selected, moveAction.GetTargets());
                    combatActions[action].Highlight();
                }
            }

            // Player is selected
            Vector2Int lastMove = new Vector2Int(-1, -1);
            Stack<CombatActionType> oldActions = new Stack<CombatActionType>();
            while (selected != null)
            {
                // TODO: pull up combat actions menu
                cursor.DisableControls();
                cursor.DisableCursor();
                // wait for it to finish

                // check for confirm to move or attack
                if (Input.GetButtonDown("Submit"))
                {

                }
                // check for cancel and deselect
                if (Input.GetButtonDown("Cancel"))
                {

                }
                yield return null;
            }
            // Deselect
            map.ClearDebugLines();
            map.ClearDebugTiles();
        }
    }

    IEnumerator FinishPlayerTurn(List<CombatUnit> units)
    {
        // wait until there are no units or they are all done with their turn
        while (units.Count > 0 && units.Any(x => x != null && x.hasTurn))
        {
            yield return null;
        }
        // finish
        StopCoroutine(playerUI);
        isFinished = true;
    }

    public void Stop()
    {
        // TODO: deactivate player UI
        isFinished = true;
    }
}

public interface ITurnHandler
{
    bool isFinished { get; }
    void Turn(List<CombatUnit> units);
    void Stop();
}