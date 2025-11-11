using UnityEngine;

public enum UpgradeType
{
    Creature,
    Land,
    World
}

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Game Data/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Basic Info")]
    public int index;
    public string upgradeName;
    public UpgradeType type;
    [TextArea(2, 4)] public string description;
    public int cost;
    public bool isPurchased = false;
}
