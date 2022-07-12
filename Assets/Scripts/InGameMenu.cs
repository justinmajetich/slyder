using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InGameMenu : MonoBehaviour
{
    public static event Action OnFadeToScene;
    public static event Action OnMenuOpened;

    [SerializeField]
    GameObject menuRoot;
    bool menuIsOpen = false;
    bool waitingOnFade = false;

    private void Awake()
    {
        menuRoot.SetActive(false);
    }

    void OnEnable()
    {
        SceneFade.OnFadeComplete += EnableMenuContent;
    }

    void OnDisable()
    {
        SceneFade.OnFadeComplete -= EnableMenuContent;
    }

    public void OnOpenMenu(InputAction.CallbackContext value)
    {
        if (!menuIsOpen && value.action.WasPerformedThisFrame())
        {
            OnMenuOpened?.Invoke();
            waitingOnFade = true;
        }
    }

    public void OnCloseMenu(InputAction.CallbackContext value)
    {
        if (menuIsOpen && value.action.WasPerformedThisFrame())
        {
            OnResumeGame();
        }
    }

    void EnableMenuContent()
    {
        if (waitingOnFade)
        {
            waitingOnFade = false;
            menuIsOpen = true;
            menuRoot.SetActive(true);
        }
    }

    public void OnResumeGame()
    {
        EventSystem.current.SetSelectedGameObject(null);
        menuRoot.SetActive(false);
        menuIsOpen = false;
        OnFadeToScene?.Invoke();
    }
}
