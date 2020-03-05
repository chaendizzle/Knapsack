using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLine : MonoBehaviour
{
    public Vector2 start;
    public Vector2 end;

    LineRenderer[] lines;

    // Start is called before the first frame update
    void Start()
    {
        lines = GetComponentsInChildren<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (LineRenderer line in lines)
        {
            line.SetPosition(0, start);
            line.SetPosition(1, end);
        }
    }

    public void SetLine(Vector2 start, Vector2 end)
    {
        this.start = start;
        this.end = end;
    }
}
