using System;
using System.Collections;
using UnityEngine;
using Pathfinding;
using NodeCanvas.DialogueTrees;

public class CharacterSpriteController : MonoBehaviour
{
    public static event Action OnPlayerDidStep;

    public enum Orientation
    {
        N, NE, E, SE, S, SW, W, NW, NA
    }

    public enum EmoteState
    {
        Neutral,
        Happy,
        Sad
    }

    [Serializable]
    public class CharacterSprites
    {
        public OrientedSprites N = new OrientedSprites();
        public OrientedSprites NE = new OrientedSprites();
        public OrientedSprites E = new OrientedSprites();
        public OrientedSprites SE = new OrientedSprites();
        public OrientedSprites S = new OrientedSprites();
        public OrientedSprites SW = new OrientedSprites();
        public OrientedSprites W = new OrientedSprites();
        public OrientedSprites NW = new OrientedSprites();
    }

    [Serializable]
    public class OrientedSprites
    {
        public BodySpriteSet body;
        public EyeSpriteSet eye;
        public MouthSpriteSet mouth;
    }

    [Serializable]
    public class BodySpriteSet
    {
        public Sprite idle;
        public Sprite walkRight;
        public Sprite walkLeft;
    }

    [Serializable]
    public class EyeSpriteSet
    {
        public Sprite neutral;
        public Sprite happy;
        public Sprite sad;
        public Sprite neutralBlinking;
        public Sprite happyBlinking;
        public Sprite sadBlinking;

    }

    [Serializable]
    public class MouthSpriteSet
    {
        public Sprite neutral;
        public Sprite happy;
        public Sprite sad;
        public Sprite neutralTalking;
        public Sprite happyTalking;
        public Sprite sadTalking;
    }

    [Serializable]
    public class FaceAnchorPositions
    {
        public Vector2 N;
        public Vector2 NE;
        public Vector2 E;
        public Vector2 SE;
        public Vector2 S;
        public Vector2 SW;
        public Vector2 W;
        public Vector2 NW;
    }

    [Header("Character Transform")]
    public Transform charTransform;

    public bool lockRotation = false;

    [Header("Sprite Renderers")]
    public SpriteRenderer bodyRenderer;
    public SpriteRenderer eyeRenderer;
    public SpriteRenderer mouthRenderer;

    [Header("Face Anchoring")]
    [SerializeField]
    Transform faceAnchor;

    [SerializeField, Tooltip("Position at which to anchor the face renderer for each body rotation.")]
    FaceAnchorPositions faceAnchorPositions = new FaceAnchorPositions();

    [Header("Sprites")]
    [SerializeField]
    CharacterSprites characterSprites = new CharacterSprites();
    OrientedSprites activeSprites;

    bool isActiveActor = false;

    [Header("Animation Parameters")]
    [Range(1, 2), Tooltip("Increases the duration of each talk cycle.")]
    public float talkSpeedModifier = 1.5f;

    float maxVelocity;
    float velocity;

    // ----Character State----
    Orientation orientation;
    Orientation lastOrientation = Orientation.NA;

    Vector2 lastPosition;
    float lastZRotation;

    EmoteState activeEmoteState = EmoteState.Neutral;

    bool isWalking = false;
    bool isSteppingLeft = false;
    bool isSteppingRight = false;

    bool isBlinking = false;

    bool isTalking = false;
    bool mouthIsOpen = false;


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

    private void Start()
    {
        if (GetComponent<AIPath>())
        {
            maxVelocity = GetComponent<AIPath>().maxSpeed;
        }

        lastZRotation = charTransform.eulerAngles.z;
        lastPosition = charTransform.position;

        UpdateCharacterOrientation();
        StartCoroutine(Walking());
        StartCoroutine(Blinking());
    }

    void Update()
    {
        // If character has changed rotation...
        if (!lockRotation && charTransform.eulerAngles.z != lastZRotation)
        {
            UpdateCharacterOrientation();

            lastZRotation = charTransform.eulerAngles.z;
        }

        velocity = (((Vector2)charTransform.position - lastPosition) / Time.deltaTime).magnitude;
        isWalking = velocity > 0.01f;

        lastPosition = charTransform.position;
    }

    void UpdateCharacterOrientation()
    {
        orientation = (charTransform.eulerAngles.z) switch
        {
            float z when z <= 22.5 || z > 337.5 => Orientation.N,
            float z when z <= 337.5 && z > 292.5 => Orientation.NE,
            float z when z <= 292.5 && z > 247.5 => Orientation.E,
            float z when z <= 247.5 && z > 202.5 => Orientation.SE,
            float z when z <= 202.5 && z > 157.5 => Orientation.S,
            float z when z <= 157.5 && z > 112.5 => Orientation.SW,
            float z when z <= 112.5 && z > 67.5 => Orientation.W,
            float z when z <= 67.5 && z > 22.5 => Orientation.NW,
            _ => Orientation.NA
        };

        if (orientation == Orientation.NA)
        {
            Debug.Log("Rotation out of range: " + charTransform.eulerAngles.z.ToString());
        }
        else
        {
            // If orientation has changed or is not assigned...
            if (lastOrientation == Orientation.NA || orientation != lastOrientation)
            {
                activeSprites = orientation switch
                {
                    Orientation.N => characterSprites.N,
                    Orientation.NE => characterSprites.NE,
                    Orientation.E => characterSprites.E,
                    Orientation.SE => characterSprites.SE,
                    Orientation.S => characterSprites.S,
                    Orientation.SW => characterSprites.SW,
                    Orientation.W => characterSprites.W,
                    _ => characterSprites.NW
                };

                // Reposition face renderer for new orientation.
                faceAnchor.localPosition = orientation switch
                {
                    Orientation.N => faceAnchorPositions.N,
                    Orientation.NE => faceAnchorPositions.NE,
                    Orientation.E => faceAnchorPositions.E,
                    Orientation.SE => faceAnchorPositions.SE,
                    Orientation.S => faceAnchorPositions.S,
                    Orientation.SW => faceAnchorPositions.SW,
                    Orientation.W => faceAnchorPositions.W,
                    _ => faceAnchorPositions.NW
                };

                SetBodySprite();
                SetEyeSprite();
                SetMouthSprite();

                lastOrientation = orientation;
            }
        }
    }

    void SetBodySprite()
    {
        if (isSteppingLeft)
        {
            bodyRenderer.sprite = activeSprites.body.walkLeft;
        }
        else if (isSteppingRight)
        {
            bodyRenderer.sprite = activeSprites.body.walkRight;
        }
        else
        {
            bodyRenderer.sprite = activeSprites.body.idle;
        }
    }

    void SetEyeSprite()
    {
        eyeRenderer.sprite = activeEmoteState switch
        {
            EmoteState.Happy => isBlinking ? activeSprites.eye.happyBlinking : activeSprites.eye.happy,
            EmoteState.Sad => isBlinking ? activeSprites.eye.sadBlinking : activeSprites.eye.sad,
            _ => isBlinking ? activeSprites.eye.neutralBlinking : activeSprites.eye.neutral
        };
    }

    void SetMouthSprite()
    {
        mouthRenderer.sprite = activeEmoteState switch
        {
            EmoteState.Happy => mouthIsOpen ? activeSprites.mouth.happyTalking : activeSprites.mouth.happy,
            EmoteState.Sad => mouthIsOpen ? activeSprites.mouth.sadTalking : activeSprites.mouth.sad,
            _ => mouthIsOpen ? activeSprites.mouth.neutralTalking : activeSprites.mouth.neutral
        };
    }

    void OnSubtitlesRequest(SubtitlesRequestInfo info)
    {
        // If this actor is the speaker...
        if (info.actor == GetComponentInParent<IDialogueActor>())
        {
            isActiveActor = true;
        }
    }

    void OnEmotionExpressed(int emoteKey)
    {
        if (isActiveActor)
        {
            activeEmoteState = (EmoteState)emoteKey;
            SetEyeSprite();
            SetMouthSprite();
        }
    }

    private void OnTalk(float speed, char character)
    {
        if (!isTalking && isActiveActor && char.IsLetterOrDigit(character))
        {
            StartCoroutine(Talking(speed));
        }
    }

    private void OnDialogueWillContinue()
    {
        // Reset emote state to idle.
        if (isActiveActor)
        {
            activeEmoteState = EmoteState.Neutral;
            SetEyeSprite();
            SetMouthSprite();
            isActiveActor = false;
        }
    }

    IEnumerator Talking(float speed)
    {
        // Add small randomization to duration of each talk cycle.
        float talkSpeed = Mathf.Clamp(speed, 0.03f, 0.25f) * (talkSpeedModifier + UnityEngine.Random.Range(0.0f, 0.5f));

        isTalking = true;

        mouthIsOpen = true;
        SetMouthSprite();

        yield return new WaitForSeconds(talkSpeed);

        mouthIsOpen = false;
        SetMouthSprite();

        yield return new WaitForSeconds(talkSpeed);

        isTalking = false;
    }

    IEnumerator Walking()
    {
        bool lastStepWasRight = true;

        while (true)
        {
            while (isWalking)
            {
                if (!isSteppingLeft && !isSteppingRight && lastStepWasRight)
                {
                    lastStepWasRight = false;
                    isSteppingLeft = true;
                    SetBodySprite();
                    OnPlayerDidStep?.Invoke();

                }
                else if (!isSteppingLeft && !isSteppingRight && !lastStepWasRight)
                {
                    lastStepWasRight = true;
                    isSteppingRight = true;
                    SetBodySprite();
                    OnPlayerDidStep?.Invoke();

                }
                else
                {
                    isSteppingLeft = false;
                    isSteppingRight = false;
                    SetBodySprite();

                }

                // Modulate duration of step intervals relative to character velocity.
                yield return new WaitForSeconds(Mathf.Clamp((maxVelocity - velocity) / 10f, 0.175f, maxVelocity));
            }

            isSteppingLeft = false;
            isSteppingRight = false;
            SetBodySprite();

            yield return null;
        }
    }

    IEnumerator Blinking()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(3f, 6f));

            isBlinking = true;
            SetEyeSprite();

            yield return new WaitForSeconds(0.15f);

            isBlinking = false;
            SetEyeSprite();
        }
    }
}
