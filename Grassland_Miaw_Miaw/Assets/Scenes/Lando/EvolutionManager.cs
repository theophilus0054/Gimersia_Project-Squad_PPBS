using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public enum Effect
{
    None,
    Burn,
    Freeze,
    Heal,
    Poison,
    Shield,
    SpeedBoost
}

[System.Serializable]
public class EvolutionData
{
    public int index;
    public string name;          // ✅ added name
    public int tier;             // ✅ added tier
    public int[] possibleEvolutions;
    public int baseCost;

    public float hp;
    public float atk;
    public float atkSpeed;
    public float projectileSpeed;
    public int tileRange;
    public Effect[] effects;

    public GameObject prefab; // prefab reference

    public EvolutionData(
        int index,
        string name,
        int tier,
        int[] possibleEvolutions,
        int baseCost,
        float hp,
        float atk,
        float atkSpeed,
        float projectileSpeed,
        int tileRange,
        Effect[] effects,
        GameObject prefab)
    {
        this.index = index;
        this.name = name;
        this.tier = tier;
        this.possibleEvolutions = possibleEvolutions;
        this.baseCost = baseCost;
        this.hp = hp;
        this.atk = atk;
        this.atkSpeed = atkSpeed;
        this.projectileSpeed = projectileSpeed;
        this.tileRange = tileRange;
        this.effects = effects;
        this.prefab = prefab;
    }

    public override string ToString()
    {
        string evoList = possibleEvolutions.Length > 0 ? string.Join(", ", possibleEvolutions) : "None";
        string effectList = effects.Length > 0 ? string.Join(", ", effects) : "None";
        return $"[{index}] {name} (Tier {tier}) | Cost:{baseCost} | HP:{hp} ATK:{atk} SPD:{atkSpeed} ProjSPD : {projectileSpeed} Range:{tileRange} | Effects: {effectList} | Next: {evoList}";
    }
}

public class EvolutionManager : MonoBehaviour
{
    public static EvolutionManager Instance { get; private set; }

    public List<EvolutionData> evolutions = new();

    [Header("Runtime References")]
    private GameObject currentInstance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {

    }
    
    public EvolutionData GetEvolution(int index)
    {
        if (index < 0 || index >= evolutions.Count)
        {
            Debug.LogWarning($"Index {index} out of range for evolutions list!");
            return null;
        }
        return evolutions[index];
    }

    public void SetEvolution(int index, EvolutionData newData)
    {
        if (index < 0 || index >= evolutions.Count)
        {
            Debug.LogWarning($"Index {index} out of range for evolutions list!");
            return;
        }
        evolutions[index] = newData;
    }

    public void SpawnEvolution(int evolutionIndex)
    {
        if (evolutionIndex < 0 || evolutionIndex >= evolutions.Count)
        {
            Debug.LogWarning($"Evolution index {evolutionIndex} invalid!");
            return;
        }

        if (currentInstance != null)
            Destroy(currentInstance);

        EvolutionData data = evolutions[evolutionIndex];

        if (data.prefab != null)
        {
            currentInstance = Instantiate(data.prefab, transform.position, Quaternion.identity, transform);
        }

        Debug.Log($"🌱 Spawned {data.name} (Tier {data.tier}) prefab: {data.prefab?.name}");
    }

    public bool CanEvolveTo(int currentEvolution, int targetIndex)
    {
        EvolutionData current = evolutions[currentEvolution];
        foreach (int possible in current.possibleEvolutions)
        {
            if (possible == targetIndex)
                return true;
        }
        return false;
    }
}
