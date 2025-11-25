using UnityEngine;

[CreateAssetMenu(fileName = "StageSummon", menuName = "GameData/Stage Summon")]
public class StageSummon : ScriptableObject
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
    public int[] waveIndices;

    public int GetRandomEnemyIndex()
    {
        if (enemyIndices.Length == 0 || spawnChances.Length != enemyIndices.Length)
        {
            Debug.LogError("StageSummonSO: enemyIndices and spawnChances mismatch or empty!");
            return -1;
        }

        float total = 0f;
        foreach (var v in spawnChances) total += v;

        float rand = Random.Range(0f, total);
        float cumulative = 0f;

        for (int i = 0; i < enemyIndices.Length; i++)
        {
            cumulative += spawnChances[i];
            if (rand <= cumulative)
                return enemyIndices[i];
        }

        return enemyIndices[0];
    }
}
