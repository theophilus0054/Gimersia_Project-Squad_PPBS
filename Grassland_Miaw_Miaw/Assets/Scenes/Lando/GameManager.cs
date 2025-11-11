using UnityEngine;
using System.IO;

[System.Serializable]
public class GameData
{
    public int totalCoins;
    public int highestStage;
    public int[,] gridLayout; // 5x6 grid, 0 = kosong
    public bool[] unlockedIndex; // setiap index evolution: true = unlocked, false = locked
    public int currentProgress;
    public int targetProgress;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int totalCoins { get; private set; }
    public int highestStage { get; private set; }

    public int[,] gridLayout = new int[5, 6]; // 5 rows x 6 columns
    public bool[] unlockedIndex;

    public int currentProgress { get; private set; }
    public int targetProgress { get; private set; }

    private string savePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Application.persistentDataPath + "/gamedata.json";
        ResetData();
        LoadData();
    }

    void Start()
    {
        LoadGrid(); // summon semua creature sesuai JSON saat start
        UIManager.Instance.coinText.text = totalCoins.ToString();
    }

    // -------------------------
    // Coin methods
    // -------------------------
    public void AddCoins(int amount)
    {
        totalCoins += amount;
        Debug.Log($"Added {amount} coins. Total now: {totalCoins}");
        UIManager.Instance.coinText.text = totalCoins.ToString();
        SaveData();
    }

    public bool SpendCoins(int amount)
    {
        if (totalCoins >= amount)
        {
            AudioManager.Instance.PlayBuyInteraction();
            totalCoins -= amount;
            UIManager.Instance.coinText.text = totalCoins.ToString();
            SaveData();
            return true;
        }
        return false;
    }

    // -------------------------
    // Stage methods
    // -------------------------
    public void UpdateHighestStage(int stage)
    {
        if (stage > highestStage)
        {
            highestStage = stage;
            SaveData();
        }
    }

    public void addStageProgress(int current)
    {
        currentProgress += current;
        Debug.Log($"Stage Progress Updated: {currentProgress}/{targetProgress}");
        SaveData();
    }


    public void SetStageProgress(int current, int target, bool force)
    {
        // Cek apakah progress sudah ada
        if (currentProgress != 0 && targetProgress != 0 && !force)
        {
            Debug.LogWarning($"Stage progress sudah ada (Current: {currentProgress}, Target: {targetProgress}). Tidak menimpa data lama.");
            return; // jangan overwrite
        }

        // Pastikan current <= target dan target minimal 1
        currentProgress = Mathf.Clamp(current, 0, target);
        targetProgress = Mathf.Max(target, 1);

        SaveData();
    }

    // -------------------------
    // Grid methods
    // -------------------------
    public void SetGridCell(int row, int col, int creatureID)
    {
        if (row < 0 || row >= 5 || col < 0 || col >= 6)
            return;

        gridLayout[row, col] = creatureID;
        SaveData();
    }

    public int GetGridCell(int row, int col)
    {
        if (row < 0 || row >= 5 || col < 0 || col >= 6)
            return -1;

        return gridLayout[row, col];
    }

    public void LoadGrid()
    {
        if (SummonManager.Instance == null)
        {
            Debug.LogWarning("SummonManager belum siap, grid tidak bisa di-load");
            return;
        }

        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                int creatureID = gridLayout[row, col];
                if (creatureID > 0) // 0 = kosong
                {
                    SummonManager.SummonEvolution(creatureID, row, col, false); // false = jangan simpan lagi
                }
            }
        }
    }

    // -------------------------
    // Unlock methods
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
            unlockedIndex = new bool[100]; // ganti 100 sesuai jumlah evolusi

        if (index >= 0 && index < unlockedIndex.Length)
        {
            unlockedIndex[index] = true;
            SaveData();
        }
    }

    // -------------------------
    // Save & Load JSON
    // -------------------------
    public void SaveData()
    {
        GameData data = new GameData
        {
            totalCoins = totalCoins,
            highestStage = highestStage,
            gridLayout = gridLayout,
            unlockedIndex = unlockedIndex,
            currentProgress = currentProgress,
            targetProgress = targetProgress
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public void LoadData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            totalCoins = data.totalCoins;
            highestStage = data.highestStage;

            if (data.gridLayout != null && data.gridLayout.Length == 5 * 6)
                gridLayout = data.gridLayout;
            else
                gridLayout = new int[5, 6];

            if (data.unlockedIndex != null)
                unlockedIndex = data.unlockedIndex;
            else
                unlockedIndex = new bool[100];

            currentProgress = data.currentProgress;
            targetProgress = data.targetProgress;
        }
        else
        {
            ResetData();
        }
    }

    // -------------------------
    // Reset everything
    // -------------------------
    public void ResetData()
    {
        totalCoins = 10;
        highestStage = 1;
        gridLayout = new int[5, 6];
        unlockedIndex = new bool[100];
        currentProgress = 0;
        targetProgress = StageManager.Instance.stageSummons[0].stageTargetProgress;
        unlockedIndex = new bool[100];
        unlockedIndex[0] = true;
        
        // Reset semua effects di creatures
        foreach(CreatureData creature in SummonGUIManager.Instance.allCreatures)
        {
            if (creature != null)
            {
                creature.Effects.Clear(); // Clear HashSet
                creature.SyncEffectsToArray(); // Sync ke array
            }
        }
        
        // Reset semua upgrades
        foreach(UpgradeData upgrade in UpgradeGUIManager.Instance.allUpgrades)
        {
            if (upgrade != null)
            {
                upgrade.isPurchased = false;
            }
        }
        
        SaveData();
    }
}
