using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("SFX")]
    public AudioSource sfxSource;

    public AudioClip walkSound;
    public AudioClip deathSound;
    public AudioClip winSound;

    private bool soundEnabled = true;
    const string SOUND_KEY = "SoundEnabled";
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        soundEnabled = PlayerPrefs.GetInt(SOUND_KEY, 1) == 1;
        ApplySoundState();
    }

    public void PlayWalk()
    {
        if (!soundEnabled) return;
        sfxSource.PlayOneShot(walkSound);
    }

    public void PlayDeath()
    {
        if (!soundEnabled) return;
        sfxSource.PlayOneShot(deathSound);
    }

    public void PlayWin()
    {
        if (!soundEnabled) return;
        sfxSource.PlayOneShot(winSound);
    }

    public void ToggleSound()
    {
        soundEnabled = !soundEnabled;

        PlayerPrefs.SetInt(SOUND_KEY, soundEnabled ? 1 : 0);
        PlayerPrefs.Save();

        ApplySoundState();
    }

    void ApplySoundState()
    {
        AudioListener.volume = soundEnabled ? 1f : 0f;
    }

    public bool IsSoundEnabled()
    {
        return soundEnabled;
    }
}