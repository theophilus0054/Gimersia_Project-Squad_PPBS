using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public static ObjectManager Instance; // static access

    [Header("Spawn Points / Prefabs")]
    public GameObject creatureSpawn;
    public GameObject enemySpawn;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // hanya satu instance
            return;
        }
        Instance = this;
        // Optional: persist across scenes
        // DontDestroyOnLoad(gameObject);
    }
}
