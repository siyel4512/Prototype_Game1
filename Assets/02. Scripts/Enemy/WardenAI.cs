using UnityEngine;
using UnityEngine.AI;

public class WardenAI : EnemyBase
{
    public enum WardenState { Patrol, Suspicious, Chase, Lost, AlertPatrol, Caught }

    [Header("감지")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float closeDetectionRadius = 1.5f;
    [SerializeField] private float fieldOfView = 90f;
    [SerializeField] private float chaseBreakRadius = 8f;

    [Header("이동")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float suspiciousSpeed = 2.5f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float alertPatrolSpeed = 3f;
    [SerializeField] private float chaseStoppingDistance = 1f;

    [Header("순찰")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointWaitTime = 1f;

    [Header("스캔")]
    [SerializeField] private float scanSpeed = 60f;
    [SerializeField] private float scanRange = 60f;

    [Header("추격")]
    [SerializeField] private float chaseFOV = 270f;
    [SerializeField] private float losBreakDuration = 1.5f;

    [Header("타이머")]
    [SerializeField] private float suspiciousDuration = 3f;
    [SerializeField] private float alertPatrolDuration = 20f;

    [Header("수색")]
    [SerializeField] private float searchRadius = 3f;

    [Header("잡힘 (테스트)")]
    [SerializeField] private float caughtDuration = 2f;

    private WardenState state = WardenState.Patrol;
    private Vector3 lastKnownPosition;
    private int currentWaypointIndex;
    private float stateTimer;
    private float caughtTimer;
    private float alertPatrolTimer;
    private float losBreakTimer;
    private bool isWaiting;
    private bool arrived;
    private bool scanDone;
    private float scanAngle;
    private float scanDir = 1f;
    private Vector2 scanBaseDir;
    private LineRenderer fovRenderer;

    protected override void Start()
    {
        base.Start();
        ChangeState(WardenState.Patrol);
        InitFOVRenderer();
    }

    protected override void Update()
    {
        base.Update();

        switch (state)
        {
            case WardenState.Patrol:      UpdatePatrol();      break;
            case WardenState.Suspicious:  UpdateSuspicious();  break;
            case WardenState.Chase:       UpdateChase();       break;
            case WardenState.Lost:        UpdateLost();        break;
            case WardenState.AlertPatrol: UpdateAlertPatrol(); break;
            case WardenState.Caught:      UpdateCaught();      break;
        }

        UpdateSprite();
        UpdateFOVRenderer();
    }

    // ── 상태 업데이트 ─────────────────────────────────────

    private void UpdatePatrol()
    {
        if (DetectPlayer()) { ChangeState(WardenState.Suspicious); return; }
        if (waypoints.Length == 0) return;

        if (isWaiting)
        {
            UpdateScan();
            if (DetectPlayer()) { ChangeState(WardenState.Suspicious); return; }
            if (scanDone)
            {
                isWaiting = false;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                agent.SetDestination(waypoints[currentWaypointIndex].position);
            }
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.3f)
        {
            isWaiting = true;
            BeginScan();
        }
    }

    private void UpdateSuspicious()
    {
        // 시야 안에 있으면 즉시 Chase
        if (DetectPlayer()) { lastKnownPosition = player.position; ChangeState(WardenState.Chase); return; }

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f) { ChangeState(WardenState.Patrol); return; }

        if (!arrived)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.3f)
            {
                arrived = true;
                BeginScan();
            }
        }
        else
        {
            UpdateScan();
            if (DetectPlayer()) { ChangeState(WardenState.Chase); return; }
            if (scanDone) ChangeState(WardenState.Patrol);
        }
    }

    private void UpdateChase()
    {
        if (player == null) { ChangeState(WardenState.Lost); return; }
        if (Vector3.Distance(transform.position, player.position) > chaseBreakRadius) { ChangeState(WardenState.Lost); return; }

        if (DetectPlayer())
        {
            losBreakTimer = losBreakDuration;
            lastKnownPosition = player.position;
        }
        else
        {
            losBreakTimer -= Time.deltaTime;
            if (losBreakTimer <= 0f) { ChangeState(WardenState.Lost); return; }
        }

        if (Vector3.Distance(transform.position, player.position) <= chaseStoppingDistance)
        {
            ChangeState(WardenState.Caught); return;
        }

        agent.SetDestination(lastKnownPosition);
    }

    private void UpdateLost()
    {
        if (DetectPlayer()) { lastKnownPosition = player.position; ChangeState(WardenState.Chase); return; }

        if (!arrived)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.3f)
            {
                arrived = true;
                BeginScan();
            }
        }
        else
        {
            UpdateScan();
            if (DetectPlayer()) { ChangeState(WardenState.Chase); return; }
            if (scanDone) ChangeState(WardenState.AlertPatrol);
        }
    }

    private void UpdateAlertPatrol()
    {
        alertPatrolTimer -= Time.deltaTime;
        if (alertPatrolTimer <= 0f) { ChangeState(WardenState.Patrol); return; }

        // Alert 중엔 Suspicious 없이 즉시 Chase
        if (DetectPlayer()) { lastKnownPosition = player.position; ChangeState(WardenState.Chase); return; }
        if (waypoints.Length == 0) return;

        if (isWaiting)
        {
            UpdateScan();
            if (DetectPlayer()) { ChangeState(WardenState.Chase); return; }
            if (scanDone)
            {
                isWaiting = false;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                agent.SetDestination(waypoints[currentWaypointIndex].position);
            }
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.3f)
        {
            isWaiting = true;
            BeginScan();
        }
    }

    private void UpdateCaught()
    {
        caughtTimer -= Time.deltaTime;
        if (caughtTimer <= 0f) ChangeState(WardenState.Patrol);
    }

    // ── 스캔 ──────────────────────────────────────────────

    private void BeginScan()
    {
        scanBaseDir = lastMoveDirection;
        scanAngle = 0f;
        scanDir = 1f;
        scanDone = false;
        agent.SetDestination(transform.position);
    }

    private void UpdateScan()
    {
        scanAngle += scanDir * scanSpeed * Time.deltaTime;
        if (scanAngle >= scanRange)       { scanAngle = scanRange;  scanDir = -1f; }
        else if (scanAngle <= -scanRange) { scanDone = true; }
        lastMoveDirection = Quaternion.Euler(0, 0, scanAngle) * (Vector3)scanBaseDir;
    }

    // ── 상태 전환 ─────────────────────────────────────────

    private void ChangeState(WardenState newState)
    {
        state = newState;
        arrived = false;
        isWaiting = false;

        switch (newState)
        {
            case WardenState.Patrol:
                agent.speed = patrolSpeed;
                agent.stoppingDistance = 0f;
                if (waypoints.Length > 0)
                    agent.SetDestination(waypoints[currentWaypointIndex].position);
                break;
            case WardenState.Suspicious:
                agent.speed = suspiciousSpeed;
                agent.stoppingDistance = 0f;
                agent.SetDestination(lastKnownPosition);
                stateTimer = suspiciousDuration;
                break;
            case WardenState.Chase:
                agent.speed = chaseSpeed;
                agent.stoppingDistance = 0f;
                if (player != null) lastKnownPosition = player.position;
                losBreakTimer = losBreakDuration;
                break;
            case WardenState.Lost:
                agent.speed = patrolSpeed;
                agent.stoppingDistance = 0f;
                agent.SetDestination(lastKnownPosition);
                break;
            case WardenState.AlertPatrol:
                agent.speed = alertPatrolSpeed;
                agent.stoppingDistance = 0f;
                alertPatrolTimer = alertPatrolDuration;
                if (waypoints.Length > 0)
                    agent.SetDestination(waypoints[currentWaypointIndex].position);
                break;
            case WardenState.Caught:
                caughtTimer = caughtDuration;
                agent.SetDestination(transform.position);
                break;
        }
    }

    // ── 감지 ──────────────────────────────────────────────

    private bool DetectPlayer()
    {
        if (player == null) return false;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > detectionRadius) return false;

        // 근접 시 시야각 무관하게 감지
        if (dist <= closeDetectionRadius) return HasLineOfSight();

        float fov = state == WardenState.Chase ? chaseFOV : fieldOfView;
        Vector2 dirToPlayer = (player.position - transform.position).normalized;
        if (Vector2.Angle(lastMoveDirection, dirToPlayer) >= fov * 0.5f) return false;
        return HasLineOfSight();
    }

    // ── 디버그 시각화 ─────────────────────────────────────

    private void InitFOVRenderer()
    {
        var go = new GameObject("FOV_Debug");
        go.transform.SetParent(transform);
        fovRenderer = go.AddComponent<LineRenderer>();
        fovRenderer.material = new Material(Shader.Find("Sprites/Default"));
        fovRenderer.startWidth = 0.06f;
        fovRenderer.endWidth = 0.06f;
        fovRenderer.useWorldSpace = true;
        fovRenderer.loop = false;
        fovRenderer.positionCount = 23;
    }

    private void UpdateFOVRenderer()
    {
        if (fovRenderer == null) return;
        Color c = state == WardenState.Caught      ? Color.magenta :
                  state == WardenState.Chase        ? Color.red :
                  state == WardenState.AlertPatrol  ? new Color(1f, 0.5f, 0f) :
                  state == WardenState.Suspicious   ? Color.cyan :
                  DetectPlayer()                    ? Color.red : Color.yellow;
        fovRenderer.startColor = c;
        fovRenderer.endColor   = c;

        float activeFOV = state == WardenState.Chase ? chaseFOV : fieldOfView;
        Vector3 origin = transform.position;
        fovRenderer.SetPosition(0, origin);
        for (int i = 0; i <= 20; i++)
        {
            float t = (float)i / 20f;
            float angle = Mathf.Lerp(activeFOV * 0.5f, -activeFOV * 0.5f, t);
            Vector3 dir = Quaternion.Euler(0, 0, angle) * (Vector3)lastMoveDirection;
            fovRenderer.SetPosition(i + 1, origin + dir * detectionRadius);
        }
        fovRenderer.SetPosition(22, origin);
    }

    private void OnGUI()
    {
        if (Camera.main == null) return;
        var sp = Camera.main.WorldToScreenPoint(transform.position);
        sp.y = Screen.height - sp.y;
        string extra = state == WardenState.Caught      ? $"\n★ CAUGHT ({caughtTimer:F1}s)" :
                       state == WardenState.AlertPatrol  ? $"\n⚠ ALERT ({alertPatrolTimer:F0}s)" : "";
        GUI.Label(new Rect(sp.x - 40, sp.y - 50, 200, 80),
            $"[Warden] {state}\nFOV: {fieldOfView}°  R: {detectionRadius}{extra}");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 left  = Quaternion.Euler(0, 0,  fieldOfView * 0.5f) * (Vector3)lastMoveDirection * detectionRadius;
        Vector3 right = Quaternion.Euler(0, 0, -fieldOfView * 0.5f) * (Vector3)lastMoveDirection * detectionRadius;
        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position, transform.position + right);
    }
}
