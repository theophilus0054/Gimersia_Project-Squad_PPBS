using UnityEngine;

public class ButtonDetector : MonoBehaviour
{
    public int evolutionIndex = 0;

    private void OnMouseDown()
    {
        SummonManager.SummonEvolution(evolutionIndex);
    }
}
