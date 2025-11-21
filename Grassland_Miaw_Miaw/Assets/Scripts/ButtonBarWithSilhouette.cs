using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ButtonBarWithSilhouette : MonoBehaviour
{
    [Header("Buttons & Icons")]
    public List<Button> buttons;
    public List<RectTransform> icons;

    [Header("Size Settings")]
    public Vector2 selectedSize = new Vector2(313f, 130f);
    public Vector2 normalSize = new Vector2(222f, 110f);
    public float animationDuration = 0.2f;

    [Header("Icon Scaling")]
    public float selectedIconScale = 1.2f;

    [Header("Silhouette")]
    public RectTransform silhouette;

    public GameObject lockedPopup1;
    public GameObject lockedPopup5;
    public float silhouetteMoveSpeed = 15f; // Kecepatan follow (semakin besar semakin cepat)

    private int selectedIndex = 2;
    private RectTransform targetIcon;

    void Start()
    {
        if (buttons.Count == 0) return;

        // Setup LayoutElement untuk semua button
        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i;
            
            LayoutElement layout = buttons[i].GetComponent<LayoutElement>();
            if (layout == null)
                layout = buttons[i].gameObject.AddComponent<LayoutElement>();

            buttons[i].onClick.AddListener(() => OnButtonClicked(index));
        }

        ApplySizeInstant(selectedIndex);

        // Set target icon awal
        if (selectedIndex >= 0 && selectedIndex < icons.Count && icons[selectedIndex] != null)
        {
            targetIcon = icons[selectedIndex];
            if (silhouette != null)
                silhouette.position = targetIcon.position;
        }
    }

    void Update()
    {
        // Silhouette selalu mengikuti posisi target icon
        if (silhouette != null && targetIcon != null)
        {
            silhouette.position = Vector3.Lerp(
                silhouette.position, 
                targetIcon.position, 
                Time.deltaTime * silhouetteMoveSpeed
            );
        }
    }

    void ApplySizeInstant(int selected)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            LayoutElement layout = buttons[i].GetComponent<LayoutElement>();

            if (i == selected)
            {
                layout.preferredWidth = selectedSize.x;
                layout.preferredHeight = selectedSize.y;

                // Scale icon/logo
                if (i < icons.Count && icons[i] != null) {
                    icons[i].localScale = Vector3.one * selectedIconScale;
                    icons[i].GetComponent<LogoScript>().addPositionY = 10;
                }
            }
            else
            {
                layout.preferredWidth = normalSize.x;
                layout.preferredHeight = normalSize.y;
                
                // Reset icon/logo scale
                if (i < icons.Count && icons[i] != null)
                    icons[i].localScale = Vector3.one;
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    private Coroutine fadeRoutine1;
    private Coroutine fadeRoutine5;
    private Coroutine animateRoutine;
    public void OnButtonClicked(int index)
    {
        AudioManager.Instance.PlayButtonClick();
        if (index == 0)
        {
            if (fadeRoutine1 != null)
            {
                StopCoroutine(fadeRoutine1);
            }

            // Start fade baru
            AudioManager.Instance.PlayDeniedInteraction();
            fadeRoutine1 = StartCoroutine(ActivateAndFadeOut(lockedPopup1, 2f));
            return;
        }
        else if (index == 4)
        {
            if (fadeRoutine5 != null)
            {
                StopCoroutine(fadeRoutine5);
            }

            // Start fade baru
            AudioManager.Instance.PlayDeniedInteraction();
            fadeRoutine5 = StartCoroutine(ActivateAndFadeOut(lockedPopup5, 2f));
            return;
        }
        selectedIndex = index;

        // Update target icon
        if (index >= 0 && index < icons.Count && icons[index] != null)
            targetIcon = icons[index];


        if (animateRoutine != null)
        {
            StopCoroutine(animateRoutine);
        }
        CameraController.Instance.MoveCameraSmooth(index + 1, 0.5f);
        animateRoutine = StartCoroutine(AnimateButtons());
    }

    IEnumerator AnimateButtons()
    {
        float time = 0f;
        Vector2[] startSizes = new Vector2[buttons.Count];
        Vector2[] targetSizes = new Vector2[buttons.Count];
        float[] startIconScales = new float[buttons.Count];
        float[] targetIconScales = new float[buttons.Count];

        for (int i = 0; i < buttons.Count; i++)
        {
            LayoutElement layout = buttons[i].GetComponent<LayoutElement>();
            startSizes[i] = new Vector2(layout.preferredWidth, layout.preferredHeight);
            targetSizes[i] = (i == selectedIndex) ? selectedSize : normalSize;

            // Icon scale
            if (i < icons.Count && icons[i] != null)
            {
                startIconScales[i] = icons[i].localScale.x;
                targetIconScales[i] = (i == selectedIndex) ? selectedIconScale : 1f;
            }
        }

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / animationDuration);

            for (int i = 0; i < buttons.Count; i++)
            {
                LayoutElement layout = buttons[i].GetComponent<LayoutElement>();
                layout.preferredWidth = Mathf.Lerp(startSizes[i].x, targetSizes[i].x, t);
                layout.preferredHeight = Mathf.Lerp(startSizes[i].y, targetSizes[i].y, t);

                // Animate icon scale
                if (i < icons.Count && icons[i] != null)
                {
                    float scale = Mathf.Lerp(startIconScales[i], targetIconScales[i], t);
                    icons[i].localScale = Vector3.one * scale;
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            yield return null;
        }

        ApplySizeInstant(selectedIndex);
    }

    public IEnumerator ActivateAndFadeOut(GameObject obj, float duration)
    {
        // --- Step 1: Aktifkan GameObject ---
        obj.SetActive(true);
        obj.transform.localScale = Vector3.one * 0.9f;
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();

        // --- Step 2: Scale up ---
        float elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.1f);
            float scale = Mathf.Lerp(0.9f, 1f, t);
            obj.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        obj.transform.localScale = Vector3.one; // pastikan scale = 1

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration); // dari 1 ke 0
            cg.alpha = alpha;
            yield return null;
        }

        cg.alpha = 1;

        // --- Step 3: Deactivate GameObject ---
        obj.SetActive(false);
    }
}