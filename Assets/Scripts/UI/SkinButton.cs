using UnityEngine;

public class SkinButton : MonoBehaviour
{
    public int skinID;
    public int price;

    public SkinManager skinManager;

    public void BuyOrApply()
    {
        int unlocked = PlayerPrefs.GetInt("SkinUnlocked_" + skinID, 0);

        if (unlocked == 1)
        {
            skinManager.ApplySkin(skinID);
        }
        else
        {
            int coins = PlayerPrefs.GetInt("Coins", 0);

            if (coins >= price)
            {
                coins -= price;
                PlayerPrefs.SetInt("Coins", coins);

                PlayerPrefs.SetInt("SkinUnlocked_" + skinID, 1);

                skinManager.ApplySkin(skinID);
            }
        }
    }
}