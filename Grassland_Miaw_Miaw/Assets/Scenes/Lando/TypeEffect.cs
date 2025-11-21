using System.Collections;
using UnityEngine;
using TMPro;

public class TypeEffect : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    [TextArea]
    public string fullText;
    public float typingSpeed = 0.05f;

    private Coroutine typingCoroutine;

    void Start()
    {
        // Start typing automatically (optional)
        PlayTyping();
    }

    public void PlayTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        textMeshPro.text = ""; // Clear first

        foreach (char c in fullText)
        {
            textMeshPro.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void Skip()
    {
        // Instantly finish typing
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        textMeshPro.text = fullText;
    }
}
