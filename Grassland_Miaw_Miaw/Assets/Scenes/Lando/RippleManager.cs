using System.Collections;
using UnityEngine;

public class RippleManager : MonoBehaviour
{
    public static RippleManager Instance { get; private set; }
    [SerializeField] public float _shockwaveTime = 3f;
    private Coroutine _shockwaveCoroutine;
    private Material _material;

    private static int _waveDistanceFromCenter = Shader.PropertyToID("_WaveDistanceFromCenter");

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        _material = GetComponent<SpriteRenderer>().material;
    }

    public void callShockwave()
    {
        if (_shockwaveCoroutine != null)
        {
            StopCoroutine(_shockwaveCoroutine);
        }
        _shockwaveCoroutine = StartCoroutine(ShockwaveCoroutine(-0.1f, 1.2f));
    }

    private IEnumerator ShockwaveCoroutine(float startPos, float endPos)
    {
        float elapsed = 0f;
        _material.SetFloat(_waveDistanceFromCenter, startPos);

        while (elapsed < _shockwaveTime)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / _shockwaveTime);
            float wavePosition = Mathf.Lerp(startPos, endPos, normalizedTime);
            _material.SetFloat(_waveDistanceFromCenter, wavePosition);
            yield return null;
        }

        _material.SetFloat(_waveDistanceFromCenter, endPos);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
