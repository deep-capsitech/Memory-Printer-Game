using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InstructionsUIController : MonoBehaviour
{
    [Header("UI References")]
    public Transform powerupContainer;
    public GameObject powerupItemPrefab;

    [Header("Powerup Data")]
    public List<Powerup> powerups = new List<Powerup>();

    [System.Serializable]
    public class Powerup
    {
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;
    }

    void OnEnable()
    {
        GeneratePowerups();
    }

    void GeneratePowerups()
    {
        // Clear old
        foreach (Transform child in powerupContainer)
        {
            Destroy(child.gameObject);
        }

        // Create items
        foreach (Powerup data in powerups)
        {
            GameObject obj = Instantiate(powerupItemPrefab, powerupContainer);

            Image icon = obj.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI descText = obj.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();

            icon.sprite = data.icon;
            descText.text = FormatText(data.description);
        }
    }

    // Auto bullet formatting
    string FormatText(string raw)
    {
        string[] lines = raw.Split('\n');
        string formatted = "";

        foreach (string line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
                formatted += "• " + line + "\n";
        }

        return formatted;
    }
}