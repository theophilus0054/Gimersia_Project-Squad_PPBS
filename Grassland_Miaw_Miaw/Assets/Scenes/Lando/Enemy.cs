using System.Drawing;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float maxHP = 50f;
    public float atk = 10f;
    public float moveSpeed = 1f; // kecepatan gerak ke kiri
    public int enemyIndex = 0;
    public int pointProgression = 1;

    private float hp;
    private Rigidbody2D rb;
    private bool isDead = false;

    void Awake()
    {
        EnemyData data = EnemyManager.Instance.GetEnemy(enemyIndex);
        maxHP = data.hp;
        atk = data.atk;
        moveSpeed = data.movementSpeed;
        hp = maxHP;
        rb = GetComponent<Rigidbody2D>();

        // pastikan tidak jatuh karena gravitasi
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (!isDead)
        {
            MoveLeft();
        }
    }

    void MoveLeft()
    {
        if(GetComponent<EnemyAttack>() != null && GetComponent<EnemyAttack>().enemiesInRange.Count > 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        // gerak konstan ke kiri
        rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
    }
 
    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        hp -= amount;
        Debug.Log($"Enemy {name} took {amount} damage.");
        if (hp <= 0)
        {
            GameManager.Instance.addStageProgress(pointProgression);
            Die();
        }
    }

    public bool IsDead => isDead;

    void Die()
    {
        isDead = true;

        // hentikan gerak
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // bisa tambah animasi atau efek di sini
        Destroy(gameObject, 0.2f); // beri delay sedikit biar bisa main animasi
    }
}
