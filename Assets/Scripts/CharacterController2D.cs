using System.Collections;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using NodeCanvas.DialogueTrees;
using Pathfinding;


public class CharacterController2D : MonoBehaviour
{
    public static event Action OnClickedWalkable;
    public static event Action<ExpressiveDialogueActor, ExpressiveDialogueActor> OnClickedStartDialogue;
    public static event Action<Constants.Scene> OnHoveringSceneTransition;
    public static event Action<Constants.Scene> OnClickedSceneExit;

    PlayerInput playerInput;

    [Header("Movement Settings")]
    [Tooltip("The transform object to be used as the player's current movement waypoint.")]
    public GameObject waypointPrefab;
    Transform waypoint;
    AIPath pathfinder;

    [Header("Interaction Settings")]
    public DialogueManager dialogueManager;
    ExpressiveDialogueActor actorSelf;

    RaycastHit2D hit;
    Vector2 lastMousePosition;
    
    [SerializeField, Tooltip("Distance at which an interaction can be initiated.")]
    float interactRange = 1.0f;
    Coroutine waitToInteract;
    bool isInteracting = false;

    [Header("Cursor Textures")]
    [SerializeField]
    Texture2D defaultCursor;
    [SerializeField]
    Texture2D interactableCursor;
    [SerializeField]
    Texture2D walkableCursor;
    [SerializeField]
    Vector2 cursorHotspot = Vector2.zero;


    void OnEnable()
    {
        DialogueTree.OnDialogueStarted += OnDialogueDisablesActions;
        DialogueTree.OnDialogueFinished += OnDialogueEnablesActions;
        InGameMenu.OnMenuOpened += OnDisableActions;
        InGameMenu.OnFadeToScene += OnEnableActions;
    }

    void OnDisable()
    {
        DialogueTree.OnDialogueStarted -= OnDialogueDisablesActions;
        DialogueTree.OnDialogueFinished -= OnDialogueEnablesActions;
        InGameMenu.OnMenuOpened -= OnDisableActions;
        InGameMenu.OnFadeToScene -= OnEnableActions;
    }

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        actorSelf = GetComponent<ExpressiveDialogueActor>();

        // Setup pathfinding components.
        waypoint = Instantiate(waypointPrefab, transform.position, Quaternion.identity).transform;
        pathfinder = GetComponent<AIPath>();
        GetComponent<AIDestinationSetter>().target = waypoint;

        // Setup cursor.
        Cursor.SetCursor(defaultCursor, cursorHotspot, CursorMode.Auto);
    }

    void Update()
    {
        if (!isInteracting)
        {
            MouseHover();
        }
    }

    void MouseHover()
    {
        Vector2 currentMousePosition = Mouse.current.position.ReadValue();

        // Only update hover if mouse has moved since last frame.
        if (lastMousePosition != currentMousePosition)
        {
            hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(currentMousePosition), Vector2.zero);

            // Collision: Interactable
            if (hit.collider && hit.collider.gameObject.layer == LayerMask.NameToLayer(Constants.LayerMaskInteractable))
            {
                Cursor.SetCursor(interactableCursor, cursorHotspot, CursorMode.Auto);
            }
            // Collision: Scene Exit
            else if (hit.collider && hit.collider.gameObject.layer == LayerMask.NameToLayer(Constants.LayerMaskSceneExit))
            {
                OnHoveringSceneTransition?.Invoke(hit.collider.GetComponent<SceneExit>().targetScene);
            }
            else
            {
                Cursor.SetCursor(defaultCursor, cursorHotspot, CursorMode.Auto);
            }
        }

        // Store current position for comparison next frame.
        lastMousePosition = currentMousePosition;
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        // Collision: Walkable
        if (context.performed && hit.collider)
        {
            // If collider is walkable, move to hit position.
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer(Constants.LayerMaskWalkable))
            {
                UpdatePath(hit.point);
                OnClickedWalkable?.Invoke();
            }

            // Collision: Interactable.
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer(Constants.LayerMaskInteractable))
            {
                Vector2 interactablePosition = hit.collider.transform.position;

                // Position player to right of NPC for interaction [Demo Only]
                UpdatePath(interactablePosition + (-Vector2.right * interactRange));

                ExpressiveDialogueActor nonPlayerActor = hit.collider.gameObject.GetComponentInParent<ExpressiveDialogueActor>();

                waitToInteract = StartCoroutine(WaitToInteract(() => {
                    StartCoroutine(FaceInteraction());
                    OnClickedStartDialogue?.Invoke(actorSelf, nonPlayerActor);
                }));

                // The following code is a dynamic way of positioning player to NPC, but irrelevant for demo.
                //// If interactable is out of range, set and move to new target position within range.
                //if (Vector2.Distance(transform.position, interactablePosition) > interactRange)
                //{
                //    // Sets waypoint a given distance from clicked interactable.
                //    Vector2 dir = (Vector2)transform.position - interactablePosition;
                //    UpdatePath(interactablePosition + (dir.normalized * interactRange));
                //}
            }

            // Collision: Scene Exit
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer(Constants.LayerMaskSceneExit))
            {
                UpdatePath(hit.collider.transform.position);

                Constants.Scene targetScene = hit.collider.GetComponent<SceneExit>().targetScene;
                waitToInteract = StartCoroutine(WaitToInteract(() => OnClickedSceneExit?.Invoke(targetScene)));
            }
        }
    }

    void UpdatePath(Vector3 newPosition)
    {
        // If a coroutine is waiting to interact, stop it.
        if (waitToInteract != null)
        {
            StopCoroutine(waitToInteract);
        }

        waypoint.position = newPosition;
        pathfinder.SearchPath();
    }
    IEnumerator FaceInteraction()
    {
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, 221f);

        float time = 0f;

        while (time < 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, time);

            time += 0.15f;

            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator WaitToInteract(Action interactCallback)
    {
        // Wait for player to arrive at waypoint before invoking interaction.
        while ((Vector2)transform.position != (Vector2)waypoint.position)
        {
            yield return null;
        }

        interactCallback();
    }

    void OnDialogueEnablesActions(DialogueTree obj)
    {
        OnEnableActions();
    }

    void OnDialogueDisablesActions(DialogueTree obj)
    {
        OnDisableActions();
    }

    void OnEnableActions()
    {
        playerInput.SwitchCurrentActionMap("Player");
        isInteracting = false;
    }

    void OnDisableActions()
    {
        playerInput.SwitchCurrentActionMap("Menu");
        isInteracting = true;
        Cursor.SetCursor(defaultCursor, cursorHotspot, CursorMode.Auto);
    }
}
