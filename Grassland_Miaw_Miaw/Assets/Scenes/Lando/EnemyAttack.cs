using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public GameObject projectilePrefab;
    public Transform fireOrigin;
    public float fireRate = 5f;

    [HideInInspector] public List<Transform> enemiesInRange = new List<Transform>();

    private float fireCooldown = 0f;
    private Animator animator;
    private int enemyIndex;
    private bool targetDetected = false;

    void Start()
    {
        // Animator reference
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogWarning($"{name}: Animator not found! Attack animations won't play.");

        // Enemy component check
        var enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError($"{name} is missing Enemy component!");
            enabled = false;
            return;
        }
        enemyIndex = enemy.enemyIndex;

        // EnemyManager check
        if (EnemyManager.Instance == null)
        {
            Debug.LogError("EnemyManager.Instance is NULL! Make sure it's in the scene!");
            enabled = false;
            return;
        }

        var enemyData = EnemyManager.Instance.GetEnemy(enemyIndex);
        if (enemyData == null)
        {
            Debug.LogError($"Enemy data for index {enemyIndex} not found!");
            enabled = false;
            return;
        }

        fireRate = enemyData.atkspd;
    }

    void Update()
    {
        Transform target = GetNearestCreature();

        if (target != null)
        {
            // Jika target baru terdeteksi, beri delay sebelum menyerang
            if (!targetDetected)
            {
                targetDetected = true;
                fireCooldown = 1f / fireRate; // delay pertama sebelum menyerang
            }

            fireCooldown -= Time.deltaTime;

            if (fireCooldown <= 0f)
            {
                FireAt(target);
                fireCooldown = 1f / fireRate; // reset cooldown setelah menyerang
            }
        }
        else
        {
            targetDetected = false; // reset flag jika tidak ada target
            fireCooldown = 0f;      // optional reset cooldown
        }
    }

    Transform GetNearestCreature()
    {
        Transform nearest = null;
        float bestDist = float.MaxValue;

        for (int i = enemiesInRange.Count - 1; i >= 0; i--)
        {
            if (enemiesInRange[i] == null)
            {
                Debug.LogWarning($"{name}: Creature at index {i} is null, removing from list.");
                enemiesInRange.RemoveAt(i);
                continue;
            }

            float d = Vector2.Distance(transform.position, enemiesInRange[i].position);
            if (d < bestDist)
            {
                bestDist = d;
                nearest = enemiesInRange[i];
            }
        }

        return nearest;
    }

    void FireAt(Transform target)
    {
        if (animator != null)
        {
            // Trigger animasi attack
            animator.SetTrigger("DetectCreature");
        }

        // Projectile akan muncul lewat Animation Event (ShootProjectile)
    }

    // ===== Fungsi yang dipanggil dari Animation Event =====
    public void ShootProjectile()
    {
        Transform target = GetNearestCreature();
        if (target == null)
            return;

        if (projectilePrefab == null || fireOrigin == null)
        {
            Debug.LogWarning($"{name}: Cannot spawn projectile. Missing prefab or fireOrigin.");
            return;
        }

        var projObj = Instantiate(projectilePrefab, fireOrigin.position, Quaternion.identity);
        EnemyProjectile p = projObj.GetComponent<EnemyProjectile>();
        if (p != null)
        {
            int enemyIndex = GetComponent<Enemy>().enemyIndex;
            p.Init(target, enemyIndex);
        }

        Debug.Log($"{name}: Enemy Projectile spawned at {fireOrigin.position} targeting {target.name}");
    }
}
