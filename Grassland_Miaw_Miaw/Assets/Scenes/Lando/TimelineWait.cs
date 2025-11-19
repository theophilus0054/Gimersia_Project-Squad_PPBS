using UnityEngine;
using UnityEngine.Playables;
using System.Collections;

public class TimelineWait : MonoBehaviour
{
    public PlayableDirector director;

    // Called by Signal
    public void WaitForTutorialIndex(int index)
    {
        // 🔹 Cari tutorial berikutnya yang belum selesai
        while (index < TutorialManager.Instance.tutorialProgress.Length &&
            TutorialManager.Instance.IsTutorialDone(index))
        {
            index++;
        }

        // 🔹 Kalau semua sudah selesai, langsung lanjut tanpa nunggu
        if (index >= TutorialManager.Instance.tutorialProgress.Length)
        {
            Debug.Log("✅ All tutorials are completed, continuing timeline...");
            return;
        }

        // 🔹 Pause Timeline dan tunggu sampai tutorial ini selesai
        var root = director.playableGraph.GetRootPlayable(0);
        root.SetSpeed(0); 
        StartCoroutine(WaitUntilTutorialDone(index));
    }

    private IEnumerator WaitUntilTutorialDone(int index)
    {
        // Wait until the condition is true
        yield return new WaitUntil(() => TutorialManager.Instance.IsTutorialDone(index));

        // Resume timeline
        var root = director.playableGraph.GetRootPlayable(0);
        root.SetSpeed(1);
    }
}
