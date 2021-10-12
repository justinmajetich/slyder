using UnityEngine;
using UnityEngine.InputSystem;
using NodeCanvas.DialogueTrees;


public class CharacterInteraction : MonoBehaviour
{
    DialogueActor actorSelf;
    public DialogueManager dialogueManager;

    private void Start()
    {
        actorSelf = GetComponent<DialogueActor>();
        dialogueManager = GameObject.Find("DialogueManager").GetComponent<DialogueManager>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("OnInteract!!!");

        if (context.performed)
        {
            int layerMask = 1 << LayerMask.NameToLayer("NPC");

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.25f, layerMask);

            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("NPC"))
                {
                    // ????? Check if actor is already in dialogue? Disable input?

                    dialogueManager.StartDialogue(actorSelf);
                }
            }
        }
    }

    //void Update()
    //{
    //    int layerMask = 1 << LayerMask.NameToLayer("NPC");

    //    if (Input.GetKeyDown(KeyCode.E))
    //    {
    //        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.25f, layerMask);

    //        foreach (Collider2D hit in hits)
    //        {
    //            if (hit.CompareTag("NPC"))
    //            {
    //                // ????? Check if actor is already in dialogue? Disable input?

    //                dialogueManager.StartDialogue(actorSelf);
    //            }
    //        }
    //    }
    //}
}
