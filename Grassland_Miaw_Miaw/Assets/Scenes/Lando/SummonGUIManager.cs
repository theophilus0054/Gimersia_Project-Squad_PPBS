using UnityEngine;
using TMPro;

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
    public Transform prefabCreatureDisplay; // ubah ke Transform, biar posisinya jelas

    [Header("Coin & Button")]
    public Collider2D summonCollider;
    public SpriteRenderer summonButton;

    [Header("Current Creature")]
    public int currentIndex = 0;
    private CreatureData currentCreature;

    private GameObject currentPreviewInstance; // simpan instance dari prefab preview

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
        // bisa aktifin kalau mau langsung tampil:
        // ShowCreatureByIndex(currentIndex);
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

        // Hapus preview creature kalau ada
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

        // 🔹 Tampilkan UI
        descriptionPanel.SetActive(true);
        if (nameText) nameText.text = creature.creatureName;
        if (statsText) statsText.text = $"Range: {creature.range} | Damage: {creature.damage}";
        if (descText) descText.text = creature.description;
        if (costText) costText.text = $"Cost: {creature.cost}";
        if (currentText)
            currentText.text = creature.type == CreatureType.Unagi ? $"T{creature.index + 1}" : "";

        // 🔹 Spawn preview prefab di posisi display
        if (prefabCreatureDisplay && creature.displayPrefab)
        {
            // hapus preview lama
            if (currentPreviewInstance)
                Destroy(currentPreviewInstance);

            // instantiate prefab baru
            currentPreviewInstance = Instantiate(
                creature.displayPrefab,
                prefabCreatureDisplay.position,
                Quaternion.identity
            );

            // optional: jadikan child dari display supaya rapi
            currentPreviewInstance.transform.SetParent(prefabCreatureDisplay);
        }

        CheckCost();
    }

    private bool CheckCost()
    {
        if (summonCollider == null || summonButton == null)  return false;
        if (currentCreature == null) return false;

        bool cukup = GameManager.Instance.totalCoins >= currentCreature.cost;

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
            Debug.LogWarning($"No empty DropArea found to summon evolution.");
            return;
        }
        DropArea target = SummonManager.Instance.FindNextEmptyDropArea();
        if (target == null)
        {
            GameObject obj = Instantiate(UIManager.Instance.popupWarningPrefab, UIManager.Instance.popupWarningSlot.transform);
            obj.GetComponentInChildren<TextMeshProUGUI>().text = "Your base is full";
            Debug.LogWarning($"No empty DropArea found to summon evolution.");
            return;
        }
        if (currentCreature != null)
        {
            GameManager.Instance.SpendCoins(currentCreature.cost);
            SummonManager.SummonEvolution(currentCreature.index);
            CheckCost();
        }
    }

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
