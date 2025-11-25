using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveSet", menuName = "Game/Wave Set")]
public class WaveSet : ScriptableObject
{
    public List<WaveData> waves = new List<WaveData>();
}
