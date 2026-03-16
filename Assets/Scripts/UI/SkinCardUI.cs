using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkinCardUI : MonoBehaviour
{
    public Image skinImage;
    public TMP_Text skinName;

    public TMP_Text priceText;
    public TMP_Text buttonText;

    public GameObject coinIcon;

    public Image cardBackground;
    public Sprite normalCard;
    public Sprite selectedCard;

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
            // LOCKED
            buttonText.text = "BUY";

            priceText.text = data.price.ToString();

            coinIcon.SetActive(true);
            cardBackground.sprite = normalCard;
        }
        else
        {
            // BOUGHT
            priceText.text = "000";        // 👈 only change price
            coinIcon.SetActive(true);      // 👈 keep icon visible

            if (selected == index)
            {
                buttonText.text = "SELECTED";
                cardBackground.sprite = selectedCard;
            }
            else
            {
                buttonText.text = "APPLY";
                cardBackground.sprite = normalCard;
            }
        }

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        int unlocked = PlayerPrefs.GetInt("SkinUnlocked_" + skinIndex, skinIndex == 0 ? 1 : 0);

        if (unlocked == 0)
        {
            RobotSkinManager.Instance.BuySkin(skinIndex, skinData.price);
        }
        else
        {
            RobotSkinManager.Instance.ApplySkin(skinIndex);
        }
    }
}