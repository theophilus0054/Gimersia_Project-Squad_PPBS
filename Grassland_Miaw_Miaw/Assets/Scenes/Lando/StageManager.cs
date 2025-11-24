using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class StageSummon
{
    [Header("Stage Info")]
    public string stageName;
    public int stageTargetProgress = 100;

    [Header("Enemies")]
    public int[] enemyIndices;
    [Range(0f, 100f)]
    public float[] spawnChances;

    [Header("Stage Multipliers")]
    public float hpMultiplier = 1f;
    public float atkMultiplier = 1f;
    public float coinsMultiplier = 1f;
    public float spawnIntensity = 0.1f;

    [Header("Wave References")]
    public int[] waveIndices; // << Index ke WaveManager.waves

    public int GetRandomEnemyIndex()
    {
        if (enemyIndices.Length == 0 || spawnChances.Length != enemyIndices.Length)
        {
            Debug.LogError("StageSummon: enemyIndices and spawnChances mismatch or empty!");
            return -1;
        }

        float total = spawnChances.Sum();
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

    [Header("Stage Settings")]
    public StageSummon[] stageSummons;

    [Header("References")]
    public WaveManager waveManager;
    public EnemyManager enemyManager;

    [Header("Row Settings")]
    public int totalRows = 5;
    private Dictionary<int, double> rowWeights;
    private HashSet<int> pickedRowsThisCycle;

    public int currentStage = 1;
    public bool isSummonPhase = false;
    public bool targetAchieved = false;

    // Multipliers for current stage
    private float hpMultiplier = 1f;
    private float atkMultiplier = 1f;
    private float coinsMultiplier = 1f;
    private float spawnIntensity = 0.1f;

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
        currentStage = GameManager.Instance.highestStage;
        UIManager.Instance.UpdateStageText(currentStage);
        StartStage(currentStage);
    }

    public void UpdateTargetAchieved()
    {
        if(GameManager.Instance.currentProgress >= GameManager.Instance.targetProgress && !targetAchieved)
        {
            targetAchieved = true;
            if (GameManager.Instance.highestStage < stageSummons.Length)
            {
                if(GameManager.Instance.currentProgress == GameManager.Instance.targetProgress)
                {
                    UIManager.Instance.WaveSurrenderPanel.GetComponent<SurrenderScript>().ActiveButton();
                    isSummonPhase = false;
                    return;
                }
            }
        }
    }

    void StartStage(int stageNumber)
    {
        isSummonPhase = true;
        DeleteAllChildren();

        StageSummon stageData = stageSummons[stageNumber - 1];

        // Apply stage-specific multipliers
        hpMultiplier = stageData.hpMultiplier;
        atkMultiplier = stageData.atkMultiplier;
        coinsMultiplier = stageData.coinsMultiplier;
        spawnIntensity = stageData.spawnIntensity;

        // Initialize row weights
        rowWeights = new Dictionary<int, double>();
        pickedRowsThisCycle = new HashSet<int>();
        for (int i = 0; i < totalRows; i++)
            rowWeights[i] = 1.0;

        if (summonCoroutine != null)
        {
            StopCoroutine(summonCoroutine);
            summonCoroutine = null;
        }

        summonCoroutine = StartCoroutine(SummonPhaseCoroutine());
    }

    Coroutine summonCoroutine;

    IEnumerator SummonPhaseCoroutine()
    {
        StageSummon stageData = stageSummons[currentStage - 1];
        yield return new WaitForSeconds(1f); // Delay sebelum mulai summon

        while (isSummonPhase)
        {
            int enemyIndex = stageData.GetRandomEnemyIndex();
            int row = GetWeightedRandomRow();

            enemyManager.SpawnEnemy(enemyIndex, row);

            yield return new WaitForSeconds(0.5f / spawnIntensity);
        }

        Debug.Log("✅ Summon phase completed, start stage waves!");

        // Run waves using indices from WaveManager
        if (stageData.waveIndices != null && stageData.waveIndices.Length > 0)
        {
            currentWaveCoroutine = StartCoroutine(RunStageWavesByIndex(stageData.waveIndices));
            yield return currentWaveCoroutine;
        }
    }

    private Coroutine currentWaveCoroutine;

    private IEnumerator RunStageWavesByIndex(int[] waveIndices)
    {
        DeleteAllChildren();
        BGMManager.Instance.ToggleBGM();
        foreach (int waveIndex in waveIndices)
        {
            // Validasi index
            if (waveIndex < 0 || waveIndex >= waveManager.waves.Count)
            {
                Debug.LogError($"❌ Invalid wave index: {waveIndex}");
                continue;
            }

            Wave wave = waveManager.waves[waveIndex];
            Debug.Log($"🌊 Starting Wave {waveIndex}: {wave.waveName}");

            yield return StartCoroutine(waveManager.SpawnWave(wave));
            yield return new WaitForSeconds(wave.delayBeforeNextWave);
        }

        Debug.Log($"🏆 Stage {currentStage} waves completed!");
        OnWaveComplete();
    }

    public void FailedWave()
    {
        DeleteAllChildren();
        BGMManager.Instance.ToggleBGM();
        SlideStageScript.Instance.SlidePlay(UIManager.Instance.waveFailedFrame, 15f, 1f, false);
        Debug.Log("💀 Wave Failed! Returning to summon phase...");

        // Stop current wave coroutine
        if (currentWaveCoroutine != null)
        {
            StopCoroutine(currentWaveCoroutine);
            currentWaveCoroutine = null;
        }

        if (summonCoroutine != null)
        {
            StopCoroutine(summonCoroutine);
            summonCoroutine = null;
        }

        // Stop all wave spawning in WaveManager
        waveManager.StopAllCoroutines();

        // Return to summon phase
        isSummonPhase = true;
        UIManager.Instance.WaveStagePanel.GetComponent<SlideButton>().ActiveButton();
        summonCoroutine = StartCoroutine(SummonPhaseCoroutine());
    }

    int GetWeightedRandomRow()
    {
        if (pickedRowsThisCycle.Count == totalRows)
        {
            pickedRowsThisCycle.Clear();
            for (int i = 0; i < totalRows; i++)
                rowWeights[i] = 1.0;
        }

        double total = rowWeights.Values.Sum();
        double roll = Random.value * total;
        double cumulative = 0;
        int chosenRow = -1;

        foreach (var pair in rowWeights)
        {
            cumulative += pair.Value;
            if (roll <= cumulative)
            {
                chosenRow = pair.Key;
                break;
            }
        }

        pickedRowsThisCycle.Add(chosenRow);

        // Adjust weights
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
        return chosenRow + 1;
    }

    void NormalizeRowWeights()
    {
        double total = rowWeights.Values.Sum();
        foreach (var key in rowWeights.Keys.ToList())
            rowWeights[key] /= total / totalRows;
    }

    public void OnWaveComplete()
    {
        BGMManager.Instance.ToggleBGM();
        UIManager.Instance.WaveSurrenderPanel.GetComponent<SurrenderScript>().StopWave(false);
        SlideStageScript.Instance.SlidePlay(UIManager.Instance.waveFinishedFrame, 15f, 1f, false);
        Debug.Log($"🏆 Stage {currentStage} cleared!");
        NextStage();
    }

    public void NextStage()
    {
        currentStage++;
        if (currentStage > stageSummons.Length)
        {
            Debug.Log("🎯 All stages completed!");
            currentStage = stageSummons.Length;
        }
        else
        {
            targetAchieved = false;
            UIManager.Instance.UpdateStageText(currentStage);
        }

        if(currentStage > GameManager.Instance.highestStage)
        {
            Debug.Log("🔄 New stage reached, resetting progress.");
            GameManager.Instance.UpdateHighestStage(currentStage);
            GameManager.Instance.SetStageProgress(0, stageSummons[currentStage - 1].stageTargetProgress, true);
        } else
        {
            Debug.Log($"gabisa stage {currentStage} <= highest {GameManager.Instance.highestStage}");
        }

        StartStage(currentStage);
    }

    public void DeleteAllChildren()
    {
        for (int i = ObjectManager.Instance.enemySpawn.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(ObjectManager.Instance.enemySpawn.transform.GetChild(i).gameObject);
        }
    }

    // Helpers for scaled stats
    public float GetScaledHP(float baseHP) => baseHP * hpMultiplier;
    public float GetScaledATK(float baseATK) => baseATK * atkMultiplier;
    public int GetScaledCoins(int baseCoins) => Mathf.RoundToInt(baseCoins * coinsMultiplier);
}