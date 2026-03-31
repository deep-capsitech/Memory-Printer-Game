using UnityEngine;

public class SkinManager : MonoBehaviour
{
    public Renderer robotRenderer;

    public Material blueMat;
    public Material redMat;
    public Material greenMat;
    public Material whiteMat;

    void Start()
    {
        ApplySavedSkin();
    }

    public void ApplySkin(int skinID)
    {
        switch (skinID)
        {
            case 0:
                robotRenderer.material = blueMat;
                break;

            case 1:
                robotRenderer.material = redMat;
                break;

            case 2:
                robotRenderer.material = greenMat;
                break;

            case 3:
                robotRenderer.material = whiteMat;
                break;
        }
       

    PlayerPrefs.SetInt("SelectedSkin", skinID);
        AnalyticsManager.LogEvent("skin_selected",
      ("skin_id", skinID),
      ("skin_name", GetSkinName(skinID)));
    }

    void ApplySavedSkin()
    {
        int skin = PlayerPrefs.GetInt("SelectedSkin", 0);
        ApplySkin(skin);
    }
    string GetSkinName(int skinID)
    {
        switch (skinID)
        {
            case 0: return "blue";
            case 1: return "red";
            case 2: return "green";
            case 3: return "white";
            default: return "unknown";
        }
    }
}