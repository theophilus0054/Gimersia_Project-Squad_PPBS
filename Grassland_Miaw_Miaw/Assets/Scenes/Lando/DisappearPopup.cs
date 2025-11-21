using UnityEngine;
using System.Collections;
using TMPro;

public class DisappearPopup : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.Instance.PlayDeniedInteraction();
        StartCoroutine(ActivateAndFadeOut(1f));
    }

    public IEnumerator ActivateAndFadeOut(float duration)
    {
        // --- Step 1: Aktifkan GameObject ---
        gameObject.transform.localScale = Vector3.one * 0.9f; // mulai dari 0.6
        CanvasGroup cg = gameObject.GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        // --- Step 2: Scale up ---
        float elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.1f);
            float scale = Mathf.Lerp(0.9f, 1f, t);
            gameObject.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        gameObject.transform.localScale = Vector3.one; // pastikan scale = 1

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration); // dari 1 ke 0
            cg.alpha = alpha;
            yield return null;
        }

        cg.alpha = 1;

        // --- Step 3: Deactivate GameObject ---
        Destroy(gameObject);
    }
}
