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

    [Header("Stage Info")]
    public TextMeshPro stageText;

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
            stageNext.SetActive(false);
        else
            stageNext.SetActive(true);
    }

    // Optional: Button callbacks
    public void OnClickStageBack()
    {
        int prevStage = Mathf.Max(1, StageManager.Instance.currentStage - 1) - 1;
        StageManager.Instance.currentStage = prevStage;
        StageManager.Instance.NextStage();
        UpdateStageText(prevStage+1);
    }

    public void OnClickStageNext()
    {
        if(StageManager.Instance.currentStage < GameManager.Instance.highestStage)
        { 
            UpdateStageText(StageManager.Instance.currentStage+1);
            StageManager.Instance.NextStage();
        }
    }
}
