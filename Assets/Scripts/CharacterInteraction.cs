using UnityEngine;
using UnityEngine.InputSystem;
using NodeCanvas.DialogueTrees;

public class CharacterInteraction : MonoBehaviour
{
    ExpressiveDialogueActor actorSelf;
    public DialogueManager dialogueManager;

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

    private void Start()
    {
        actorSelf = GetComponent<ExpressiveDialogueActor>();
        dialogueManager = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            int layerMask = 1 << LayerMask.NameToLayer("NPC");

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.25f, layerMask);

            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("NPC"))
                {
                    dialogueManager.StartDialogue(actorSelf, hit.GetComponentInParent<ExpressiveDialogueActor>());
                }
            }
        }
    }

    private void DisablePlayerActions(DialogueTree obj)
    {
        GetComponent<PlayerInput>().actions.Disable();
    }

    private void EnablePlayerActions(DialogueTree obj)
    {
        GetComponent<PlayerInput>().actions.Enable();
    }
}
