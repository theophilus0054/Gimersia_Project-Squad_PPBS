using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class GameData
{
    public long totalCoins;
    public int highestStage;
    public List<int> gridLayoutList = new List<int>();
    public bool[] unlockedIndex;
    public int currentProgress;
    public int targetProgress;
    public int[] purchasedUpgrade;
    public bool finishedTutorial;

    public List<int> creaturePurchaseKeys = new List<int>();
    public List<int> creaturePurchaseValues = new List<int>();
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public long totalCoins { get; private set; }
    public int highestStage { get; private set; }
    public bool finishedTutorial = false;

    [Header("Grid Data")]
    public int[,] gridLayout = new int[5, 6];
    public bool[] unlockedIndex;

    [Header("Progress")]
    public int currentProgress { get; private set; }
    public int targetProgress { get; private set; }

    [Header("Purchases")]
    public Dictionary<int, int> creaturePurchaseCount = new Dictionary<int, int>();

    private string savePath;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        savePath = Application.persistentDataPath + "/gamedata.json";

        LoadData();

        if (!finishedTutorial)
        {
            Debug.Log("🧩 Tutorial belum selesai — GameManager tidak disimpan antar scene.");
            // Jangan pakai DontDestroyOnLoad biar ke-reset di tutorial
        }

        DontDestroyOnLoad(gameObject);
        Debug.Log("✅ GameManager persist antar scene aktif.");
    }

    private void Start()
    {
        if(UpgradeGUIManager.Instance != null)
            UpgradeGUIManager.Instance.UpdateUpgrades();
        IsUnlocked(15);
        IsUnlocked(30);

        if (UIManager.Instance != null)
            UIManager.Instance.coinText.text = ScaleNumber(totalCoins);
    }

    // -------------------------
    // Coin Methods
    // -------------------------
    public void AddCoins(int amount)
    {
        totalCoins += amount;
        UIManager.Instance.coinText.text = ScaleNumber(totalCoins);
        SaveData();
    }

    public bool SpendCoins(int amount)
    {
        if (totalCoins >= amount)
        {
            AudioManager.Instance.PlayBuyInteraction();
            totalCoins -= amount;
            UIManager.Instance.coinText.text = ScaleNumber(totalCoins);
            SaveData();
            return true;
        }
        return false;
    }

    public static string ScaleNumber(long value)
    {
        if (value >= 1_000_000_000)
            return (value / 1_000_000_000f).ToString("0.#") + "B";
        if (value >= 1_000_000)
            return (value / 1_000_000f).ToString("0.#") + "M";
        if (value >= 1_000)
            return (value / 1_000f).ToString("0.#") + "K";

        return value.ToString();
    }

    // -------------------------
    // Stage Methods
    // -------------------------
    public void UpdateHighestStage(int stage)
    {
        if (stage > highestStage)
        {
            highestStage = stage;
            SaveData();
        }
    }

    public void AddStageProgress(int value)
    {
        currentProgress += value;
        SaveData();
    }

    public void SetStageProgress(int current, int target, bool force)
    {
        if (!force && currentProgress != 0 && targetProgress != 0)
            return;

        currentProgress = Mathf.Clamp(current, 0, target);
        targetProgress = Mathf.Max(target, 1);
        SaveData();
    }

    // -------------------------
    // Grid Methods
    // -------------------------
    public void SetGridCell(int row, int col, int creatureID)
    {
        // validasi index 1..5 dan 1..6
        if (row < 1 || row > 5 || col < 1 || col > 6)
            return;

        int r = row - 1;
        int c = col - 1;

        gridLayout[r, c] = Mathf.Clamp(creatureID, -1, 50);
        SaveData();
    }

    public int GetGridCell(int row, int col)
    {
        if (row < 1 || row > 5 || col < 1 || col > 6)
            return -1;

        int r = row - 1;
        int c = col - 1;

        int id = gridLayout[r, c];
        return (id < -1 || id > 50) ? -1 : id;
    }

    public void LoadGrid()
    {
        if (SummonManager.Instance == null)
        {
            Debug.LogWarning("SummonManager belum siap, grid tidak bisa di-load");
            return;
        }

        for (int r = 0; r < 5; r++)
        {
            for (int c = 0; c < 6; c++)
            {
                int creatureID = gridLayout[r, c];
                if (creatureID >= 0 && creatureID <= 50)
                    SummonManager.SummonEvolution(creatureID, r+1, c+1, false);
            }
        }
    }

    // -------------------------
    // Unlock Methods
    // -------------------------
    public bool IsUnlocked(int index)
    {
        if (unlockedIndex == null || index < 0 || index >= unlockedIndex.Length)
            return false;
        return unlockedIndex[index];
    }

    public void UnlockIndex(int index)
    {
        if (unlockedIndex == null)
            unlockedIndex = new bool[100];

        if (index >= 0 && index < unlockedIndex.Length)
        {
            unlockedIndex[index] = true;
            SaveData();
        }
    }

    // -------------------------
    // Save / Load
    // -------------------------
    public void SaveData()
    {
        if (!finishedTutorial)
        {
            Debug.Log("⏸️ Tutorial belum selesai — data tidak disimpan.");
            return;
        }

        GameData data = new GameData
        {
            totalCoins = totalCoins,
            highestStage = highestStage,
            unlockedIndex = unlockedIndex,
            currentProgress = currentProgress,
            targetProgress = targetProgress,
            finishedTutorial = finishedTutorial,
            purchasedUpgrade = UpgradeGUIManager.Instance != null
                ? UpgradeGUIManager.Instance.allUpgrades
                    .Select((u, i) => u.isPurchased ? i : -1)
                    .Where(i => i != -1)
                    .ToArray()
                : new int[0],

            creaturePurchaseKeys = creaturePurchaseCount.Keys.ToList(),
            creaturePurchaseValues = creaturePurchaseCount.Values.ToList()
        };

        // Flatten grid to list
        data.gridLayoutList.Clear();
        for (int r = 0; r < 5; r++)
            for (int c = 0; c < 6; c++)
                data.gridLayoutList.Add(gridLayout[r, c]);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"💾 Data saved to {savePath}");
    }

    public void LoadData()
    {
        if (!File.Exists(savePath))
        {
            ResetData();
            return;
        }

        string json = File.ReadAllText(savePath);
        GameData data = JsonUtility.FromJson<GameData>(json);

        totalCoins = data.totalCoins;
        highestStage = data.highestStage;
        unlockedIndex = data.unlockedIndex ?? new bool[100];
        currentProgress = data.currentProgress;
        targetProgress = data.targetProgress;
        finishedTutorial = data.finishedTutorial;

        // Restore grid
        int[] grid = data.gridLayoutList.ToArray();
        gridLayout = new int[5, 6];
        for (int i = 0; i < grid.Length && i < 30; i++)
            gridLayout[i / 6, i % 6] = grid[i];

        // Rebuild creature purchase dictionary
        creaturePurchaseCount.Clear();
        for (int i = 0; i < data.creaturePurchaseKeys.Count; i++)
            creaturePurchaseCount[data.creaturePurchaseKeys[i]] = data.creaturePurchaseValues[i];

        if (UpgradeGUIManager.Instance != null)
        {
            foreach (var upgrade in UpgradeGUIManager.Instance.allUpgrades)
                upgrade.isPurchased = false;

            foreach (int i in data.purchasedUpgrade)
                UpgradeGUIManager.Instance.allUpgrades[i].isPurchased = true;

            UpgradeGUIManager.Instance.UpdateUpgrades();
        }

        // 🔥 DEBUG: Print isi dictionary
        Debug.Log("======== DICTIONARY PURCHASE COUNT ========");
        if (creaturePurchaseCount.Count == 0)
        {
            Debug.Log("❌ Dictionary EMPTY !");
        }
        else
        {
            foreach (var kv in creaturePurchaseCount)
            {
                Debug.Log($"KEY: {kv.Key}  |  VALUE: {kv.Value}");
            }
        }
        Debug.Log("===========================================");

        Debug.Log("✅ Game data loaded successfully.");
    }


    public void ResetData()
    {
        totalCoins = 10;
        highestStage = 1;
        gridLayout = new int[5, 6];
        for (int r = 0; r < 5; r++)
            for (int c = 0; c < 6; c++)
                gridLayout[r, c] = -1;

        unlockedIndex = new bool[100];
        unlockedIndex[0] = true;
        currentProgress = 0;
        targetProgress = StageManager.Instance != null ? StageManager.Instance.stageSummons[0].stageTargetProgress : 10;
        creaturePurchaseCount.Clear();

        if (!finishedTutorial)
        {
            Debug.Log("🚫 Tutorial belum selesai — data tidak direset penuh.");
            return;
        }

        if (UpgradeGUIManager.Instance != null)
        {
            foreach (UpgradeData upgrade in UpgradeGUIManager.Instance.allUpgrades)
                upgrade.isPurchased = false;
        }

        if (SummonGUIManager.Instance != null)
        {
            foreach (CreatureData creature in SummonGUIManager.Instance.allCreatures)
            {
                creature.Effects.Clear();
                creature.SyncEffectsToArray();
            }
        }

        SaveData();
    }

    public void CompleteTutorial()
    {
        finishedTutorial = true;
        ResetData();
        SaveData();
        DontDestroyOnLoad(gameObject);
        Debug.Log("🎉 Tutorial selesai — GameManager sekarang persist antar scene.");
    }
}
