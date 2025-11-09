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
    public float movementSpeed;
    public float atk;
    public float atkspd;
    public GameObject prefab;

    public EnemyData(
        int index,
        string name,
        EnemyType type,
        float hp,
        float movementSpeed,
        float atk,
        float atkspd,
        GameObject prefab)
    {
        this.index = index;
        this.name = name;
        this.type = type;
        this.hp = hp;
        this.movementSpeed = movementSpeed;
        this.atk = atk;
        this.atkspd = atkspd;
        this.prefab = prefab;
    }

    public override string ToString()
    {
        return $"[{index}] {name} ({type}) | HP: {hp} | ATK: {atk} | ATKSPD: {atkspd} | Speed: {movementSpeed} | Prefab: {prefab?.name ?? "None"}";
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
        DontDestroyOnLoad(gameObject);
    }

    // ✅ Getter
    public EnemyData GetEnemy(int index)
    {
        if (index < 0 || index >= enemies.Count)
        {
            Debug.LogWarning($"Enemy index {index} out of range!");
            return null;
        }
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

    // ✅ Spawn enemy by index
    public void SpawnEnemy(int index, int row)
    {
        if (index < 0 || index >= enemies.Count)
        {
            Debug.LogWarning($"⚠️ Enemy index {index} invalid!");
            return;
        }

        if (row < 1 || row > 6)
        {
            Debug.LogWarning($"⚠️ Invalid row {row}! Must be between 1 and 5.");
            return;
        }

        EnemyData data = enemies[index];

        if (data.prefab == null)
        {
            Debug.LogError($"❌ Enemy prefab for {data.name} is NULL!");
            return;
        }

        // --- Hitung posisi berdasarkan row ---
        float baseX = 10.3f;
        float baseY = 3.4075f;
        float offsetY = 1.6875f * (row - 1);

        Vector3 spawnPos = new Vector3(baseX, baseY - offsetY, 0f);

        GameObject enemyObj = Instantiate(data.prefab, spawnPos, Quaternion.identity);
        enemyObj.name = $"{data.name}_Row{row}";

        Debug.Log($"👾 Spawned enemy '{data.name}' at Row {row}, Pos {spawnPos}");
    }

    // ✅ Optional: Get all enemies of a certain type
    public List<EnemyData> GetEnemiesOfType(EnemyType type)
    {
        return enemies.FindAll(e => e.type == type);
    }
}
