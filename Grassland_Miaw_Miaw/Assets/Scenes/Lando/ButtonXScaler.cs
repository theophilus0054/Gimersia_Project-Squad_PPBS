using UnityEngine;

public class ButtonXScaler : MonoBehaviour
{
    [Header("Objects to Scale")]
    public GameObject[] objects; // assign your 3 objects in inspector

    [Header("Animation Settings")]
    public float targetScaleX = 1.3f;
    public float normalScaleX = 1f;
    public float smoothSpeed = 5f; // higher = faster animation

    private int selectedIndex = -1;

    /// <summary>
    /// Call this to highlight an object (X scale will animate)
    /// </summary>
    void Start()
    {
        HighlightObject(0);
    }
    public void HighlightObject(int index)
    {
        selectedIndex = index;
    }

    private void Update()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null) continue;

            Vector3 scale = objects[i].transform.localScale;
            float targetX = (i == selectedIndex) ? targetScaleX : normalScaleX;

            // Smoothly animate X scale
            scale.x = Mathf.Lerp(scale.x, targetX, smoothSpeed * Time.deltaTime);
            objects[i].transform.localScale = scale;
        }
    }
}
