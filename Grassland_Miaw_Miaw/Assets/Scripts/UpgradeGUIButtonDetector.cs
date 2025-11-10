using UnityEngine;
using UnityEngine.UI;

public class UpgradeGUIButtonDetector : MonoBehaviour
{
    public enum ButtonType { PetType, LandType, Unlock }
    public enum PetUpgradeType { NAVBAR, StickyBullet, KnockbackPunch, ThornArmor }
    public enum LandUpgradeType { NAVBAR, LandExpansion1, LandExpansion2, LandExpansion3, LandExpansion4 }

    [Header("Button Settings")]
    public ButtonType buttonType;
    public PetUpgradeType petUpgradeType;
    public LandUpgradeType landUpgradeType;

    // 👇 Flags that other scripts can read
    private bool isPetButtonClicked = false;
    private bool isFarmButtonClicked = false;

    // 👇 Public getters (read-only)
    public bool IsPetButtonClicked => isPetButtonClicked;
    public bool IsFarmButtonClicked => isFarmButtonClicked;

    private void OnMouseDown()
    {
        ResetClickFlags();

        // Detect which type was clicked
        switch (buttonType)
        {
            case ButtonType.PetType:
                isPetButtonClicked = true;
                HandlePetButtonClick();
                break;

            case ButtonType.LandType:
                isFarmButtonClicked = true;
                HandleFarmButtonClick();
                break;

            case ButtonType.Unlock:
                Debug.Log("Unlock Button Clicked");
                break;
        }
    }

    private void HandlePetButtonClick()
    {
        switch (petUpgradeType)
        {
            case PetUpgradeType.NAVBAR: Debug.Log("Pet Upgrade Navbar Clicked"); isPetButtonClicked = true; break;
            case PetUpgradeType.StickyBullet: Debug.Log("Sticky Bullet Upgrade Clicked"); break;
            case PetUpgradeType.KnockbackPunch: Debug.Log("Knockback Punch Upgrade Clicked"); break;
            case PetUpgradeType.ThornArmor: Debug.Log("Thorn Armor Upgrade Clicked"); break;
        }
    }

    private void HandleFarmButtonClick()
    {
        switch (landUpgradeType)
        {
            case LandUpgradeType.NAVBAR: Debug.Log("Land Upgrade Navbar Clicked"); isFarmButtonClicked = true; break;
            case LandUpgradeType.LandExpansion1: Debug.Log("Land Expansion 1 Upgrade Clicked"); break;
            case LandUpgradeType.LandExpansion2: Debug.Log("Land Expansion 2 Upgrade Clicked"); break;
            case LandUpgradeType.LandExpansion3: Debug.Log("Land Expansion 3 Upgrade Clicked"); break;
            case LandUpgradeType.LandExpansion4: Debug.Log("Land Expansion 4 Upgrade Clicked"); break;
        }
    }

    // ✅ Optional: reset manually
    public void ResetClickFlags()
    {
        isPetButtonClicked = false;
        isFarmButtonClicked = false;
    }
}
