using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public SkinDatabase database;

    public Transform contentParent;

    public GameObject skinCardPrefab;

    public GameObject robotRoot;

    private Renderer[] robotRenderers;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        robotRenderers = robotRoot.GetComponentsInChildren<Renderer>();

        GenerateShop();
        ApplySavedSkin();
    }

    void GenerateShop()
    {
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
            ApplySkin(index);

            RefreshShopUI();
        }
    }

    void RefreshShopUI()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        GenerateShop();
    }

    public void ApplySkin(int index)
    {
        Material mat = database.skins[index].skinMaterial;

        foreach (Renderer r in robotRenderers)
        {
            r.material = mat;
        }

        PlayerPrefs.SetInt("SelectedSkin", index);

        RefreshShopUI();
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