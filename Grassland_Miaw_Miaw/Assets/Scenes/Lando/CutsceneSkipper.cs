using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.UI;
using System.Collections;

public class CutsceneSkipper : MonoBehaviour
{
    [Header("UI Text")]
    public CanvasGroup confirmText; // Letakkan CanvasGroup pada teks “Tap again to skip”
    public float fadeDuration = 0.25f;
    public float confirmTime = 3f;

    [Header("Timeline")]
    public PlayableDirector director;

    private bool waitingConfirm = false;
    private float timer = 0f;

    void Start()
    {
        Time.timeScale = 1f;
        confirmText.alpha = 0;

        if (director != null)
            director.stopped += OnTimelineFinish;
    }

    void Update()
    {
        // klik kiri / screen tap
        if (Input.GetMouseButtonDown(0))
        {
            if (!waitingConfirm)
            {
                // Klik pertama
                waitingConfirm = true;
                timer = confirmTime;
                StartCoroutine(FadeText(confirmText, 1)); // Fade in
            }
            else
            {
                // Klik kedua → skip langsung
                LoadNextScene();
            }
        }

        // Hitung mundur konfirmasi
        if (waitingConfirm)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                waitingConfirm = false;
                StartCoroutine(FadeText(confirmText, 0)); // Fade out
            }
        }
    }

    private void OnTimelineFinish(PlayableDirector obj)
    {
        LoadNextScene();
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(1);
    }

    IEnumerator FadeText(CanvasGroup cg, float target)
    {
        float start = cg.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        cg.alpha = target;
    }
}
