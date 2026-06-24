using UnityEngine;

public class PlayerFootstep : MonoBehaviour
{
    [SerializeField] private float walkThreshold = 3f;
    [SerializeField] private float sneakFootstepRadius = 1f;
    [SerializeField] private float runFootstepRadius = 4f;

    public static PlayerFootstep Instance { get; private set; }
    public float FootstepRadius { get; private set; }

    private Rigidbody2D rb;
    private Vector2 lastPosition;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        lastPosition = rb.position;
    }

    private void FixedUpdate()
    {
        float speed = Vector2.Distance(rb.position, lastPosition) / Time.fixedDeltaTime;
        lastPosition = rb.position;
        if (speed <= 0.1f)
            FootstepRadius = 0f;
        else if (speed <= walkThreshold)
            FootstepRadius = sneakFootstepRadius;
        else
            FootstepRadius = runFootstepRadius;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || FootstepRadius <= 0f) return;
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, FootstepRadius);
    }
}
