using UnityEngine;

public class RewardObject : MonoBehaviour
{
    public System.Action onClick;

    void OnMouseDown() // harus ada collider
    {
        onClick?.Invoke();
        onClick = null; // supaya klik tidak double
    }
}