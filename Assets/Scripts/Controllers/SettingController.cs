using UnityEngine;

public class SettingController : MonoBehaviour
{
    public GameObject settingsPanel;
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }
}
