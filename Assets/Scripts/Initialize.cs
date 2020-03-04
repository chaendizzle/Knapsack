using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initialize : MonoBehaviour
{
    public HexGridMap map;
    public CombatCursor cursor;

    // control initialize order of control objects
    void Awake()
    {
        if (map != null)
        {
            map.Initialize();
        }
        if (cursor != null)
        {
            cursor.Initialize();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
