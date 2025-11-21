using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 0f;
    public float damage = 10;
    private Transform target;

    // dipanggil waktu peluru dibuat
    public void Init(Transform targetTransform, int index)
    {
        target = targetTransform;
        speed = EnemyManager.Instance.GetEnemy(index).movementSpeed+2f;
        damage = EnemyManager.Instance.GetEnemy(index).atk;
    }

    void Update()
    {
        if (target == null)
        {
            Debug.Log($"{name}: Target is null, destroying projectile.");
            Destroy(gameObject);
            return;
        }

        // gerakkan peluru menuju target
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
    }

    private bool hit = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hit) return;
        if (other.CompareTag("Creature"))
        {
            hit = true;
            var dmgComp = other.GetComponent<IDamageable>();
            if (dmgComp != null) dmgComp.TakeDamage(damage, Effect.None);
            Invoke(nameof(DestroyProjectile), 0.5f);
        }
    }

    void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
