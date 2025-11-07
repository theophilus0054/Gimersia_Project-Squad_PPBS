using UnityEngine;

public class DragScript : MonoBehaviour
{
    private Collider2D col;
    private Vector3 startDragPosition;
    public int posX = 1;
    public int posY = 1;
    private bool isDragging = false;

    public int evolutionIndex = 0;

    void Start()
    {
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            Collider2D hit = Physics2D.OverlapPoint(mousePos);
            if (hit != null && hit == col)
            {
                startDragPosition = transform.position;
                isDragging = true;
            }
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            transform.position = GetMousePosition();
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            col.enabled = false;
            
            // ✅ Dapatkan SEMUA collider di posisi drop
            Collider2D[] allHits = Physics2D.OverlapPointAll(transform.position);
            
            col.enabled = true;

            // ✅ Cari DropArea dari semua collider yang terdeteksi
            IDragDrop dropArea = null;
            Collider2D dropAreaCollider = null;

            foreach (var hit in allHits)
            {
                if (hit.TryGetComponent(out IDragDrop area))
                {
                    dropArea = area;
                    dropAreaCollider = hit;
                    break; // Ketemu DropArea, stop search
                }
            }

            if (dropArea != null)
            {
                if (!dropArea.getFilled())
                {
                    // --- Normal drop ---
                    HandleNormalDrop(dropArea);
                }
                else
                {
                    // --- Area filled: check for merge ---
                    HandleMergeAttempt(dropArea, dropAreaCollider);
                }
            }
            else
            {
                Debug.LogWarning("Dem ga kena");
                transform.position = startDragPosition;
            }
        }
    }

    private void HandleNormalDrop(IDragDrop dropArea)
    {
        Vector2 oldPos = new Vector2(((posY - 1) * 1.73f) - 7.6006f, ((posX - 1) * -1.72f) + 3.4075f);
        Collider2D areaCollider = Physics2D.OverlapPoint(oldPos);

        if (areaCollider != null && areaCollider.TryGetComponent(out IDragDrop leaveArea))
        {
            leaveArea.OnItemLeave(this);
        }

        posX = dropArea.GetX();
        posY = dropArea.GetY();

        dropArea.OnItemDrop(this);
    }

    private void HandleMergeAttempt(IDragDrop dropArea, Collider2D hitCollider)
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(hitCollider.transform.position);

        foreach (var col2 in colliders)
        {
            if (col2.TryGetComponent(out DragScript other))
            {
                if (other != this)
                {
                    if (other.evolutionIndex == this.evolutionIndex && EvolutionManager.Instance.CanEvolveTo(evolutionIndex, evolutionIndex+1))
                    {
                        Vector2 oldPos = new Vector2(((posY - 1) * 1.73f) - 7.6006f, ((posX - 1) * -1.72f) + 3.4075f);
                        Collider2D areaCollider = Physics2D.OverlapPoint(oldPos);

                        if (areaCollider != null && areaCollider.TryGetComponent(out IDragDrop leaveArea))
                        {
                            leaveArea.OnItemLeave(this);
                        }
                        Debug.Log($"🧬 Merge detected at ({dropArea.GetX()}, {dropArea.GetY()})!");
                        int nextEvolution = this.evolutionIndex + 1;

                        Destroy(other.gameObject);
                        Destroy(this.gameObject);

                        // spawn evolution baru di posisi merge
                        SummonManager.SummonEvolution(nextEvolution, (DropArea) dropArea);
                        return;
                    }
                    else
                    {
                        Debug.Log("Cant merge");
                        transform.position = startDragPosition;
                        return;
                    }
                }
            }
        }

        transform.position = startDragPosition;
    }

    public Vector3 GetMousePosition()
    {
        Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        p.z = 0f;
        return p;
    }
}
