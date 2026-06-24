using UnityEngine;
using UnityEngine.AI;

public class LurkerAI : EnemyBase
{
    private enum State { Idle, Chase, Lost, Caught }

    [Header("감지")]
    [SerializeField] private float runDetectRadius = 4f;
    [SerializeField] private float sneakDetectRadius = 1f;
    [SerializeField] private float speedThreshold = 3f;

    [Header("이동")]
    [SerializeField] private float wanderSpeed = 1f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float chaseStoppingDistance = 1f;

    [Header("배회")]
    [SerializeField] private float wanderRadius = 5f;
    [SerializeField] private float wanderWaitTime = 2f;

    [Header("수색")]
    [SerializeField] private float searchRadius = 3f;

    [Header("잡힘 (테스트)")]
    [SerializeField] private float caughtDuration = 2f;

    private State state = State.Idle;
    private Vector3 lastKnownPos;
    private Vector3 lastDestination;
    private Vector3 wanderOrigin;
    private int searchIndex;
    private int searchCount;
    private bool arrivedAtLastKnown;
    private bool isWanderWaiting;
    private float wanderTimer;
    private float caughtTimer;
    private LineRenderer ring;
    private LineRenderer sneakRing;

    protected override void Start()
    {
        base.Start();
        wanderOrigin = transform.position;
        agent.speed = wanderSpeed;
        agent.SetDestination(wanderOrigin + (Vector3)Random.insideUnitCircle * wanderRadius);
        InitRings();
    }

    protected override void Update()
    {
        base.Update();
        UpdateRings();

        switch (state)
        {
            case State.Idle:   UpdateIdle();   break;
            case State.Chase:  UpdateChase();  break;
            case State.Lost:   UpdateLost();   break;
            case State.Caught: UpdateCaught(); break;
        }

        UpdateSprite();
    }

    // ── 감지 ──────────────────────────────────────────────

    private bool IsRunning() =>
        playerFootstep != null && playerFootstep.FootstepRadius >= speedThreshold;

    private bool IsSneaking() =>
        playerFootstep != null &&
        playerFootstep.FootstepRadius > 0f &&
        playerFootstep.FootstepRadius < speedThreshold;

    private bool CanDetectRun() =>
        IsRunning() && Vector3.Distance(transform.position, player.position) <= runDetectRadius;

    private bool CanDetectSneak() =>
        IsSneaking() && Vector3.Distance(transform.position, player.position) <= sneakDetectRadius;

    private bool CanDetectAny() => CanDetectRun() || CanDetectSneak();

    // ── 상태 업데이트 ─────────────────────────────────────

    private void UpdateIdle()
    {
        if (CanDetectAny()) { lastKnownPos = player.position; EnterChase(); return; }

        if (isWanderWaiting)
        {
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f)
            {
                isWanderWaiting = false;
                agent.SetDestination(wanderOrigin + (Vector3)Random.insideUnitCircle * wanderRadius);
            }
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.3f)
        {
            isWanderWaiting = true;
            wanderTimer = wanderWaitTime;
            agent.SetDestination(transform.position);
        }
    }

    private void UpdateChase()
    {
        // 소리 감지 시 lastKnownPos 갱신하며 계속 추격
        if (CanDetectAny()) lastKnownPos = player.position;

        if (Vector3.Distance(lastKnownPos, lastDestination) > 0.5f)
        {
            agent.SetDestination(lastKnownPos);
            lastDestination = lastKnownPos;
        }

        if (player != null && Vector3.Distance(transform.position, player.position) <= chaseStoppingDistance)
        {
            EnterCaught(); return;
        }

        // lastKnownPos 도착 후 감지 불가 시 Lost
        if (!agent.pathPending && agent.remainingDistance < 0.3f)
        {
            if (!CanDetectAny()) EnterLost();
        }
    }

    private void UpdateLost()
    {
        if (CanDetectAny()) { lastKnownPos = player.position; EnterChase(); return; }

        if (!arrivedAtLastKnown)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.3f)
            {
                arrivedAtLastKnown = true;
                if (player != null && Vector3.Distance(transform.position, player.position) <= chaseStoppingDistance)
                    EnterCaught();
                else
                    MoveToNextSearchPoint();
            }
        }
        else
        {
            if (!agent.pathPending && agent.remainingDistance < 0.3f)
            {
                if (searchIndex < searchCount)
                    MoveToNextSearchPoint();
                else
                    EnterIdle();
            }
        }
    }

    private void UpdateCaught()
    {
        caughtTimer -= Time.deltaTime;
        if (caughtTimer <= 0f) EnterIdle();
    }

    // ── 이동 ──────────────────────────────────────────────

    private void MoveToNextSearchPoint()
    {
        agent.SetDestination(lastKnownPos + (Vector3)Random.insideUnitCircle * searchRadius);
        searchIndex++;
    }

    // ── 상태 전환 ─────────────────────────────────────────

    private void EnterIdle()
    {
        state = State.Idle;
        agent.speed = wanderSpeed;
        agent.stoppingDistance = 0f;
        isWanderWaiting = false;
        agent.SetDestination(wanderOrigin + (Vector3)Random.insideUnitCircle * wanderRadius);
    }

    private void EnterChase()
    {
        state = State.Chase;
        agent.speed = chaseSpeed;
        agent.stoppingDistance = 0f;
        agent.SetDestination(lastKnownPos);
        lastDestination = lastKnownPos;
    }

    private void EnterLost()
    {
        state = State.Lost;
        arrivedAtLastKnown = false;
        searchIndex = 0;
        searchCount = Random.Range(1, 3);
        agent.speed = wanderSpeed;
        agent.stoppingDistance = 0f;
        agent.SetDestination(lastKnownPos);
        lastDestination = lastKnownPos;
    }

    private void EnterCaught()
    {
        state = State.Caught;
        caughtTimer = caughtDuration;
        agent.SetDestination(transform.position);
    }

    // ── 디버그 시각화 ─────────────────────────────────────

    private void InitRings()
    {
        var go = new GameObject("Ring");
        go.transform.SetParent(transform);
        ring = go.AddComponent<LineRenderer>();
        ring.material = new Material(Shader.Find("Sprites/Default"));
        ring.startWidth = 0.08f;
        ring.endWidth = 0.08f;
        ring.positionCount = 33;
        ring.useWorldSpace = true;
        ring.loop = true;

        var go2 = new GameObject("SneakRing");
        go2.transform.SetParent(transform);
        sneakRing = go2.AddComponent<LineRenderer>();
        sneakRing.material = new Material(Shader.Find("Sprites/Default"));
        sneakRing.startWidth = 0.05f;
        sneakRing.endWidth = 0.05f;
        sneakRing.positionCount = 33;
        sneakRing.useWorldSpace = true;
        sneakRing.loop = true;
    }

    private void UpdateRings()
    {
        // 메인 링: 달리기 감지 반경 (초록=미감지 / 빨강=감지 중 / 마젠타=잡힘)
        Color c = state == State.Caught ? Color.magenta :
                  CanDetectRun() ? Color.red : Color.green;
        ring.startColor = c;
        ring.endColor   = c;
        for (int i = 0; i < 33; i++)
        {
            float angle = i * 2f * Mathf.PI / 32f;
            ring.SetPosition(i, transform.position + new Vector3(Mathf.Cos(angle) * runDetectRadius, Mathf.Sin(angle) * runDetectRadius, 0f));
        }

        // 스닉 링: 걷기 감지 반경 (흰색=미감지 / 빨강=감지 중)
        Color sc = CanDetectSneak() ? Color.red : Color.white;
        sneakRing.startColor = sc;
        sneakRing.endColor   = sc;
        for (int i = 0; i < 33; i++)
        {
            float angle = i * 2f * Mathf.PI / 32f;
            sneakRing.SetPosition(i, transform.position + new Vector3(Mathf.Cos(angle) * sneakDetectRadius, Mathf.Sin(angle) * sneakDetectRadius, 0f));
        }
    }

    private void OnGUI()
    {
        if (Camera.main == null) return;
        var sp = Camera.main.WorldToScreenPoint(transform.position);
        sp.y = Screen.height - sp.y;
        float fr = playerFootstep != null ? playerFootstep.FootstepRadius : 0f;
        string extra = state == State.Caught ? $"\n★ CAUGHT ({caughtTimer:F1}s)" : "";
        GUI.Label(new Rect(sp.x - 40, sp.y - 60, 200, 100),
            $"[Lurker] {state}\nRunR: {runDetectRadius:F1}  SneakR: {sneakDetectRadius:F1}\nFootstepR: {fr:F1}{extra}");
    }
}
