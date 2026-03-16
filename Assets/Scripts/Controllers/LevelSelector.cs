using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelector : MonoBehaviour
{
    public Image buttonFrame;
    public Image lockIcon;
    public Image[] stars;
    public TextMeshProUGUI levelText;

    public Sprite filledStar;
    public Sprite emptyStar;

    private Button button;
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

        // LEVEL NUMBER COLOR SAME AS WORLD TITLE
        levelText.color = world.primaryColor;

        // LOCK ICON
        lockIcon.gameObject.SetActive(!unlocked);

        // STARS (keep sprite colors)
        for (int i = 0; i < stars.Length; i++)
        {
            if (i < starCount)
            {
                stars[i].sprite = filledStar;
                stars[i].color = Color.white;
            }
            else
            {
                stars[i].sprite = emptyStar;
                stars[i].color = Color.white;
            }
        }

        button.interactable = unlocked;
    }

    void OnClicked()
    {
        GameManagerCycle.Instance.OnLevelSelected(levelNumber);
    }
}