using UnityEngine;

[ExecuteAlways] // berjalan di Editor & Play Mode
public class LogoScript : MonoBehaviour
{
    [Header("Options")]
    public bool centerPosition = true;
    public bool centerRotation = false;
    public bool centerScale = false;

    [Header("Offset Tambahan")]
    public float addPositionX = 0f;
    public float addPositionY = 0f;

    void Update()
    {
        if (transform.parent == null) return;

        // Kalau parent UI (RectTransform)
        if (transform.parent is RectTransform parentRect)
        {
            if (centerPosition)
            {
                // hitung posisi tengah berdasarkan ukuran parent & pivot
                Vector2 parentSize = parentRect.rect.size;
                Vector2 pivotOffset = new Vector2(
                    (0.5f - parentRect.pivot.x) * parentSize.x,
                    (0.5f - parentRect.pivot.y) * parentSize.y
                );

                transform.localPosition = new Vector3(
                    pivotOffset.x + addPositionX,
                    pivotOffset.y + addPositionY,
                    0f
                );
            }
        }
        else
        {
            // fallback kalau bukan UI object
            if (centerPosition)
                transform.localPosition = new Vector3(addPositionX, addPositionY, 0f);
        }

        if (centerRotation)
            transform.localRotation = Quaternion.identity;

        if (centerScale)
            transform.localScale = Vector3.one;
    }
}
