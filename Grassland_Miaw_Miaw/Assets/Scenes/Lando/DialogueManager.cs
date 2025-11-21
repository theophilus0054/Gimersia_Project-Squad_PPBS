using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public TextMeshPro dialogueText;
    public GameObject continueIndicator; // e.g., "Press Space to continue" icon
    public float typingSpeed = 0.03f;

    [Header("Dialogue Data")]
    public string[] dialogueLines;
    public AudioSource sc;
    public AudioClip[] sound;
    private bool canContinue;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (continueIndicator != null)
            continueIndicator.SetActive(false);
    }

    public void StartDialogue()
    {
        // cari tutorial index pertama yang masih false
        int nextIndex = -1;
        for (int i = 0; i < TutorialManager.Instance.tutorialProgress.Length; i++)
        {
            if (!TutorialManager.Instance.tutorialProgress[i])
            {
                nextIndex = i;
                Debug.Log("Next index" + nextIndex);
                break;
            }
        }

        if (nextIndex == -1)
        {
            Debug.Log("✅ Semua tutorial sudah selesai!");
            return;
        }

        Debug.Log($"▶️ Starting Tutorial {nextIndex}");
        StartCoroutine(PlayDialogueSequence(nextIndex));
    }

    private IEnumerator PlayDialogueSequence(int tutorialIndex)
    {
        sc.PlayOneShot(sound[tutorialIndex]);
        yield return StartCoroutine(TypeSentence(dialogueLines[tutorialIndex]));

            // wait for player press
        canContinue = true;
        if (continueIndicator != null) continueIndicator.SetActive(true);

        yield return new WaitUntil(() => (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && canContinue);

        if (continueIndicator != null) continueIndicator.SetActive(false);
        canContinue = false;

        TutorialManager.Instance.CompleteTutorial(tutorialIndex);
    }

    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";

        foreach (char c in sentence)
        {
            dialogueText.text += c;

            if (Input.GetKey(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                dialogueText.text = sentence; // instantly show full line
                break;
            }

            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
