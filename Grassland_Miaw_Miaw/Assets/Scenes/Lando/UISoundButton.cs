using UnityEngine;
using UnityEngine.EventSystems;

public class UISoundButton : MonoBehaviour, IPointerEnterHandler
{
    public void OnMouseDown()
    {
        AudioManager.Instance.PlayButtonClick();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.PlayButtonHover();
    }
}
