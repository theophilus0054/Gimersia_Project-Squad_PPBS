using UnityEngine;
using System.Collections;

public class Creature : MonoBehaviour, IDamageable
{
    [Header("Creature Stats")]
    public CreatureData data;

    [Header("Revive Settings")]
    public float reviveDelay = 10f;

    private float currentHP;
    private bool isDead = false;
    private Animator animator;
    private Collider2D col;

    void Awake()
    {
        if (data == null)
        {
            data = EvolutionManager.Instance.GetEvolution(GetComponent<DragScript>().evolutionIndex);
        }

        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();

        currentHP = data.hp;
    }

    // -------------------------
    // Damage & Death
    // -------------------------
    public void TakeDamage(float amount, Effect status)
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
        if (isDead) return;
        isDead = true;

        animator?.SetTrigger("isDead");
        if (col != null)
            col.enabled = false;

        StartCoroutine(HandleRevive());
    }

    private IEnumerator HandleRevive()
    {
        // tunggu waktu reviveDelay dulu
        yield return new WaitForSeconds(reviveDelay);

        // tunggu sampai summon phase aktif
        while (!StageManager.Instance.isSummonPhase)
            yield return null;

        Revive();
    }

    private void Revive()
    {
        isDead = false;
        currentHP = data.hp;
        col.enabled = true;
        animator?.SetTrigger("isRevived");

        Debug.Log($"{name} has revived!");
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
