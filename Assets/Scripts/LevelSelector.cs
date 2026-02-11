using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    [Header("Level Info")]
    public int levelNumber;

    [Header("Sprites")]
    public Sprite lockedSprite;
    public Sprite unlockedSprite;

    private Button button;
    private Image buttonImage;

    void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        button.onClick.AddListener(OnLevelClicked);
    }

    void OnEnable()
    {
        UpdateLevelState();
    }

    void UpdateLevelState()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        if (levelNumber <= unlockedLevel)
        {
            buttonImage.sprite = unlockedSprite;
            button.interactable = true;
        }
        else
        {
            buttonImage.sprite = lockedSprite;
            button.interactable = false;
        }
    }
    void OnLevelClicked()
    {
        GameManagerCycle.Instance.OnLevelSelected(levelNumber);
    }
}
