using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TopViewMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private InputSystem_Actions inputActions;
    private Vector2 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Update()
    {
        movement = inputActions.Player.Move.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
    }
}
