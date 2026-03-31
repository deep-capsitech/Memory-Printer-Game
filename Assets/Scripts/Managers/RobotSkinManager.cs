using UnityEngine;

public class RobotSkinManager : MonoBehaviour
{
    public static RobotSkinManager Instance;

    public SkinDatabase database;

    public Transform contentParent;
    public GameObject skinCardPrefab;

    public GameObject robotRoot;

    private Renderer[] robotRenderers;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        robotRenderers = robotRoot.GetComponentsInChildren<Renderer>();

        ApplySavedSkin();
        GenerateShop();
    }

    void GenerateShop()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < database.skins.Length; i++)
        {
            GameObject card = Instantiate(skinCardPrefab, contentParent);

            SkinCardUI ui = card.GetComponent<SkinCardUI>();
            ui.Setup(database.skins[i], i);
        }
    }

    public void BuySkin(int index, int price)
    {
        if (GameEconomyManager.Instance.SpendCoins(price))
        {
            PlayerPrefs.SetInt("SkinUnlocked_" + index, 1);
            PlayerPrefs.Save();
            AnalyticsManager.LogEvent("skin_purchased",
           ("skin_id", index),
           ("price", price),
           ("remaining_coins", GameEconomyManager.Instance.GetCoins()));


            GenerateShop();
        }
    }

    public void ApplySkin(int index)
    {
        Material mat = database.skins[index].skinMaterial;

        foreach (Renderer r in robotRenderers)
        {
            r.material = mat;
        }

        PlayerPrefs.SetInt("SelectedSkin", index);
        PlayerPrefs.Save();

        GenerateShop();
    }
    void ApplySavedSkin()
    {
        int skin = PlayerPrefs.GetInt("SelectedSkin", 0);

        Material mat = database.skins[skin].skinMaterial;

        foreach (Renderer r in robotRenderers)
        {
            r.material = mat;
        }
    }
}