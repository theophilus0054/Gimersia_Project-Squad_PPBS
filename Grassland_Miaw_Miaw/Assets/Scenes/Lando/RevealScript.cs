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
    public float clickFadeDuration = 1f;
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

    // ====================================================
    // Cached components
    // ====================================================
    private CreatureAttack cachedAttack;
    private DragScript cachedDrag;

    // ====================================================
    // Init
    // ====================================================
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

    // ====================================================
    // Summon helper
    // ====================================================
    public GameObject SummonPrefab(GameObject prefab, Vector3 spawnPos)
    {
        GameObject icon = Instantiate(prefab, spawnPos, Quaternion.identity);

        icon.transform.localScale = Vector3.one;
        icon.transform.rotation = Quaternion.identity;
        icon.transform.position = spawnPos;

        return icon;
    }

    // ====================================================
    // PUBLIC API
    // ====================================================
    public void RevealPlay(GameObject targetObj, Action onComplete = null)
    {
        EnsureManager();

        if (currentRoutine != null)
        {
            InputLockManager.Instance.UnlockInput();
            StopCoroutine(currentRoutine);
            ResetTargetState();
        }

        CacheAndDisableComponents(targetObj);
        currentRoutine = StartCoroutine(RevealSequence(targetObj, onComplete));
    }

    public void RevealPlayInstantMove(GameObject targetObj, Action onComplete = null)
    {
        EnsureManager();

        if (currentRoutine != null)
        {
            InputLockManager.Instance.UnlockInput();
            StopCoroutine(currentRoutine);
            ResetTargetState();
        }

        CacheAndDisableComponents(targetObj);
        currentRoutine = StartCoroutine(RevealSequenceInstantMove(targetObj, onComplete));
    }

    public void RevealPlayInstantMoveDied(GameObject targetObj, Action onComplete = null)
    {
        EnsureManager();

        if (currentRoutine != null)
        {
            InputLockManager.Instance.UnlockInput();
            StopCoroutine(currentRoutine);
            ResetTargetState();
        }

        CacheAndDisableComponents(targetObj);
        currentRoutine = StartCoroutine(RevealSequenceInstantMoveDied(targetObj, onComplete));
    }

    // ====================================================
    // COMPONENT CACHE/RESTORE
    // ====================================================
    private void CacheAndDisableComponents(GameObject obj)
    {
        cachedAttack = obj.GetComponent<CreatureAttack>();
        cachedDrag = obj.GetComponent<DragScript>();

        if (cachedAttack != null)
        {
            cachedAttack.enabled = false;
        }

        if (cachedDrag != null)
        {
            cachedDrag.enabled = false;
            cachedDrag.col.enabled = false;
        }
    }

    private void RestoreComponents()
    {
        if (cachedAttack != null)
            cachedAttack.enabled = true;

        if (cachedDrag != null)
        {
            cachedDrag.enabled = true;
            cachedDrag.col.enabled = true;
        }
    }

    // ====================================================
    // RESET
    // ====================================================
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

        // 🔥 restore komponen jika coroutine dihentikan
        RestoreComponents();
    }

    // ====================================================
    // INSTANT MOVE
    // ====================================================
    private IEnumerator RevealSequenceInstantMove(GameObject targetObj, Action onComplete)
    {
        InputLockManager.Instance.LockInput();
        target = targetObj;
        onCompleteCallback = onComplete;

        spriteRenderer = target.GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        originalPos = target.transform.position;
        originalScale = target.transform.localScale;

        spriteRenderer.color = Color.black;

        target.transform.position = unlockSummon.position;

        target.transform.DOScale(originalScale * scaleMultiplier, moveDuration)
            .SetEase(Ease.OutBack);

        yield return new WaitForSeconds(moveDuration);

        yield return StartCoroutine(OnReveal());
        yield return StartCoroutine(ReturnToOriginal());

        RestoreComponents(); // restore normal
        onCompleteCallback?.Invoke();
        currentRoutine = null;
        InputLockManager.Instance.UnlockInput();
        CreatureButtonManager.Instance.GenerateButtons();
    }

    private IEnumerator RevealSequenceInstantMoveDied(GameObject targetObj, Action onComplete)
    {
        InputLockManager.Instance.LockInput();
        target = targetObj;
        onCompleteCallback = onComplete;

        spriteRenderer = target.GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        originalPos = target.transform.position;
        originalScale = target.transform.localScale;

        spriteRenderer.color = Color.black;

        target.transform.position = unlockSummon.position;

        target.transform.DOScale(originalScale * scaleMultiplier, moveDuration)
            .SetEase(Ease.OutBack);

        yield return new WaitForSeconds(moveDuration);

        yield return StartCoroutine(OnReveal());

        RestoreComponents(); // restore normal
        onCompleteCallback?.Invoke();
        currentRoutine = null;
        InputLockManager.Instance.UnlockInput();
        CreatureButtonManager.Instance.GenerateButtons();
        Destroy(target);
    }

    // ====================================================
    // MOVE + SCALE
    // ====================================================
    private IEnumerator RevealSequence(GameObject targetObj, Action onComplete)
    {
        InputLockManager.Instance.LockInput();
        target = targetObj;
        onCompleteCallback = onComplete;

        spriteRenderer = target.GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        originalPos = target.transform.position;
        originalScale = target.transform.localScale;

        spriteRenderer.color = Color.black;

        target.transform.DOMove(unlockSummon.position, moveDuration).SetEase(Ease.InOutSine);
        target.transform.DOScale(originalScale * scaleMultiplier, moveDuration).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(moveDuration);

        yield return StartCoroutine(OnReveal());
        yield return StartCoroutine(ReturnToOriginal());

        RestoreComponents();
        onCompleteCallback?.Invoke();
        currentRoutine = null;
        InputLockManager.Instance.UnlockInput();
        TutorialManager.Instance?.CompleteTutorial(15);
    }

    // ====================================================
    // EFFECTS
    // ====================================================
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
