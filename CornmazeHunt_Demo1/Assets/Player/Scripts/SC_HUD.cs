using UnityEngine;
using UnityEngine.UI;

public class SC_HUD : MonoBehaviour
{
    [SerializeField] private SC_PlayerMovement mPlayerData;

    [SerializeField] private Slider mStaminaBar;

    [SerializeField] private GameObject mHealth1;
    [SerializeField] private GameObject mHealth2;
    [SerializeField] private GameObject mHealth3;

    [SerializeField] private Color mHealthFullColor;
    [SerializeField] private Color mHealthEmptyColor;

    private void Update()
    {
        mStaminaBar.value = mPlayerData.GetStamina() / mPlayerData.GetMaxStamina();

        if (mPlayerData.GetHealth() < 3) { mHealth3.GetComponent<Image>().color = mHealthEmptyColor; }
        else { mHealth3.GetComponent<Image>().color = mHealthFullColor; }

        if (mPlayerData.GetHealth() < 2) { mHealth2.GetComponent<Image>().color = mHealthEmptyColor; }
        else { mHealth2.GetComponent<Image>().color = mHealthFullColor; }

        if (mPlayerData.GetHealth() < 2) { mHealth1.GetComponent<Image>().color = mHealthEmptyColor; }
        else { mHealth1.GetComponent<Image>().color = mHealthFullColor; }
    }
}
