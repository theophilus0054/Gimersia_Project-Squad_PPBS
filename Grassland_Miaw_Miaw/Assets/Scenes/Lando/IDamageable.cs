using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount, Effect status);
    bool IsDead { get; }
}
