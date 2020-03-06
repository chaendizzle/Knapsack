using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapHealthBar : MonoBehaviour
{
    public CombatUnit unit;
    RectTransform rect;
    RectTransform worldCanvas;

    void Awake()
    {
        worldCanvas = GameObject.FindGameObjectWithTag("WorldCanvas").GetComponent<RectTransform>();
        rect = GetComponent<RectTransform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    // follow attached unit
    void LateUpdate()
    {
        Vector2 output = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(worldCanvas, Camera.main.WorldToScreenPoint(unit.transform.position), Camera.main, out output);
        rect.anchoredPosition = output;
    }
}
