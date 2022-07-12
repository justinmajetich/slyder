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
    public DialogueTree dialogueAsset;

    bool cameraInZoomedPosition = false;

    SubtitlesRequestInfo activeSubtitleInfo;


    void OnEnable()
    {
        DialogueTree.OnSubtitlesRequest += OnSubtitlesRequest;
        DialogueTree.OnDialogueFinished += OnDialogueFinished;
        SubtitleAnimator.OnAnimationComplete += OnSubtitleAnimationComplete;
        CharacterController2D.OnClickedStartDialogue += OnStartDialogue;
        CameraAnimation.OnCameraZoomed += OnCameraInPosition;
    }

    void OnDisable()
    {
        DialogueTree.OnSubtitlesRequest -= OnSubtitlesRequest;
        DialogueTree.OnDialogueFinished -= OnDialogueFinished;
        SubtitleAnimator.OnAnimationComplete -= OnSubtitleAnimationComplete;
        CharacterController2D.OnClickedStartDialogue -= OnStartDialogue;
        CameraAnimation.OnCameraZoomed -= OnCameraInPosition;
    }

    void Start()
    {
        controller = GetComponent<DialogueTreeController>();
    }

    void OnStartDialogue(ExpressiveDialogueActor instigator, ExpressiveDialogueActor nonPlayerActor)
    {
        //StartCoroutine(FaceActors(instigator, nonPlayerActor));
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

    void OnCameraInPosition()
    {
        cameraInZoomedPosition = true;
    }

    void OnDialogueFinished(DialogueTree tree)
    {
        cameraInZoomedPosition = false;
        StartCoroutine(UnloadDialogueUI());
    }

    IEnumerator LoadDialogueUI(DialogueActor instigator, IDialogueActor nonPlayerActor)
    {
        while (!cameraInZoomedPosition)
        {
            yield return null;
        }

        if (!SceneManager.GetSceneByBuildIndex((int)Constants.Scene.DialogueUI).isLoaded)
        {
            yield return SceneManager.LoadSceneAsync((int)Constants.Scene.DialogueUI, LoadSceneMode.Additive);
        }

        // Load actor references into dialogue asset.
        dialogueAsset.SetActorReference(nonPlayerActor.name, nonPlayerActor);



        // Start dialogue tree.
        controller.StartDialogue(dialogueAsset, instigator, null);
    }

    IEnumerator UnloadDialogueUI()
    {
        if (SceneManager.GetSceneByBuildIndex((int)Constants.Scene.DialogueUI).isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync((int)Constants.Scene.DialogueUI);
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

    // Rotates actors to face each other. Commented out for demo.
    //IEnumerator FaceActors(ExpressiveDialogueActor actorA, ExpressiveDialogueActor actorB)
    //{
    //    // Get angle needed to face actors toward on another.
    //    float angle = Mathf.Atan2(actorB.transform.position.y - actorA.transform.position.y, actorB.transform.position.x - actorA.transform.position.x) * Mathf.Rad2Deg;

    //    Quaternion actorATargetRot = Quaternion.Euler(0f, 0f, angle - 90f);
    //    Quaternion actorBTargetRot = Quaternion.Euler(0f, 0f, angle + 90f);

    //    float time = 0f;

    //    // Rotate actors over time.
    //    while (time < 1f)
    //    {
    //        actorA.transform.rotation = Quaternion.Slerp(actorA.transform.rotation, actorATargetRot, time);
    //        actorB.transform.rotation = Quaternion.Slerp(actorB.transform.rotation, actorBTargetRot, time);

    //        time += 0.12f;

    //        yield return new WaitForSeconds(0.05f);
    //    }
    //}
}
