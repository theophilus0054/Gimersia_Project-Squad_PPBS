using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DragScript : MonoBehaviour
{
    public Collider2D col;
    private Vector3 startDragPosition;
    public int posX = 1;
    public int posY = 1;
    public bool isDragging = false;

    public int evolutionIndex = 0;
    
    private Camera mainCam;

    void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    void Start()
    {
        mainCam = Camera.main;
        
        if(col == null)
            Debug.LogError($"{name}: Collider2D not found!");
    }

    void Update()
    {
        // Cek input mouse/touch
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryStartDrag();
        }

        // Update posisi saat dragging
        if (isDragging)
        {
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                transform.position = GetMousePosition();
            }
            else
            {
                // Mouse released
                EndDrag();
            }
        }
    }

    private void TryStartDrag()
    {
        Vector2 mousePos = GetMousePosition();
        Collider2D[] allHits = Physics2D.OverlapPointAll(mousePos);
        if(InputLockManager.Instance.IsLocked) return;

        foreach (var hit in allHits)
        {
            if (hit == col)
            {
                AudioManager.Instance.PlayPickupCreature();
                startDragPosition = transform.position;
                isDragging = true;

                // ⚡ Disable collider supaya tidak diserang
                col.enabled = false;

                // Opsional: disable script attack juga
                var attack = GetComponent<CreatureAttack>();
                if (attack != null)
                    attack.enabled = false;

                return;
            }
        }
    }


    private void EndDrag()
    {
        isDragging = false;

        // Re-enable collider
        col.enabled = true;

        // Re-enable attack script
        var attack = GetComponent<CreatureAttack>();
        if (attack != null)
            attack.enabled = true;
        Collider2D[] allHits = Physics2D.OverlapPointAll(transform.position);
        col.enabled = true;

        Debug.Log($"Checking colliders at drop position {transform.position}: {allHits.Length} found");
        IDragDrop dropArea = null;
        Collider2D dropAreaCollider = null;

        foreach (var hit in allHits)
        {
            Debug.Log($"Hit collider: {hit.name}, tag: {hit.tag}");
            if (hit.TryGetComponent(out IDragDrop area))
            {
                Debug.Log($"Found DropArea: {hit.name}, assigning dropArea");
                dropArea = area;
                dropAreaCollider = hit;
                AudioManager.Instance.PlayDropCreature();
                break;
            }
        }

        if (dropArea != null)
        {
            if (!dropArea.getFilled())
            {
                HandleNormalDrop(dropArea);
            }
            else
            {
                HandleMergeAttempt(dropArea, dropAreaCollider);
            }
        }
        else
        {
            Debug.LogWarning("No DropArea detected at drop position");
            transform.position = startDragPosition;
        }
    }

    private void HandleNormalDrop(IDragDrop dropArea)
    {
        // hit semua collider di posisi lama
        Vector2 oldPos = new Vector2(((posY - 1) * 1.73f) - 7.6006f, ((posX - 1) * -1.72f) + 3.4075f);
        Collider2D[] oldColliders = Physics2D.OverlapPointAll(oldPos);

        Debug.Log($"Checking old position colliders at {oldPos}: {oldColliders.Length} found");
        bool foundLeaveArea = false;
        foreach (var col in oldColliders)
        {
            Debug.Log($"Hit collider: {col.name}, tag: {col.tag}");
            if (col.CompareTag("DropArea") && col.TryGetComponent(out IDragDrop leaveArea))
            {
                Debug.Log($"Found DropArea at oldPos: {col.name}, calling OnItemLeave");
                leaveArea.OnItemLeave(this);
                foundLeaveArea = true;
                break;
            }
        }

        if (!foundLeaveArea)
            Debug.LogError($"No DropArea found at old position {oldPos}");

        // Update posisi ke dropArea baru
        posX = dropArea.GetX();
        posY = dropArea.GetY();
        dropArea.OnItemDrop(this, evolutionIndex);
    }

    private void HandleMergeAttempt(IDragDrop dropArea, Collider2D hitCollider)
    {
        // Debug posisi lama
        Vector2 oldPos = new Vector2(((posY - 1) * 1.73f) - 7.6006f, ((posX - 1) * -1.72f) + 3.4075f);
        Collider2D[] oldColliders = Physics2D.OverlapPointAll(oldPos);
        Debug.Log($"Checking old position colliders at {oldPos}: {oldColliders.Length} found");

        // Debug posisi merge
        Vector2 mergePos = hitCollider.transform.position;
        
        col.enabled = false;
        Collider2D[] colliders = Physics2D.OverlapPointAll(mergePos);
        col.enabled = true;
        
        Debug.Log($"Checking merge position colliders at {mergePos}: {colliders.Length} found");

        bool merged = false;

        foreach (var col2 in colliders)
        {
            Debug.Log($"Hit collider: {col2.name}, tag: {col2.tag}");
            if (col2.TryGetComponent(out DragScript other) && other != this)
            {
                if (other.evolutionIndex == this.evolutionIndex)
                {
                    if(EvolutionManager.Instance.CanEvolveTo(evolutionIndex, evolutionIndex + 1)){
                        Debug.Log($"🧬 Merge detected at ({dropArea.GetX()}, {dropArea.GetY()})!");

                        int nextEvolution = this.evolutionIndex + 1;

                        // kosongkan DropArea lama
                        foreach (var col in oldColliders)
                        {
                            if (col.CompareTag("DropArea") && col.TryGetComponent(out IDragDrop leaveArea))
                            {
                                leaveArea.OnItemLeave(this);
                                break;
                            }
                        }

                        Destroy(other.gameObject);
                        Destroy(this.gameObject);

                        AudioManager.Instance.PlayMergeCreature();
                        SummonManager.SummonEvolution(nextEvolution, (DropArea)dropArea);
                        merged = true;
                        break;
                    }
                    else
                    {
                        GameObject obj = Instantiate(UIManager.Instance.popupWarningPrefab, UIManager.Instance.popupWarningSlot.transform);
                        obj.GetComponentInChildren<TextMeshProUGUI>().text = "Max Evolution!";
                    }
                } 
                else
                {
                    GameObject obj = Instantiate(UIManager.Instance.popupWarningPrefab, UIManager.Instance.popupWarningSlot.transform);
                    obj.GetComponentInChildren<TextMeshProUGUI>().text = "Wrong evolution!";
                }
            }
        }

        if (!merged)
        {
            Debug.Log("Cant merge, returning to start position");
            transform.position = startDragPosition;
        }
    }

    public Vector3 GetMousePosition()
    {
        if (Mouse.current == null) return transform.position;
        
        Vector3 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;
        return worldPos;
    }

    private void OnDrawGizmos()
    {
        // 1️⃣ Gizmo untuk collider object ini
        Collider2D myCol = GetComponent<Collider2D>();
        if (myCol != null)
        {
            Gizmos.color = isDragging ? Color.cyan : Color.green;
            Gizmos.DrawWireCube(myCol.bounds.center, myCol.bounds.size);
        }

        // 2️⃣ Gizmo untuk posisi lama (oldPos)
        Vector2 oldPos = new Vector2(((posY - 1) * 1.73f) - 7.6006f, ((posX - 1) * -1.72f) + 3.4075f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(oldPos, Vector2.one * 0.5f);

        // 3️⃣ Gizmo untuk posisi drag sekarang
        if (isDragging)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }

        // 4️⃣ Semua collider di posisi drag
        if (Application.isPlaying && myCol != null)
        {
            Collider2D[] hits = Physics2D.OverlapPointAll(transform.position);
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            foreach (var hit in hits)
            {
                if (hit != myCol) // Skip diri sendiri
                {
                    Gizmos.DrawWireCube(hit.bounds.center, hit.bounds.size);
                }
            }
        }
    }
}