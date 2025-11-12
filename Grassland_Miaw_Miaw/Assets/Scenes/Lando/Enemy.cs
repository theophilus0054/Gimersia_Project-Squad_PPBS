using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float maxHP = 50f;
    public float atk = 10f;
    public float moveSpeed = 1f;
    public int enemyIndex = 0;
    public int baseCoin = 10;
    public int pointProgression = 1;

    private float hp;
    private Rigidbody2D rb;
    public bool getProgress = true;
    public bool dropItemOnDeath = true;
    private bool isDead = false;

    // --- Flicker variables ---
    private SpriteRenderer spriteRenderer;
    private Material originalMaterials;

    [Header("Damage Flash Settings")]
    public Material flashMaterial; // assign shader flicker di sini
    public Color flashColor = Color.white;
    public float flashDuration = 0.2f;

    private Coroutine flashRoutine;

    // --- Slow Effect Variables ---
    private float originalMoveSpeed;
    private Coroutine slowCoroutine;
    private bool isSlowed = false;

    void Awake()
    {
        // --- Data enemy ---
        EnemyData data = EnemyManager.Instance.GetEnemy(enemyIndex);
        maxHP = StageManager.Instance.GetScaledHP(data.hp);
        atk = StageManager.Instance.GetScaledATK(data.atk);
        baseCoin = StageManager.Instance.GetScaledCoins(data.baseCoin);
        moveSpeed = data.movementSpeed;
        hp = maxHP;

        // --- Komponen ---
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // --- Material array ---
        originalMaterials = spriteRenderer.material;

        // --- Store original speed ---
        originalMoveSpeed = moveSpeed;
    }

    void Update()
    {
        if (!isDead)
            MoveLeft();
    }

    void MoveLeft()
    {
        var atkComp = GetComponent<EnemyAttack>();
        if (atkComp != null && atkComp.enemiesInRange.Count > 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
    }

    // ============================================================
    // 💥 DAMAGE + FLASH + SLOW EFFECT
    // ============================================================
    bool once = false;
    public void TakeDamage(float amount, Effect status)
    {
        if (IsDead) return;

        hp -= amount;
        Debug.Log($"Enemy {name} took {amount} damage. ({hp}/{maxHP})");

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashEffect());

        // Apply slow effect jika status adalah Slow
        if (status == Effect.Slow)
        {
            ApplySlowEffect(3f, 0.3f); // 5 detik, 30% slow
        }

        if (hp <= 0 && !once)
        {
            once = true;
            if (dropItemOnDeath)
            {
                if (getProgress)
                {
                    GameManager.Instance.addStageProgress(pointProgression);
                    StageManager.Instance.UpdateTargetAchieved();
                }
                ObjectManager.Instance.SummonCoin(gameObject, Random.Range(baseCoin, Mathf.RoundToInt(baseCoin * 1.3f)));
            }
            GetComponent<Animator>()?.SetTrigger("isDead");
            AudioManager.Instance.PlayEnemyDead(gameObject.GetComponent<AudioSource>());
            GetComponent<Collider2D>().enabled = false;
            isDead = true;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    // ============================================================
    // 🐌 SLOW EFFECT SYSTEM (NON-STACKABLE)
    // ============================================================
    public void ApplySlowEffect(float duration, float slowPercentage)
    {
        // Jika sudah dalam keadaan slowed, jangan apply slow baru
        if (isSlowed)
        {
            Debug.Log($"[Slow] Enemy already slowed, ignoring new slow effect");
            return;
        }

        // Start slow coroutine
        if (slowCoroutine != null)
            StopCoroutine(slowCoroutine);
        
        slowCoroutine = StartCoroutine(SlowRoutine(duration, slowPercentage));
    }

    private IEnumerator SlowRoutine(float duration, float slowPercentage)
    {
        isSlowed = true;
        SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
        sr.color = new Color(0.9f, 0.9f, 1f);
        
        // Apply slow
        float slowMultiplier = 1f - slowPercentage;
        moveSpeed = originalMoveSpeed * slowMultiplier;
        
        Debug.Log($"[Slow] Applied {slowPercentage * 100}% slow for {duration}s. Speed: {moveSpeed}");

        // Tunggu sampai duration selesai
        yield return new WaitForSeconds(duration);

        // Remove slow effect
        moveSpeed = originalMoveSpeed;
        isSlowed = false;
        slowCoroutine = null;
        sr.color = Color.white;
        
        Debug.Log($"[Slow] Slow effect ended. Speed restored to: {moveSpeed}");
    }

    // Method untuk force remove slow effect (jika diperlukan)
    public void RemoveSlowEffect()
    {
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
            slowCoroutine = null;
        }
        
        moveSpeed = originalMoveSpeed;
        isSlowed = false;
        Debug.Log($"[Slow] Slow effect forcibly removed. Speed: {moveSpeed}");
    }

    // ============================================================
    // ✨ FLASH EFFECT
    // ============================================================
    private IEnumerator FlashEffect()
    {
        spriteRenderer.material = originalMaterials;

        originalMaterials.SetFloat("_FlashAmount", 1f);

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float flashAmount = 1f - (elapsed / flashDuration); // dari 1 ke 0
            originalMaterials.SetFloat("_FlashAmount", flashAmount);
            yield return null;
        }

        originalMaterials.SetFloat("_FlashAmount", 0f);
        flashRoutine = null;
    }

    // ============================================================
    // ☠️ DEATH
    // ============================================================
    public bool IsDead => isDead;

    public IEnumerator Die()
    {
        // Hentikan semua effect ketika mati
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
            slowCoroutine = null;
        }
        isSlowed = false;
        moveSpeed = originalMoveSpeed;

        int duration = 1; // durasi fade out dalam detik
        float elapsed = 0f;
        Color originalColor = spriteRenderer.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }

    // ============================================================
    // 🔧 PUBLIC METHODS FOR EXTERNAL ACCESS
    // ============================================================
    public bool IsCurrentlySlowed()
    {
        return isSlowed;
    }

    public float GetCurrentMoveSpeed()
    {
        return moveSpeed;
    }

    public float GetOriginalMoveSpeed()
    {
        return originalMoveSpeed;
    }

    void OnDisable()
    {
        // Cleanup coroutines ketika object dinonaktifkan
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
            slowCoroutine = null;
        }
        isSlowed = false;
    }
}