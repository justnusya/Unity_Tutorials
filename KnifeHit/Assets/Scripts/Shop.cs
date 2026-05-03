using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Shop : MonoBehaviour
{
    public TMP_Text coinsText;

    [Header("Knife Prices")]
    public int[] knifePrices = { 0, 150, 250, 400 };

    [Header("Buttons")]
    public Button[] actionButtons;

    private int totalCoins;

    void Start()
    {
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (coinsText != null)
            coinsText.text = totalCoins.ToString();

        for (int i = 0; i < actionButtons.Length; i++)
        {
            if (actionButtons[i] == null) continue;

            string knifeKey = "KnifeBought_" + i;
            int isBought = PlayerPrefs.GetInt(knifeKey, (i == 0 ? 1 : 0));
            string buttonLabel = "";
            int selectedKnife = PlayerPrefs.GetInt("SelectedKnife", 0);

            if (isBought == 1)
            {
                buttonLabel = (selectedKnife == i) ? "Selected" : "Select";
                actionButtons[i].interactable = (selectedKnife != i);
            }
            else
            {
                buttonLabel = "Buy (" + knifePrices[i] + ")";
                actionButtons[i].interactable = true;
            }
            Text legacyText = actionButtons[i].GetComponentInChildren<Text>();
            if (legacyText != null) legacyText.text = buttonLabel;
            TMP_Text tmpText = actionButtons[i].GetComponentInChildren<TMP_Text>();
            if (tmpText != null) tmpText.text = buttonLabel;
        }
    }

    public void OnButtonClick(int knifeIndex)
    {
        string knifeKey = "KnifeBought_" + knifeIndex;
        int isBought = PlayerPrefs.GetInt(knifeKey, (knifeIndex == 0 ? 1 : 0));

        if (isBought == 1)
        {
            PlayerPrefs.SetInt("SelectedKnife", knifeIndex);
        }
        else
        {
            if (totalCoins >= knifePrices[knifeIndex])
            {
                totalCoins -= knifePrices[knifeIndex];
                PlayerPrefs.SetInt("TotalCoins", totalCoins);
                PlayerPrefs.SetInt(knifeKey, 1);
                PlayerPrefs.SetInt("SelectedKnife", knifeIndex);
            }
            else return;
        }

        PlayerPrefs.Save();
        UpdateUI();
    }
}