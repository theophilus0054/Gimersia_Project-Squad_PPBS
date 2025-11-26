using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TipsManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text tipsText;      // TextMeshPro text
    public float changeInterval = 3f; // Waktu tiap tips berganti
    public float fadeDuration = 0.5f; // Durasi fade

    [Header("Tips List")]
    [TextArea(2,5)]
    public string[] tips = new string[]
    {
        "Some creatures can revive during Summon Phase, but not in Wave Phase.",
        "There are two phases: Summon and Wave. Master both to survive battles.",
        "Higher stages give more seashells, but enemies become stronger too.",
        "Upgrading is one way to buff your creatures.",
        "Unagi isn't fond of aliens, so use that to your advantage in fights.",
        "Clear all enemies in the Wave Phase to unlock the next stage.",
        "Progress through stages to unlock new and exciting creatures.",
        "Merge creatures of the same type to evolve them into stronger forms."
    };

    private int currentIndex = -1;

    void Start()
    {
        if(tips.Length == 0)
        {
            Debug.LogWarning("TipsManager: Tips list is empty!");
            return;
        }
    }
    public void ShowNextTip()
    {
        int newIndex;
        do
        {
            newIndex = Random.Range(0, tips.Length);
        } while(newIndex == currentIndex && tips.Length > 1);

        currentIndex = newIndex;
        StartCoroutine(FadeText(tips[currentIndex]));
    }

    private IEnumerator FadeText(string newText)
    {
        // Fade out
        for(float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            tipsText.alpha = 1 - t / fadeDuration;
            yield return null;
        }
        tipsText.alpha = 0;

        tipsText.text = "Tips : " + newText;

        // Fade in
        for(float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            tipsText.alpha = t / fadeDuration;
            yield return null;
        }
        tipsText.alpha = 1;
    }
}
