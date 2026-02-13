using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelector : MonoBehaviour
{
    [Header("Runtime Data")]
    public int levelNumber;

    [Header("UI References")]
    public Image lockIcon;
    public Image[] stars;
    public TextMeshProUGUI levelText;

    [Header("Star Sprites")]
    public Sprite filledStar;
    public Sprite emptyStar;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);
    }

    public void Setup(int level, bool unlocked, int starCount, Color themeColor)
    {
        levelNumber = level;
        levelText.text = level.ToString();

        GetComponent<Image>().color = themeColor;

        button.interactable = unlocked;
        lockIcon.gameObject.SetActive(!unlocked);

        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].enabled = true;
            stars[i].sprite = (i < starCount) ? filledStar : emptyStar;
        }
    }


    void OnClicked()
    {
        GameManagerCycle.Instance.OnLevelSelected(levelNumber);
    }
}
