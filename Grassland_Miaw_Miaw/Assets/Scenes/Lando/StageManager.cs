using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class StageSummon
{
    public string stageName;
    public int[] enemyIndices;
    [Range(0f, 100f)]
    public float[] spawnChances;
    public int stageTargetProgress = 100;

    public int GetRandomEnemyIndex()
    {
        if (enemyIndices.Length == 0 || spawnChances.Length != enemyIndices.Length)
        {
            Debug.LogError("StageSummon: enemyIndices and spawnChances mismatch or empty!");
            return -1;
        }

        float total = 0f;
        foreach (var chance in spawnChances)
            total += chance;

        float rand = Random.Range(0f, total);
        float cumulative = 0f;

        for (int i = 0; i < enemyIndices.Length; i++)
        {
            cumulative += spawnChances[i];
            if (rand <= cumulative)
                return enemyIndices[i];
        }

        return enemyIndices[0]; // fallback
    }
}

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("Stage Info")]
    public int currentStage = 1;
    public float hpMultiplier = 1f;
    public float atkMultiplier = 1f;
    public float spawnIntensity = 0.1f;

    [Header("Stage Summons")]
    public StageSummon[] stageSummons;

    [Header("References")]
    public WaveManager waveManager;
    public EnemyManager enemyManager;

    [Header("Row Settings")]
    public int totalRows = 5; // number of rows
    private Dictionary<int, double> rowWeights;
    private HashSet<int> pickedRowsThisCycle;

    private int summonedCount = 0;
    private bool isSummonPhase = false;

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
        StartStage(currentStage);
        GameManager.Instance.SetStageProgress(0, stageSummons[currentStage - 1].stageTargetProgress, true);
    }

    void StartStage(int stageNumber)
    {
        summonedCount = 0;
        isSummonPhase = true;
        ApplyStageSettings();

        // initialize row weights
        rowWeights = new Dictionary<int, double>();
        pickedRowsThisCycle = new HashSet<int>();
        for (int i = 0; i < totalRows; i++)
            rowWeights[i] = 1.0;

        StartCoroutine(SummonPhaseCoroutine());
    }

    IEnumerator SummonPhaseCoroutine()
    {
        StageSummon stageData = stageSummons[currentStage - 1];

        while (isSummonPhase)
        {
            int enemyIndex = stageData.GetRandomEnemyIndex();
            int row = GetWeightedRandomRow();

            enemyManager.SpawnEnemy(enemyIndex, row);
            summonedCount++;

            yield return new WaitForSeconds(0.5f / spawnIntensity); // spawn pacing
        }

        Debug.Log("✅ Summon phase completed, start waves!");
    }

    int GetWeightedRandomRow()
    {
        // Reset if all rows picked once
        if (pickedRowsThisCycle.Count == totalRows)
        {
            pickedRowsThisCycle.Clear();
            for (int i = 0; i < totalRows; i++)
                rowWeights[i] = 1.0; // reset weights
        }

        double total = rowWeights.Values.Sum();
        double roll = Random.value * (float)total;
        double cumulative = 0;
        int chosenRow = -1;

        foreach (var pair in rowWeights)
        {
            cumulative += pair.Value;
            if (roll <= cumulative)
            {
                chosenRow = pair.Key + 1;
                break;
            }
        }

        pickedRowsThisCycle.Add(chosenRow);

        // Decrease chosen row weight, increase others slightly
        double decreaseFactor = 0.5;
        double increaseBoost = 0.2;
        foreach (var key in rowWeights.Keys.ToList())
        {
            if (key == chosenRow)
                rowWeights[key] = Mathf.Max(0.1f, (float)(rowWeights[key] * decreaseFactor));
            else
                rowWeights[key] += increaseBoost;
        }

        NormalizeRowWeights();

        return chosenRow;
    }

    void NormalizeRowWeights()
    {
        double total = rowWeights.Values.Sum();
        foreach (var key in rowWeights.Keys.ToList())
            rowWeights[key] /= total / totalRows;
    }

    void OnWaveComplete()
    {
        Debug.Log($"🏆 Stage {currentStage} cleared!");
        NextStage();
    }

    public void NextStage()
    {
        currentStage++;
        if (currentStage > stageSummons.Length)
        {
            Debug.Log("🎯 All stages completed!");
            return;
        }

        StartStage(currentStage);
    }

    void ApplyStageSettings()
    {
        switch (currentStage)
        {
            case 1:
                hpMultiplier = 1f;
                atkMultiplier = 1f;
                spawnIntensity = 0.05f;
                break;
            case 2:
                hpMultiplier = 1.1f;
                atkMultiplier = 1.1f;
                spawnIntensity = 0.05f;
                break;
            default:
                hpMultiplier = 1f + (currentStage - 1) * 0.1f;
                atkMultiplier = hpMultiplier;
                spawnIntensity = 1f + (currentStage - 1) * 0.15f;
                break;
        }
    }

    public float GetScaledHP(float baseHP) => baseHP * hpMultiplier;
    public float GetScaledATK(float baseATK) => baseATK * atkMultiplier;
}
