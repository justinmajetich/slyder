using System.Collections;
using UnityEngine;
using NodeCanvas.DialogueTrees;


public class ExpressionController : MonoBehaviour
{
    ExpressiveDialogueActor activeActor;

    [Range(1,2), Tooltip("Increases the duration of each talk cycle.")]
    public float talkSpeedModifier = 1.5f;

    int currentEmoteState = 0;
    bool isTalking = false;
    
    void OnEnable()
    {
        ExpressionTagParser.OnEmotionExpressed += OnEmotionExpressed;
        DialogueTree.OnSubtitlesRequest += OnSubtitlesRequest;
        DialogueManager.OnDialogueWillContinue += OnDialogueWillContinue;
        SubtitleAnimator.OnTalk += OnTalk;
    }

    void OnDisable()
    {
        ExpressionTagParser.OnEmotionExpressed -= OnEmotionExpressed;
        DialogueTree.OnSubtitlesRequest -= OnSubtitlesRequest;
        DialogueManager.OnDialogueWillContinue -= OnDialogueWillContinue;
        SubtitleAnimator.OnTalk -= OnTalk;
    }

    void OnSubtitlesRequest(SubtitlesRequestInfo info)
    {
        activeActor = (ExpressiveDialogueActor)info.actor;
        currentEmoteState = 0;
    }

    void OnEmotionExpressed(int emoteKey)
    {
        if (activeActor && activeActor.eyeSprites[emoteKey] && activeActor.mouthSprites[emoteKey])
        {
            activeActor.eyesRenderer.sprite = activeActor.eyeSprites[emoteKey];
            activeActor.mouthRenderer.sprite = activeActor.mouthSprites[emoteKey];
            currentEmoteState = emoteKey;
        }
    }

    private void OnTalk(float speed)
    {
        // If not already talking, has a reference, and talk sprite...
        if (!isTalking && activeActor && activeActor.mouthSprites[1])
        {
            StartCoroutine(Talk(speed));
        }
    }

    IEnumerator Talk(float speed)
    {
        // Add subtle randomization to duration of each talk cycle.
        float talkSpeed = speed * (talkSpeedModifier + Random.Range(0.0f, 0.5f));

        isTalking = true;

        // Assign talking sprite.
        activeActor.mouthRenderer.sprite = activeActor.mouthSprites[1];

        yield return new WaitForSeconds(talkSpeed);

        // Assign idle sprite
        activeActor.mouthRenderer.sprite = activeActor.mouthSprites[currentEmoteState];

        yield return new WaitForSeconds(talkSpeed);

        isTalking = false;
    }


    // Reset emote state to idle.
    private void OnDialogueWillContinue()
    {
        if (activeActor && activeActor.eyeSprites[0] && activeActor.mouthSprites[0])
        {
            activeActor.eyesRenderer.sprite = activeActor.eyeSprites[0];
            activeActor.mouthRenderer.sprite = activeActor.mouthSprites[0];
        }
    }
}
