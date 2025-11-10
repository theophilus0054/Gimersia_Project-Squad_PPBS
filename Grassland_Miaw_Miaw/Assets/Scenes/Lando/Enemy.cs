using UnityEngine;
using System.Collections;
using UnityEditor.SceneManagement;
using System.Security.Cryptography;

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
    // 💥 DAMAGE + FLASH
    // ============================================================
    bool once = false;
    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        hp -= amount;
        Debug.Log($"Enemy {name} took {amount} damage. ({hp}/{maxHP})");

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashEffect());

        if (hp <= 0 && !once)
        {
            once = true;
            if (dropItemOnDeath)
            {
                if(getProgress)
                {
                    GameManager.Instance.addStageProgress(pointProgression);
                    StageManager.Instance.UpdateTargetAchieved();
                }
                GameManager.Instance.AddCoins(Random.Range(baseCoin, Mathf.RoundToInt(baseCoin * 1.3f)));
            }
            GetComponent<Animator>()?.SetTrigger("isDead");
            GetComponent<Collider2D>().enabled = false;
            isDead = true;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

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
}
