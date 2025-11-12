using UnityEngine;
using System.Collections;

public class ObjectManager : MonoBehaviour
{
    public static ObjectManager Instance;

    [Header("Spawn Points / Prefabs")]
    public GameObject creatureSpawn;
    public GameObject enemySpawn;

    public GameObject coinObject;
    public Transform coinLocation;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Optional: DontDestroyOnLoad(gameObject);
    }

    public void SummonCoin(GameObject obj, int coinCount)
    {
        GameObject coin = Instantiate(coinObject, obj.transform.position, Quaternion.identity);
        StartCoroutine(MoveCoinToTarget(coin, coinLocation.position, 1f, coinCount));
    }

    private IEnumerator MoveCoinToTarget(GameObject coin, Vector3 target, float duration, int coins)
    {
        Vector3 startPos = coin.transform.position;
        float time = 0f;
        yield return new WaitForSeconds(1f);

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            // Optional: tambahkan efek lerp curve
            coin.transform.position = Vector3.Lerp(startPos, target, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        // Pastikan posisi akhir di target
        coin.transform.position = target;

        // Bisa tambah efek ketika coin sampai (misal: tambah skor, animasi pop, destroy)
        Destroy(coin, 0.2f);
        GameManager.Instance.AddCoins(coins);
    }
}
