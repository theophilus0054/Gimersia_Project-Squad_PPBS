using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 6f;
    public float damage = 10;
    private Transform target;

    // dipanggil waktu peluru dibuat
    public void Init(Transform targetTransform, int index)
    {
        target = targetTransform;
        speed = EvolutionManager.Instance.GetEvolution(index).projectileSpeed;
        damage = EvolutionManager.Instance.GetEvolution(index).atk;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // gerakkan peluru menuju target
        transform.position += Vector3.right * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // pastikan peluru kena musuh
        if (other.CompareTag("Enemy"))
        {
            // ambil komponen IDamageable dari musuh
            var dmgComp = other.GetComponent<IDamageable>();
            if (dmgComp != null)
            {
                dmgComp.TakeDamage(damage);
            }

            // hancurkan peluru setelah kena
            Destroy(gameObject);
        }
    }
}
