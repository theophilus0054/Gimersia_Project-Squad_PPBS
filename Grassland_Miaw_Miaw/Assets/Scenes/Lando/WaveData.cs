using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class WaveEnemy
{
    public int enemyIndex;
    public int count;
}


[CreateAssetMenu(fileName = "WaveData", menuName = "Game/Wave Data")]
public class WaveData : ScriptableObject
{
    public string waveName;

    [Tooltip("Daftar musuh yang akan muncul dalam wave ini")]
    public List<WaveEnemy> enemies = new List<WaveEnemy>();

    [Tooltip("Delay antar spawn musuh dalam 1 wave")]
    public float delayBetweenSpawns = 1f;

    [Tooltip("Delay sebelum wave berikutnya dimulai")]
    public float delayBeforeNextWave = 3f;
}
