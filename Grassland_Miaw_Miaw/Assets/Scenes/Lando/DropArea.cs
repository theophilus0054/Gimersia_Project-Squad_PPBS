using UnityEngine;

public class DropArea : MonoBehaviour, IDragDrop
{
    [SerializeField] public int x;
    [SerializeField] public int y;
    [SerializeField] bool filled = false;

    public int GetX()
    {
        return x;
    }
    public int GetY()
    {
        return y;
    }
    public bool getFilled()
    {
        return filled;
    }
    public void OnItemDrop(DragScript drop)
    {
        drop.transform.position = transform.position;
        filled = true;
    }

    public void OnItemLeave(DragScript drop)
    {
        filled = false;
    }
}
