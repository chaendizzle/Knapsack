using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MovementType
{
    GROUND, AIR
}

public enum CombatSide
{
    NULL, PLAYER, OTHER, ENEMY
}

public class CombatUnit : MonoBehaviour
{
    public CombatSide combatSide;

    public MovementType movementType;
    public float maxMovement;
    public float currentMovement;

    public bool hasTurn { get; set; }

    public Vector2Int pos;
    // most units are size one
    public Vector2Int size = Vector2Int.one;

    HexGridMap map;
    Vector2 moveTarget;
    float moveAnimSpd = 5f;

    Coroutine move;
    Coroutine attack;

    void Awake()
    {
        map = HexGridMap.GetCombatMap();
    }

    // Start is called before the first frame update
    void Start()
    {
        // draw health bar under player
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 p = Vector2.MoveTowards(transform.position, moveTarget, moveAnimSpd * Time.deltaTime);
        transform.position = new Vector3(p.x, p.y, transform.position.z);
    }

    public void BeginCombat()
    {
        pos = map.WorldToArrayPos(transform.position);
        moveTarget = map.ArrayPosToWorld(pos);
    }

    public void BeginTurn()
    {
        currentMovement = maxMovement;
        hasTurn = true;
    }

    public bool isMoving = false;
    // moves as far as possible along the given path.
    public Coroutine Move(List<PathResultNode> path)
    {
        isMoving = true;
        // copy to avoid modifying original path
        path = new List<PathResultNode>(path);
        // remove all nodes greater than max movement
        for (int i = 0; i < path.Count; i++)
        {
            if (path[i].cost > currentMovement)
            {
                path.RemoveAt(i);
                i--;
            }
        }
        // remove nodes from end that would end on other units
        for (int i = path.Count - 1; i >= 0; i++)
        {
            if (map.units[path[i].pos.x, path[i].pos.y] != null)
            {
                path.RemoveAt(i);
                i--;
            }
            // if we find one open space, go there
            else
            {
                break;
            }
        }
        // kick off move coroutine
        move = StartCoroutine(MoveAnimation(path));
        return move;
    }
    IEnumerator MoveAnimation(List<PathResultNode> path)
    {
        // move to each waypoint in the path
        foreach (Vector2Int pos in path.Select(x => x.pos))
        {
            // set target
            moveTarget = map.ArrayPosToWorld(pos);
            // wait until reached
            while (Vector2.Distance(moveTarget, transform.position) > 0.01f)
            {
                yield return null;
            }
        }
        // finish
        isMoving = false;
        move = null;
    }

    public bool isAttacking = false;
    public bool CanAttack(CombatUnit target)
    {
        // TODO: add weapon types
        // check for one tile away
        Vector2Int d = target.pos - pos;
        return Mathf.Abs(d.x) == 1 ^ Mathf.Abs(d.y) == 1;
    }
    public Coroutine Attack(CombatUnit target)
    {
        isAttacking = true;
        // run attack animation
        // deal damage
        // wait for attack animation to finish
        attack = StartCoroutine(AttackAnimation(target));
        return attack;
    }
    IEnumerator AttackAnimation(CombatUnit target)
    {
        // run attack animation
        yield return new WaitForSeconds(1f);
        // deal damage
        target.TakeDamage(10);
        // wait for attack animation to finish
        yield return new WaitForSeconds(1f);
        // finish
        isAttacking = false;
        attack = null;
    }

    public Coroutine TakeDamage(int amount)
    {
        // create little damage number that floats up
        return null;
    }

    public void Stop()
    {
        if (move != null)
        {
            StopCoroutine(move);
        }
        if (attack != null)
        {
            StopCoroutine(attack);
        }
    }
}
