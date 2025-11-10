using UnityEngine;

public class SpriteScroll : MonoBehaviour
{
    [Header("References")]
    public Transform content;           // The scrollable content parent

    [Header("Scroll Settings")]
    public bool vertical = true;        // Enable vertical scroll
    public bool horizontal = false;     // Enable horizontal scroll
    public float scrollSpeed = 1f;      // Drag sensitivity
    public float inertia = 0.9f;        // How fast scroll slows after release
    public float scrollWheelSpeed = 2f; // Speed when using scroll wheel

    [Header("Bounds")]
    public float upperLimit = 2f;       // Top / right limit
    public float lowerLimit = -2f;      // Bottom / left limit

    private bool dragging = false;
    private Vector3 lastMousePos;
    private Vector3 velocity;

    void Update()
    {
        HandleInput();
        ApplyInertia();
        ClampContent();
    }

    void HandleInput()
    {
        // Start dragging
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dragging = true;
        }

        // Stop dragging
        if (Input.GetMouseButtonUp(0))
            dragging = false;

        // While dragging
        if (dragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 delta = mousePos - lastMousePos;
            delta.z = 0;

            // Apply scroll in chosen directions
            Vector3 move = Vector3.zero;
            if (vertical) move.y = delta.y * scrollSpeed;
            if (horizontal) move.x = delta.x * scrollSpeed;

            content.localPosition += move;
            velocity = move / Time.deltaTime; // store for inertia

            lastMousePos = mousePos;
        }

        // Scroll wheel (optional)
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 move = Vector3.zero;
            if (vertical) move.y = scroll * scrollWheelSpeed * Time.deltaTime;
            if (horizontal) move.x = scroll * scrollWheelSpeed * Time.deltaTime;

            content.localPosition += move;
            velocity = move / Time.deltaTime;
        }
    }

    void ApplyInertia()
    {
        if (!dragging)
        {
            content.localPosition += velocity * Time.deltaTime;
            velocity *= inertia;

            // stop when slow
            if (velocity.magnitude < 0.01f)
                velocity = Vector3.zero;
        }
    }

    void ClampContent()
    {
        Vector3 pos = content.localPosition;

        if (vertical)
        {
            if (pos.y > upperLimit) { pos.y = upperLimit; velocity.y = 0; }
            if (pos.y < lowerLimit) { pos.y = lowerLimit; velocity.y = 0; }
        }

        if (horizontal)
        {
            if (pos.x > upperLimit) { pos.x = upperLimit; velocity.x = 0; }
            if (pos.x < lowerLimit) { pos.x = lowerLimit; velocity.x = 0; }
        }

        content.localPosition = pos;
    }
}
