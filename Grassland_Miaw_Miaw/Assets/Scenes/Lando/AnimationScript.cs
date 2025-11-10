using UnityEngine;
using System;
using System.Collections;

public class AnimationScript : MonoBehaviour
{
    private static AnimationScript instance;
    private static Camera cam;

    [Header("Reveal Settings")]
    public float moveDuration = 1.5f;
    public float scaleMultiplier = 2f;
    public float clickFadeDuration = 1f;
    public float shakeDuration = 0.5f;
    public float shakeAngle = 10f;
    public float animationZOffset = -5f;
    public GameObject whiteLightPrefab;

    [Header("Slide Settings")]
    public float slideDuration = 1.5f;
    public float cooldownDuration = 3f;
    public float xOffset = 5f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 originalPos;
    private Vector3 originalScale;
    private GameObject target;
    private bool clicked = false;
    private Action onCompleteCallback;

    // ===================================================
    // 🧩 Singleton Setup
    // ===================================================
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

    private static void EnsureManager()
    {
        if (instance == null)
        {
            GameObject mgr = new GameObject("AnimationScriptManager");
            instance = mgr.AddComponent<AnimationScript>();
            DontDestroyOnLoad(mgr);
        }

        if (cam == null)
            cam = Camera.main;
    }

    // ===================================================
    // 🎬 STATIC ENTRY POINTS
    // ===================================================
    public static void RevealPlay(GameObject targetObj, Action onComplete = null)
    {
        EnsureManager();
        instance.StartCoroutine(instance.RevealSequence(targetObj, onComplete));
    }

    public static void SlidePlay(GameObject targetObj, float distance = 5f, float duration = 2f, bool easeOutExit = true, Action onComplete = null)
    {
        EnsureManager();
        instance.StartCoroutine(instance.SlideSequence(targetObj, distance, duration, easeOutExit, onComplete));
    }

    // ===================================================
    // 🎥 REVEAL ANIMATION
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

        // Pindah ke tengah & membesar
        Vector3 centerPos = cam.ScreenToWorldPoint(
            new Vector3(Screen.width / 2, Screen.height / 2, -cam.transform.position.z)
        );
        centerPos.z = animationZOffset;

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

        // Tunggu klik
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

        // Efek klik (reveal)
        yield return StartCoroutine(OnClickReveal());

        // Kembali ke posisi semula
        yield return StartCoroutine(ReturnToOriginal());

        onCompleteCallback?.Invoke();
    }

    private IEnumerator OnClickReveal()
    {
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

            float fadeInTime = 0f;
            float fadeInDuration = clickFadeDuration * 0.5f;
            while (fadeInTime < fadeInDuration)
            {
                fadeInTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, fadeInTime / fadeInDuration);
                lightColor.a = alpha;
                lightSprite.color = lightColor;
                yield return null;
            }

            float time = 0f;
            while (time < clickFadeDuration)
            {
                time += Time.deltaTime;
                float alpha = 1f - Mathf.PingPong(time * 2f, 0.5f);
                lightColor.a = alpha;
                lightSprite.color = lightColor;
                yield return null;
            }

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

        float t = 0f;
        while (t < clickFadeDuration)
        {
            t += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(Color.black, originalColor, t / clickFadeDuration);
            yield return null;
        }
        spriteRenderer.color = originalColor;

        yield return StartCoroutine(ShakeObject());
        whiteLightPrefab?.SetActive(false);
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

    // ===================================================
    // 🎞️ SLIDE ANIMATION
    // ===================================================

    private IEnumerator SlideSequence(GameObject targetObj, float distance, float duration, bool easeOutExit, Action onComplete)
    {
        target = targetObj;
        Vector3 start = target.transform.position;
        Vector3 middle = target.transform.position - Vector3.right * distance;
        Vector3 end = target.transform.position - (Vector3.right * distance * 2f);

        // Geser kanan → tengah (cepat → lambat)
        float t = 0f;
        while (t < duration / 2f)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, t / (duration / 2f));
            target.transform.position = Vector3.Lerp(start, middle, progress);
            yield return null;
        }

        yield return new WaitForSeconds(1f); // cooldown di tengah

        // Tengah → kiri (pilihan gaya easing)
        t = 0f;
        while (t < duration / 2f)
        {
            t += Time.deltaTime;

            float progress;
            if (easeOutExit)
                progress = Mathf.SmoothStep(0, 1, t / (duration / 2f)); // cepat → lambat
            else
                progress = Mathf.Pow(t / (duration / 2f), 2f); // lambat → cepat (dramatis)

            target.transform.position = Vector3.Lerp(middle, end, progress);
            yield return null;
        }

        target.transform.position = end;
        target.transform.position = start;
        if(target.name == "WaveFailedFrame")
        {
            UIManager.Instance.WaveStagePanel.GetComponent<SlideButton>().ActiveButton();
        }
        onComplete?.Invoke();
    }


    private IEnumerator MoveObject(GameObject obj, Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, t / duration);
            obj.transform.position = Vector3.Lerp(from, to, progress);
            yield return null;
        }
        obj.transform.position = to;
    }
}
