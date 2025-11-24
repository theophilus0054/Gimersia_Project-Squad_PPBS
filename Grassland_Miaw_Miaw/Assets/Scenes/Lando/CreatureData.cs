using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class EffectData
{
    public Effect effectType;
    [Range(0, 100)] public float chanceToApply; // % chance to apply

    // Override Equals dan GetHashCode agar HashSet bisa membandingkan berdasarkan effectType
    public override bool Equals(object obj)
    {
        if (obj is EffectData other)
            return effectType == other.effectType;
        return false;
    }

    public override int GetHashCode()
    {
        return effectType.GetHashCode();
    }
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
    Puffer,
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
    public EffectData[] effectsArray; // Array untuk Unity Inspector
    
    // HashSet untuk memastikan tidak ada duplikat
    private HashSet<EffectData> effectsSet;
    
    public HashSet<EffectData> Effects
    {
        get
        {
            if (effectsSet == null)
            {
                effectsSet = new HashSet<EffectData>();
                if (effectsArray != null)
                {
                    foreach (var effect in effectsArray)
                    {
                        if (effect != null && effect.effectType != Effect.None)
                        {
                            effectsSet.Add(effect);
                        }
                    }
                }
            }
            return effectsSet;
        }
    }

    [Header("Prefab Reference")]
    public GameObject summonPrefab;
    public GameObject displayPrefab;

    // Method untuk menambah effect dengan validasi
    public bool AddEffect(EffectData newEffect)
    {
        if (newEffect == null || newEffect.effectType == Effect.None)
            return false;

        return Effects.Add(newEffect);
    }

    // Method untuk menghapus effect
    public bool RemoveEffect(Effect effectType)
    {
        var effectToRemove = Effects.FirstOrDefault(e => e.effectType == effectType);
        if (effectToRemove != null)
        {
            return Effects.Remove(effectToRemove);
        }
        return false;
    }

    // Sinkronisasi dari Set ke Array (untuk Inspector)
    public void SyncEffectsToArray()
    {
        if (effectsSet != null)
        {
            effectsArray = effectsSet.ToArray();
        }
    }

    // Dipanggil saat ScriptableObject di-load
    private void OnEnable()
    {
        effectsSet = null; // Reset agar di-rebuild dari array
    }

    public override string ToString()
    {
        string evoList = (possibleEvolutions != null && possibleEvolutions.Length > 0)
            ? string.Join(", ", System.Array.ConvertAll(possibleEvolutions, x => x != null ? x.creatureName : "null"))
            : "None";

        string effectList = (Effects != null && Effects.Count > 0)
            ? string.Join(", ", Effects.Select(e => e != null ? $"{e.effectType}({e.chanceToApply}%)" : "null"))
            : "None";

        return $"[{index}] {creatureName} (Tier {tier}) | Cost:{cost} | HP:{hp} ATK:{atk} SPD:{atkSpeed} ProjSPD:{projectileSpeed} Range:{tileRange} | Effects: {effectList} | Evolves to: {evoList}";
    }
}