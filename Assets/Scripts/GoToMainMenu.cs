using System;
using System.Collections;
using UnityEngine;

public class GoToMainMenu : MonoBehaviour
{
    public static event Action OnCutToMainMenu;
    public static event Action OnFadeToMainMenu;
    public static event Action<float> OnWaitForAudioFade;

    public void OnMainMenu()
    {
        if (GameManager.current.GetGameState().currentScene == Constants.Scene.Credits)
        {
            OnFadeToMainMenu?.Invoke();
        }
        else
        {
            StartCoroutine(WaitForAudioFade());
        }
    }

    IEnumerator WaitForAudioFade()
    {
        float duration = 0.1f;
        
        OnWaitForAudioFade?.Invoke(duration);

        yield return new WaitForSeconds(duration);

        OnCutToMainMenu?.Invoke();
    }
}
