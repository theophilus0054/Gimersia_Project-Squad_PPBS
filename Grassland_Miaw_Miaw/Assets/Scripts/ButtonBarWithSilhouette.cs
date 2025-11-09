using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ButtonBarWithSilhouette : MonoBehaviour
{
    [SerializeField]
    [Header("Buttons & Icons")]
    public List<Button> buttons;
    public List<RectTransform> icons;

    [Header("Scaling Settings")]
    public float selectedButtonScale = 1.2f;
    public float iconSelectedScale = 1.3f;
    public float neighborStretch = 1.05f;
    public float animationDuration = 0.2f;

    [Header("Silhouette")]
    public RectTransform silhouette;
    public float silhouetteMoveDuration = 0.25f;

    private int selectedIndex = -1;
    private Vector3[] originalButtonScales;
    private Vector3[] originalIconScales;

    void Start()
    {
        originalButtonScales = new Vector3[buttons.Count];
        originalIconScales = new Vector3[buttons.Count];

        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i;
            originalButtonScales[i] = buttons[i].transform.localScale;
            originalIconScales[i] = icons[i].localScale;

            buttons[i].onClick.AddListener(() => OnButtonClicked(index));
        }

        // Set default selected index to button[2]
        if (buttons.Count > 2)
        {
            selectedIndex = 2;

            // Apply initial scaling
            for (int i = 0; i < buttons.Count; i++)
            {
                if (i == selectedIndex)
                {
                    buttons[i].transform.localScale = originalButtonScales[i] * selectedButtonScale;
                    icons[i].localScale = originalIconScales[i] * iconSelectedScale;
                }
                else if (Mathf.Abs(i - selectedIndex) == 1)
                {
                    buttons[i].transform.localScale = originalButtonScales[i] * neighborStretch;
                    icons[i].localScale = originalIconScales[i];
                }
                else
                {
                    buttons[i].transform.localScale = originalButtonScales[i];
                    icons[i].localScale = originalIconScales[i];
                }
            }
            
            // Position silhouette correctly at start
            if (silhouette != null)
            {
                StartCoroutine(SetInitialSilhouettePosition());
            }
        }
    }

    void OnButtonClicked(int index)
    {
        selectedIndex = index;
        StopAllCoroutines();
        StartCoroutine(AnimateButtons());

        if (silhouette != null)
            StartCoroutine(MoveSilhouetteTo(buttons[index].transform));
    }

    IEnumerator AnimateButtons()
    {
        float time = 0f;
        Vector3[] startButtonScales = new Vector3[buttons.Count];
        Vector3[] targetButtonScales = new Vector3[buttons.Count];
        Vector3[] startIconScales = new Vector3[buttons.Count];
        Vector3[] targetIconScales = new Vector3[buttons.Count];

        for (int i = 0; i < buttons.Count; i++)
        {
            startButtonScales[i] = buttons[i].transform.localScale;
            startIconScales[i] = icons[i].localScale;

            if (i == selectedIndex)
            {
                targetButtonScales[i] = originalButtonScales[i] * selectedButtonScale;
                targetIconScales[i] = originalIconScales[i] * iconSelectedScale;
            }
            else if (Mathf.Abs(i - selectedIndex) == 1)
            {
                targetButtonScales[i] = originalButtonScales[i] * neighborStretch;
                targetIconScales[i] = originalIconScales[i];
            }
            else
            {
                targetButtonScales[i] = originalButtonScales[i];
                targetIconScales[i] = originalIconScales[i];
            }
        }

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / animationDuration);

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].transform.localScale = Vector3.Lerp(startButtonScales[i], targetButtonScales[i], t);
                icons[i].localScale = Vector3.Lerp(startIconScales[i], targetIconScales[i], t);
            }
            yield return null;
        }

        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].transform.localScale = targetButtonScales[i];
            icons[i].localScale = targetIconScales[i];
        }
    }

    IEnumerator MoveSilhouetteTo(Transform target)
    {
        // Kita menggunakan .position (Vector3) karena parent-nya mungkin berbeda
        Vector3 startPos = silhouette.position;
        Vector3 endPos = target.position;
        float time = 0f;

        while (time < silhouetteMoveDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / silhouetteMoveDuration);
            silhouette.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        silhouette.position = endPos;
    }

    IEnumerator SetInitialSilhouettePosition()
    {
        // Menunggu sampai akhir frame
        // Pada titik ini, HLG dan semua sistem layout UI
        // sudah selesai menghitung posisi.
        yield return new WaitForEndOfFrame();

        // Sekarang, kita bisa dengan aman mengambil posisi tombol
        // dan mengaturnya ke silhouette
        if (selectedIndex != -1 && selectedIndex < buttons.Count)
        {
            silhouette.position = buttons[selectedIndex].transform.position;
        }
    }
}