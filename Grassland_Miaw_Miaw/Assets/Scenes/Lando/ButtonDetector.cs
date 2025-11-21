using UnityEngine;

public class ButtonDetector : MonoBehaviour
{
    public int evolutionIndex = 0;

    private void OnMouseDown()
    {
        Debug.Log("Summon T1 Unagi");
        SummonManager.SummonEvolution(evolutionIndex);
    }
}
