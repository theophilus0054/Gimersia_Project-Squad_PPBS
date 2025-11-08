using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveEnemy
{
    public int enemyIndex;      // index di EnemyManager
    public int count;           // berapa musuh muncul
    public float spawnDelay;    // delay antar musuh
}

[System.Serializable]
public class Wave
{
    public string waveName;
    public List<WaveEnemy> enemies = new List<WaveEnemy>();
    public float delayBeforeNext = 3f; // waktu tunggu ke wave berikutnya
}

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Settings")]
    public List<Wave> waves = new List<Wave>();
    public bool autoStart = true;

    [Header("Runtime Info")]
    private int currentWaveIndex = 0;
    private bool isSpawning = false;

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
        if (autoStart)
            StartCoroutine(StartWaves());
    }

    public IEnumerator StartWaves()
    {
        Debug.Log("🚀 Starting Waves...");
        for (int i = 0; i < waves.Count; i++)
        {
            currentWaveIndex = i;
            yield return StartCoroutine(SpawnWave(waves[i]));
            yield return new WaitForSeconds(waves[i].delayBeforeNext);
        }
        Debug.Log("🎉 All waves completed!");
    }

    IEnumerator SpawnWave(Wave wave)
    {
        Debug.Log($"🌊 Starting Wave: {wave.waveName}");
        isSpawning = true;

        foreach (WaveEnemy we in wave.enemies)
        {
            for (int i = 0; i < we.count; i++)
            {
                if (EnemyManager.Instance == null)
                {
                    Debug.LogError("EnemyManager.Instance is NULL! Can't spawn enemies!");
                    yield break;
                }

                EnemyData data = EnemyManager.Instance.GetEnemy(we.enemyIndex);
                if (data == null)
                {
                    Debug.LogWarning($"Enemy index {we.enemyIndex} invalid for wave {wave.waveName}!");
                    continue;
                }

                // Pilih row secara random 1-5
                int row = Random.Range(1, 6);

                // Spawn musuh di row
                EnemyManager.Instance.SpawnEnemy(we.enemyIndex, row);

                yield return new WaitForSeconds(we.spawnDelay);
            }
        }

        isSpawning = false;
        Debug.Log($"✅ Wave {wave.waveName} completed!");
    }

    public void StartNextWave()
    {
        if (isSpawning)
        {
            Debug.LogWarning("Still spawning current wave!");
            return;
        }

        if (currentWaveIndex + 1 >= waves.Count)
        {
            Debug.Log("⚡ No more waves left!");
            return;
        }

        currentWaveIndex++;
        StartCoroutine(SpawnWave(waves[currentWaveIndex]));
    }

    public void RestartWaves()
    {
        StopAllCoroutines();
        currentWaveIndex = 0;
        StartCoroutine(StartWaves());
    }

    public void ConfigureWaveSettings(float intensity, bool randomizeEnemyTypes = false, bool fastSpawn = false)
    {
        foreach (Wave w in waves)
        {
            foreach (WaveEnemy we in w.enemies)
            {
                // spawn delay dikurang biar lebih intens
                if (fastSpawn)
                    we.spawnDelay = Mathf.Max(0.1f, we.spawnDelay / intensity);

                // jumlah musuh meningkat sesuai intensitas
                we.count = Mathf.CeilToInt(we.count * intensity);

                // randomisasi tipe musuh
                if (randomizeEnemyTypes && EnemyManager.Instance != null)
                {
                    int randIndex = Random.Range(0, EnemyManager.Instance.enemies.Count);
                    we.enemyIndex = randIndex;
                }
            }
        }
    }

    public bool IsLastWave => currentWaveIndex >= waves.Count - 1;
}
