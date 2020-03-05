using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugArrow : MonoBehaviour
{
    public Vector2 start;
    public Vector2 end;

    Vector2 largeArrow;
    Vector2 smallArrow;

    LineRenderer smallLine;
    LineRenderer largeLine;

    // Start is called before the first frame update
    void Start()
    {
        largeLine = GetComponent<LineRenderer>();
        smallLine = transform.Find("InnerLine").GetComponent<LineRenderer>();
        largeArrow = new Vector2(largeLine.GetPosition(0).x, largeLine.GetPosition(1).x) - Vector2.one * 0.15f;
        smallArrow = new Vector2(smallLine.GetPosition(0).x, smallLine.GetPosition(1).x) - Vector2.one * 0.15f;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 dir = (end - start).normalized * 1.2f;
        largeLine.SetPosition(0, end + dir * largeArrow.x);
        largeLine.SetPosition(1, end + dir * largeArrow.y);
        smallLine.SetPosition(0, end + dir * smallArrow.x);
        smallLine.SetPosition(1, end + dir * smallArrow.y);
    }

    public void SetLine(Vector2 start, Vector2 end)
    {
        this.start = start;
        this.end = end;
    }
}
