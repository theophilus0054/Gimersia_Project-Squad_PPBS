using UnityEngine;
using TMPro;
using UnityEditor.SceneManagement;

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
    public GameObject waveFailedFrame;
    public GameObject waveFinishedFrame;

    [Header("Stage Info")]
    public TextMeshPro stageText;
    public TextMeshProUGUI coinText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Optional: inisialisasi stage text
        UpdateStageText(StageManager.Instance.currentStage);
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
            if(stageNumber >= StageManager.Instance.stageSummons.Length)
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
        if(StageManager.Instance.currentStage < GameManager.Instance.highestStage && StageManager.Instance.isSummonPhase)
        { 
            UpdateStageText(StageManager.Instance.currentStage+1);
            StageManager.Instance.NextStage();
        }
    }
}
