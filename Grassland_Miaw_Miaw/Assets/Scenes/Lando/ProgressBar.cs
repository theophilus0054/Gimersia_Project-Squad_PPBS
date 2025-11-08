using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    [Header("Bar Objects")]
    [SerializeField] private Transform fillTransform;    // BarFill
    [SerializeField] private Transform iconTransform;    // ProgressIcon
    [SerializeField] private float barWidth = 5f;        // lebar penuh bar

    [Header("Animation")]
    [SerializeField] private float smoothSpeed = 5f;

    void Update()
    {
        if (GameManager.Instance == null || fillTransform == null || iconTransform == null)
            return;

        int current = GameManager.Instance.currentProgress;
        int target = GameManager.Instance.targetProgress;
        if (target <= 0) target = 1;

        float fillRatio = Mathf.Clamp01((float)current / target);
        float targetWidth = barWidth * fillRatio;

        // Smooth scale hanya di X
        Vector3 localScale = fillTransform.localScale;
        localScale.x = Mathf.Lerp(localScale.x, targetWidth, Time.deltaTime * smoothSpeed);
        fillTransform.localScale = localScale;

        // Geser posisi fill supaya tetap dari kiri
        Vector3 fillPos = fillTransform.position;
        fillPos.x = transform.position.x - barWidth / 2f + localScale.x / 2f;
        fillTransform.position = fillPos;

        // Icon di ujung bar
        if (iconTransform != null)
        {
            Vector3 iconPos = fillPos;
            iconPos.x += localScale.x / 2f;
            iconTransform.position = iconPos;
        }
    }
}
