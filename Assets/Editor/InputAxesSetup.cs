using UnityEngine;
using UnityEditor;

/// <summary>
/// One-time, idempotent setup for the free-movement key rebind:
///   - New Horizontal1/Horizontal2 axes on the arrow-key / A-D pairs that used to be Rotation1/Rotation2.
///   - Rotation1/Rotation2 rebound to I/O (P1) and R/T (P2).
/// InputManager.asset is binary, so this edits it the way the Unity Editor itself would,
/// via SerializedObject, instead of hand-editing the file.
/// </summary>
public static class InputAxesSetup
{
    [MenuItem("Parashoot/Setup Free-Movement Input Axes")]
    public static void ApplyAxes()
    {
        Object asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
        SerializedObject so = new SerializedObject(asset);
        SerializedProperty axes = so.FindProperty("m_Axes");

        UpsertAxis(axes, "Horizontal1", "left", "right", "Vertical1");
        UpsertAxis(axes, "Horizontal2", "a", "d", "Vertical2");
        RebindButtons(axes, "Rotation1", "i", "o");
        RebindButtons(axes, "Rotation2", "r", "t");

        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();

        LogAxis(axes, "Horizontal1");
        LogAxis(axes, "Horizontal2");
        LogAxis(axes, "Rotation1");
        LogAxis(axes, "Rotation2");
    }

    private static SerializedProperty FindAxis(SerializedProperty axes, string name)
    {
        for (int i = 0; i < axes.arraySize; i++)
        {
            SerializedProperty entry = axes.GetArrayElementAtIndex(i);
            if (entry.FindPropertyRelative("m_Name").stringValue == name)
                return entry;
        }
        return null;
    }

    private static void RebindButtons(SerializedProperty axes, string name, string negativeButton, string positiveButton)
    {
        SerializedProperty entry = FindAxis(axes, name);
        if (entry == null)
        {
            Debug.LogError($"InputAxesSetup: axis '{name}' not found, cannot rebind.");
            return;
        }
        entry.FindPropertyRelative("negativeButton").stringValue = negativeButton;
        entry.FindPropertyRelative("positiveButton").stringValue = positiveButton;
    }

    private static void UpsertAxis(SerializedProperty axes, string name, string negativeButton, string positiveButton, string templateName)
    {
        SerializedProperty entry = FindAxis(axes, name);
        if (entry == null)
        {
            SerializedProperty template = FindAxis(axes, templateName);
            if (template == null)
            {
                Debug.LogError($"InputAxesSetup: template axis '{templateName}' not found, cannot create '{name}'.");
                return;
            }

            int newIndex = axes.arraySize;
            axes.InsertArrayElementAtIndex(newIndex);
            entry = axes.GetArrayElementAtIndex(newIndex);

            // Clone tuning fields from the template so the new axis feels identical.
            entry.FindPropertyRelative("descriptiveName").stringValue = "";
            entry.FindPropertyRelative("descriptiveNegativeName").stringValue = "";
            entry.FindPropertyRelative("altNegativeButton").stringValue = "";
            entry.FindPropertyRelative("altPositiveButton").stringValue = "";
            entry.FindPropertyRelative("gravity").floatValue = template.FindPropertyRelative("gravity").floatValue;
            entry.FindPropertyRelative("dead").floatValue = template.FindPropertyRelative("dead").floatValue;
            entry.FindPropertyRelative("sensitivity").floatValue = template.FindPropertyRelative("sensitivity").floatValue;
            entry.FindPropertyRelative("snap").boolValue = template.FindPropertyRelative("snap").boolValue;
            entry.FindPropertyRelative("invert").boolValue = template.FindPropertyRelative("invert").boolValue;
            entry.FindPropertyRelative("type").intValue = template.FindPropertyRelative("type").intValue;
            entry.FindPropertyRelative("axis").intValue = template.FindPropertyRelative("axis").intValue;
            entry.FindPropertyRelative("joyNum").intValue = template.FindPropertyRelative("joyNum").intValue;
        }

        entry.FindPropertyRelative("m_Name").stringValue = name;
        entry.FindPropertyRelative("negativeButton").stringValue = negativeButton;
        entry.FindPropertyRelative("positiveButton").stringValue = positiveButton;
    }

    private static void LogAxis(SerializedProperty axes, string name)
    {
        SerializedProperty entry = FindAxis(axes, name);
        if (entry == null)
        {
            Debug.LogError($"InputAxesSetup: '{name}' missing after apply.");
            return;
        }
        Debug.Log($"InputAxesSetup: {name} negative='{entry.FindPropertyRelative("negativeButton").stringValue}' " +
                   $"positive='{entry.FindPropertyRelative("positiveButton").stringValue}' " +
                   $"type={entry.FindPropertyRelative("type").intValue} " +
                   $"gravity={entry.FindPropertyRelative("gravity").floatValue} " +
                   $"sensitivity={entry.FindPropertyRelative("sensitivity").floatValue}");
    }
}
