using UnityEngine;

public class ShopUIController : MonoBehaviour
{
    public GameObject shopMainPanel;
    public GameObject robotSkinPanel;
    public GameObject coinsPanel;

    void Start()
    {
        ShowMainShop();
    }

    public void ShowMainShop()
    {
        shopMainPanel.SetActive(true);
        robotSkinPanel.SetActive(false);
        coinsPanel.SetActive(false);
    }

    public void OpenRobotSkinShop()
    {
        shopMainPanel.SetActive(false);
        robotSkinPanel.SetActive(true);
        coinsPanel.SetActive(false);
    }

    public void OpenCoinsShop()
    {
        shopMainPanel.SetActive(false);
        robotSkinPanel.SetActive(false);
        coinsPanel.SetActive(true);
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);
    }
}