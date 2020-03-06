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

    void Awake()
    {
        combat = CombatFlow.GetInstance();
        cursor = CombatCursor.GetInstance();
        map = HexGridMap.GetCombatMap();
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

            // Select player
            List<PathResultNode> movable = null;
            Dictionary<Vector2Int, bool> movableDict = null;
            Dictionary<CombatUnit, List <Vector2Int>> attackable = null;
            Dictionary<Vector2Int, List<CombatUnit>> attackableUnits = null;
            HighlightTiles(selected, ref movable, ref movableDict, ref attackable, ref attackableUnits);

            // Player is selected
            Vector2Int lastMove = new Vector2Int(-1, -1);
            while (selected != null)
            {
                // draw movement path to cursor on move tile
                bool onMove = movableDict.ContainsKey(cursor.pos);
                bool onAttack = attackable.Keys.Select(x => x.pos).Contains(cursor.pos);
                if (onMove)
                {
                    lastMove = cursor.pos;
                    map.SetDebugPath(map.pathfinder.FindPath(selected, selected.pos, cursor.pos));
                }
                // draw movement path to cursor on attack tile
                else if (onAttack)
                {
                    // if we can attack from the last move position, keep it
                    // if not, change move path
                    var attackPoints = attackable[map.units[cursor.pos.x, cursor.pos.y]];
                    if (!attackPoints.Contains(lastMove))
                    {
                        lastMove = attackPoints[0];
                        map.SetDebugPath(map.pathfinder.FindPath(selected, selected.pos, attackPoints[0]));
                    }
                }
                // check for confirm to move or attack
                if (Input.GetButtonDown("Submit"))
                {
                    if (onMove)
                    {
                        // move to lastMove
                        map.ClearDebugTiles();
                        map.ClearDebugLines();
                        yield return selected.Move(map.pathfinder.FindPath(selected, selected.pos, lastMove));
                        HighlightTiles(selected, ref movable, ref movableDict, ref attackable, ref attackableUnits);
                        lastMove = new Vector2Int(-1, -1);
                    }
                    else if (onAttack)
                    {
                        // move to lastMove then attack
                        map.ClearDebugTiles();
                        map.ClearDebugLines();
                        if (lastMove.x >= 0)
                        {
                            yield return selected.Move(map.pathfinder.FindPath(selected, selected.pos, lastMove));
                        }
                        yield return selected.Attack(map.units[cursor.pos.x, cursor.pos.y]);
                        selected.hasTurn = false;
                        selected = null;
                    }
                }
                // check for cancel and deselect
                if (Input.GetButtonDown("Cancel"))
                {
                    selected = null;
                }
                yield return null;
            }
            // Deselect
            map.ClearDebugLines();
            map.ClearDebugTiles();
        }
    }

    // highlight a selected unit's tiles
    void HighlightTiles(CombatUnit selected, ref List<PathResultNode> movable,
        ref Dictionary<Vector2Int, bool> movableDict,
        ref Dictionary<CombatUnit, List<Vector2Int>> attackable,
        ref Dictionary<Vector2Int, List<CombatUnit>> attackableUnits)
    {
        map.ClearDebugTiles();
        map.ClearDebugLines();
        // highlight movable tiles blue
        movable = map.pathfinder.FindDestinations(selected, selected.pos, selected.currentMovement);
        movableDict = new Dictionary<Vector2Int, bool>();
        foreach (Vector2Int pos in movable.Select(x => x.pos))
        {
            if (map.units[pos.x, pos.y] == null)
            {
                map.SetDebugTile(pos, Color.blue);
                movableDict[pos] = true;
            }
        }
        // highlight attackable tiles red
        List<CombatUnit> enemies = combat.GetOpponents(selected.combatSide);
        attackable = new Dictionary<CombatUnit, List<Vector2Int>>();
        attackableUnits = new Dictionary<Vector2Int, List<CombatUnit>>();
        foreach (Vector2Int pos in movable.Select(x => x.pos).Concat(new List<Vector2Int>() { selected.pos }))
        {
            foreach (CombatUnit enemy in enemies)
            {
                if (selected.CanAttack(pos, enemy))
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
        foreach (CombatUnit enemy in attackable.Keys)
        {
            map.SetDebugTile(enemy.pos, Color.red);
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