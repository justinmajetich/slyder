using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine;

public class SceneExit : MonoBehaviour
{
    Animator animator;
    bool isHovered = false;

    public Transform spawn;
    public Constants.Scene targetScene;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        CharacterController2D.OnHoveringSceneTransition += OnMouseHoveringSceneTransition;
    }

    private void OnDisable()
    {
        CharacterController2D.OnHoveringSceneTransition -= OnMouseHoveringSceneTransition;
    }

    void OnMouseHoveringSceneTransition(Constants.Scene hoveredTargetScene)
    {
        if (hoveredTargetScene == targetScene && !isHovered)
        {
            isHovered = true;
            animator.SetBool("isHovered", isHovered);
            StartCoroutine(TrackMouseHover());
        }
    }

    IEnumerator TrackMouseHover()
    {
        while (true)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), Vector2.zero);

            if (hit.collider && hit.collider.gameObject.layer == LayerMask.NameToLayer(Constants.LayerMaskSceneExit))
            {
                yield return null;
            }
            else
            {
                break;
            }
        }

        isHovered = false;
        animator.SetBool("isHovered", isHovered);
    }
}
