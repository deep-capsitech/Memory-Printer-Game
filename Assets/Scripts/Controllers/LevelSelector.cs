using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelector : MonoBehaviour
{
    [Header("UI")]
    public Image buttonFrame;
    public Image lockIcon;
    public Image[] stars;
    public TextMeshProUGUI levelText;

    [Header("Star Sprites (WHITE ONLY)")]
    public Sprite filledStar;
    public Sprite emptyStar;

    private Button button;
    private Material runtimeTextMaterial;
    private int levelNumber;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);
    }

    public void Setup(int level, bool unlocked, int starCount, WorldData world)
    {
        levelNumber = level;
        levelText.text = level.ToString();

        // 🔹 BUTTON FRAME
        buttonFrame.color = world.primaryColor;

        // 🔹 LEVEL TEXT OUTLINE (IMPORTANT)
        runtimeTextMaterial = Instantiate(levelText.fontMaterial);
        runtimeTextMaterial.SetColor("_OutlineColor", world.primaryColor);
        levelText.fontMaterial = runtimeTextMaterial;
        levelText.color = Color.white;

        // 🔹 LOCK ICON (WHITE SVG / PNG REQUIRED)
        lockIcon.color = world.secondaryColor;
        lockIcon.gameObject.SetActive(!unlocked);

        // 🔹 STARS
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].enabled = true;
            Color filledColor = new Color(1f, 0.85f, 0f);  // gold
            Color emptyColor = new Color(0.5f, 0.5f, 0.5f); // gray

            if (i < starCount)
            {
                stars[i].sprite = filledStar;
                stars[i].color = filledColor;
            }
            else
            {
                stars[i].sprite = emptyStar;
                stars[i].color = emptyColor;
            }
        }

        button.interactable = unlocked;
    }

    void OnClicked()
    {
        GameManagerCycle.Instance.OnLevelSelected(levelNumber);
    }
}
