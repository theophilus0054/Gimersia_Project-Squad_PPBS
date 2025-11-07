using UnityEngine;
using UnityEngine.UI;

public interface IDragDrop
{
    void OnItemDrop(DragScript drop);

    void OnItemLeave(DragScript drop);

    public int GetX();
    public int GetY();
    public bool getFilled();
}