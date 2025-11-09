using UnityEngine;
using System;
using System.Collections;

public class AnimationScript : MonoBehaviour
{
    private static AnimationScript instance; 
    private static Camera cam;

    [Header("Default Settings")]
    public float moveDuration = 1.5f;
    public float scaleMultiplier = 2f;
    public float clickFadeDuration = 1f;
    public float shakeDuration = 0.5f;
    public float shakeAngle = 10f;
    public GameObject whiteLightPrefab;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 originalPos;
    private Vector3 originalScale;
    private GameObject target;
    private bool clicked = false;
    private Action onCompleteCallback;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ===================================================
    // 📢 STATIC ENTRY POINT
    // ===================================================
    public static void Play(GameObject targetObj, Action onComplete = null)
    {
        // Pastikan manager ada
        if (instance == null)
        {
            GameObject mgr = new GameObject("AnimationScriptManager");
            instance = mgr.AddComponent<AnimationScript>();
            DontDestroyOnLoad(mgr);
        }

        if (cam == null)
            cam = Camera.main;

        instance.StartCoroutine(instance.RevealSequence(targetObj, onComplete));
    }

    // ===================================================
    // 🎬 MAIN SEQUENCE
    // ===================================================
    private IEnumerator RevealSequence(GameObject targetObj, Action onComplete)
    {
        target = targetObj;
        onCompleteCallback = onComplete;

        spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("AnimationScript: Target tidak memiliki SpriteRenderer!");
            yield break;
        }

        originalColor = spriteRenderer.color;
        originalPos = target.transform.position;
        originalScale = target.transform.localScale;
        clicked = false;

        spriteRenderer.color = Color.black;

        // 1️⃣ Pindah ke tengah & membesar
        Vector3 centerPos = cam.ScreenToWorldPoint(
            new Vector3(Screen.width / 2, Screen.height / 2, -cam.transform.position.z)
        );

        float t = 0f;
        Vector3 startPos = target.transform.position;
        Vector3 startScale = target.transform.localScale;
        Vector3 endScale = startScale * scaleMultiplier;

        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, t / moveDuration);
            target.transform.position = Vector3.Lerp(startPos, centerPos, progress);
            target.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            yield return null;
        }

        // 2️⃣ Jadi siluet hitam

        // 3️⃣ Tunggu klik pada target
        while (!clicked)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
                Collider2D hit = Physics2D.OverlapPoint(mousePos);

                if (hit && hit.gameObject == target)
                {
                    clicked = true;
                    break;
                }
            }
            yield return null;
        }

        // 4️⃣ Efek klik (reveal + shake)
        yield return StartCoroutine(OnClickReveal());

        // 5️⃣ Kembali ke posisi semula
        yield return StartCoroutine(ReturnToOriginal());

        // ✅ Callback selesai
        onCompleteCallback?.Invoke();
    }

    private IEnumerator OnClickReveal()
    {
        // 🔆 ENABLE EXISTING WHITE LIGHT
        SpriteRenderer lightSprite = null;

        if (whiteLightPrefab != null)
        {
            whiteLightPrefab.SetActive(true);
            lightSprite = whiteLightPrefab.GetComponent<SpriteRenderer>();
        }

        if (lightSprite != null)
        {
            Color lightColor = lightSprite.color;
            lightColor.a = 0f;
            lightSprite.color = lightColor;

            // 🌟 Fade IN
            float fadeInTime = 0f;
            float fadeInDuration = clickFadeDuration * 0.5f; // first half of time for fade-in
            while (fadeInTime < fadeInDuration)
            {
                fadeInTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, fadeInTime / fadeInDuration);
                lightColor.a = alpha;
                lightSprite.color = lightColor;
                yield return null;
            }

            // 💫 Flicker effect (optional, smoother than PingPong)
            float time = 0f;
            while (time < clickFadeDuration)
            {
                time += Time.deltaTime;
                float alpha = 1f - Mathf.PingPong(time * 2f, 0.5f); // flicker range 0.5–1
                lightColor.a = alpha;
                lightSprite.color = lightColor;
                yield return null;
            }

            // 🔅 Fade OUT
            float fadeOutTime = 0f;
            while (fadeOutTime < fadeInDuration)
            {
                fadeOutTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, fadeOutTime / fadeInDuration);
                lightColor.a = alpha;
                lightSprite.color = lightColor;
                yield return null;
            }
        }

        // 🌈 Reveal original color
        float t = 0f;
        while (t < clickFadeDuration)
        {
            t += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(Color.black, originalColor, t / clickFadeDuration);
            yield return null;
        }
        spriteRenderer.color = originalColor;

        // 🌀 Shake
        yield return StartCoroutine(ShakeObject());

        // 🔻 Turn off after complete
        whiteLightPrefab.SetActive(false);
    }

    private IEnumerator ShakeObject()
    {
        float t = 0f;
        Quaternion originalRot = target.transform.rotation;

        while (t < shakeDuration)
        {
            t += Time.deltaTime;
            float angle = Mathf.Sin(t * Mathf.PI * 6f) * shakeAngle;
            target.transform.rotation = originalRot * Quaternion.Euler(0, 0, angle);
            yield return null;
        }

        target.transform.rotation = originalRot;
    }

    private IEnumerator ReturnToOriginal()
    {
        float t = 0f;
        Vector3 startPos = target.transform.position;
        Vector3 startScale = target.transform.localScale;

        while (t < moveDuration * 0.7f)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, t / (moveDuration * 0.7f));
            target.transform.position = Vector3.Lerp(startPos, originalPos, progress);
            target.transform.localScale = Vector3.Lerp(startScale, originalScale, progress);
            yield return null;
        }

        target.transform.position = originalPos;
        target.transform.localScale = originalScale;
    }
}
