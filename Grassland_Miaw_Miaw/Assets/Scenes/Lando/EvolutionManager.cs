using UnityEngine;
using System.Collections.Generic;

public class EvolutionManager : MonoBehaviour
{
    public static EvolutionManager Instance { get; private set; }

    [Header("Evolution Database")]
    public List<CreatureData> evolutions = new();

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

    public CreatureData GetEvolution(int index)
    {
        if (index < 0 || index >= evolutions.Count)
        {
            Debug.LogWarning($"Index {index} out of range for evolutions list!");
            return null;
        }
        return evolutions[index];
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

        CreatureData data = evolutions[evolutionIndex];

        if (data.summonPrefab != null)
        {
            currentInstance = Instantiate(data.summonPrefab, transform.position, Quaternion.identity, transform);
        }

        Debug.Log($"🌱 Spawned {data.creatureName} (Tier {data.tier}) prefab: {data.summonPrefab?.name}");
    }

    public bool CanEvolveTo(int currentEvolutionIndex, int targetIndex)
    {
        if (currentEvolutionIndex < 0 || currentEvolutionIndex >= evolutions.Count)
            return false;

        CreatureData current = evolutions[currentEvolutionIndex];

        foreach (var evo in current.possibleEvolutions)
        {
            if (evo != null && evo.index == targetIndex)
                return true;
        }
        return false;
    }
}
