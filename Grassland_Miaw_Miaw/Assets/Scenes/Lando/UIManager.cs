using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Stage Navigation Buttons")]
    public GameObject stageBack;
    public GameObject stageNext;
    public GameObject WaveStagePanel;
    public GameObject WaveSurrenderPanel;
    public GameObject WaveEndlessPanel;

    [Header("Wave Condition Frame")]
    public GameObject waveNotificationPanel;
    public GameObject waveFailedFrame;
    public GameObject waveFinishedFrame;

    [Header("Stage Info")]
    public TextMeshPro stageText;
    public TextMeshProUGUI coinText;

    [Header("Popup")]
    public GameObject popupWarningSlot;
    public GameObject popupWarningPrefab;
    public GameObject popupFeatureIndex;

    public Image objectBackground;   // Drag ke inspector
    private bool isFastForward = false;


    public void ToggleFastForward()
    {
        if (isFastForward)
            TurnOffFastForward();
        else
            TurnOnFastForward();
    }

    public void TurnOnFastForward()
    {
        isFastForward = true;
        Time.timeScale = 2f;
        SetAlpha(objectBackground, 1f);
    }

    public void TurnOffFastForward()
    {
        isFastForward = false;
        Time.timeScale = 1f;
        SetAlpha(objectBackground, 0f);
    }

    private void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;

        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public static string ScaleNumber(long value)
    {
        if (value >= 1_000_000_000)
            return (value / 1_000_000_000f).ToString("0.#") + "B";
        if (value >= 1_000_000)
            return (value / 1_000_000f).ToString("0.#") + "M";
        if (value >= 1_000)
            return (value / 1_000f).ToString("0.#") + "K";

        return value.ToString();
    }

    void Start()
    {
        coinText.text = ScaleNumber(GameManager.Instance.totalCoins);
    }

    void Update()
    {
        if (StageManager.Instance.isSummonPhase)
        {
            if (WaveSurrenderPanel.GetComponent<SurrenderScript>() != null)
            {
                WaveSurrenderPanel.GetComponent<SurrenderScript>().DeactivateButton();
            }
        } else
        {
            if (WaveStagePanel.GetComponent<SlideButton>() != null)
            {
                WaveStagePanel.GetComponent<SlideButton>().DeactivateButton();
            }
        }
    }



    public void UpdateStageText(int stageNumber)
    {
        if (stageText != null)
            stageText.text = $"1 - {stageNumber}";
        if (stageNumber <= 1)
            stageBack.SetActive(false);
        else
            stageBack.SetActive(true);

        if(stageNumber >= GameManager.Instance.highestStage)
        {
            stageNext.SetActive(false);
            if(stageNumber >= StageManager.Instance.stageDatabase.stages.Count())
            {
                WaveEndlessPanel.GetComponent<EndlessScript>().ActiveButton();
                return;
            }
            if(GameManager.Instance.currentProgress >= GameManager.Instance.targetProgress)
            {
                WaveEndlessPanel.GetComponent<EndlessScript>().DeactivateButton(true);
            } else
            {
                WaveEndlessPanel.GetComponent<EndlessScript>().DeactivateButton(false);
            }
        }
        else
        {
            stageNext.SetActive(true);
            WaveEndlessPanel.GetComponent<EndlessScript>().ActiveButton();
            WaveStagePanel.GetComponent<SlideButton>().DeactivateButton();
        }
    }

    // Optional: Button callbacks
    public void OnClickStageBack()
    {
        if (StageManager.Instance.isSummonPhase)
        {
            int prevStage = Mathf.Max(1, StageManager.Instance.currentStage - 1) - 1;
            StageManager.Instance.currentStage = prevStage;
            StageManager.Instance.NextStage();
            UpdateStageText(prevStage+1);
        }
    }

    public void OnClickStageNext()
    {
        if (StageManager.Instance.currentStage < GameManager.Instance.highestStage && StageManager.Instance.isSummonPhase)
        {
            UpdateStageText(StageManager.Instance.currentStage + 1);
            StageManager.Instance.NextStage();
        }
    }

    Coroutine indexRoutine;
    public void openIndex()
    {
        if (indexRoutine != null)
        {
            StopCoroutine(indexRoutine);
        }
        AudioManager.Instance.PlayDeniedInteraction();
        indexRoutine = StartCoroutine(ActivateAndFadeOut(1f));
    }

    public IEnumerator ActivateAndFadeOut(float duration)
    {
        // --- Step 1: Aktifkan GameObject ---
        popupFeatureIndex.SetActive(true);
        popupFeatureIndex.transform.localScale = Vector3.one * 0.9f;
        CanvasGroup cg = popupFeatureIndex.GetComponent<CanvasGroup>();
        if (cg == null) cg = popupFeatureIndex.AddComponent<CanvasGroup>();

        // --- Step 2: Scale up ---
        float elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.1f);
            float scale = Mathf.Lerp(0.9f, 1f, t);
            popupFeatureIndex.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        popupFeatureIndex.transform.localScale = Vector3.one; // pastikan scale = 1

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration); // dari 1 ke 0
            cg.alpha = alpha;
            yield return null;
        }

        cg.alpha = 1;

        // --- Step 3: Deactivate GameObject ---
        popupFeatureIndex.SetActive(false);
    }
    
    public void OnExitGamePressed()
    {
        Debug.Log("🚪 Exit Game pressed");

    #if UNITY_EDITOR
        // Kalau sedang di Unity Editor — stop play mode
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        // Kalau di build (PC, Android, dll) — keluar dari game
        Application.Quit();
    #endif
    }
}
