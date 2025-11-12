using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveEnemy
{
    public int enemyIndex;      // index di EnemyManager
    public int count;           // jumlah musuh yang muncul
}

[System.Serializable]
public class Wave
{
    public string waveName;
    public List<WaveEnemy> enemies = new List<WaveEnemy>();
    [Tooltip("Delay antar spawn untuk seluruh wave")]
    public float delayBetweenSpawns = 1f;
    [Tooltip("Delay sebelum wave berikutnya")]
    public float delayBeforeNextWave = 3f;
}

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Settings")]
    public List<Wave> waves = new List<Wave>();

    [Header("Runtime Info")]
    private int currentWaveIndex = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        
    }

    public IEnumerator StartWaves()
    {
        Debug.Log("🚀 Starting Waves...");
        for (int i = 0; i < waves.Count; i++)
        {
            currentWaveIndex = i;
            yield return StartCoroutine(SpawnWave(waves[i]));
            yield return new WaitForSeconds(waves[i].delayBeforeNextWave);
        }
        Debug.Log("🎉 All waves completed!");
    }

    public IEnumerator SpawnWave(Wave wave)
    {
        Debug.Log($"🌊 Starting Wave: {wave.waveName}");

        // Salin list supaya original tidak terganggu
        List<WaveEnemy> enemiesLeft = new List<WaveEnemy>();
        foreach (var we in wave.enemies)
            enemiesLeft.Add(new WaveEnemy { enemyIndex = we.enemyIndex, count = we.count });

        // Parent tempat semua musuh
        Transform parent = ObjectManager.Instance.enemySpawn.transform;

        while (enemiesLeft.Count > 0)
        {
            // Pilih WaveEnemy secara random
            int randomIndex = Random.Range(0, enemiesLeft.Count);
            WaveEnemy selected = enemiesLeft[randomIndex];

            // Spawn 1 musuh sebagai child parent
            int row = Random.Range(1, 6); // misal 5 row (1-4)
            EnemyManager.Instance.SpawnEnemy(selected.enemyIndex, row, parent);

            // Kurangi count
            selected.count--;
            if (selected.count <= 0)
                enemiesLeft.RemoveAt(randomIndex);

            // Delay antar spawn (sama untuk semua musuh di wave)
            yield return new WaitForSeconds(wave.delayBetweenSpawns);
        }

        // Tunggu sampai semua musuh di parent mati
        while (parent.childCount > 0)
        {
            yield return null;
        }

        Debug.Log($"✅ Wave {wave.waveName} completed!");
    }


    public void RestartWaves()
    {
        StopAllCoroutines();
        currentWaveIndex = 0;
        StartCoroutine(StartWaves());
    }

    public bool IsLastWave => currentWaveIndex >= waves.Count - 1;
}
