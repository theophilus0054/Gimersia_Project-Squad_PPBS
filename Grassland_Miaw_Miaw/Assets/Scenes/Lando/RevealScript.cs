using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;

public class RevealScript : MonoBehaviour
{
    public static RevealScript Instance { get; private set; }
    private static Camera cam;

    [Header("Reveal Settings")]
    public float moveDuration = 1.5f;
    public float scaleMultiplier = 2f;
    public float clickFadeDuration = 1f; // tetap dipakai untuk fade light & sprite
    public float shakeDuration = 0.5f;
    public float shakeAngle = 10f;
    public GameObject whiteLightPrefab;

    [Header("Target World Position")]
    public Transform unlockSummon;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 originalPos;
    private Vector3 originalScale;
    private GameObject target;

    private Coroutine currentRoutine;
    private Action onCompleteCallback;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void EnsureManager()
    {
        if (cam == null)
            cam = Camera.main;
    }

    public void RevealPlay(GameObject targetObj, Action onComplete = null)
    {
        EnsureManager();

        // Stop routine lama dan reset
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            ResetTargetState();
        }

        currentRoutine = StartCoroutine(RevealSequence(targetObj, onComplete));
    }

    private void ResetTargetState()
    {
        if (target == null) return;

        target.transform.DOKill();
        spriteRenderer?.DOKill();
        whiteLightPrefab?.transform.DOKill();

        target.transform.position = originalPos;
        target.transform.localScale = originalScale;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        if (whiteLightPrefab != null)
            whiteLightPrefab.SetActive(false);
    }

    private IEnumerator RevealSequence(GameObject targetObj, Action onComplete)
    {
        target = targetObj;
        onCompleteCallback = onComplete;

        spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Target tidak memiliki SpriteRenderer!");
            yield break;
        }

        originalColor = spriteRenderer.color;
        originalPos = target.transform.position;
        originalScale = target.transform.localScale;

        // Set awal warna hitam
        spriteRenderer.color = Color.black;

        if (unlockSummon == null)
        {
            Debug.LogError("unlockSummon belum di-assign!");
            yield break;
        }

        // ===== MOVE TO TARGET + SCALE =====
        target.transform.DOMove(unlockSummon.position, moveDuration).SetEase(Ease.InOutSine);
        target.transform.DOScale(originalScale * scaleMultiplier, moveDuration).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(moveDuration);

        // ===== REVEAL + SHAKE =====
        yield return StartCoroutine(OnReveal());

        // ===== RETURN TO ORIGINAL =====
        yield return StartCoroutine(ReturnToOriginal());

        onCompleteCallback?.Invoke();
        currentRoutine = null;
    }

    private IEnumerator OnReveal()
    {
        SpriteRenderer lightSprite = null;

        if (whiteLightPrefab != null)
        {
            whiteLightPrefab.SetActive(true);
            lightSprite = whiteLightPrefab.GetComponent<SpriteRenderer>();
        }

        AudioManager.Instance.PlayUnlockNewCreature();

        if (lightSprite != null)
        {
            lightSprite.color = new Color(1, 1, 1, 0);

            lightSprite.DOFade(1f, clickFadeDuration * 0.5f);
            yield return new WaitForSeconds(clickFadeDuration * 0.5f);

            lightSprite.DOFade(0.5f, 0.25f).SetLoops(4, LoopType.Yoyo);
            yield return new WaitForSeconds(1f);

            lightSprite.DOFade(0f, clickFadeDuration * 0.5f);
            yield return new WaitForSeconds(clickFadeDuration * 0.5f);
        }

        spriteRenderer.DOColor(originalColor, clickFadeDuration);
        yield return new WaitForSeconds(clickFadeDuration);

        yield return StartCoroutine(ShakeObject());

        whiteLightPrefab?.SetActive(false);
        TutorialManager.Instance?.CompleteTutorial(15);
    }

    private IEnumerator ShakeObject()
    {
        target.transform.DOShakeRotation(shakeDuration, new Vector3(0, 0, shakeAngle), 20, 90);
        yield return new WaitForSeconds(shakeDuration);
    }

    private IEnumerator ReturnToOriginal()
    {
        float returnDuration = moveDuration * 0.7f;

        target.transform.DOMove(originalPos, returnDuration).SetEase(Ease.InOutSine);
        target.transform.DOScale(originalScale, returnDuration).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(returnDuration);
    }
}
