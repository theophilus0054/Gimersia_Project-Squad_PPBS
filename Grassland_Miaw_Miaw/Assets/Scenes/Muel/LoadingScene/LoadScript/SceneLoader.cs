using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Attach this script to a GameObject in your "LoadScene"

public class SceneLoader : MonoBehaviour
{
    // In the Inspector, drag your "LoadingBar" GameObject
    [SerializeField]
    private LoadingBarSprite loadingBar;

    // Set this to the name of the scene you want to load
    [SerializeField]
    private string sceneToLoad;

    // --- NEW VARIABLE ---
    // Add a minimum time (in seconds) to show the loading bar.
    // You can change this in the Inspector.
    [SerializeField]
    private float minLoadTime = 2.0f;

    void Start()
    {
        // Check if dependencies are set
        if (loadingBar == null)
        {
            Debug.LogError("SceneLoader: 'LoadingBar' is not assigned in the Inspector!");
            return;
        }
        if (string.IsNullOrEmpty(sceneToLoad))
        {
             Debug.LogError("SceneLoader: 'SceneToLoad' is not set in the Inspector!");
             return;
        }

        // Start loading the scene in the background
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        float elapsedTime = 0f;

        // Start the asynchronous operation
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);

        // --- MODIFICATION 1 ---
        // Prevent the scene from activating as soon as it's ready.
        // We will control when it activates.
        operation.allowSceneActivation = false;

        // Loop until BOTH conditions are met:
        // 1. The scene is at least 90% loaded (operation.progress < 0.9f)
        // 2. The minimum load time has passed (elapsedTime < minLoadTime)
        while (operation.progress < 0.9f || elapsedTime < minLoadTime)
        {
            // Count up our timer
            elapsedTime += Time.deltaTime;

            // Calculate progress based on TIME (0.0 to 1.0)
            float timeProgress = Mathf.Clamp01(elapsedTime / minLoadTime);

            // Calculate progress based on actual LOAD (0.0 to 1.0)
            float loadProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // The progress bar will show the SLOWER of the two.
            // This ensures the bar waits for both the real load AND the min time.
            float displayProgress = Mathf.Min(timeProgress, loadProgress);

            loadingBar.SetProgress(displayProgress);

            // Wait until the next frame
            yield return null;
        }

        // --- Loop is done. Now we are ready to switch scenes. ---

        // Force the bar to 100% just in case
        loadingBar.SetProgress(1.0f);

        // --- FIX 2: THE "FREEZE" ---
        // Reset the Time Scale to 1 (normal speed) to un-pause the game.
        // This fixes the "freeze" issue in the next scene.
        Time.timeScale = 1.0f;

        // --- MODIFICATION 2 ---
        // Now we allow the scene to finally activate.
        operation.allowSceneActivation = true;
    }
}