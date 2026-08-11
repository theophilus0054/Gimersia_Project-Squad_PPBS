using UnityEngine;
using System.Collections.Generic;

public class CreatureButtonManager : MonoBehaviour
{
    public static CreatureButtonManager Instance { get; private set; }
    [Header("Unlock Data")]
    public List<int> unlockIndexes;

    [Header("Sprites")]
    public List<Sprite> unlockedSprites;

    [Header("Prefabs")]
    public GameObject buttonPrefab;  // prefab normal
    public GameObject lockedPrefab;  // prefab locked

    [Header("Spawn Parent")]
    public Transform buttonParent;

    void Awake()
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
        GenerateButtons();
    }

    public void GenerateButtons()
    {
        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < unlockIndexes.Count; i++)
        {
            int localIndex = i;
            bool isUnlocked = GameManager.Instance.IsUnlocked(unlockIndexes[i]);

            GameObject obj;

            // =====================================================
            // UNLOCKED
            // =====================================================
            if (isUnlocked)
            {
                obj = Instantiate(buttonPrefab, buttonParent);

                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                sr.sprite = unlockedSprites[i];

                // langsung ADD component clicker
                var clicker = obj.AddComponent<CreatureButtonClick>();
                clicker.index = unlockIndexes[localIndex];
            }
            // =====================================================
            // LOCKED
            // =====================================================
            else
            {
                obj = Instantiate(lockedPrefab, buttonParent);
            }
        }
        Instantiate(lockedPrefab, buttonParent);
        Instantiate(lockedPrefab, buttonParent);
        Instantiate(lockedPrefab, buttonParent);
    }
}
