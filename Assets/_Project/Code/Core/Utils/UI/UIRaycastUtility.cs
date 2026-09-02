using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class UIRaycastUtility
{
    private static readonly List<RaycastResult> _results = new();

    public static bool IsPointerOver(GameObject target, Vector2 screenPos)
    {
        var eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPos
        };

        _results.Clear();
        EventSystem.current.RaycastAll(eventData, _results);

        foreach (var r in _results)
        {
            if (r.gameObject == target || r.gameObject.transform.IsChildOf(target.transform))
                return true;
        }

        return false;
    }
}