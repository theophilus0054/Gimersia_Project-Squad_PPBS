using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public enum EnemyType
{
    Normal,
    Fast,
    Tank,
    Boss,
    Flying,
    Summoner
}

[System.Serializable]
public class EnemyData
{
    public int index;
    public string name;
    public EnemyType type;
    public float hp;
    public int baseCoin;
    public float movementSpeed;
    public float atk;
    public float atkspd;
    public GameObject prefab;

    public EnemyData(
        int index,
        string name,
        EnemyType type,
        float hp,
        int baseCoin,
        float movementSpeed,
        float atk,
        float atkspd,
        GameObject prefab)
    {
        this.index = index;
        this.name = name;
        this.type = type;
        this.hp = hp;
        this.baseCoin = baseCoin;
        this.movementSpeed = movementSpeed;
        this.atk = atk;
        this.atkspd = atkspd;
        this.prefab = prefab;
    }

    public override string ToString()
    {
        return $"[{index}] {name} ({type}) | HP: {hp} | BaseCoin: {baseCoin} | ATK: {atk} | ATKSPD: {atkspd} | Speed: {movementSpeed} | Prefab: {prefab?.name ?? "None"}";
    }
}

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("Enemy Database")]
    public List<EnemyData> enemies = new();

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
    }

    // ✅ Getter
    public EnemyData GetEnemy(int index)
    {
        if (index < 0 || index >= enemies.Count)
        {
            Debug.LogWarning($"Enemy index {index} out of range!");
            return null;
        }
        Debug.LogWarning(enemies[index].ToString());
        return enemies[index];
    }

    // ✅ Setter
    public void SetEnemy(int index, EnemyData newData)
    {
        if (index < 0 || index >= enemies.Count)
        {
            Debug.LogWarning($"Enemy index {index} out of range!");
            return;
        }
        enemies[index] = newData;
    }

    public void SpawnEnemy()
    {
        SpawnEnemy(0, 3);
    }

    // ✅ Spawn enemy by index
    public GameObject SpawnEnemy(int index, int row)
    {
        if (index < 0 || index >= enemies.Count)
        {
            Debug.LogWarning($"⚠️ Enemy index {index} invalid!");
            return null;
        }

        if (row < 1 || row > 6)
        {
            Debug.LogWarning($"⚠️ Invalid row {row}! Must be between 1 and 5.");
            return null;
        }

        EnemyData data = enemies[index];

        if (data.prefab == null)
        {
            Debug.LogError($"❌ Enemy prefab for {data.name} is NULL!");
            return null;
        }

        // --- Hitung posisi berdasarkan row ---
        float baseX = 10.3f;
        float baseY = 3.4075f;
        float offsetY = 1.6875f * (row - 1);

        Vector3 spawnPos = new Vector3(baseX, baseY - offsetY, 0f);

        GameObject enemyObj = Instantiate(data.prefab, spawnPos, Quaternion.identity, ObjectManager.Instance.enemySpawn.transform);

        enemyObj.name = $"{data.name}_Row{row}";
        enemyObj.GetComponent<Enemy>().dropItemOnDeath = StageManager.Instance.isSummonPhase;
        enemyObj.GetComponent<Enemy>().getProgress = (StageManager.Instance.currentStage == GameManager.Instance.highestStage);

        Debug.Log($"👾 Spawned enemy '{data.name}' at Row {row}, Pos {spawnPos}");
        return enemyObj;
    }

    // ✅ Optional: Get all enemies of a certain type
    public List<EnemyData> GetEnemiesOfType(EnemyType type)
    {
        return enemies.FindAll(e => e.type == type);
    }
}
