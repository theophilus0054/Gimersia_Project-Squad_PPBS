using UnityEngine;

public class DropArea : MonoBehaviour, IDragDrop
{
    [SerializeField] public int x;
    [SerializeField] public int y;
    [SerializeField] bool filled = false;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Color filledColor = new Color(0.5f, 0.5f, 0.5f, 0f);

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"{name}: SpriteRenderer not found!");
        }
        else
        {
            originalColor = spriteRenderer.color;
        }
    }

    public int GetX() => x;
    public int GetY() => y;
    public bool getFilled() => filled;

    public void OnItemDrop(DragScript drop, int evoIndex)
    {
        drop.transform.position = transform.position;
        GameManager.Instance.SetGridCell(x, y, evoIndex);
        filled = true;
        UpdateColor();
    }

    public void OnItemLeave(DragScript drop)
    {
        filled = false;
        GameManager.Instance.SetGridCell(x, y, -1);
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (spriteRenderer == null) return;

        spriteRenderer.color = filled ? filledColor : originalColor;
    }
}
