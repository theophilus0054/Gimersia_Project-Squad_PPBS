using UnityEngine;

[CreateAssetMenu(fileName = "StageDatabase", menuName = "GameData/Stage Database")]
public class StageDatabase : ScriptableObject
{
    public StageSummon[] stages;
}
