using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 6f;
    public float damage = 10;
    int indexEvo = 0;
    Effect projectileType;
    private Transform target;

    // dipanggil waktu peluru dibuat
    public void Init(Transform targetTransform, int index, Effect type)
    {
        indexEvo = index;
        target = targetTransform;
        speed = EvolutionManager.Instance.GetEvolution(index).projectileSpeed;
        damage = EvolutionManager.Instance.GetEvolution(index).atk;
        projectileType = type;
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
                if (projectileType == Effect.Slow)
                {
                    dmgComp.TakeDamage(damage, Effect.Slow);
                } else if (projectileType == Effect.Bleed)
                {
                    dmgComp.TakeDamage(damage, Effect.Bleed);
                }
                else if (projectileType == Effect.PufferAtk)
                {
                    dmgComp.TakeDamage(damage, Effect.PufferAtk);
                }
                else
                {
                    if(SummonGUIManager.Instance.allCreatures[indexEvo].type == CreatureType.Puffer)
                    {
                        Destroy(gameObject);
                        return;
                    }
                    dmgComp.TakeDamage(damage, Effect.None);
                }
            }

            if(SummonGUIManager.Instance.allCreatures[indexEvo].type == CreatureType.Unagi)
            {
                AudioManager.Instance.PlayBubble();
                ParticleManager.Instance.SummonParticleWater(gameObject, 0.5f);
            }
            // hancurkan peluru setelah kena
            Destroy(gameObject);
        }
    }
}
