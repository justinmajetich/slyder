using System;
using UnityEngine;
using UnityEngine.UI;

public class SceneFade : MonoBehaviour
{
    public static event Action OnFadeComplete;

    [SerializeField]
    Animator animator;
    [SerializeField]
    Image image;

    private void Awake()
    {
        image.enabled = true;
    }

    void OnEnable()
    {
        GameManager.OnFadeToWhite += OnFadeToWhite;
        InGameMenu.OnFadeToScene += OnFadeToScene;
        InGameMenu.OnMenuOpened += OnFadeToWhite;
    }

    void OnDisable()
    {
        GameManager.OnFadeToWhite -= OnFadeToWhite;
        InGameMenu.OnFadeToScene -= OnFadeToScene;
        InGameMenu.OnMenuOpened -= OnFadeToWhite;
    }

    void OnFadeToScene()
    {
        animator.SetTrigger("fadeIn");
    }

    void OnFadeToWhite()
    {
        animator.SetTrigger("fadeOut");
    }

    public void FadeComplete()
    {
        OnFadeComplete?.Invoke();
    }
}
