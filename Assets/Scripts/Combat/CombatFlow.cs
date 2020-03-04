using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatFlow : MonoBehaviour
{
    public List<CombatUnit> players;
    public List<CombatUnit> others;
    public List<CombatUnit> enemies;

    PlayerTurnHandler playerTurn;
    ITurnHandler otherTurn;
    EnemyTurnHandler enemyTurn;

    bool combatActive = false;

    HexGridMap map;

    public static CombatFlow GetInstance()
    {
        return GameObject.FindGameObjectWithTag("CombatController").GetComponent<CombatFlow>();
    }

    void Awake()
    {
        map = HexGridMap.GetCombatMap();
        playerTurn = GetComponent<PlayerTurnHandler>();
        otherTurn = null;
        enemyTurn = GetComponent<EnemyTurnHandler>();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCombat();
    }

    // Update is called once per frame
    Coroutine cycleCoroutine;
    void Update()
    {
        if (combatActive && cycleCoroutine == null)
        {
            cycleCoroutine = StartCoroutine(CombatCycle());
        }
    }

    public void StartCombat()
    {
        combatActive = true;
        // set unit placements on map
        foreach (CombatUnit unit in players)
        {
            unit.BeginCombat();
            map.units[unit.pos.x, unit.pos.y] = CombatSide.PLAYER;
        }
        foreach (CombatUnit unit in others)
        {
            unit.BeginCombat();
            map.units[unit.pos.x, unit.pos.y] = CombatSide.OTHER;
        }
        foreach (CombatUnit unit in enemies)
        {
            unit.BeginCombat();
            map.units[unit.pos.x, unit.pos.y] = CombatSide.ENEMY;
        }
    }

    public void StopCombat()
    {
        combatActive = false;
        // stop all combat coroutines
        if (cycleCoroutine != null)
        {
            StopCoroutine(cycleCoroutine);
            cycleCoroutine = null;
        }
        if (turnCoroutine != null)
        {
            StopCoroutine(turnCoroutine);
            turnCoroutine = null;
        }
        // stop all handler coroutines
        foreach (CombatSide side in Enum.GetValues(typeof(CombatSide)))
        {
            GetHandler(side).Stop();
        }
    }

    // take turn for all sides
    Coroutine turnCoroutine;
    IEnumerator CombatCycle()
    {
        foreach (CombatSide side in Enum.GetValues(typeof(CombatSide)))
        {
            if (side == CombatSide.NULL)
            {
                continue;
            }
            turnCoroutine = StartCoroutine(Turn(side));
            yield return turnCoroutine;
            turnCoroutine = null;
        }
        cycleCoroutine = null;
    }

    // take turn for one side
    IEnumerator Turn(CombatSide side)
    {
        List<CombatUnit> units = GetUnits(side);
        // update combat sides
        BeginTurns(units, side);
        // if handler does not exist, skip turn
        ITurnHandler handler = GetHandler(side);
        if (handler == null)
        {
            yield break;
        }
        // if handler exists, take turn
        handler.Turn(units);
        while (!handler.isFinished)
        {
            yield return null;
        }
    }

    void BeginTurns(List<CombatUnit> units, CombatSide side)
    {
        foreach (CombatUnit unit in units)
        {
            unit.combatSide = side;
            unit.BeginTurn();
        }
    }

    public List<CombatUnit> GetUnits(CombatSide side)
    {
        switch (side)
        {
            case CombatSide.PLAYER:
                return players;
            case CombatSide.OTHER:
                return others;
            case CombatSide.ENEMY:
                return enemies;
        }
        return null;
    }

    public List<CombatUnit> GetOpponents(CombatSide side)
    {
        switch (side)
        {
            case CombatSide.PLAYER:
                return enemies;
            case CombatSide.OTHER:
                return enemies;
            case CombatSide.ENEMY:
                return players;
        }
        return null;
    }

    public ITurnHandler GetHandler(CombatSide side)
    {
        switch (side)
        {
            case CombatSide.PLAYER:
                return playerTurn;
            case CombatSide.OTHER:
                return otherTurn;
            case CombatSide.ENEMY:
                return enemyTurn;
        }
        return null;
    }
}
