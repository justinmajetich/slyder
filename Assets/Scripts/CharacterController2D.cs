using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController2D : MonoBehaviour
{
    public float moveSpeed = 5f;
    bool isMoving = false;
    Vector2 moveValue = Vector2.zero;

    SpriteRenderer m_SpriteRenderer;
    Sprite[] activeSpriteSet;

    public Sprite[] upFacingSprites = new Sprite[4];
    public Sprite[] rightFacingSprites = new Sprite[4];
    public Sprite[] downFacingSprites = new Sprite[4];
    public Sprite[] leftFacingSprites = new Sprite[4];

    [SerializeField, Tooltip("Speed of sprite animation in seconds")]
    float spriteAnimationSpeed = 0.15f;

    private void Start()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
    }

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

    //void MoveForward()
    //{
    //    float verticalInput = Input.GetAxisRaw("Vertical");
    //    float horizontalInput = Input.GetAxisRaw("Horizontal");

    //    if (verticalInput != 0f || horizontalInput != 0f)
    //    {
    //        if (!isMoving)
    //        {
    //            isMoving = true;
    //            //StartCoroutine(AnimateSpriteWalking());
    //        }

    //        if (verticalInput != 0f && horizontalInput != 0f)
    //        {
    //            verticalInput *= 0.6f;
    //            horizontalInput *= 0.6f;
    //        }

    //        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0f);

    //        transform.Translate(speed * Time.deltaTime * movement);

    //        //SetSpriteForward(movement.normalized);
    //    }
    //    else
    //    {
    //        isMoving = false;
    //    }
    //}

    void SetSpriteForward(Vector3 direction)
    {
        if (direction.y > 0f)
        {
            activeSpriteSet = upFacingSprites;
        }
        else if (direction.x > 0f)
        {
            activeSpriteSet = rightFacingSprites;
        }
        else if (direction.y < 0f)
        {
            activeSpriteSet = downFacingSprites;
        }
        else
        {
            activeSpriteSet = leftFacingSprites;
        }
    }

    IEnumerator AnimateSpriteWalking()
    {
        int frame = 0;

        while (isMoving)
        {
            m_SpriteRenderer.sprite = activeSpriteSet[frame];

            if (frame < 3)
            {
                frame++;
            }
            else
            {
                frame = 0;
            }

            yield return new WaitForSeconds(spriteAnimationSpeed);
        }
    }
}
