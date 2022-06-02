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
    public void StartDialogue(ExpressiveDialogueActor instigator, ExpressiveDialogueActor nonPlayerActor)
    {
        StartCoroutine(FaceActors(instigator, nonPlayerActor));
        GetActorOrientations(instigator, nonPlayerActor);
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
        if (!SceneManager.GetSceneByName("DialogueUI").isLoaded)
        {
            yield return SceneManager.LoadSceneAsync("DialogueUI", LoadSceneMode.Additive);
        }

        // Query for appropriate dialogue asset ID based on game state and conditions.

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

    void GetActorOrientations(ExpressiveDialogueActor actorA, ExpressiveDialogueActor actorB)
    {
        if (Vector3.Dot(actorA.transform.position.normalized - actorB.transform.position.normalized, Vector3.right) <= 0f)
        {
            actorA.dialogueOrientation = DialogueOrientation.Left;
            actorB.dialogueOrientation = DialogueOrientation.Right;
        }
        else
        {
            actorA.dialogueOrientation = DialogueOrientation.Right;
            actorB.dialogueOrientation = DialogueOrientation.Left;
        }
    }

    IEnumerator FaceActors(ExpressiveDialogueActor actorA, ExpressiveDialogueActor actorB)
    {
        // Get angle needed to face actors toward on another.
        float angle = Mathf.Atan2(actorB.transform.position.y - actorA.transform.position.y, actorB.transform.position.x - actorA.transform.position.x) * Mathf.Rad2Deg;

        Quaternion actorATargetRot = Quaternion.Euler(0f, 0f, angle - 90f);
        Quaternion actorBTargetRot = Quaternion.Euler(0f, 0f, angle + 90f);

        float time = 0f;

        // Rotate actors over time.
        while (time < 1f)
        {
            actorA.transform.rotation = Quaternion.Slerp(actorA.transform.rotation, actorATargetRot, time);
            actorB.transform.rotation = Quaternion.Slerp(actorB.transform.rotation, actorBTargetRot, time);

            time += 0.12f;

            yield return new WaitForSeconds(0.05f);
        }
    }
}
