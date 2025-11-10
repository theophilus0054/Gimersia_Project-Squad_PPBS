using UnityEngine;

public class SummonGUIButtonDetector : MonoBehaviour
{
    [SerializeField]
    public enum ButtonType
    {
        Type1,
        Type2,
        Type3,
        Summon,
        LeftArrow,
        RightArrow
    }

    [Header("Button Settings")]
    public ButtonType buttonType;
    
    private void OnMouseDown()
    {
        if (buttonType == ButtonType.Summon)
        {
            Debug.Log("Summon Button Clicked");
            // Add summon logic here
        }
        else if (buttonType == ButtonType.Type1)
        {
            Debug.Log("Type 1 Button Clicked");
            // Add logic for Type 1 button
        }
        else if (buttonType == ButtonType.Type2)
        {
            Debug.Log("Type 2 Button Clicked");
            // Add logic for Type 2 button
        }
        else if (buttonType == ButtonType.Type3)
        {
            Debug.Log("Type 3 Button Clicked");
            // Add logic for Type 3 button
        }
        else if (buttonType == ButtonType.LeftArrow)
        {
            Debug.Log("Left Arrow Button Clicked");
            // Add logic for Left Arrow button
        }
        else if (buttonType == ButtonType.RightArrow)
        {
            Debug.Log("Right Arrow Button Clicked");
            // Add logic for Right Arrow button
        }
    }
}
