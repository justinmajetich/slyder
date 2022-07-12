using UnityEngine;

public class WaypointAnimation : MonoBehaviour
{
    Animator animator;

    private void OnEnable()
    {
        CharacterController2D.OnClickedWalkable += OnClickedWalkable;
    }

    private void OnDisable()
    {
        CharacterController2D.OnClickedWalkable -= OnClickedWalkable;
    }

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void OnClickedWalkable()
    {
        animator.SetTrigger("onClickedWalkable");
    }
}
