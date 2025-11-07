using UnityEngine;

public class SummonManager : MonoBehaviour
{
    public static SummonManager Instance { get; private set; }

    [Header("Grid Settings")]
    public Transform dropAreaParent; // 👉 drag parent DropArea kamu ke sini di Inspector

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

    /// <summary>
    /// Summon prefab dari EvolutionManager ke DropArea kosong pertama
    /// </summary>
    public static void SummonEvolution(int evolutionIndex)
    {
        if (Instance == null)
        {
            Debug.LogError("❌ SummonManager not initialized!");
            return;
        }

        if (EvolutionManager.Instance == null)
        {
            Debug.LogError("❌ EvolutionManager not found!");
            return;
        }

        DropArea target = Instance.FindNextEmptyDropArea();
        if (target == null)
        {
            Debug.LogWarning("⚠ No empty DropArea available!");
            return;
        }

        EvolutionData evoData = EvolutionManager.Instance.evolutions[evolutionIndex];
        GameObject prefab = evoData.prefab;

        if (prefab == null)
        {
            Debug.LogError($"❌ Prefab for evolution index {evolutionIndex} not set!");
            return;
        }

        GameObject newObj = Instantiate(prefab, target.transform.position, Quaternion.identity);
        newObj.GetComponent<DragScript>().posX = target.x;
        newObj.GetComponent<DragScript>().posY = target.y;
        target.OnItemDrop(newObj.GetComponent<DragScript>());
        Debug.Log($"✨ Summoned {evoData.name} (Tier {evoData.tier}) at DropArea ({target.x}, {target.y})");
    }

    public static void SummonEvolution(int evolutionIndex, DropArea hitCollider)
    {
        if (Instance == null)
        {
            Debug.LogError("❌ SummonManager not initialized!");
            return;
        }

        if (EvolutionManager.Instance == null)
        {
            Debug.LogError("❌ EvolutionManager not found!");
            return;
        }

        DropArea target = hitCollider;
        if (target == null)
        {
            Debug.LogWarning("⚠ No empty DropArea available!");
            return;
        }

        EvolutionData evoData = EvolutionManager.Instance.evolutions[evolutionIndex];
        GameObject prefab = evoData.prefab;

        if (prefab == null)
        {
            Debug.LogError($"❌ Prefab for evolution index {evolutionIndex} not set!");
            return;
        }

        GameObject newObj = Instantiate(prefab, target.transform.position, Quaternion.identity);
        newObj.GetComponent<DragScript>().posX = target.x;
        newObj.GetComponent<DragScript>().posY = target.y;
        target.OnItemDrop(newObj.GetComponent<DragScript>());
        Debug.Log($"✨ Summoned {evoData.name} (Tier {evoData.tier}) at DropArea ({target.x}, {target.y})");
    }

    private DropArea FindNextEmptyDropArea()
    {
        if (dropAreaParent == null)
        {
            Debug.LogError("⚠ dropAreaParent not assigned!");
            return null;
        }

        DropArea[] allAreas = dropAreaParent.GetComponentsInChildren<DropArea>();

        // Urutkan berdasarkan (x,y)
        System.Array.Sort(allAreas, (a, b) =>
        {
            if (a.x == b.x)
                return a.y.CompareTo(b.y);
            return a.x.CompareTo(b.x);
        });

        foreach (var area in allAreas)
        {
            if (!area.getFilled())
                return area;
        }

        return null;
    }
}
