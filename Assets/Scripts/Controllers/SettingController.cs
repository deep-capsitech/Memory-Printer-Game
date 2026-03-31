using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject settingsPanel;
    public GameObject languagePanel;
    public GameObject infoPanel;

    [Header("Sound UI")]
    public Image soundIcon;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    [Header("Language Texts")]
    public TMP_Text[] languageTexts;

    [Header("Text Materials")]
    public Material defaultMaterial;
    public Material outlineMaterial;

    private bool isSoundOn;
    private int selectedLanguageIndex = -1;

    // ---------- INIT ----------
    void Start()
    {
        isSoundOn = SoundManager.Instance.IsSoundEnabled();

        selectedLanguageIndex = PlayerPrefs.GetInt("language", 0);

        UpdateIcon();
        ApplyLanguageSelection();

        HideAll();
        menuPanel.SetActive(true);
    }

    // ---------- LANGUAGE ----------
    public void SelectLanguage(int index)
    {
        selectedLanguageIndex = index;

        // Save selection
        PlayerPrefs.SetInt("language", index);

        ApplyLanguageSelection();
    }

    void ApplyLanguageSelection()
    {
        for (int i = 0; i < languageTexts.Length; i++)
        {
            if (i == selectedLanguageIndex)
                languageTexts[i].fontMaterial = outlineMaterial;
            else
                languageTexts[i].fontMaterial = defaultMaterial;
        }
    }

    // ---------- PANEL CONTROL ----------
    void HideAll()
    {
        if (menuPanel) menuPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (languagePanel) languagePanel.SetActive(false);
        if (infoPanel) infoPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        if (menuPanel != null)
            menuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        if (infoPanel != null)
            infoPanel.SetActive(false);
    }
    public void CloseSettings()
    {
        HideAll();
        menuPanel.SetActive(true);
    }

    public void OpenLanguage()
    {
        HideAll();
        languagePanel.SetActive(true);
    }

    public void CloseLanguage()
    {
        HideAll();
        settingsPanel.SetActive(true);
    }

    public void OpenInfo()
    {
        HideAll();
        infoPanel.SetActive(true);
    }

    public void CloseInfo()
    {
        HideAll();
        settingsPanel.SetActive(true);
    }

    // ---------- SOUND ----------
    public void ToggleSound()
    {
        SoundManager.Instance.ToggleSound(); // 🔥 CALL MAIN SYSTEM

        isSoundOn = SoundManager.Instance.IsSoundEnabled();

        PlayerPrefs.SetInt("sound", isSoundOn ? 1 : 0);

        UpdateIcon();
    }

    void UpdateIcon()
    {
        if (soundIcon != null)
            soundIcon.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
    }
}