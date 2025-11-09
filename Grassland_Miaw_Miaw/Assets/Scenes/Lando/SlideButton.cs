using UnityEngine;
using System.Collections;

public class SlideButton : MonoBehaviour
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

    void OnMouseDown()
    {
        StartWave();
    }

    // StartWave slide
    public void StartWave()
    {
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
}
