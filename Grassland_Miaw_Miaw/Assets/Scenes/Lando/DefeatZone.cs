using UnityEngine;

public class DefeatZone : MonoBehaviour
{
    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Target Seen: {other.name}");

        if (other.TryGetComponent<IDamageable>(out var dmg) && other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                StartCoroutine(enemy.Die());
                if (!StageManager.Instance.isSummonPhase)
                {
                    UIManager.Instance.WaveSurrenderPanel.GetComponent<SurrenderScript>().StopWave(true);
                }
            }
        }
    }
}
