using System.Collections;
using UnityEngine;
using NodeCanvas.DialogueTrees;


public class ExpressiveDialogueActor : DialogueActor
{
    [Header("Sprite Renderers")]
    public SpriteRenderer eyesRenderer;
    public SpriteRenderer mouthRenderer;

    [Header("Sprites")]
    public Sprite[] bodySprites = new Sprite[8];
    public Sprite[] eyeSprites = new Sprite[5];
    public Sprite[] mouthSprites = new Sprite[5];

    [Header("Dialogue UI")]
    public Transform dialogueAnchor;

    private void Start()
    {
        if (!eyesRenderer)
        {
            eyesRenderer = GetComponentsInChildren<SpriteRenderer>()[1];
        }

        if (!mouthRenderer)
        {
            mouthRenderer = GetComponentsInChildren<SpriteRenderer>()[2];
        }

        StartCoroutine(Blinking());
    }

    IEnumerator Blinking()
    {
        while (true)
        {
            if (eyeSprites[1])
            {
                yield return new WaitForSeconds(Random.Range(3f, 6f));

                // Temporarily hold reference to current eye expression sprite.
                Sprite currentEyeExpression = eyesRenderer.sprite;

                eyesRenderer.sprite = eyeSprites[1];

                yield return new WaitForSeconds(0.15f);

                eyesRenderer.sprite = currentEyeExpression;
            }
        }
    }
}