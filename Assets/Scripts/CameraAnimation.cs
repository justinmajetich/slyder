using System;
using System.Collections;
using System.Collections.Generic;
using NodeCanvas.DialogueTrees;
using UnityEngine;

public class CameraAnimation : MonoBehaviour
{
    public static event Action OnCameraZoomed;

    Camera m_Camera;

    [Header("Zoom")]
    [SerializeField, Range(0f, 1f)]
    float zoomSpeed = 0.1f;

    [SerializeField]
    float zoomProjectionSize = 3f;
    float defaultProjectionSize;

    [SerializeField, Range(0, 4)]
    float zoomYOffset = 3.5f;

    Vector3 initialCameraPos;

    private void OnEnable()
    {
        CharacterController2D.OnClickedStartDialogue += OnDialogueStart;
        DialogueTree.OnDialogueFinished += OnDialogueEnd;
        GameManager.OnPlayBedroomMonologue += OnDialogueStart;
    }

    private void OnDisable()
    {
        CharacterController2D.OnClickedStartDialogue -= OnDialogueStart;
        DialogueTree.OnDialogueFinished -= OnDialogueEnd;
        GameManager.OnPlayBedroomMonologue -= OnDialogueStart;
    }

    private void Awake()
    {
        m_Camera = GetComponent<Camera>();
        initialCameraPos = m_Camera.transform.position;
        defaultProjectionSize = m_Camera.orthographicSize;
    }

    void OnDialogueStart(ExpressiveDialogueActor actorA, ExpressiveDialogueActor actorB)
    {
        List<Vector2> actorPositions = new List<Vector2> { };

        foreach (ExpressiveDialogueActor actor in new ExpressiveDialogueActor[] { actorA, actorB })
        {
            if (actor != null)
            {
                actorPositions.Add(actor.transform.position);
            }
        }

        StartCoroutine(Zoom(actorPositions, zoomProjectionSize));
    }

    void OnDialogueEnd(DialogueTree arg)
    {
        StartCoroutine(Zoom(null, defaultProjectionSize));
    }

    IEnumerator Zoom(List<Vector2> targetPositions, float targetPorjectionSize)
    {
        float zoomVelocity = 0f;
        Vector2 panVelocity = Vector2.zero;
        Vector2 target;

        if (targetPositions == null)
        {
            target = initialCameraPos;
        }
        else if (targetPositions.Count == 1)
        {
            target = targetPositions[0] + new Vector2(0f, zoomYOffset);
        }
        else
        {
            Vector2 pointA = targetPositions[0];
            Vector2 pointB = targetPositions[1];

            target = new Vector2((pointA.x + pointB.x) / 2, (pointA.y + pointB.y) / 2) + new Vector2(0f, zoomYOffset);
        }

        while (Mathf.Abs(m_Camera.orthographicSize - targetPorjectionSize) >= 0.01f)
        {
            m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, targetPorjectionSize, ref zoomVelocity, zoomSpeed);
            Vector2 increment = Vector2.SmoothDamp(transform.position, target, ref panVelocity, zoomSpeed);
            transform.position = new Vector3(increment.x, increment.y, initialCameraPos.z);

            yield return null;
        }

        if (m_Camera.orthographicSize > targetPorjectionSize)
        {
            OnCameraZoomed?.Invoke();
        }
        else
        {
            m_Camera.orthographicSize = defaultProjectionSize;
            transform.position = initialCameraPos;
        }
    }
}
