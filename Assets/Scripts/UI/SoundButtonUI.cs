using UnityEngine;
using UnityEngine.UI;

public class SoundButtonUI : MonoBehaviour
{
    public Image icon;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    void Start()
    {
        UpdateIcon();
    }

    public void ToggleSound()
    {
        SoundManager.Instance.ToggleSound();
        UpdateIcon();
    }

    void UpdateIcon()
    {
        icon.sprite = SoundManager.Instance.IsSoundEnabled()
            ? soundOnSprite
            : soundOffSprite;
    }
}
