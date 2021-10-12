using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.DialogueTrees;

public class DialogueManager : MonoBehaviour
{
    public DialogueTreeController controller;
    public DialogueTree[] dialogues = new DialogueTree[0];

    private void Start()
    {
        controller = GetComponent<DialogueTreeController>();
    }

    // Eventually, this function would take a ditionary of conditions
    // to be run against the game state in order to retrieve the appropriate dialogue tree.
    public void StartDialogueTree(DialogueActor instigator)
    {
        controller.StartDialogue(dialogues[0], instigator, null);
    }
}
