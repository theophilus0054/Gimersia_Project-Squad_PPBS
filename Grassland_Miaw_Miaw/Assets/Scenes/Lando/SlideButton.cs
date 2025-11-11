using UnityEngine;
using System.Collections;
using System.Linq;

public class SlideButton : MonoBehaviour
{
    public float yActive = 0f;    // relatif ke parent
    public float yStart = 1.5f;   // relatif ke parent
    public float duration = 0.5f;

    private Coroutine slideCoroutine;

    // ActiveButton slide
    public void ActiveButton()
    {
        if (UIManager.Instance.WaveEndlessPanel.GetComponent<EndlessScript>().isActive || GameManager.Instance.currentProgress < GameManager.Instance.targetProgress || GameManager.Instance.highestStage != StageManager.Instance.currentStage)
        {
            Debug.Log("❌ Tidak bisa aktifkan tombol");
            return;
        }
        
        if (StageManager.Instance.currentStage >= StageManager.Instance.stageSummons.Length)
        {
            UIManager.Instance.WaveEndlessPanel.GetComponent<EndlessScript>().ActiveButton();
            return;
        }
        StartSlide(yActive);
    }

    void OnMouseDown()
    {   AudioManager.Instance.PlayDropCreature();
        // ✅ Cek apakah tombol sudah di posisi aktif sebelum bisa diklik
        if (IsAtActivePosition())
        {
            StartWave();
        }
        else
        {
            Debug.Log("❌ Belum bisa ditekan — tombol belum di posisi aktif!");
        }
    }

    public void DeactivateButton()
    {
        StartSlide(yStart);
    }

    // StartWave slide
    public void StartWave()
    {
        UIManager.Instance.WaveSurrenderPanel.GetComponent<SurrenderScript>().ActiveButton();
        StartSlide(yStart);
        Debug.Log("Wave Started!");
        StageManager.Instance.isSummonPhase = false;
    }

    private void StartSlide(float targetRelativeY)
    {
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideToRelativeY(targetRelativeY));
    }

    private IEnumerator SlideToRelativeY(float targetRelativeY)
    {
        float elapsed = 0f;
        float startY = transform.position.y;
        float targetY = (transform.parent ? transform.parent.position.y : 0f) + targetRelativeY;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newY = Mathf.Lerp(startY, targetY, elapsed / duration);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z); // X/Z tetap
            yield return null;
        }

        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
    }

    private bool IsAtActivePosition()
    {
        float targetY = (transform.parent ? transform.parent.position.y : 0f) + yActive;
        return Mathf.Abs(transform.position.y - targetY) < 0.01f; // toleransi kecil
    }
}
