using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("SFX")]
    public AudioSource sfxSource;

    [Header("MUSIC")]
    public AudioSource musicSource; // MUST be bgm object AudioSource
    public AudioClip bgm;

    [Header("SFX Clips")]
    public AudioClip walkSound;
    public AudioClip deathSound;
    public AudioClip winSound;
    public AudioClip danceSound;
    public AudioClip buttonClickSound;

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

        // 🔥 PRELOAD CLIP (THIS REMOVES DELAY)
        sfxSource.playOnAwake = false;
        sfxSource.clip = buttonClickSound;
        sfxSource.Stop();

        PlayBGM();
        ApplySoundState();
    }

    // ================= BGM =================
    public void PlayBGM()
    {
        if (musicSource.clip == bgm && musicSource.isPlaying)
            return;

        musicSource.clip = bgm;
        musicSource.loop = true;

        musicSource.Play(); // always start once
    }

    // ================= SFX =================
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

    public void PlayButtonClick()
    {
        if (!soundEnabled) return;

        // 🔥 INSTANT PLAY (NO DELAY)
        sfxSource.Stop(); // ensures restart
        sfxSource.clip = buttonClickSound;
        sfxSource.Play();
    }

    public void PlayDance(float duration)
    {
        if (!soundEnabled) return;
        StartCoroutine(PlayDanceForDuration(duration));
    }

    IEnumerator PlayDanceForDuration(float duration)
    {
        if (!soundEnabled) yield break;

        sfxSource.clip = danceSound;
        sfxSource.Play();

        float t = 0;

        while (t < duration)
        {
            if (!soundEnabled)
            {
                sfxSource.Stop();
                yield break;
            }

            t += Time.deltaTime;
            yield return null;
        }

        sfxSource.Stop();
    }

    // ================= TOGGLE =================
    public void ToggleSound()
    {
        soundEnabled = !soundEnabled;

        PlayerPrefs.SetInt(SOUND_KEY, soundEnabled ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("Sound Toggled: " + soundEnabled);

        ApplySoundState();
    }

    // ================= APPLY STATE =================
    void ApplySoundState()
    {
        Debug.Log("Applying Sound State: " + soundEnabled);

        // 🔥 CONTROL EVERYTHING HERE ONLY
        sfxSource.mute = !soundEnabled;
        musicSource.mute = !soundEnabled;

        if (!soundEnabled)
        {
            Debug.Log("SOUND OFF");

            sfxSource.Stop(); // stop current effects
        }
        else
        {
            Debug.Log("SOUND ON");

            if (!musicSource.isPlaying && musicSource.clip != null)
            {
                musicSource.Play();
            }
        }
    }

    public bool IsSoundEnabled()
    {
        return soundEnabled;
    }
}