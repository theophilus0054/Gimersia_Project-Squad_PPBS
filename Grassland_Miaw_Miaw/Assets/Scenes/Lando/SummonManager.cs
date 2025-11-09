using UnityEngine;

public class SummonManager : MonoBehaviour
{
    public static SummonManager Instance { get; private set; }

    [Header("Grid Settings")]
    public Transform dropAreaParent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"SummonManager duplicate instance found on {gameObject.name}, destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("SummonManager instance initialized.");
    }

    // -------------------------
    // Summon overloads
    // -------------------------
    public static void SummonEvolution(int evolutionIndex)
    {
        DropArea target = Instance.FindNextEmptyDropArea();
        if (target == null)
        {
            Debug.LogWarning($"No empty DropArea found to summon evolution {evolutionIndex}.");
        }
        SummonEvolution(evolutionIndex, target);
    }

    public static void SummonEvolution(int evolutionIndex, DropArea target)
    {
        SummonEvolution(evolutionIndex, target, true);
    }

    public static void SummonEvolution(int evolutionIndex, int row, int col, bool saveToGrid = true)
    {
        DropArea target = Instance.FindDropAreaByPosition(row, col);
        if (target == null)
        {
            Debug.LogWarning($"DropArea at position ({row},{col}) not found for summoning evolution {evolutionIndex}.");
        }
        SummonEvolution(evolutionIndex, target, saveToGrid);
    }

    private static void SummonEvolution(int evolutionIndex, DropArea target, bool saveToGrid)
    {
        if (Instance == null)
        {
            Debug.LogError("SummonManager instance not initialized!");
            return;
        }

        if (target == null)
        {
            Debug.LogWarning($"Target DropArea is null for evolutionIndex {evolutionIndex}.");
            return;
        }

        if (EvolutionManager.Instance == null)
        {
            Debug.LogError("EvolutionManager instance not found! Cannot summon evolution.");
            return;
        }


        // Ambil data evolusi
        EvolutionData evoData = EvolutionManager.Instance.evolutions[evolutionIndex];
        if (evoData.prefab == null)
        {
            Debug.LogError($"Prefab for evolution index {evolutionIndex} ({evoData.name}) not set!");
            return;
        }

        GameObject newObj = Instantiate(evoData.prefab, target.transform.position, Quaternion.identity, ObjectManager.Instance.creatureSpawn.transform);
        DragScript drag = newObj.GetComponent<DragScript>();
        drag.enabled = false;

        if (GameManager.Instance != null)
        {
            if (!GameManager.Instance.IsUnlocked(evolutionIndex) && evolutionIndex != 0)
            {
                Debug.LogWarning($"❌ Evolution index {evolutionIndex} belum di-unlock! ({EvolutionManager.Instance.evolutions[evolutionIndex].name})");
                drag.enabled = false;
                GameManager.Instance.UnlockIndex(evolutionIndex);
                // 🔁 Jalankan animasi, lalu aktifkan kembali drag
                AnimationScript.Play(newObj, () =>
                {
                    drag.enabled = true;
                    Debug.Log($"{newObj.name} animasi selesai, DragScript diaktifkan kembali.");
                });
            }
            else
            {
                drag.enabled = true;
            }
        }
        else
        {
            Debug.LogWarning("⚠️ GameManager belum siap, tidak bisa cek status unlock.");
        }
        if (drag == null)
        {
            Debug.LogError($"Instantiated object for evolution {evoData.name} has no DragScript component!");
        }
        else
        {
            drag.posX = target.x;
            drag.posY = target.y;
            target.OnItemDrop(drag);
        }

        if (saveToGrid)
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager instance not found! Cannot save grid cell.");
            }
            else
            {
                GameManager.Instance.SetGridCell(target.x, target.y, evolutionIndex);
            }
        }

        Debug.Log($"✨ Summoned {evoData.name} (Tier {evoData.tier}) at ({target.x},{target.y})");
    }

    // -------------------------
    // Find DropArea helpers
    // -------------------------
    private DropArea FindNextEmptyDropArea()
    {
        if (dropAreaParent == null)
        {
            Debug.LogError("DropArea parent is null! Cannot find empty DropArea.");
            return null;
        }

        DropArea[] allAreas = dropAreaParent.GetComponentsInChildren<DropArea>();
        System.Array.Sort(allAreas, (a, b) =>
        {
            if (a.x == b.x) return a.y.CompareTo(b.y);
            return a.x.CompareTo(b.x);
        });

        foreach (var area in allAreas)
        {
            if (!area.getFilled()) return area;
        }

        Debug.LogWarning("All DropAreas are filled, cannot find empty area.");
        return null;
    }

    private DropArea FindDropAreaByPosition(int row, int col)
    {
        if (dropAreaParent == null)
        {
            Debug.LogError("DropArea parent is null! Cannot find DropArea by position.");
            return null;
        }

        DropArea[] allAreas = dropAreaParent.GetComponentsInChildren<DropArea>();
        foreach (var area in allAreas)
        {
            if (area.x == row && area.y == col) return area;
        }

        Debug.LogWarning($"DropArea at position ({row},{col}) not found.");
        return null;
    }
}
