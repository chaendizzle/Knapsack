using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerTurnHandler : MonoBehaviour, ITurnHandler
{
    public bool isFinished { get; private set; }

    CombatCursor cursor;

    void Awake()
    {
        cursor = CombatCursor.GetInstance();
    }

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
        // activate player UI, then let the player do their work
        StartCoroutine(PlayerUI(units));
        // finish when all players have no actions
        StartCoroutine(FinishPlayerTurn(units));
    }

    int scrollIndex = 0;
    IEnumerator PlayerUI(List<CombatUnit> units)
    {
        // No player selected
        CombatUnit playerSelected = null;
        while (playerSelected == null)
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
            yield return null;
        }
        yield break;
    }

    IEnumerator FinishPlayerTurn(List<CombatUnit> units)
    {
        // wait until there are no units or they are all done with their turn
        while (units.Count > 0 && units.Any(x => x != null && x.hasTurn))
        {
            yield return null;
        }
        // finish
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