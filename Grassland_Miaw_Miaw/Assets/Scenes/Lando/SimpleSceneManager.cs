using UnityEngine;
using UnityEngine.SceneManagement;


public class SimpleSceneManager : MonoBehaviour
{
    public void GoToScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void ResetGameData()
    {
        GameManager.Instance.ResetData(true);
    }
}
