using UnityEngine;

public class SpriteChanger : MonoBehaviour
{
    public string spriteName; // e.g. "Zombie"
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // auto-detect base name from current sprite if empty
        if (string.IsNullOrEmpty(spriteName) && spriteRenderer.sprite != null)
            spriteName = spriteRenderer.sprite.name;
    }

    // called from Animation Event
    public void SetSprite()
    {
        // path: Assets/Resources/Creature/Zombie_Dead.png
        string path = $"Creature/{spriteName}_Dead";

        Sprite newSprite = Resources.Load<Sprite>(path);

        if (newSprite == null)
        {
            Debug.LogWarning($"❌ Sprite not found at path: {path}");
            return;
        }

        spriteRenderer.sprite = newSprite;
        Debug.Log($"✅ {name}: Sprite changed to {newSprite.name}");
    }
}
