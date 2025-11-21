using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    public DropArea dropArea;
    public bool startCheck = false;

    [Tooltip("Mark which tutorials are completed.")]
    public bool[] tutorialProgress = new bool[20]; // e.g., [false, true, false, ...]

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (dropArea.getFilled() && !IsTutorialDone(8))
        {
            CompleteTutorial(8);
        }

        if (IsTutorialDone(10) && startCheck && !IsTutorialDone(11))
        {
            if (ObjectManager.Instance.enemySpawn.transform.childCount == 0)
            {
                CompleteTutorial(11);
            }
        }
    }

    public void checkEnemy()
    {
        startCheck = true;
    }

    public bool IsTutorialDone(int index)
    {
        if (index < 0 || index >= tutorialProgress.Length) return false;
        return tutorialProgress[index];
    }

    public void CompleteTutorial(int index)
    {
        if (index < 0 || index >= tutorialProgress.Length)
            return;

        // 🔹 Cek kalau bukan tutorial pertama
        if (index > 0 && !tutorialProgress[index - 1])
        {
            Debug.LogWarning($"❌ Cannot complete Tutorial {index} because Tutorial {index - 1} is not done yet!");
            return;
        }

        // 🔹 Kalau sudah, tandai sebagai complete
        tutorialProgress[index] = true;
        Debug.Log($"✅ Tutorial {index} completed!");
    }

    public void EndTutorial()
    {
        GameManager.Instance.finishedTutorial = true;
        GameManager.Instance.SaveData();
        Debug.Log("Tutorial selesai! Pindah ke loading scene...");
        Time.timeScale = 1f; // jaga-jaga kalau tutorial di-pause
        SceneManager.LoadScene("LoadScene");
    }
}
