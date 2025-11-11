using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingBarSprite : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform barFillTransform; // Sprite fill object

    [Header("Settings")]
    [SerializeField] private float fillSpeed = 3f;  // how smooth the fill animation is
    [SerializeField] private float startX = -3f;    // position at 0% (fully empty)
    [SerializeField] private float endX = 3f;       // position at 100% (fully filled)

    private float targetProgress = 0f;
    private float currentProgress = 0f;

    void Start()
    {
        if (barFillTransform == null)
        {
            Debug.LogError("LoadingBarSprite: 'barFillTransform' is not assigned!");
            return;
        }

        // Start from 0 progress
        SetProgress(0f);
    }

    void Update()
    {
        // Smoothly approach the target fill value
        currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * fillSpeed);

        // Calculate new X position between start and end
        float newX = Mathf.Lerp(startX, endX, currentProgress);

        // Apply the position smoothly
        Vector3 pos = barFillTransform.localPosition;
        pos.x = newX;
        barFillTransform.localPosition = pos;
    }

    /// <summary>
    /// Sets the fill progress (0.0 empty → 1.0 full)
    /// </summary>
    public void SetProgress(float progress)
    {
        targetProgress = Mathf.Clamp01(progress);
    }
}
