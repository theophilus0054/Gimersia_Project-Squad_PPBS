using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;   // << DOTWEEN

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;

    [Header("Reward Data")]
    public StageRewardData rewardData;

    [Header("Prefabs")]
    public GameObject coinBagPrefab;

    public Transform spawnArea;

    [Header("Spawn Randomization")]
    public float randomYMin = -0.5f;
    public float randomYMax = 0.5f;

    [Header("Light Effect")]
    public GameObject lightShowPrefab;
    public Vector3 lightOffset = new Vector3(0, 0.5f, 0);

    [Header("Timing")]
    public float autoTriggerDelay = 1.5f;

    [Header("Spawn Animation")]
    public float bounceHeight = 0.4f;
    public float bounceDuration = 0.25f;
    public float scalePunch = 0.25f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ======================================================================
    // DIPANGGIL DARI StageManager
    // ======================================================================
    public void GiveStageReward(int stage)
    {
        foreach (var entry in rewardData.rewards)
        {
            if (entry.stageNumber == stage)
            {
                TriggerReward(entry);
            }
        }
    }

    void TriggerReward(RewardEntry entry)
    {
        switch (entry.type)
        {
            case RewardType.Coins:
                StartCoroutine(SpawnCoinBag(entry.value, entry.splitCount));
                break;

            case RewardType.UnlockEvolution:
                StartCoroutine(SpawnUnlockedEvolution(entry.value));
                break;
        }
    }

    // ======================================================================
    // HELPERS
    // ======================================================================
    Vector3 GetRandomSpawnPos()
    {
        Vector3 pos = spawnArea.position;
        pos.y += Random.Range(randomYMin, randomYMax);
        return pos;
    }

    void SpawnLightAt(Vector3 pos)
    {
        if (lightShowPrefab == null) return;

        GameObject light = Instantiate(lightShowPrefab, pos + lightOffset, Quaternion.identity);
        Destroy(light, 2f);
    }

    void PlaySpawnBounce(GameObject obj)
    {
        Vector3 original = obj.transform.position;

        Sequence seq = DOTween.Sequence();

        // Up
        seq.Append(obj.transform.DOMoveY(original.y + bounceHeight, bounceDuration)
            .SetEase(Ease.OutQuad));

        // Down
        seq.Append(obj.transform.DOMoveY(original.y, bounceDuration)
            .SetEase(Ease.InQuad));

        // Scale punch
        obj.transform.DOPunchScale(Vector3.one * scalePunch, 0.25f, 10, 1f);
    }

    // ======================================================================
    // COIN BAG
    // ======================================================================
    IEnumerator SpawnCoinBag(int amount, int split)
    {
        Vector3 spawnPos = GetRandomSpawnPos();

        GameObject bag = Instantiate(coinBagPrefab, spawnPos, Quaternion.identity);

        SpawnLightAt(spawnPos);
        PlaySpawnBounce(bag);   // << bounce anim

        yield return new WaitForSeconds(autoTriggerDelay);

        int valuePerCoin = Mathf.RoundToInt((float)amount / split);

        for (int i = 0; i < split; i++)
        {
            ObjectManager.Instance.SummonCoinWithRandomOffset(bag, valuePerCoin);
        }

        Destroy(bag);
    }

    // ======================================================================
    // EVOLUTION
    // ======================================================================
    IEnumerator SpawnUnlockedEvolution(int evolutionIndex)
    {
        Vector3 spawnPos = GetRandomSpawnPos();

        GameObject icon = Instantiate(
            SummonGUIManager.Instance.allCreatures[evolutionIndex].displayPrefab,
            spawnPos,
            Quaternion.identity
        );

        // Reset transform
        Transform t = icon.transform;
        t.localScale = new Vector3(1, 1, 1);
        icon.GetComponent<SpriteRenderer>().color = Color.black;

        SpawnLightAt(spawnPos);
        PlaySpawnBounce(icon);     // << bounce anim

        yield return new WaitForSeconds(autoTriggerDelay);

        // Cari drop area
        DropArea target = SummonManager.Instance.FindNextEmptyDropArea();

        // Spawn evolution creature
        CreatureData evoData = EvolutionManager.Instance.evolutions[evolutionIndex];


        if (target == null)
        {

            GameObject newObjs = Instantiate(
                evoData.summonPrefab,
                spawnPos,
                Quaternion.identity,
                ObjectManager.Instance.creatureSpawn.transform
            );

            DragScript drags = newObjs.GetComponent<DragScript>();
            drags.enabled = false;

            GameManager.Instance.UnlockIndex(evolutionIndex);
            RevealScript.Instance.RevealPlayInstantMoveDied(newObjs, () =>
            {
                PlaySpawnBounce(newObjs);    // << bounce makhluk muncul
                drags.enabled = true;
            });
            Destroy(icon);
            yield break;
        }

        GameObject newObj = Instantiate(
            evoData.summonPrefab,
            target.transform.position,
            Quaternion.identity,
            ObjectManager.Instance.creatureSpawn.transform
        );

        DragScript drag = newObj.GetComponent<DragScript>();
        drag.enabled = false;

        drag.posX = target.x;
        drag.posY = target.y;
        target.OnItemDrop(drag, evolutionIndex);

        GameManager.Instance.SetGridCell(target.x, target.y, evolutionIndex);
        GameManager.Instance.UnlockIndex(evolutionIndex);

        // Reveal + bounce
        RevealScript.Instance.RevealPlayInstantMove(newObj, () =>
        {
            PlaySpawnBounce(newObj);    // << bounce makhluk muncul
            drag.enabled = true;
        });

        Destroy(icon);
    }
}
