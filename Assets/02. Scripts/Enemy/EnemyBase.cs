using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyBase : MonoBehaviour
{
    [Header("공통 감지")]
    [SerializeField] protected LayerMask wallLayer;

    protected NavMeshAgent agent;
    protected SpriteRenderer spriteRenderer;
    protected Transform player;
    protected PlayerFootstep playerFootstep;
    protected Vector2 lastMoveDirection = Vector2.down;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    protected virtual void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerFootstep = playerObj.GetComponent<PlayerFootstep>();
        }
    }

    protected virtual void Update()
    {
        var pos = transform.position;
        if (pos.z != 0f) transform.position = new Vector3(pos.x, pos.y, 0f);
    }

    protected bool HasLineOfSight()
    {
        if (player == null) return false;
        return Physics2D.Linecast(transform.position, player.position, wallLayer).collider == null;
    }

    protected bool DetectFootstep()
    {
        if (playerFootstep == null || player == null) return false;
        return playerFootstep.FootstepRadius > 0f &&
               Vector3.Distance(transform.position, player.position) <= playerFootstep.FootstepRadius;
    }

    protected void UpdateSprite()
    {
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = agent.velocity.normalized;
            if (spriteRenderer != null)
                spriteRenderer.flipX = agent.velocity.x > 0;
        }
    }
}
