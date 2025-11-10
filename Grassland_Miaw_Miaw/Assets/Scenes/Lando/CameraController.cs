using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    public Camera mainCamera; // Assign via Inspector atau pakai Camera.main
    public float slideDistance = 20f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    // -------------------------------
    // Fungsi pindah posisi kamera
    // -------------------------------
    public void MoveCameraSmooth(int positionIndex, float duration = 1f)
    {
        Vector3 targetPos = mainCamera.transform.position;

        switch (positionIndex)
        {
            case 1: targetPos.x = -(slideDistance*2); targetPos.y = 0f; break;
            case 2: targetPos.x = -slideDistance; targetPos.y = 0f; break;
            case 3: targetPos.x = 0f; targetPos.y = 0f; break;
            case 4: targetPos.x = slideDistance; targetPos.y = 0f; break;
            case 5: targetPos.x = slideDistance*2; targetPos.y = 0f; break;
        }

        StopAllCoroutines();
        StartCoroutine(SmoothMove(targetPos, duration));
    }

    private IEnumerator SmoothMove(Vector3 targetPos, float duration)
    {
        Vector3 startPos = mainCamera.transform.position;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, t / duration);
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, progress);
            yield return null;
        }

        mainCamera.transform.position = targetPos;
    }

}
