using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICombatAI
{
    bool isFinished { get; }
    void Turn();
    void Stop();
}

public static class CombatAIUtils
{

}