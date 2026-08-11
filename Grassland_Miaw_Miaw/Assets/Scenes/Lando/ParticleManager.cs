using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    [Header("Particle Prefabs")]
    public GameObject slowParticlePrefab;
    public GameObject bleedParticlePrefab;
    public GameObject waterParticlePrefab;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Generic summon function with custom duration and optional parent
    /// </summary>
    private void SummonParticle(GameObject target, GameObject particlePrefab, float duration, bool beChild)
    {
        if(target == null || particlePrefab == null)
        {
            Debug.LogWarning("ParticleManager: Target or prefab is null!");
            return;
        }

        GameObject particleInstance = Instantiate(particlePrefab, target.transform.position, Quaternion.identity);

        // Set parent if requested
        if (beChild)
            particleInstance.transform.SetParent(target.transform);

        // Destroy particle after custom duration
        if (particleInstance != null)
            Destroy(particleInstance, duration);
    }

    // === Specific particle summon functions ===

    public void SummonParticleSlow(GameObject target, float duration, bool beChild = true) 
    {
        SummonParticle(target, slowParticlePrefab, duration, beChild);
    }

    public void SummonParticleBleed(GameObject target, float duration, bool beChild = true) 
    {
        SummonParticle(target, bleedParticlePrefab, duration, beChild);
    }

    public void SummonParticleWater(GameObject target, float duration, bool beChild = false) 
    {
        SummonParticle(target, waterParticlePrefab, duration, beChild);
    }
}
