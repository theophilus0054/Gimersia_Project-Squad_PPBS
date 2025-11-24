using UnityEngine;
using System.Collections;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("Audio Clips")]
    public AudioClip music1;
    public AudioClip music2;

    [Header("Fade Settings")]
    public float fadeDuration = 2f;

    public AudioSource audioSource1;
    public AudioSource audioSource2;
    Coroutine isPlaying;

    private bool isPlayingFirst = true;
    private bool isTransitioning = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        audioSource1.loop = true;
        audioSource2.loop = true;

        audioSource1.clip = music1;
        audioSource2.clip = music2;

        // mulai musik pertama
        audioSource1.volume = 1f;
        audioSource1.Play();
        audioSource2.volume = 0f;
    }

    public void ToggleBGM()
    {
        if (isTransitioning){
            if (isPlayingFirst)
            {
                audioSource1.Stop();
                if(!audioSource2.isPlaying)
                    audioSource2.Play();
                audioSource1.volume = 0f;
                audioSource2.volume = 0f;
            } 
            else 
            {
                audioSource2.Stop();
                if(!audioSource1.isPlaying)
                    audioSource1.Play();
                audioSource2.volume = 0f;
                audioSource1.volume = 0f;
            }
            StopCoroutine(isPlaying);
        }

        if (isPlayingFirst)
        {
            isPlaying = StartCoroutine(FadeOutThenIn(audioSource1, audioSource2));
        }
        else
        {
            isPlaying = StartCoroutine(FadeOutThenIn(audioSource2, audioSource1));
        }

        isPlayingFirst = !isPlayingFirst;
    }

    private IEnumerator FadeOutThenIn(AudioSource from, AudioSource to)
    {
        isTransitioning = true;

        // Fade out musik pertama
        float timer = 0f;
        float startVolume = from.volume;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            from.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            yield return null;
        }

        from.volume = 0f;
        from.Stop();

        RippleManager.Instance.callShockwave();
        if (!isPlayingFirst)
        {
            AudioManager.Instance.PlayWaveSplash();
            SlideStageScript.Instance.SlidePlay(UIManager.Instance.waveNotificationPanel, 20f, 2f, false);
            yield return new WaitForSeconds(2f);
        }

        // Mulai musik berikutnya
        to.volume = 0f;
        to.Play();

        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            to.volume = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }

        to.volume = 1f;
        isTransitioning = false;
    }
}
