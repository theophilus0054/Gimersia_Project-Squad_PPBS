using UnityEngine;
using UnityEngine.EventSystems;

public class InputLockManager : MonoBehaviour
{
    public static InputLockManager Instance;

    public bool IsLocked { get; set; }

    private EventSystem eventSystem;

    void Awake()
    {
        Instance = this;
        eventSystem = EventSystem.current;
    }

    public void LockInput()
    {
        IsLocked = true;

        if (eventSystem != null)
            eventSystem.enabled = false;
    }

    public void UnlockInput()
    {
        IsLocked = false;

        if (eventSystem != null)
            eventSystem.enabled = true;
    }
}
