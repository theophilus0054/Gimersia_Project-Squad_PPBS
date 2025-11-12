using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class UpgradeGUIManager : MonoBehaviour
{
    public static UpgradeGUIManager Instance { get; private set; }

    [Header("Upgrade Database")]
    public UpgradeData[] allUpgrades;

    [Header("UI References")]
    public GameObject descriptionPanel;
    public TextMeshPro nameText;
    public TextMeshPro descText;
    public TextMeshPro costText;

    [Header("Coin & Button")]
    public Collider2D UpgradeCollider;
    public SpriteRenderer UpgradeButton;

    [Header("Current Upgrade")]
    public int currentIndex = 0;
    private UpgradeData currentUpgrade;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void consumeCoins()
    {
        if (currentUpgrade != null)
        {
            GameManager.Instance.SpendCoins(currentUpgrade.cost);
            currentUpgrade.isPurchased = true;
            CheckCost();
            UpdateUpgrades();
        }
    }

    void Start()
    {
        ClearAllText();
        UpdateUpgrades();
        // bisa aktifin kalau mau langsung tampil:
        // ShowUpgradeByIndex(currentIndex);
    }

    void Update()
    {
        if (currentUpgrade != null)
            CheckCost();
    }

    public void ClearAllText()
    {
        if (nameText) nameText.text = "";
        if (descText) descText.text = "";
        if (costText) costText.text = "";
        if (descriptionPanel) descriptionPanel.SetActive(false);
    }

    public void ShowUpgradeByIndex(int index)
    {
        UpgradeData upgrade = null;

        foreach (var c in allUpgrades)
        {
            if (c.index == index)
            {
                upgrade = c;
                break;
            }
        }

        if (upgrade == null)
        {
            Debug.LogWarning($"Upgrade with index {index} not found!");
            ClearAllText();
            return;
        }

        currentUpgrade = upgrade;

        // 🔹 Tampilkan UI
        descriptionPanel.SetActive(true);
        if (nameText) nameText.text = upgrade.upgradeName;
        if (descText) descText.text = upgrade.description;
        if (costText) costText.text = $"{upgrade.cost}";

        CheckCost();
    }

    private void CheckCost()
    {
        if (UpgradeCollider == null || UpgradeButton == null) return;
        if (currentUpgrade == null) return;

        // Cek apakah upgrade sudah dibeli
        if (currentUpgrade.isPurchased)
        {
            UpgradeCollider.enabled = false;
            UpgradeButton.color = new Color(0.5f, 0.5f, 0.5f, 1f); // abu-abu
            if (costText) costText.text = "Purchased";
            return;
        }

        bool cukup = GameManager.Instance.totalCoins >= currentUpgrade.cost;

        UpgradeCollider.enabled = cukup;
        UpgradeButton.color = cukup
            ? Color.white
            : new Color(0.937f, 0.824f, 0.808f, 1f);
    }


    public void UpdateUpgrades()
    {
        for (int i = 0; i < allUpgrades.Length; i++)
        {
            if (allUpgrades[i].isPurchased)
            {
                ApplyUpgradeEffect(allUpgrades[i]);
            }
        }
    }

    private void ApplyUpgradeEffect(UpgradeData upgrade)
    {
        switch (upgrade.index)
        {
            case 0:
                for (int i = 0; i < SummonGUIManager.Instance.allCreatures.Length; i++)
                {
                    CreatureData creature = SummonGUIManager.Instance.allCreatures[i];
                    if (creature.type == CreatureType.Unagi)
                    {
                        // Tambahkan efek Slow menggunakan AddEffect
                        creature.AddEffect(new EffectData { effectType = Effect.Slow, chanceToApply = 10f });
                        
                        // Sinkronkan ke array untuk Inspector (opsional, tergantung kebutuhan)
                        creature.SyncEffectsToArray();
                    }
                }
                break;
        }

        Debug.Log($"{upgrade.name} applied!");
    }
}
