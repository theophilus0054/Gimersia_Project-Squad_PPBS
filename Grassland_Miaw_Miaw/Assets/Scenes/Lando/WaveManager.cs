using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Data")]
    public WaveSet waveSet;   // <-- Drag ScriptableObject WaveSet di sini

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

    public IEnumerator StartWaves()
    {
        if (waveSet == null || waveSet.waves.Count == 0)
        {
            Debug.LogError("❌ WaveSet belum di-assign!");
            yield break;
        }

        BGMManager.Instance.ToggleBGM();
        Debug.Log("🚀 Starting Waves...");

        for (int i = 0; i < waveSet.waves.Count; i++)
        {
            currentWaveIndex = i;
            WaveData wave = waveSet.waves[i];

            RippleManager.Instance.callShockwave();

            yield return StartCoroutine(SpawnWave(wave));
            yield return new WaitForSeconds(wave.delayBeforeNextWave);
        }

        Debug.Log("🎉 All waves completed!");
    }

    public IEnumerator SpawnWave(WaveData wave)
    {
        Debug.Log($"🌊 Starting Wave: {wave.waveName}");

        // Copy list agar original tidak berubah
        List<WaveEnemy> enemiesLeft = new List<WaveEnemy>();
        foreach (var we in wave.enemies)
        {
            enemiesLeft.Add(new WaveEnemy { enemyIndex = we.enemyIndex, count = we.count });
        }

        Transform parent = ObjectManager.Instance.enemySpawn.transform;

        while (enemiesLeft.Count > 0)
        {
            int randomIndex = Random.Range(0, enemiesLeft.Count);
            WaveEnemy selected = enemiesLeft[randomIndex];

            int row = Random.Range(1, 6);
            EnemyManager.Instance.SpawnEnemy(selected.enemyIndex, row);

            selected.count--;
            if (selected.count <= 0)
                enemiesLeft.RemoveAt(randomIndex);

            yield return new WaitForSeconds(wave.delayBetweenSpawns);
        }

        // Tunggu musuh habis
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

    public bool IsLastWave => currentWaveIndex >= waveSet.waves.Count - 1;
}
