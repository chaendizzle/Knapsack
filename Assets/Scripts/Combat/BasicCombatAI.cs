using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// example combat AI
// finds nearest enemy and melee attacks them
// does not work with ranged units
// does nothing if no enemies are accessible
public class BasicCombatAI : MonoBehaviour, ICombatAI
{
    public bool isFinished { get; private set; }

    public CombatSide side;

    CombatUnit unit;
    CombatFlow combat;
    HexGridMap map;

    void Awake()
    {
        combat = CombatFlow.GetInstance();
        map = HexGridMap.GetCombatMap();
        unit = GetComponent<CombatUnit>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Coroutine ai;
    public void Turn()
    {
        isFinished = false;
        ai = StartCoroutine(RunAI());
    }

    // stop all coroutines
    public void Stop()
    {
        isFinished = true;
        if (ai != null)
        {
            StopCoroutine(ai);
        }
        unit.Stop();
    }
    
    IEnumerator RunAI()
    {
        // choose target (closest opponent)
        List<CombatUnit> opponents = new List<CombatUnit>(combat.GetOpponents(side));
        float dist = float.MaxValue;
        CombatUnit closest = null;
        List<PathResultNode> closestPath = null;
        foreach (CombatUnit opponent in opponents)
        {
            // check that path exists
            List<PathResultNode> path = map.pathfinder.FindPath(unit, unit.pos, opponent.pos);
            if (path == null)
            {
                continue;
            }
            // find closest opponent
            if (path[path.Count - 1].cost < dist)
            {
                closest = opponent;
                dist = path[path.Count - 1].cost;
                closestPath = path;
            }
        }
        // if there are targets
        if (closest != null)
        {
            // move towards opponent
            yield return unit.Move(closestPath);

            // attack if able
            if (unit.CanAttack(closest))
            {
                yield return unit.Attack(closest);
            }
        }
        // finish
        ai = null;
        isFinished = true;
    }
}
