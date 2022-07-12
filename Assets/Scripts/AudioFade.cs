using UnityEngine;
using UnityEngine.Audio;

public class AudioFade : MonoBehaviour
{
    [SerializeField]
    AudioMixer mixer;

    [SerializeField]
    AudioMixerSnapshot main;

    [SerializeField]
    AudioMixerSnapshot fadeOut;

    readonly float defaultFadeDuration = 1f;

    private void OnEnable()
    {
        GameManager.OnFadeToWhite += OnFadeOut;
        GoToMainMenu.OnWaitForAudioFade += OnFadeOut;
    }

    private void OnDisable()
    {
        GameManager.OnFadeToWhite -= OnFadeOut;
        GoToMainMenu.OnWaitForAudioFade -= OnFadeOut;
    }

    private void Start()
    {
        main.TransitionTo(0f);
    }

    void OnFadeOut()
    {
        fadeOut.TransitionTo(defaultFadeDuration);
    }

    void OnFadeOut(float duration)
    {
        fadeOut.TransitionTo(duration);
    }
}