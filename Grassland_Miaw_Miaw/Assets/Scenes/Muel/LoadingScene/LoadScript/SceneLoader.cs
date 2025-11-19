using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private LoadingBarSprite loadingBar;
    [SerializeField] private GameObject tutorialChoiceUI;
    [SerializeField] private GameObject loadingCompleteText;

    [Header("Scene Names")]
    [SerializeField] private string mainSceneName = "MainScene";
    [SerializeField] private string tutorialSceneName = "TutorialScene";

    [Header("Settings")]
    [SerializeField] private float minLoadTime = 2.0f;

    private bool isReadyToProceed = false;
    private bool skipTutorial = false;

    private void Start()
    {
        if (loadingBar == null)
        {
            Debug.LogError("❌ SceneLoader: Missing LoadingBarSprite reference!");
            return;
        }

        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        // ✅ Cek dulu apakah tutorial sudah selesai
        bool shouldShowTutorialChoice = GameManager.Instance != null && !GameManager.Instance.finishedTutorial;
        
        string targetScene = shouldShowTutorialChoice ? tutorialSceneName : mainSceneName;
        
        Debug.Log($"🎮 Loading scene: {targetScene}, ShowChoice: {shouldShowTutorialChoice}");

        float elapsedTime = 0f;
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        operation.allowSceneActivation = false;

        // Loading progress
        while (operation.progress < 0.9f || elapsedTime < minLoadTime)
        {
            elapsedTime += Time.deltaTime;
            float timeProgress = Mathf.Clamp01(elapsedTime / minLoadTime);
            float loadProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float displayProgress = Mathf.Min(timeProgress, loadProgress);
            loadingBar.SetProgress(displayProgress);
            yield return null;
        }

        loadingBar.SetProgress(1f);
        yield return new WaitForSeconds(0.3f);

        // ✅ Jika tutorial belum selesai, tampilkan UI pilihan
        if (shouldShowTutorialChoice)
        {
            loadingCompleteText?.SetActive(false);
            tutorialChoiceUI?.SetActive(true);

            Debug.Log("⏸️ Waiting for user choice...");

            // Tunggu pilihan user
            yield return new WaitUntil(() => isReadyToProceed);

            Debug.Log($"✅ User choice received! Skip: {skipTutorial}");

            tutorialChoiceUI?.SetActive(false);

            // 🔧 Jika skip, langsung load main scene (synchronous)
            if (skipTutorial)
            {
                Debug.Log("⏭️ Skipping to main scene...");
                Time.timeScale = 1f;
                SceneManager.LoadScene(mainSceneName);
                yield break;
            }
            
            Debug.Log("▶️ User chose tutorial - activating scene");
        }

        // Aktivasi scene (tutorial atau main)
        Debug.Log($"🚀 Activating scene: {targetScene}");
        Time.timeScale = 1f;
        operation.allowSceneActivation = true;
    }

    // --- UI BUTTON EVENTS ---
    public void OnPlayTutorialPressed()
    {
        Debug.Log("▶️ OnPlayTutorialPressed() called!");
        skipTutorial = false;
        isReadyToProceed = true;
    }

    public void OnSkipTutorialPressed()
    {
        Debug.Log("⏭ OnSkipTutorialPressed() called!");
        
        // Tandai tutorial sudah selesai
        if (GameManager.Instance != null)
        {
            GameManager.Instance.finishedTutorial = true;
            GameManager.Instance.SaveData();
            Debug.Log("💾 Tutorial marked as finished and saved");
        }
        else
        {
            Debug.LogWarning("⚠️ GameManager.Instance is null!");
        }
        
        skipTutorial = true;
        isReadyToProceed = true;
        
        Debug.Log($"✅ Skip flags set!");
    }
}