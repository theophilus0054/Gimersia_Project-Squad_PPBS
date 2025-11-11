using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Renderer))]
public class ObjectButton : MonoBehaviour
{
    [Header("Event On Click")]
    public UnityEvent onClick;

    [Header("Hover Settings")]
    public UnityEvent onHoverEnter;
    public UnityEvent onHoverExit;
    [Range(0f, 1f)] public float hoverDarkness = 0.7f; // 0 = black, 1 = original color
    public float colorLerpSpeed = 8f; // speed of color transition

    private Renderer objRenderer;
    private Color originalColor;
    private Color targetColor;

    private void Awake()
    {
        objRenderer = GetComponent<Renderer>();
        originalColor = objRenderer.material.color; // store original color
        targetColor = originalColor;
    }

    private void Update()
    {
        // Smoothly interpolate color
        objRenderer.material.color = Color.Lerp(objRenderer.material.color, targetColor, colorLerpSpeed * Time.deltaTime);
    }

    private void OnMouseDown()
    {
        onClick?.Invoke();
    }

    private void OnMouseEnter()
    {
        targetColor = originalColor * hoverDarkness; // darken
        onHoverEnter?.Invoke();
    }

    private void OnMouseExit()
    {
        targetColor = originalColor; // revert
        onHoverExit?.Invoke();
    }
}
