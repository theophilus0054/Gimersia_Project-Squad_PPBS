using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    [Header("Bar Objects")]
    [SerializeField] private Transform backgroundTransform; // BarBackground
    [SerializeField] private Transform fillTransform;       // BarFill
    [SerializeField] private Transform iconTransform;       // ProgressIcon
    [SerializeField] private float barWidth = 3f;           // lebar penuh bar
    [SerializeField] private float backgroundOffsetX = -1f; // offset X dari background

    [Header("Animation")]
    [SerializeField] private float smoothSpeed = 5f;

    void Update()
    {
        if (GameManager.Instance == null || fillTransform == null)
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
        Vector3 fillPos = fillTransform.localPosition;
        fillPos.x = backgroundOffsetX - barWidth / 2f + localScale.x / 2f;
        fillTransform.localPosition = fillPos;

        // Icon di ujung bar
        if (iconTransform != null)
        {
            Vector3 iconPos = iconTransform.localPosition;
            iconPos.x = backgroundOffsetX - barWidth / 2f + localScale.x;
            iconTransform.localPosition = iconPos;
        }
    }
}