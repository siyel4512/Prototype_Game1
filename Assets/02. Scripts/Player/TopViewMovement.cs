using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class TopViewMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sneakSpeed = 2f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;

    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        if (ClueUIPanel.IsInventoryOpen) return;
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (ClueUIPanel.IsInventoryOpen) return;
        bool sneaking = UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed;
        float speed = sneaking ? sneakSpeed : moveSpeed;
        rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
    }

    private void Update()
    {
        if (ClueUIPanel.IsInventoryOpen)
        {
            moveInput = Vector2.zero;
            animator.SetBool(IsMoving, false);
            return;
        }

        bool moving = Vector2.zero != moveInput;
        animator.SetBool(IsMoving, moving);

        if (moving)
        {
            animator.SetFloat(MoveX, moveInput.x);
            animator.SetFloat(MoveY, moveInput.y);

            if (moveInput.x != 0)
                spriteRenderer.flipX = moveInput.x > 0;
        }
    }
}
