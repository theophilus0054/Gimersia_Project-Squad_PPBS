using UnityEngine;

public class TrashManager : MonoBehaviour
{
    public static TrashManager Instance { get; private set; }

    public GameObject trashDropArea;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
