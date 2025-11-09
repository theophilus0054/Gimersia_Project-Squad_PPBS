using UnityEngine;
using System.Collections;

public class Creature : MonoBehaviour, IDamageable
{
    [Header("Creature Stats")]
    public EvolutionData data;

    private float currentHP;
    private bool isDead = false;

    void Awake()
    {
        if (data == null)
        {
            data = EvolutionManager.Instance.GetEvolution(GetComponent<DragScript>().evolutionIndex);
        }

        // Initialize stats from EvolutionData
        currentHP = data.hp;
    }

    void Update()
    {

    }

    // -------------------------
    // Damage & Death
    // -------------------------
    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHP -= amount;
        Debug.Log($"{name} took {amount} damage. HP left: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public bool IsDead => isDead;

    protected virtual void Die()
    {
        isDead = true;
        GetComponent<Animator>()?.SetTrigger("isDead");
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
    }

    // -------------------------
    // Utility
    // -------------------------
    public override string ToString()
    {
        if (data != null)
            return data.ToString();
        return base.ToString();
    }
}
