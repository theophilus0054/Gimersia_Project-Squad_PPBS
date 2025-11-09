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

    public void OnButtonClicked(int index)
    {
        selectedIndex = index;
        
        // Update target icon
        if (index >= 0 && index < icons.Count && icons[index] != null)
            targetIcon = icons[index];

        StopAllCoroutines();
        StartCoroutine(AnimateButtons());
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
}