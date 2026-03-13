using UnityEngine;
using UnityEngine.EventSystems;

public class SettingController : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject menuPanel;
    public void OpenSettings()
    {
        if(menuPanel != null)
            menuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    public void OpenInfo()
    {
        infoPanel.SetActive(true);
    }

    public void CloseInfo()
    {
        infoPanel.SetActive(false);
    }
}

//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//public class SettingsUIController : MonoBehaviour
//{
//    [Header("GameObjects")]
//    public GameObject mainMenuPanel;
//    public GameObject selectLevelPanel;

//    [Header("Main Panels")]
//    public GameObject settingsPanel;
//    public GameObject languagePopupPanel;

//    [Header("Buttons")]
//    public GameObject settingsButton;
//    public GameObject backButton;

//    private GameObject previousPanel;
//    public RectTransform languagePopupContainer;
//    public RectTransform settingsPanelContainer;

//    void Start()
//    {
//        settingsPanel.SetActive(false);
//        languagePopupPanel.SetActive(false);
//        backButton.SetActive(false);
//    }
//    void Update()
//    {
//#if UNITY_EDITOR || UNITY_STANDALONE
//        if (Input.GetMouseButtonDown(0))
//        {
//            HandleOutsideClick(Input.mousePosition);
//        }
//#else
//        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
//        {
//            HandleOutsideClick(Input.GetTouch(0).position);
//        }
//#endif
//    }
//    void HandleOutsideClick(Vector2 screenPos)
//    {
//        // Ignore UI button clicks
//        if (EventSystem.current.IsPointerOverGameObject())
//            return;

//        // LANGUAGE POPUP OPEN
//        if (languagePopupPanel.activeSelf)
//        {
//            if (!RectTransformUtility.RectangleContainsScreenPoint(
//                languagePopupContainer,
//                screenPos,
//                null))
//            {
//                CloseLanguagePopup();
//            }

//            return;
//        }

//        // SETTINGS PANEL OPEN
//        if (settingsPanel.activeSelf)
//        {
//            if (!RectTransformUtility.RectangleContainsScreenPoint(
//                settingsPanelContainer,
//                screenPos,
//                null))
//            {
//                CloseSettings();
//            }
//        }
//    }
//    bool IsPointerInsidePopup()
//    {
//        return RectTransformUtility.RectangleContainsScreenPoint(
//            languagePopupContainer,
//            Input.mousePosition,
//            null
//        );
//    }
//    // SETTINGS

//    public void OpenSettings()
//    {
//        // Detect which panel is currently active
//        if (mainMenuPanel.activeSelf)
//            previousPanel = mainMenuPanel;
//        else if (selectLevelPanel.activeSelf)
//            previousPanel = selectLevelPanel;

//        settingsPanel.SetActive(true);

//        if (previousPanel != null)
//            previousPanel.SetActive(false);

//        settingsButton.SetActive(false);
//        backButton.SetActive(true);
//    }

//    public void CloseSettings()
//    {
//        settingsPanel.SetActive(false);
//        languagePopupPanel.SetActive(false);

//        // Restore only previous panel
//        if (previousPanel != null)
//            previousPanel.SetActive(true);

//        settingsButton.SetActive(true);
//        backButton.SetActive(false);
//    }

//    // LANGUAGE

//    public void OpenLanguagePopup()
//    {
//        Debug.Log("Opened panel");
//        languagePopupPanel.SetActive(true);
//    }

//    public void CloseLanguagePopup()
//    {
//        languagePopupPanel.SetActive(false);
//    }

//    public void SelectLanguage(string language)
//    {
//        PlayerPrefs.SetString("Language", language);
//        PlayerPrefs.Save();

//        CloseLanguagePopup();
//    }
//}