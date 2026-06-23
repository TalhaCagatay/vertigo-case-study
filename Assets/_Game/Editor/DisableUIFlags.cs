using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class DisableUIFlags
{
    [MenuItem("Vertigo/UI/Disable All Raycast Target && Maskable")]
    public static void DisableFlags()
    {
        int modifiedCount = 0;

        var graphics = Object.FindObjectsByType<Graphic>
            (
             FindObjectsInactive.Include,
             FindObjectsSortMode.None
            );

        foreach (var graphic in graphics)
        {
            bool changed = false;

            Undo.RecordObject(graphic, "Disable UI Flags");

            if (graphic.raycastTarget)
            {
                graphic.raycastTarget = false;
                changed               = true;
            }

            if (graphic is MaskableGraphic maskableGraphic &&
                maskableGraphic.maskable)
            {
                maskableGraphic.maskable = false;
                changed                  = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(graphic);
                modifiedCount++;
            }
        }

        Debug.Log($"Updated {modifiedCount} UI components.");
    }
}