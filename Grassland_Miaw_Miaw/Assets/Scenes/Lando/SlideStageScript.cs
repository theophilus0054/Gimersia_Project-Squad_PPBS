using UnityEngine;
using System;
using System.Collections;

public class SlideStageScript : MonoBehaviour
{
    public static SlideStageScript Instance { get; private set; }
    private static Camera cam;

    [Header("Slide Settings")]
    private GameObject target;

    // ===================================================
    // 🧩 Singleton Setup
    // ===================================================
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Debug.Log($"✅ {name} diset untuk tetap hidup antar scene (tutorial sudah selesai).");
    }

    private void EnsureManager()
    {
        if (cam == null)
            cam = Camera.main;
    }

    // ===================================================
    // 🎬 STATIC ENTRY POINTS
    // ===================================================
    public void SlidePlay(GameObject targetObj, float distance = 5f, float duration = 2f, bool easeOutExit = true, Action onComplete = null)
    {
        EnsureManager();
        StartCoroutine(SlideSequence(targetObj, distance, duration, easeOutExit, onComplete));
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
