using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using NodeCanvas.DialogueTrees;

public class DialogueManager : MonoBehaviour
{
    public DialogueTreeController controller;
    public DialogueTree[] dialogues = new DialogueTree[0];

    private void OnEnable()
    {
        DialogueTree.OnDialogueFinished += OnDialogueFinished;
    }

    private void OnDisable()
    {
        DialogueTree.OnDialogueFinished -= OnDialogueFinished;
    }

    private void Start()
    {
        controller = GetComponent<DialogueTreeController>();
    }

    // Eventually, this function would take a dictionary of conditions
    // to be run against the game state in order to retrieve the appropriate dialogue tree.
    public void StartDialogue(DialogueActor instigator, IDialogueActor nonPlayerActor)
    {
        StartCoroutine(LoadDialogueUI(instigator, nonPlayerActor));
    }

    private void OnDialogueFinished(DialogueTree tree)
    {
        StartCoroutine(UnloadDialogueUI());
    }

    IEnumerator LoadDialogueUI(DialogueActor instigator, IDialogueActor nonPlayerActor)
    {
        if (!SceneManager.GetSceneByName("DialogueUI").isLoaded)
        {
            yield return SceneManager.LoadSceneAsync("DialogueUI", LoadSceneMode.Additive);
        }

        // Load actor references into dialogue asset.
        dialogues[0].SetActorReference(nonPlayerActor.name, nonPlayerActor);

        // Start dialogue tree.
        controller.StartDialogue(dialogues[0], instigator, null);
    }

    IEnumerator UnloadDialogueUI()
    {
        if (SceneManager.GetSceneByName("DialogueUI").isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync("DialogueUI");
        }
    }
}
