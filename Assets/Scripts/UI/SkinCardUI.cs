using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkinCardUI : MonoBehaviour
{
    public Image skinImage;
    public TMP_Text skinName;
    public TMP_Text priceText;
    public Button actionButton;
    public TMP_Text buttonText;
    public GameObject coinIcon;

    private SkinData skinData;
    private int skinIndex;

    public void Setup(SkinData data, int index)
    {
        skinData = data;
        skinIndex = index;

        skinImage.sprite = data.skinIcon;
        skinName.text = data.skinName;

        int unlocked = PlayerPrefs.GetInt("SkinUnlocked_" + index, index == 0 ? 1 : 0);
        int selected = PlayerPrefs.GetInt("SelectedSkin", 0);

        if (unlocked == 0)
        {
            // NOT BOUGHT
            priceText.gameObject.SetActive(true);
            coinIcon.SetActive(true);

            priceText.text = data.price.ToString();
            buttonText.text = "BUY";
        }
        else
        {
            // BOUGHT
            priceText.gameObject.SetActive(false);
            coinIcon.SetActive(false);

            if (selected == index)
            {
                buttonText.text = "EQUIPPED";
                actionButton.interactable = false;
            }
            else
            {
                buttonText.text = "APPLY";
                actionButton.interactable = true;
            }
        }

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnClick);
    }
    void OnClick()
    {
        int unlocked = PlayerPrefs.GetInt("SkinUnlocked_" + skinIndex, skinIndex == 0 ? 1 : 0);

        if (unlocked == 0)
        {
            ShopManager.Instance.BuySkin(skinIndex, skinData.price);
        }
        else
        {
            ShopManager.Instance.ApplySkin(skinIndex);
        }
    }
}