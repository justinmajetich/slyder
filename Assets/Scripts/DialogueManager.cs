using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using NodeCanvas.DialogueTrees;
using UnityEngine.InputSystem;


public class DialogueManager : MonoBehaviour
{
    public static event Action OnDialogueWillContinue;

    public DialogueTreeController controller;
    public DialogueTree[] dialogues = new DialogueTree[0];

    SubtitlesRequestInfo activeSubtitleInfo;


    void OnEnable()
    {
        DialogueTree.OnSubtitlesRequest += OnSubtitlesRequest;
        DialogueTree.OnDialogueFinished += OnDialogueFinished;
        SubtitleAnimator.OnAnimationComplete += OnSubtitleAnimationComplete;
    }

    void OnDisable()
    {
        DialogueTree.OnSubtitlesRequest -= OnSubtitlesRequest;
        DialogueTree.OnDialogueFinished -= OnDialogueFinished;
        SubtitleAnimator.OnAnimationComplete -= OnSubtitleAnimationComplete;
    }

    void Start()
    {
        controller = GetComponent<DialogueTreeController>();
    }

    // Eventually, this function would take a dictionary of conditions
    // to be run against the game state in order to retrieve the appropriate dialogue tree.
    public void StartDialogue(DialogueActor instigator, IDialogueActor nonPlayerActor)
    {
        StartCoroutine(LoadDialogueUI(instigator, nonPlayerActor));
    }

    void OnSubtitlesRequest(SubtitlesRequestInfo info)
    {
        activeSubtitleInfo = info;
    }

    void OnSubtitleAnimationComplete()
    {
        StartCoroutine(WaitForInputToContinue());
    }

    IEnumerator WaitForInputToContinue()
    {
        // Wait for Space press to continue.
        while (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            yield return null;
        }

        OnDialogueWillContinue?.Invoke();

        // Execute subtitle request callback to continue dialogue tree.
        activeSubtitleInfo.Continue();
    }

    void OnDialogueFinished(DialogueTree tree)
    {
        StartCoroutine(UnloadDialogueUI());
    }

    IEnumerator LoadDialogueUI(DialogueActor instigator, IDialogueActor nonPlayerActor)
    {
        if (!SceneManager.GetSceneByName("DialogueUI_2.0").isLoaded)
        {
            yield return SceneManager.LoadSceneAsync("DialogueUI_2.0", LoadSceneMode.Additive);
        }

        // Query for appropriate dialogue asset ID based on game state and conditions.

        // Load actor references into dialogue asset.
        dialogues[0].SetActorReference(nonPlayerActor.name, nonPlayerActor);

        // Start dialogue tree.
        controller.StartDialogue(dialogues[0], instigator, null);
    }

    IEnumerator UnloadDialogueUI()
    {
        if (SceneManager.GetSceneByName("DialogueUI_2.0").isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync("DialogueUI_2.0");
        }
    }
}
