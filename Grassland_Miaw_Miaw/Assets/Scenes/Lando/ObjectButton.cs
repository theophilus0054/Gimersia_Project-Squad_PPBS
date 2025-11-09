using UnityEngine;
using UnityEngine.Events;
public class ObjectButton : MonoBehaviour
{
    [Header("Event On Click")]
    public UnityEvent onClick;

    private void OnMouseDown()
    {
        onClick?.Invoke();
    }
}
