using UnityEngine;

public enum RewardType
{
    Coins,
    UnlockEvolution
}

[CreateAssetMenu(fileName = "StageRewardData", menuName = "Game/Stage Reward Data")]
public class StageRewardData : ScriptableObject
{
    public RewardEntry[] rewards;
}

[System.Serializable]
public class RewardEntry
{
    public int stageNumber;

    public RewardType type;

    public int value;       // coin amount / evolutionIndex
    public int splitCount;  // for coins only
}
