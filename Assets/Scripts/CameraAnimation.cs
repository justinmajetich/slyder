using System;
using System.Collections;
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
        StartCoroutine(ZoomToDialogue(actorA, actorB));
    }

    void OnDialogueEnd(DialogueTree arg)
    {
        StartCoroutine(ZoomToDefault());
    }
    
    IEnumerator ZoomToDialogue(ExpressiveDialogueActor actorA, ExpressiveDialogueActor actorB)
    {
        float zoomVelocity = 0f;
        Vector2 panVelocity = Vector2.zero;
        Vector2 target;

        if (actorB == null)
        {
            target = (Vector2)actorA.transform.position + new Vector2(0f, zoomYOffset);
        }
        else
        {
            Vector2 pointA = actorA.transform.position;
            Vector2 pointB = actorB.transform.position;

            target = new Vector2((pointA.x + pointB.x) / 2, (pointA.y + pointB.y) / 2) + new Vector2(0f, zoomYOffset);
        }

        while (m_Camera.orthographicSize - zoomProjectionSize >= 0.01f)
        {
            m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, zoomProjectionSize, ref zoomVelocity, zoomSpeed);
            Vector2 increment = Vector2.SmoothDamp(transform.position, target, ref panVelocity, zoomSpeed);
            transform.position = new Vector3(increment.x, increment.y, initialCameraPos.z);

            yield return null;
        }

        OnCameraZoomed?.Invoke();
    }

    IEnumerator ZoomToDefault()
    {
        float zoomVelocity = 0f;
        Vector2 panVelocity = Vector2.zero;

        while (defaultProjectionSize - m_Camera.orthographicSize >= 0.01f)
        {
            m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, defaultProjectionSize, ref zoomVelocity, zoomSpeed);
            Vector2 increment = Vector2.SmoothDamp(transform.position, initialCameraPos, ref panVelocity, zoomSpeed);
            transform.position = new Vector3(increment.x, increment.y, initialCameraPos.z);

            yield return null;
        }

        m_Camera.orthographicSize = defaultProjectionSize;
        transform.position = initialCameraPos;
    }
}
