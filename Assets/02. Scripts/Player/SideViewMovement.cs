using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SideViewMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    private const float groundCheckRadius = 0.1f;

    private Rigidbody2D rb;
    private InputSystem_Actions inputActions;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()  { inputActions.Player.Enable(); }
    void OnDisable() { inputActions.Player.Disable(); }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        float h = inputActions.Player.Move.ReadValue<Vector2>().x;
        rb.linearVelocity = new Vector2(h * moveSpeed, rb.linearVelocity.y);

        if (inputActions.Player.Jump.triggered && isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
}
