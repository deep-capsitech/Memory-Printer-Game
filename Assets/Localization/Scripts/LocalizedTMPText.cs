using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedTMPText : MonoBehaviour
{
    [SerializeField] string localizationKey;
    TextMeshProUGUI text;
    public string dynamicValue;
    public bool useDynamicValue;
    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        if (LocalizationManager.Instance == null)
        {
            Debug.LogError(
                $"LocalizationManager not found for {gameObject.name}",
                this
            );
            return;
        }

        LocalizationManager.Instance.OnLanguageChanged += UpdateText;
        UpdateText();
    }

    void OnDisable()
    {
        if (LocalizationManager.Instance == null)
            return;

        LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
    }

    void UpdateText()
    {
        if (text == null) return;

        if (useDynamicValue)
            text.text = LocalizationManager.Instance.GetText(localizationKey, dynamicValue);
        else
            text.text = LocalizationManager.Instance.GetText(localizationKey);
    }
}
