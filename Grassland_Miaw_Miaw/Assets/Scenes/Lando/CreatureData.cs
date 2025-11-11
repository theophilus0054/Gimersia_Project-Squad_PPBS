using UnityEngine;

[System.Serializable]
public class EffectData
{
    public Effect effectType;
    [Range(0, 100)] public float chanceToApply; // % chance to apply
}

public enum Effect
{
    None,
    Slow,
    Bleed
}

public enum CreatureType
{
    Unagi,
    Crab,
    Jelly,
    Turtle
}

[CreateAssetMenu(fileName = "NewCreature", menuName = "Game Data/Creature Data")]
public class CreatureData : ScriptableObject
{
    [Header("Basic Info")]
    public int index;
    public string creatureName;
    public CreatureType type;
    public string range;
    public string damage;
    [TextArea(2, 4)] public string description;
    public int cost;

    [Header("Stats")]
    public float hp;
    public float atk;
    public float atkSpeed;
    public float projectileSpeed;
    public int tileRange;

    [Header("Evolution")]
    [Tooltip("References to creatures this one can evolve into")]
    public CreatureData[] possibleEvolutions;
    public int tier = 1;

    [Header("Effects")]
    public EffectData[] effects; // includes chanceToApply

    [Header("Prefab Reference")]
    public GameObject summonPrefab;
    public GameObject displayPrefab;

    public override string ToString()
    {
        string evoList = (possibleEvolutions != null && possibleEvolutions.Length > 0)
            ? string.Join(", ", System.Array.ConvertAll(possibleEvolutions, x => x != null ? x.creatureName : "null"))
            : "None";

        string effectList = (effects != null && effects.Length > 0)
            ? string.Join(", ", System.Array.ConvertAll(effects, e => e != null ? $"{e.effectType}({e.chanceToApply}%)" : "null"))
            : "None";

        return $"[{index}] {creatureName} (Tier {tier}) | Cost:{cost} | HP:{hp} ATK:{atk} SPD:{atkSpeed} ProjSPD:{projectileSpeed} Range:{tileRange} | Effects: {effectList} | Evolves to: {evoList}";
    }
}
