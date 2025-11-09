using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BottomNavBar : MonoBehaviour
{
    [System.Serializable]
    public class NavButton
    {
        public Button button;
        public Image icon;
    }

    [Header("Buttons & Tabs")]
    public List<NavButton> buttons = new List<NavButton>();
    public GameObject[] Tabs; // Each index corresponds to the button order

    [Header("Visual Settings")]
    public Color normalColor = Color.white;
    public Color activeColor = new Color(1f, 0.84f, 0f); // gold
    public float bounceHeight = 20f;
    public float bounceSpeed = 5f;
    public float scaleActive = 1.2f;
    public float scaleSpeed = 6f;
    public float fadeSpeed = 4f;

    private int activeIndex = -1;

    void Start()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i; // capture local copy for delegate
            buttons[i].button.onClick.AddListener(() => OnTabSelected(index));

            buttons[i].icon.color = normalColor;
            buttons[i].icon.rectTransform.localScale = Vector3.one;
        }

        // Initialize all tabs to inactive
        foreach (var tab in Tabs)
            if (tab) tab.SetActive(false);

        // Select first by default
        if (buttons.Count > 0)
            OnTabSelected(0);
    }

    void OnTabSelected(int index)
    {
        // Ignore if already active
        if (activeIndex == index) return;

        // Deactivate previous tab
        if (activeIndex >= 0 && activeIndex < buttons.Count)
        {
            var prev = buttons[activeIndex];
            prev.button.interactable = true;
            prev.icon.color = normalColor;
            StartCoroutine(ScaleButton(prev.icon.rectTransform, 1f));
            if (Tabs.Length > activeIndex && Tabs[activeIndex])
                StartCoroutine(FadePanel(Tabs[activeIndex], false));
        }

        // Activate new tab
        var current = buttons[index];
        current.button.interactable = false;
        current.icon.color = activeColor;
        StartCoroutine(ScaleButton(current.icon.rectTransform, scaleActive));
        if (Tabs.Length > index && Tabs[index])
            StartCoroutine(FadePanel(Tabs[index], true));

        // Bounce feedback
        StopCoroutine(nameof(BounceIcon));
        StartCoroutine(BounceIcon(current.icon.rectTransform));

        activeIndex = index;
    }

    System.Collections.IEnumerator BounceIcon(RectTransform icon)
    {
        Vector3 startPos = icon.localPosition;
        Vector3 upPos = startPos + Vector3.up * bounceHeight;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * bounceSpeed;
            icon.localPosition = Vector3.Lerp(startPos, upPos, Mathf.Sin(t * Mathf.PI * 0.5f));
            yield return null;
        }

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * bounceSpeed;
            icon.localPosition = Vector3.Lerp(upPos, startPos, Mathf.Sin(t * Mathf.PI * 0.5f));
            yield return null;
        }
    }

    System.Collections.IEnumerator FadePanel(GameObject panel, bool fadeIn)
    {
        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = panel.AddComponent<CanvasGroup>();
            group.alpha = 0;
        }

        if (fadeIn)
        {
            panel.SetActive(true);
            group.blocksRaycasts = true;
            group.interactable = true;
        }

        float target = fadeIn ? 1f : 0f;
        while (!Mathf.Approximately(group.alpha, target))
        {
            group.alpha = Mathf.MoveTowards(group.alpha, target, Time.deltaTime * fadeSpeed);
            yield return null;
        }

        if (!fadeIn)
        {
            group.blocksRaycasts = false;
            group.interactable = false;
            panel.SetActive(false);
        }
    }

    System.Collections.IEnumerator ScaleButton(RectTransform icon, float targetScale)
    {
        Vector3 target = Vector3.one * targetScale;
        while (Vector3.Distance(icon.localScale, target) > 0.01f)
        {
            icon.localScale = Vector3.Lerp(icon.localScale, target, Time.deltaTime * scaleSpeed);
            yield return null;
        }
        icon.localScale = target;
    }
}
