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
    public bool menuIsOpen = false;
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

    public void OnToggleMenu(InputAction.CallbackContext value)
    {
        if (!menuIsOpen && value.action.WasPerformedThisFrame())
        {
            OnMenuOpened?.Invoke();
            waitingOnFade = true;
        }

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
        OnFadeToScene?.Invoke();


        EventSystem.current.SetSelectedGameObject(null);
        menuRoot.SetActive(false);
        menuIsOpen = false;
    }
}
