using UnityEngine;
using System;
using System.Collections.Generic;
using DG.Tweening;

public class SlideStageScript : MonoBehaviour
{
    public static SlideStageScript Instance { get; private set; }
    private static Camera cam;

    // Simpan posisi awal tiap target
    private Dictionary<GameObject, Vector3> startPositions = new Dictionary<GameObject, Vector3>();

    void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void EnsureManager()
    {
        if (cam == null)
            cam = Camera.main;
    }

    // ===================================================
    // 🎬 ENTRY
    // ===================================================
    public void SlidePlay(GameObject target, float distance = 5f, float duration = 2f,
        bool easeOutExit = true, Action onComplete = null)
    {
        EnsureManager();
        RunSlideSequence(target, distance, duration, easeOutExit, onComplete);
    }

    // ===================================================
    // 🎞️ DOTWEEN SLIDE
    // ===================================================
    private void RunSlideSequence(GameObject target, float distance, float duration,
        bool easeOutExit, Action onComplete)
    {
        // Simpan posisi awal jika belum ada
        if (!startPositions.ContainsKey(target))
            startPositions[target] = target.transform.position;

        Vector3 start = startPositions[target];
        Vector3 mid = start - Vector3.right * distance;
        Vector3 end = start - Vector3.right * distance * 2f;
        float half = duration / 2f;

        // Hentikan animasi sebelumnya DAN reset ke posisi awal
        target.transform.DOKill();
        target.transform.position = start;

        // Buat sequence baru
        Sequence seq = DOTween.Sequence();

        seq.Append(target.transform.DOMove(mid, half).SetEase(Ease.OutQuad));
        seq.AppendInterval(1f);

        if (easeOutExit)
            seq.Append(target.transform.DOMove(end, half).SetEase(Ease.OutQuad));
        else
            seq.Append(target.transform.DOMove(end, half).SetEase(Ease.InQuad));

        seq.AppendCallback(() =>
        {
            target.transform.position = start; // reset final
        });

        seq.AppendCallback(() =>
        {
            if (target.name == "WaveFailedFrame")
            {
                UIManager.Instance.WaveStagePanel.GetComponent<SlideButton>().ActiveButton();
            }
        });

        if (onComplete != null)
            seq.OnComplete(() => onComplete.Invoke());
    }
}
