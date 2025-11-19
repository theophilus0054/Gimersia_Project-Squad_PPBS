using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SummonGUIManager : MonoBehaviour
{
    public static SummonGUIManager Instance { get; private set; }

    [Header("Creature Database")]
    public CreatureData[] allCreatures;

    [Header("UI References")]
    public GameObject descriptionPanel;
    public TextMeshPro nameText;
    public TextMeshPro statsText;
    public TextMeshPro descText;
    public TextMeshPro costText;
    public TextMeshPro currentText;
    public Transform prefabCreatureDisplay;

    [Header("Coin & Button")]
    public Collider2D summonCollider;
    public SpriteRenderer summonButton;

    [Header("Current Creature")]
    public int currentIndex = 0;
    private CreatureData currentCreature;
    private GameObject currentPreviewInstance;

    // 🔹 Tambahan: catat jumlah pembelian tiap creature
    private Dictionary<int, int> creaturePurchaseCount = new Dictionary<int, int>();

    private void Awake()
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
        ClearAllText();
    }

    void Update()
    {
        if (currentCreature != null)
            CheckCost();
    }

    public void ClearAllText()
    {
        if (nameText) nameText.text = "";
        if (statsText) statsText.text = "";
        if (descText) descText.text = "";
        if (costText) costText.text = "";
        if (currentText) currentText.text = "";
        if (descriptionPanel) descriptionPanel.SetActive(false);

        if (currentPreviewInstance)
            Destroy(currentPreviewInstance);
    }

    public void ShowCreatureByIndex(int index)
    {
        CreatureData creature = null;

        foreach (var c in allCreatures)
        {
            if (c.index == index)
            {
                creature = c;
                break;
            }
        }

        if (creature == null)
        {
            Debug.LogWarning($"Creature with index {index} not found!");
            ClearAllText();
            return;
        }

        currentCreature = creature;

        descriptionPanel.SetActive(true);
        if (nameText) nameText.text = creature.creatureName;
        if (statsText) statsText.text = $"Range: {creature.range} | Damage: {creature.damage}";
        if (descText) descText.text = creature.description;

        // 🔹 Tampilkan harga yang dinamis
        if (costText) costText.text = $"Cost: {GetCurrentCost(creature)}";

        if (currentText)
            currentText.text = creature.type == CreatureType.Unagi ? $"T{creature.index + 1}" : "";

        if (prefabCreatureDisplay && creature.displayPrefab)
        {
            if (currentPreviewInstance)
                Destroy(currentPreviewInstance);

            currentPreviewInstance = Instantiate(
                creature.displayPrefab,
                prefabCreatureDisplay.position,
                Quaternion.identity
            );
            currentPreviewInstance.transform.SetParent(prefabCreatureDisplay);
        }

        CheckCost();
    }

    private bool CheckCost()
    {
        if (summonCollider == null || summonButton == null) return false;
        if (currentCreature == null) return false;

        int currentCost = GetCurrentCost(currentCreature);
        bool cukup = GameManager.Instance.totalCoins >= currentCost;

        summonButton.color = cukup
            ? Color.white
            : new Color(0.937f, 0.824f, 0.808f, 1f);

        return cukup;
    }

    public void consumeCoins()
    {
        if (!CheckCost())
        {
            GameObject obj = Instantiate(UIManager.Instance.popupWarningPrefab, UIManager.Instance.popupWarningSlot.transform);
            obj.GetComponentInChildren<TextMeshProUGUI>().text = "Not Enough Seashell";
            return;
        }

        DropArea target = SummonManager.Instance.FindNextEmptyDropArea();
        if (target == null)
        {
            GameObject obj = Instantiate(UIManager.Instance.popupWarningPrefab, UIManager.Instance.popupWarningSlot.transform);
            obj.GetComponentInChildren<TextMeshProUGUI>().text = "Your base is full";
            return;
        }

        if (currentCreature != null)
        {
            int cost = GetCurrentCost(currentCreature);
            GameManager.Instance.SpendCoins(cost);

            // 🔹 Tambah jumlah pembelian untuk creature ini ke GameManager
            var dict = GameManager.Instance.creaturePurchaseCount;
            if (!dict.ContainsKey(currentCreature.index))
                dict[currentCreature.index] = 0;

            dict[currentCreature.index]++;

            // 🔹 Simpan langsung ke file JSON
            GameManager.Instance.SaveData();

            SummonManager.SummonEvolution(currentCreature.index);
            ShowCreatureByIndex(currentCreature.index); // refresh UI cost
        }
    }


    // 🔹 Rumus harga dinamis (tidak eksponensial)
    private int GetCurrentCost(CreatureData creature)
    {
        int baseCost = creature.cost;
        int timesBought = creaturePurchaseCount.ContainsKey(creature.index) ? creaturePurchaseCount[creature.index] : 0;

        float growthRate = 1.5f + (creature.index * 0.15f);
        growthRate = Mathf.Min(growthRate, 3.0f); // biar gak gila di level tinggi

        // 🔹 Harga = baseCost * (growthRate ^ jumlah beli)
        float scaled = baseCost * Mathf.Pow(growthRate, timesBought);

        return Mathf.RoundToInt(scaled);
    }

    // (fungsi ShowNextCreature & ShowPreviousCreature tetap sama)

    public void ShowNextCreature()
    {
        if (allCreatures.Length == 0) return;

        int startIndex = currentIndex;
        
        bool found = false;

        do
        {
            currentIndex++;
            if (currentIndex >= allCreatures.Length)
                currentIndex = 0;

            if (GameManager.Instance.unlockedIndex != null && currentIndex < GameManager.Instance.unlockedIndex.Length && GameManager.Instance.unlockedIndex[currentIndex])
            {
                found = true;
                break;
            }

        } while (currentIndex != startIndex);

        // kalau tidak ditemukan unlocked creature, balik ke 0
        if (!found)
        {
            currentIndex = 0;
            if (GameManager.Instance.unlockedIndex != null && GameManager.Instance.unlockedIndex.Length > 0 && GameManager.Instance.unlockedIndex[0])
                found = true;
        }

        if (found)
            ShowCreatureByIndex(allCreatures[currentIndex].index);
        else
            Debug.Log("No unlocked creatures available!");
    }

    public void ShowPreviousCreature()
    {
        if (allCreatures.Length == 0) return;

        int startIndex = currentIndex;
        bool found = false;

        do
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = allCreatures.Length - 1;

            if (GameManager.Instance.unlockedIndex != null &&
                currentIndex < GameManager.Instance.unlockedIndex.Length &&
                GameManager.Instance.unlockedIndex[currentIndex])
            {
                found = true;
                break;
            }

        } while (currentIndex != startIndex);

        // kalau tidak ditemukan unlocked creature, balik ke 0
        if (!found)
        {
            currentIndex = 0;
            if (GameManager.Instance.unlockedIndex != null &&
                GameManager.Instance.unlockedIndex.Length > 0 &&
                GameManager.Instance.unlockedIndex[0])
            {
                found = true;
            }
        }

        if (found)
            ShowCreatureByIndex(allCreatures[currentIndex].index);
        else
            Debug.Log("No unlocked creatures available!");
    }
}
