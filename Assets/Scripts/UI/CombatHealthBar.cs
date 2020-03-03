using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatHealthBar : MonoBehaviour
{
    public int maxHP;
    public int currentHP;
    public int HPPerBar;
    // width of fill bar at full health
    public float fillMax;
    // width of fill bar at one health
    public float fillMin;

    public RectTransform frame;
    public RectTransform fill;

    // Start is called before the first frame update
    void Start()
    {
        SetMaxHealth(18);
        SetHealth(13);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetHealth(int health)
    {
        float oneBarWidth = (fillMax - fillMin) / (maxHP - HPPerBar);
        fill.sizeDelta = new Vector2(fillMin + oneBarWidth * (health - HPPerBar), fill.sizeDelta.y);
        currentHP = health;

    }
    public void SetMaxHealth(int maxHealth)
    {
        float oneBarWidth = (fillMax - fillMin) / (maxHP - HPPerBar);
        fillMax += oneBarWidth * (maxHealth - maxHP);
        frame.sizeDelta = new Vector2(frame.sizeDelta.x + oneBarWidth * (maxHealth - maxHP), frame.sizeDelta.y);
        maxHP = maxHealth;
        
    }
}
