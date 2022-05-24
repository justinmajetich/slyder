using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NodeCanvas.DialogueTrees;
using Pathfinding;


public class CharacterController2D : MonoBehaviour
{
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
    bool isWaitingToInteract = false;
    bool isInteracting = false;

    [Header("Cursor Textures")]
    [SerializeField]
    Texture2D defaultCursor;
    [SerializeField]
    Texture2D interactableCursor;
    [SerializeField]
    Texture2D walkableCursor;


    void OnEnable()
    {
        DialogueTree.OnDialogueStarted += DisablePlayerActions;
        DialogueTree.OnDialogueFinished += EnablePlayerActions;
    }

    void OnDisable()
    {
        DialogueTree.OnDialogueStarted -= DisablePlayerActions;
        DialogueTree.OnDialogueFinished -= EnablePlayerActions;
    }

    void Start()
    {
        if (!dialogueManager)
        {
            Debug.LogError("DialogueManager must be assigned.");
            UnityEditor.EditorApplication.isPlaying = false;
        }

        actorSelf = GetComponent<ExpressiveDialogueActor>();

        // Setup pathfinding components.
        waypoint = Instantiate(waypointPrefab, transform.position, Quaternion.identity).transform;
        pathfinder = GetComponent<AIPath>();
        GetComponent<AIDestinationSetter>().target = waypoint;

        // Setup cursor.
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
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

            // Hit interactable collider.
            if (hit.collider && hit.collider.gameObject.layer == LayerMask.NameToLayer(Constants.LayerMaskInteractable))
            {
                Cursor.SetCursor(interactableCursor, Vector2.zero, CursorMode.Auto);
            }
            // Hit walkable collider.
            else if (hit.collider && hit.collider.gameObject.layer == LayerMask.NameToLayer(Constants.LayerMaskWalkable))
            {
                Cursor.SetCursor(walkableCursor, Vector2.zero, CursorMode.Auto);
            }
            // No collider hit, or no relevant collider hit.
            else
            {
                Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
            }
        }

        // Store current position for comparison next frame.
        lastMousePosition = currentMousePosition;
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        // If click occured and a hovered collider has been reported...
        if (context.performed && hit.collider)
        {
            // If collider is walkable, move to hit position.
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer(Constants.LayerMaskWalkable))
            {
                waypoint.position = hit.point;
                pathfinder.SearchPath();
            }

            // If collider is interactable, move within range and initiate interaction.
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer(Constants.LayerMaskInteractable))
            {
                Vector2 interactablePosition = hit.collider.transform.position;

                // If interactable is out of range, set and move to new target position within range.
                if (Vector2.Distance(transform.position, interactablePosition) > interactRange)
                {
                    Vector2 dir = (Vector2)transform.position - interactablePosition;

                    waypoint.position = interactablePosition + (dir.normalized * interactRange);
                    pathfinder.SearchPath();
                }
                
                if (!isWaitingToInteract)
                {
                    StartCoroutine(WaitToInteract(hit.collider.gameObject.GetComponent<ExpressiveDialogueActor>()));
                }
            }
        }
    }

    IEnumerator WaitToInteract(ExpressiveDialogueActor interactableActor)
    {
        isWaitingToInteract = true;

        // Wait for player to reach interact range if not already within range.
        while ((Vector2)transform.position != (Vector2)waypoint.position)
        {
            yield return null;
        }

        dialogueManager.StartDialogue(actorSelf, interactableActor);

        isWaitingToInteract = false;
    }

    private void DisablePlayerActions(DialogueTree obj)
    {
        GetComponent<PlayerInput>().actions.Disable();
        isInteracting = true;
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
    }

    private void EnablePlayerActions(DialogueTree obj)
    {
        GetComponent<PlayerInput>().actions.Enable();
        isInteracting = false;
    }
}
