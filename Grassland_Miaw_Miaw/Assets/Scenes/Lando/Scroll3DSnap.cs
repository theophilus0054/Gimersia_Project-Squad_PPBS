using UnityEngine;
using UnityEngine.InputSystem;

public class Scroll3DSnap : MonoBehaviour
{
    [Header("Scroll Settings")]
    public Transform content;
    public float dragSensitivity = 1f;
    public float scrollSensitivity = 2f;

    [Header("Scroll Limits")]
    public bool useLimits = true;
    public float minY = -5f;
    public float maxY = 5f;

    [Header("Debug")]
    public bool showDebugLogs = false;

    private Vector3 dragStartPos;
    private Vector3 contentStartPos;
    private bool isDragging = false;
    private Camera cam;

    private Vector2 pointerPosition;
    private bool isPressed = false;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        pointerPosition = Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;
        bool pressedThisFrame = Mouse.current != null && Mouse.current.leftButton.isPressed;

        if (pressedThisFrame && !isPressed)
            OnPointerDownManual();
        else if (!pressedThisFrame && isPressed)
            OnPointerUpManual();

        isPressed = pressedThisFrame;
        HandleDrag();
        HandleScrollWheel();
    }

    void OnPointerDownManual()
    {
        isDragging = true;
        dragStartPos = GetMouseWorld(pointerPosition);
        contentStartPos = content.localPosition;

        if (showDebugLogs)
            Debug.Log($"[Scroll3D] Pointer DOWN - Start Pos: {dragStartPos}");
    }

    void OnPointerUpManual()
    {
        isDragging = false;

        if (showDebugLogs)
            Debug.Log($"[Scroll3D] Pointer UP - Final Y: {content.localPosition.y}");
    }

    void HandleDrag()
    {
        if (!isDragging) return;

        Vector3 currentMouseWorld = GetMouseWorld(pointerPosition);
        float deltaY = (currentMouseWorld.y - dragStartPos.y) * dragSensitivity;
        float targetY = contentStartPos.y + deltaY;

        // Apply limits - langsung mentok dan tidak bisa didrag melewati batas
        if (useLimits)
        {
            if (targetY > maxY)
            {
                targetY = maxY;
                // Reset drag start position untuk mencegah dragging melewati batas
                ResetDragStart(currentMouseWorld, targetY);
            }
            else if (targetY < minY)
            {
                targetY = minY;
                // Reset drag start position untuk mencegah dragging melewati batas
                ResetDragStart(currentMouseWorld, targetY);
            }
        }

        content.localPosition = new Vector3(content.localPosition.x, targetY, content.localPosition.z);

        if (showDebugLogs && Time.frameCount % 20 == 0)
            Debug.Log($"[Scroll3D] Dragging Y: {targetY:F2}");
    }

    void HandleScrollWheel()
    {
        if (Mouse.current == null) return;

        float scrollValue = -Mouse.current.scroll.ReadValue().y;
        
        if (Mathf.Abs(scrollValue) > 0.01f)
        {
            float scrollDelta = scrollValue * scrollSensitivity * Time.deltaTime * 100f;
            float targetY = content.localPosition.y + scrollDelta;

            // Apply limits untuk scroll wheel
            if (useLimits)
            {
                targetY = Mathf.Clamp(targetY, minY, maxY);
            }

            content.localPosition = new Vector3(content.localPosition.x, targetY, content.localPosition.z);

            if (showDebugLogs)
                Debug.Log($"[Scroll3D] Scroll Wheel - Delta: {scrollDelta:F2}, New Y: {targetY:F2}");
        }
    }

    void ResetDragStart(Vector3 currentMouseWorld, float currentY)
    {
        // Reset posisi awal drag agar perhitungan delta selanjutnya konsisten
        dragStartPos = currentMouseWorld;
        contentStartPos = new Vector3(content.localPosition.x, currentY, content.localPosition.z);
    }

    Vector3 GetMouseWorld(Vector2 screenPos)
    {
        float distance = Mathf.Abs(cam.transform.position.z - content.position.z);
        return cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distance));
    }

    void OnGUI()
    {
        if (!showDebugLogs) return;
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Box("=== DRAG 3D DEBUG ===");
        GUILayout.Label($"Content Y: {content.localPosition.y:F2}");
        GUILayout.Label($"Dragging: {isDragging}");
        GUILayout.Label($"Limits: {minY:F1} - {maxY:F1}");
        
        // Tambah info scroll wheel
        float scrollValue = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
        GUILayout.Label($"Scroll Wheel: {scrollValue:F2}");
        
        GUILayout.EndArea();
    }
}