using UnityEngine;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Game/Level Database", order = 1)]
public class LevelConfigDatabase : ScriptableObject
{
    public LevelConfig[] levels;

    public LevelConfig GetLevel(int index)
    {
        if (index < levels.Length)
            return levels[index];
        
        return levels[levels.Length - 1];
    }
}