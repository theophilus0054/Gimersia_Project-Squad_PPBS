using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI to Disable When Paused")]
    public CanvasGroup[] uiToDisable;

    public bool IsPaused { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Pause()
    {
        InputLockManager.Instance.IsLocked = true;
        Time.timeScale = 0f;
        IsPaused = true;

        SetUIState(uiToDisable, false);
    }

    public void Resume()
    {
        InputLockManager.Instance.IsLocked = false;
        Time.timeScale = 1f;
        IsPaused = false;

        SetUIState(uiToDisable, true);
    }

    void SetUIState(CanvasGroup[] list, bool enable)
    {
        foreach (var cg in list)
        {
            cg.blocksRaycasts = enable;
        }
    }

    public void Toggle()
    {
        if (IsPaused) Resume();
        else Pause();
    }
}
