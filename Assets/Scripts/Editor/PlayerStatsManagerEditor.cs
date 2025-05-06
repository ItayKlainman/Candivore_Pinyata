using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerStatsManager))]
public class PlayerStatsManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Reset Player Progress (TEST ONLY)"))
        {
            ((PlayerStatsManager)target).ResetProgress();
        }
    }
}