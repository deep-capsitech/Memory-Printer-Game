using UnityEngine;

public class ShopUIManager : MonoBehaviour
{
    public GameObject mainShopPanel;
    public GameObject robotSkinPanel;
    public GameObject coinsPanel;

    void OnEnable()
    {
        ShowMainShop();
    }

    public void ShowMainShop()
    {
        mainShopPanel.SetActive(true);
        robotSkinPanel.SetActive(false);
        coinsPanel.SetActive(false);
    }

    public void OpenRobotSkinShop()
    {
        mainShopPanel.SetActive(false);
        robotSkinPanel.SetActive(true);
        coinsPanel.SetActive(false);
    }

    public void OpenCoinsShop()
    {
        mainShopPanel.SetActive(false);
        robotSkinPanel.SetActive(false);
        coinsPanel.SetActive(true);
    }

    public void CloseRobotSkinPanel()
    {
        ShowMainShop();
    }

    public void CloseCoinsPanel()
    {
        ShowMainShop();
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);
    }
}