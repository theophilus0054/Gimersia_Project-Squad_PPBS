using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Clips")]
    public AudioClip bubblePop;
    public AudioClip unagiBubble;
    public AudioClip crabPinch;
    public AudioClip puffBloat;
    public AudioClip enemyDead;
    public AudioClip buyInteraction;
    public AudioClip deniedInteraction;
    public AudioClip buttonClick;
    public AudioClip buttonHover;
    public AudioClip attackedCreature;
    public AudioClip deadCreature;
    public AudioClip mergeCreature;
    public AudioClip unlockNewCreature;
    public AudioClip pickupCreature;
    public AudioClip dropCreature;
    public AudioClip waveSplash;

    public AudioSource unlockedNewCreatureSource;
    public AudioSource wavesplashSource;

    [Header("Pitch Settings")]
    [Range(0.5f, 2f)] public float minPitch = 0.90f;
    [Range(0.5f, 2f)] public float maxPitch = 1.1f;

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

    // ============================================================
    // 🔊 GENERIC PLAY METHODS
    // ============================================================
    public void Play(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null) return;

        // Limiter
        if (lastPlayTimes.TryGetValue(clip, out float lastTime))
        {
            if (Time.time - lastTime < sameClipCooldown)
                return;
        }
        lastPlayTimes[clip] = Time.time;

        // Pitch & volume control
        source.pitch = Random.Range(minPitch, maxPitch);
        float elapsed = Time.time - lastTime;
        float volume = (elapsed < sameClipCooldown * 2f) ? overlapVolumeScale : 1f;

        source.PlayOneShot(clip, volume);
    }

    public void Play(AudioClip clip)
    {
        AudioSource source = gameObject.GetComponent<AudioSource>();
        if (source == null || clip == null) return;

        if (lastPlayTimes.TryGetValue(clip, out float lastTime))
        {
            if (Time.time - lastTime < sameClipCooldown)
                return;
        }
        lastPlayTimes[clip] = Time.time;

        source.pitch = Random.Range(minPitch, maxPitch);
        float elapsed = Time.time - lastTime;
        float volume = (elapsed < sameClipCooldown * 2f) ? overlapVolumeScale : 1f;

        source.PlayOneShot(clip, volume);
    }

    // ============================================================
    // 🧩 SHORTCUT METHODS
    // ============================================================
    public void PlayBubble(AudioSource source = null) => Play(source ?? GetMainSource(), bubblePop);
    public void PlayUnagiBubble(AudioSource source = null) => Play(source ?? GetMainSource(), unagiBubble);
    public void PlayCrabPinch(AudioSource source = null) => Play(source ?? GetMainSource(), crabPinch);
    public void PlayPuffBloat(AudioSource source = null) => Play(source ?? GetMainSource(), puffBloat);
    public void PlayEnemyDead(AudioSource source = null) => Play(source ?? GetMainSource(), enemyDead);
    public void PlayBuyInteraction(AudioSource source = null) => Play(source ?? GetMainSource(), buyInteraction);
    public void PlayDeniedInteraction(AudioSource source = null) => Play(source ?? GetMainSource(), deniedInteraction);
    public void PlayButtonClick(AudioSource source = null) => Play(source ?? GetMainSource(), buttonClick);
    public void PlayButtonHover(AudioSource source = null) => Play(source ?? GetMainSource(), buttonHover);

    public void PlayAttackedCreature(AudioSource source = null) => Play(source ?? GetMainSource(), attackedCreature);
    public void PlayDeadCreature(AudioSource source = null) => Play(source ?? GetMainSource(), deadCreature);
    public void PlayMergeCreature(AudioSource source = null) => Play(source ?? GetMainSource(), mergeCreature);
    public void PlayUnlockNewCreature(AudioSource source = null) => Play(source ?? unlockedNewCreatureSource, unlockNewCreature);
    public void PlayPickupCreature(AudioSource source = null) => Play(source ?? GetMainSource(), pickupCreature);
    public void PlayDropCreature(AudioSource source = null) => Play(source ?? GetMainSource(), dropCreature);
    public void PlayWaveSplash(AudioSource source = null) => Play(source ?? wavesplashSource, waveSplash);
    

    // ============================================================
    // 🔧 Utility
    // ============================================================
    private AudioSource GetMainSource()
    {
        AudioSource src = gameObject.GetComponent<AudioSource>();
        if (src == null) src = gameObject.AddComponent<AudioSource>();
        return src;
    }
}
