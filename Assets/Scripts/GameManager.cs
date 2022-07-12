using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static event Action OnFadeToWhite;
    public static event Action<ExpressiveDialogueActor, ExpressiveDialogueActor> OnPlayBedroomMonologue;

    public static GameManager current;
    GameStateData _data;

    bool waitingOnFade = false;

    private void Awake()
    {
        if (current != null)
        {
            Destroy(gameObject);
            return;
        }

        current = this;
        _data = new GameStateData();
        _data.currentScene = (Constants.Scene)SceneManager.GetActiveScene().buildIndex;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        StartGame.OnStartGame += OnTransitionScene;
        CharacterController2D.OnClickedSceneExit += OnTransitionScene;
        GoToMainMenu.OnFadeToMainMenu += OnTransitionScene;
        SceneFade.OnFadeComplete += OnFadeComplete;
        GoToMainMenu.OnCutToMainMenu += OnReturnToMainMenu;
    }

    private void OnDisable()
    {
        StartGame.OnStartGame -= OnTransitionScene;
        CharacterController2D.OnClickedSceneExit -= OnTransitionScene;
        GoToMainMenu.OnFadeToMainMenu -= OnTransitionScene;
        SceneFade.OnFadeComplete -= OnFadeComplete;
        GoToMainMenu.OnCutToMainMenu -= OnReturnToMainMenu;
    }

    public GameStateData GetGameState()
    {
        return _data;
    }

    public void OnReturnToMainMenu()
    {
        _data = new GameStateData();
        StartCoroutine(TransitionScenes(Constants.Scene.MainMenu));
    }

    void OnTransitionScene(Constants.Scene targetScene)
    {
        _data.nextScene = targetScene;
        waitingOnFade = true;
        OnFadeToWhite?.Invoke();
    }

    void OnFadeComplete()
    {
        if (waitingOnFade)
        {
            StartCoroutine(TransitionScenes(_data.nextScene)); 
            waitingOnFade = false;
        }
    }

    IEnumerator TransitionScenes(Constants.Scene targetScene)
    {
        yield return SceneManager.LoadSceneAsync((int)targetScene);

        // Flag bedroom as visited if not already flagged.
        if (targetScene == Constants.Scene.Bedroom && !_data.bedroomWasVisited)
        {
            _data.bedroomWasVisited = true;
            OnPlayBedroomMonologue?.Invoke(GameObject.Find("Player").GetComponent<ExpressiveDialogueActor>(), null);
        }

        PositionPlayerAtSpawn(targetScene);

        _data.currentScene = targetScene;
    }

    // Position player at appropriate entrance to next scene. For the scope of this demo,
    // this function is only relevant to transitioning to the bedroom from the kitchen.
    void PositionPlayerAtSpawn(Constants.Scene scene)
    {
        if (scene == Constants.Scene.Bedroom && _data.bedroomWasVisited)
        {
            foreach (SceneExit exit in FindObjectsOfType<SceneExit>())
            {
                if (exit.targetScene == _data.currentScene)
                {
                    Transform player = GameObject.FindGameObjectWithTag("Player").transform;
                    
                    if (player != null)
                    {
                        player.SetPositionAndRotation(exit.spawn.position, exit.spawn.rotation);
                    }
                }
            }
        }
    }
}
