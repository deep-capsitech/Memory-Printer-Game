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

    [Header("Audio")]
    public AudioSource musicSource;

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
        isSoundOn = PlayerPrefs.GetInt("sound", 1) == 1;

        // Load saved language
        selectedLanguageIndex = PlayerPrefs.GetInt("language", 0);

        ApplySoundState();
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
        isSoundOn = !isSoundOn;

        ApplySoundState();

        // Save state
        PlayerPrefs.SetInt("sound", isSoundOn ? 1 : 0);
    }

    void ApplySoundState()
    {
        // Apply audio
        if (musicSource != null)
            musicSource.mute = !isSoundOn;

        // Update icon
        if (soundIcon != null)
            soundIcon.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
    }
}