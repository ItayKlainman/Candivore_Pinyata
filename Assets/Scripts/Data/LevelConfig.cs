using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/Level Config", order = 0)]
public class LevelConfig : ScriptableObject
{
    public string levelName = "Level 1";
    public GameObject backgroundPrefab;
    public float pinataHP = 100f;
    public float timeLimit = 10f;

    // Optional: Add pinata prefab override, music, spawn pattern, etc.
}