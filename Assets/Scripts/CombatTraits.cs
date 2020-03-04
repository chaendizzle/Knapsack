﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// stores all traits and counters for this run
public class CombatTraits : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[Flags]
public enum DamageType
{
    Heat, Cold, Electric, Acid, Light, Psychic, Holy, Unholy,
    Piercing, Slashing, Bludgeoning
}

[Flags]
public enum DamageDeliveryType
{
    Fire, Water, Earth, Weapon, Material, Magic
}

[Flags]
public enum WeaponType
{
    Sword, Spear, Dagger
}

[Flags]
public enum MaterialType
{
    Iron, Steel, Brass, Bronze, Copper, Gold, Silver, Wood
}

[Flags]
public enum BiomeType
{
    Mountain, Plains, Beach, Swamp
}

public enum MoonType
{
    Full, New, Crescent, Gibbous, Eclipse
}

public enum WeatherType
{
    Sun, Cloud, Rain, Storm
}
