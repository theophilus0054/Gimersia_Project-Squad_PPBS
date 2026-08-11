using UnityEngine;
using System.Collections;
using DG.Tweening;   // << DOTWEEN

public class ObjectManager : MonoBehaviour
{
    public static ObjectManager Instance;

    [Header("Spawn Points / Prefabs")]
    public GameObject creatureSpawn;
    public GameObject enemySpawn;

    public GameObject coinObject;
    public Transform coinLocation;

    [Header("Coin Movement Settings")]
    public float offsetRange = 0.6f;       // radius random coin spawn offset
    public float bounceHeight = 0.35f;     // tinggi bounce
    public float bounceDuration = 0.28f;
    public float moveDuration = 0.8f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // =========================================================================
    // DIPANGGIL DARI RewardManager
    // =========================================================================
    public void SummonCoinWithRandomOffset(GameObject source, int coinValue)
    {
        // Random offset XY
        Vector3 offset = new Vector3(
            Random.Range(-offsetRange, offsetRange),
            Random.Range(-offsetRange, offsetRange),
            0
        );

        Vector3 spawnPos = source.transform.position + offset;

        GameObject coin = Instantiate(coinObject, spawnPos, Quaternion.identity);

        PlayCoinSequenceDOTween(coin, coinLocation.position, coinValue);
    }

    // =========================================================================
    // DOTWEEN COIN ANIMATION
    // =========================================================================
    void PlayCoinSequenceDOTween(GameObject coin, Vector3 targetPos, int coinValue)
    {
        Vector3 startPos = coin.transform.position;

        Sequence seq = DOTween.Sequence();

        // -----------------------------
        // BOUNCE UP
        // -----------------------------
        seq.Append(
            coin.transform.DOMoveY(startPos.y + bounceHeight, bounceDuration * 0.5f)
                .SetEase(Ease.OutQuad)
        );

        // -----------------------------
        // BOUNCE DOWN
        // -----------------------------
        seq.Append(
            coin.transform.DOMoveY(startPos.y, bounceDuration * 0.5f)
                .SetEase(Ease.InQuad)
        );

        // -----------------------------
        // FLY TO TARGET
        // -----------------------------
        seq.Append(
            coin.transform.DOMove(targetPos, moveDuration)
                .SetEase(Ease.InOutSine)
        );

        // -----------------------------
        // ON COMPLETE → ADD COIN + DESTROY
        // -----------------------------
        seq.OnComplete(() =>
        {
            GameManager.Instance.AddCoins(coinValue);

            // efek kecil: scale pop
            coin.transform.DOScale(1.2f, 0.08f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                Destroy(coin);
            });
        });
    }
}
