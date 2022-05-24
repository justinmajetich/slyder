using System;
using System.Collections;
using UnityEngine;
using TMPro;
using Pathfinding;

public class CharacterSpriteController : MonoBehaviour
{
    [Header("Debug")]
    public TMP_Text debugUI;

    public enum Orientation
    {
        N, NE, E, SE, S, SW, W, NW, NA
    }

    [Serializable]
    public class BodySprites
    {
        public BodySpriteSet N = new BodySpriteSet();
        public BodySpriteSet NE = new BodySpriteSet();
        public BodySpriteSet E = new BodySpriteSet();
        public BodySpriteSet SE = new BodySpriteSet();
        public BodySpriteSet S = new BodySpriteSet();
        public BodySpriteSet SW = new BodySpriteSet();
        public BodySpriteSet W = new BodySpriteSet();
        public BodySpriteSet NW = new BodySpriteSet();
    }

    [Serializable]
    public class BodySpriteSet
    {
        public Sprite idle;
        public Sprite walkRight;
        public Sprite walkLeft;
    }

    [Header("Sprites")]
    public SpriteRenderer bodyRenderer;
    public BodySprites bodySprites = new BodySprites();
    BodySpriteSet currentSpriteSet;


    [Header("Animation Parameters")]
    float maxVelocity;
    float velocity;
    bool isWalking = false;

    Orientation orientation;
    Orientation lastOrientation = Orientation.NA;

    Vector2 lastPosition;
    float lastZRotation;



    private void Start()
    {
        maxVelocity = GetComponent<AIPath>().maxSpeed;

        lastZRotation = transform.eulerAngles.z;
        lastPosition = transform.position;

        UpdateOrientation();
        StartCoroutine(WalkCycleWithIdle());
    }

    void Update()
    {
        // If character has changed rotation...
        if (transform.eulerAngles.z != lastZRotation)
        {
            UpdateOrientation();

            lastZRotation = transform.eulerAngles.z;
        }

        velocity = (((Vector2)transform.position - lastPosition) / Time.deltaTime).magnitude;
        isWalking = velocity > 0.01f;

        //debugUI.text = $"velocity: " + velocity.ToString("F4");

        lastPosition = transform.position;
    }

    void UpdateOrientation()
    {
        orientation = (transform.eulerAngles.z) switch
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
            Debug.Log("Rotation out of range: " + transform.eulerAngles.z.ToString());
        }
        else
        {
            if (lastOrientation == Orientation.NA || orientation != lastOrientation)
            {
                SetSpriteToOrientation();
                lastOrientation = orientation;
            }
        }
    }

    void SetSpriteToOrientation()
    {
        currentSpriteSet = orientation switch
        {
            Orientation.N => bodySprites.N,
            Orientation.NE => bodySprites.NE,
            Orientation.E => bodySprites.E,
            Orientation.SE => bodySprites.SE,
            Orientation.S => bodySprites.S,
            Orientation.SW => bodySprites.SW,
            Orientation.W => bodySprites.W,
            _ => bodySprites.NW
        };

        bodyRenderer.sprite = currentSpriteSet.idle;
    }

    IEnumerator WalkCycleWithIdle()
    {
        bool lastStepWasRight = true;
        bool isIdleFrame = true;

        while (true)
        {
            while (isWalking)
            {
                if (isIdleFrame && lastStepWasRight)
                {
                    bodyRenderer.sprite = currentSpriteSet.walkLeft;
                    isIdleFrame = false;
                    lastStepWasRight = false;
                }
                else if (isIdleFrame && !lastStepWasRight)
                {
                    bodyRenderer.sprite = currentSpriteSet.walkRight;
                    isIdleFrame = false;
                    lastStepWasRight = true;
                }
                else
                {
                    bodyRenderer.sprite = currentSpriteSet.idle;
                    isIdleFrame = true;
                }

                // Modulate duration of step intervals relative to character velocity.
                yield return new WaitForSeconds(Mathf.Clamp((maxVelocity - velocity) / 10f, 0.175f, maxVelocity));
            }

            bodyRenderer.sprite = currentSpriteSet.idle;

            yield return null;
        }
    }
}
