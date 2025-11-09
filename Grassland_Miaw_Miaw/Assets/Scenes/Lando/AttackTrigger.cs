using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AttackTrigger : MonoBehaviour
{
    private CreatureAttack parentAttack;
    private Collider2D col;

    void Awake()
    {
        parentAttack = GetComponentInParent<CreatureAttack>();

        col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (parentAttack == null) return;

        Debug.Log($"Target Seen: {other.name}");

        if (other.TryGetComponent<IDamageable>(out var dmg) && other.CompareTag("Enemy"))
        {
            if (!parentAttack.enemiesInRange.Contains(other.transform))
                parentAttack.enemiesInRange.Add(other.transform);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (parentAttack == null) return;

        if (parentAttack.enemiesInRange.Contains(other.transform))
            parentAttack.enemiesInRange.Remove(other.transform);
    }

    // 🔹 Gizmos
    void OnDrawGizmos()
    {
        if (col == null) col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = new Color(0f, 1f, 1f, 0.3f); // cyan transparan
        if (col is CircleCollider2D circle)
        {
            Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
        }
        else if (col is BoxCollider2D box)
        {
            Vector3 size = new Vector3(box.size.x, box.size.y, 0f);
            Vector3 center = transform.position + (Vector3)box.offset;
            Gizmos.DrawWireCube(center, size);
        }
        else if (col is PolygonCollider2D poly)
        {
            Vector3 offset = transform.position;
            Vector2[] points = poly.points;
            for (int i = 0; i < points.Length; i++)
            {
                Vector3 start = offset + (Vector3)points[i];
                Vector3 end = offset + (Vector3)points[(i + 1) % points.Length];
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
