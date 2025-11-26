    using System.Collections.Generic;
    using UnityEngine;

    public class CreatureAttack : MonoBehaviour
    {
        [Header("Attack Settings")]
        public GameObject projectilePrefab;
        public Transform fireOrigin;
        public float fireDelay = 5f;

        [HideInInspector] public List<Transform> enemiesInRange = new List<Transform>();

        private float fireCooldown = 0f;
        private Animator animator;
        private int evoIndex;
        private bool targetDetected = false;

    void Start()
    {
        // Animator reference
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogWarning($"{name}: Animator not found! Attack animations won't play.");

        // DragScript check
        var drag = GetComponent<DragScript>();
        if (drag == null)
        {
            Debug.LogError($"{name} is missing DragScript component!");
            enabled = false;
            return;
        }
        evoIndex = drag.evolutionIndex;

        // EvolutionManager check
        if (EvolutionManager.Instance == null)
        {
            Debug.LogError("EvolutionManager.Instance is NULL! Make sure it's in the scene!");
            enabled = false;
            return;
        }

        CreatureData evoData = EvolutionManager.Instance.GetEvolution(evoIndex);
        if (evoData == null)
        {
            Debug.LogError($"Evolution data for index {evoIndex} not found!");
            enabled = false;
            return;
        }

        fireDelay = evoData.atkSpeed;
    }
        

    void Update()
    {
        // Cek dulu apakah sedang di-drag
        var drag = GetComponent<DragScript>();
        if (drag != null && drag.isDragging)
            return; // skip attack saat drag

        Transform target = GetNearestEnemy();
        if (target != null)
        {
            if (!targetDetected)
            {
                targetDetected = true;
                fireCooldown = fireDelay;
            }

            fireCooldown -= Time.deltaTime;

            if (fireCooldown <= 0f)
            {
                FireAt(target);
                fireCooldown = fireDelay;
            }
        }
        else
        {
            targetDetected = false;
            fireCooldown = 0f;
        }
    }

        

    Transform GetNearestEnemy()
    {
        Transform nearest = null;
        float bestDist = float.MaxValue;

        for (int i = enemiesInRange.Count - 1; i >= 0; i--)
        {
            if (enemiesInRange[i] == null)
            {
                Debug.LogWarning($"{name}: Enemy at index {i} is null, removing from list.");
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
            Debug.Log($"{name}: Triggering attack animation.");
            // Trigger animasi attack
            animator.SetTrigger("isAttack");
        }

        // Projectile akan muncul lewat Animation Event (ShootProjectile)
    }

    // ===== Fungsi yang dipanggil dari Animation Event =====
    public void ShootProjectile()
    {
        Transform target = GetNearestEnemy();
        if (target == null)
            return;

        if (projectilePrefab == null || fireOrigin == null)
        {
            Debug.LogWarning($"{name}: Cannot spawn projectile. Missing prefab or fireOrigin.");
            return;
        }

        var projObj = Instantiate(projectilePrefab, fireOrigin.position, Quaternion.identity);
        Projectile p = projObj.GetComponent<Projectile>();
        if (p != null)
        {
            int evoIndex = GetComponent<DragScript>().evolutionIndex;
            if(SummonGUIManager.Instance.allCreatures[evoIndex].type == CreatureType.Unagi)
            {
                AudioManager.Instance.PlayUnagiBubble(gameObject.GetComponent<AudioSource>());
            } else if (SummonGUIManager.Instance.allCreatures[evoIndex].type == CreatureType.Crab)
            {
                AudioManager.Instance.PlayCrabPinch(gameObject.GetComponent<AudioSource>());
            } else if (SummonGUIManager.Instance.allCreatures[evoIndex].type == CreatureType.Puffer)
            {
                AudioManager.Instance.PlayPuffBloat(gameObject.GetComponent<AudioSource>());
            }
            
            if (RollEffect(SummonGUIManager.Instance.allCreatures[evoIndex].effectsArray) == Effect.Slow)
            {
                p.Init(target, evoIndex, Effect.Slow);
            } 
            else if (RollEffect(SummonGUIManager.Instance.allCreatures[evoIndex].effectsArray) == Effect.Bleed)
            {
                p.Init(target, evoIndex, Effect.Bleed);
            } 
            else if (RollEffect(SummonGUIManager.Instance.allCreatures[evoIndex].effectsArray) == Effect.PufferAtk)
            {
                p.Init(target, evoIndex, Effect.PufferAtk);
            }
            else
            {
                p.Init(target, evoIndex, Effect.None);
            }
        }

        Debug.Log($"{name}: Projectile spawned at {fireOrigin.position} targeting {target.name}");
    }

    public static Effect RollEffect(EffectData[] effects)
    {
        if (effects == null || effects.Length == 0)
            return Effect.None;

        float totalChance = 0f;

        // Hitung total chance untuk normalisasi (jika perlu)
        foreach (var e in effects)
        {
            totalChance += e.chanceToApply;
        }

        // Random value antara 0 dan 100
        float roll = Random.Range(0f, 100f);
        float cumulative = 0f;

        foreach (var e in effects)
        {
            cumulative += e.chanceToApply;
            if (roll <= cumulative)
            {
                return e.effectType;
            }
        }

        // Kalau roll tidak kena efek manapun, return None
        return Effect.None;
    }
}
