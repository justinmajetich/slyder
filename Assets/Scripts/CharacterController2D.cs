using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController2D : MonoBehaviour
{
    public float moveSpeed = 5f;
    bool isMoving = false;
    Vector2 moveValue = Vector2.zero;

    public Sprite[] upFacingSprites = new Sprite[4];
    public Sprite[] rightFacingSprites = new Sprite[4];
    public Sprite[] downFacingSprites = new Sprite[4];
    public Sprite[] leftFacingSprites = new Sprite[4];

    void Update()
    {
        if (isMoving)
        {
            transform.Translate(moveSpeed * Time.deltaTime * new Vector2(moveValue.x, moveValue.y));
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isMoving = true;
        }
        if (context.canceled)
        {
            isMoving = false;
        }

        moveValue = context.ReadValue<Vector2>();
    }
}
