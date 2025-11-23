using UnityEngine;
using System.Collections;

public class SurrenderScript : MonoBehaviour
{
    public float yActive = 0f;    // relatif ke parent
    public float yStart = 1.5f;   // relatif ke parent
    public float duration = 0.5f;

    private Coroutine slideCoroutine;

    // ActiveButton slide
    public void ActiveButton()
    {
        StartSlide(yActive);
    }

    public void DeactivateButton()
    {
        StartSlide(yStart);
    }

    void OnMouseDown()
    {
        AudioManager.Instance.PlayDropCreature();
        // ✅ Cek apakah tombol sudah di posisi aktif sebelum bisa diklik
        if (IsAtActivePosition())
        {
            StopWave(true);
        }
        else
        {
            Debug.Log("❌ Belum bisa ditekan — tombol belum di posisi aktif!");
        }
    }

    // StopWave slide
    public void StopWave(bool fail)
    {
        StartSlide(yStart);
        Debug.Log("Wave Stopped!");
        if (fail)
        {
            StageManager.Instance.FailedWave();
        }
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
