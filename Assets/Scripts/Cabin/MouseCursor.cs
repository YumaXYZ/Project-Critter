using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCursor : MonoBehaviour
{
    public RectTransform cursorImageTransform;
    public Canvas canvas;

    void Start()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        Vector2 mousePosition = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            mousePosition,
            canvas.worldCamera, 
            out Vector2 localMousePosition
        );

        cursorImageTransform.anchoredPosition = localMousePosition;
    }
}
