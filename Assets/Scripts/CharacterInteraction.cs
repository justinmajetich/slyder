using UnityEngine;
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

    void Update()
    {
        int layerMask = 1 << LayerMask.NameToLayer("NPC");

        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.25f, layerMask);

            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("NPC"))
                {
                    dialogueManager.StartDialogueTree(actorSelf);
                }
            }
        }
    }
}
