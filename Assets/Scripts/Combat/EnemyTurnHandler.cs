using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnHandler : MonoBehaviour, ITurnHandler
{
    public bool isFinished { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Turn(List<CombatUnit> units)
    {
        isFinished = false;
        // TODO: choose enemy turn order
        // run enemy AI
        RunEnemyTurn(units);
    }

    IEnumerator RunEnemyTurn(List<CombatUnit> units)
    {
        // wait until there are no units or they are all done with their turn
        foreach (CombatUnit unit in units)
        {
            // check for existing unit
            if (unit == null)
            {
                continue;
            }
            // if AI does not exist, end turn
            ICombatAI ai = unit.GetComponent<ICombatAI>();
            if (ai == null)
            {
                unit.hasTurn = false;
            }
            // wait until AI is done running
            while (!ai.isFinished)
            {
                // stop if we are interrupted
                if (isFinished)
                {
                    ai.Stop();
                    yield break;
                }
                // otherwise, wait
                yield return null;
            }
            unit.hasTurn = false;
        }
        // finish
        isFinished = true;
    }

    public void Stop()
    {
        isFinished = true;
    }
}
