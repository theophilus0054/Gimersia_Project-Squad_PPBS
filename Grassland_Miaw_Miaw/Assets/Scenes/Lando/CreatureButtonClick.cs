using UnityEngine;

public class CreatureButtonClick : MonoBehaviour
{
    [Range(0f, 1f)] public float hoverDarkness = 0.8f; // 0 = black, 1 = original color
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

    private void OnMouseEnter()
    {
        targetColor = originalColor * hoverDarkness; // darken
        AudioManager.Instance.PlayButtonHover();
    }

    private void OnMouseExit()
    {
        targetColor = originalColor; // revert
    }
    public int index;

    private void OnMouseDown()
    {
        if (InputLockManager.Instance != null &&
            InputLockManager.Instance.IsLocked)
            return;

        SummonGUIManager.Instance.ShowCreatureByIndex(index);
    }
}
