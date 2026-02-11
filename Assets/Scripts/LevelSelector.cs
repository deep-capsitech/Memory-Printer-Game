using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    [Header("Level Info")]
    public int levelNumber;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnLevelClicked);
    }

    void OnLevelClicked()
    {
        GameManagerCycle.Instance.OnLevelSelected(levelNumber);
    }
}
