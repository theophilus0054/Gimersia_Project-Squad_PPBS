using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Clips")]
    public AudioClip bubblePop;
    public AudioClip unagiBubble;
    public AudioClip enemyDead;

    [Header("Pitch Settings")]
    [Range(0.5f, 2f)] public float minPitch = 0.95f;
    [Range(0.5f, 2f)] public float maxPitch = 1.05f;

    [Header("Limiter Settings")]
    [Tooltip("Minimum time (in seconds) between identical clip plays")]
    public float sameClipCooldown = 0.2f;
    [Tooltip("Volume multiplier when several same clips play close together")]
    [Range(0f, 1f)] public float overlapVolumeScale = 0.7f;

    private Dictionary<AudioClip, float> lastPlayTimes = new Dictionary<AudioClip, float>();

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Plays an AudioClip at the given AudioSource with random pitch and limiter.
    /// </summary>
    public void Play(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null) return;

        // Limiter: prevent same sound from spamming too fast
        if (lastPlayTimes.TryGetValue(clip, out float lastTime))
        {
            if (Time.time - lastTime < sameClipCooldown)
                return; // skip if too soon
        }

        lastPlayTimes[clip] = Time.time;

        // Random pitch
        source.pitch = Random.Range(minPitch, maxPitch);

        // Adjust volume if many instances play close together
        float elapsed = Time.time - lastTime;
        float volume = (elapsed < sameClipCooldown * 2f) ? overlapVolumeScale : 1f;

        source.PlayOneShot(clip, volume);
    }

    public void Play(AudioClip clip)
    {
        AudioSource source = gameObject.GetComponent<AudioSource>();
        if (source == null || clip == null) return;

        // Limiter: prevent same sound from spamming too fast
        if (lastPlayTimes.TryGetValue(clip, out float lastTime))
        {
            if (Time.time - lastTime < sameClipCooldown)
                return; // skip if too soon
        }

        lastPlayTimes[clip] = Time.time;

        // Random pitch
        source.pitch = Random.Range(minPitch, maxPitch);

        // Adjust volume if many instances play close together
        float elapsed = Time.time - lastTime;
        float volume = (elapsed < sameClipCooldown * 2f) ? overlapVolumeScale : 1f;

        source.PlayOneShot(clip, volume);
    }

    // --- Shortcut methods ---
    public void PlayBubble(AudioSource source) => Play(bubblePop);
    public void PlayUnagiBubble(AudioSource source) => Play(source, unagiBubble);
    public void PlayEnemyDead(AudioSource source) => Play(source, enemyDead);
}
