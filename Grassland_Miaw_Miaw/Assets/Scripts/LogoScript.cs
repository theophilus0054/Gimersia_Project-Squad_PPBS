using UnityEngine;

[ExecuteAlways] // berjalan di editor & play mode
public class LogoScript : MonoBehaviour
{
    [Header("Options")]
    public bool centerPosition = true;
    public bool centerRotation = false;
    public bool centerScale = false;

    void Update()
    {
        if (transform.parent == null) return;

        if (centerPosition)
            transform.localPosition = Vector3.zero;

        if (centerRotation)
            transform.localRotation = Quaternion.identity;

        if (centerScale)
            transform.localScale = Vector3.one;
    }
}
