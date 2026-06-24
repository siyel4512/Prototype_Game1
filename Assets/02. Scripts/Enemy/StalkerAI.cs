using UnityEngine;
using UnityEngine.AI;

public class StalkerAI : EnemyBase
{
    private enum State { Dormant, Chase, Lost, Caught }

    [Header("감지")]
    [SerializeField] private float detectionRadius = 3f;

    [Header("이동")]
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private float returnSpeed = 3f;
    [SerializeField] private float chaseStoppingDistance = 1f;

    [Header("잡힘 (테스트)")]
    [SerializeField] private float caughtDuration = 2f;

    private State state = State.Dormant;
    private Vector3 spawnPosition;
    private Vector3 lastKnownPosition;
    private float caughtTimer;
    private LineRenderer ring;

    protected override void Start()
    {
        base.Start();
        spawnPosition = transform.position;
        agent.SetDestination(transform.position);
        InitRing();
    }

    protected override void Update()
    {
        base.Update();
        UpdateRing();

        switch (state)
        {
            case State.Dormant: UpdateDormant(); break;
            case State.Chase:   UpdateChase();   break;
            case State.Lost:    UpdateLost();    break;
            case State.Caught:  UpdateCaught();  break;
        }

        UpdateSprite();
    }

    // ── 상태 업데이트 ─────────────────────────────────────

    private void UpdateDormant()
    {
        if (DetectByRadius()) EnterChase();
    }

    private void UpdateChase()
    {
        if (player == null) { EnterLost(); return; }

        // LOS 무관, 항상 플레이어 위치 추적
        lastKnownPosition = player.position;
        agent.SetDestination(player.position);

        if (Vector3.Distance(transform.position, player.position) <= chaseStoppingDistance)
            EnterCaught();
    }

    private void UpdateLost()
    {
        // 스폰 위치 복귀 후 Dormant
        if (!agent.pathPending && agent.remainingDistance < 0.3f)
            EnterDormant();
    }

    private void UpdateCaught()
    {
        caughtTimer -= Time.deltaTime;
        if (caughtTimer <= 0f) EnterDormant();
    }

    // ── 상태 전환 ─────────────────────────────────────────

    private void EnterDormant()
    {
        state = State.Dormant;
        agent.speed = 0f;
        agent.SetDestination(transform.position);
    }

    private void EnterChase()
    {
        state = State.Chase;
        agent.speed = chaseSpeed;
        agent.stoppingDistance = 0f;
        if (player != null) lastKnownPosition = player.position;
    }

    private void EnterLost()
    {
        state = State.Lost;
        agent.speed = returnSpeed;
        agent.stoppingDistance = 0f;
        agent.SetDestination(spawnPosition);
    }

    private void EnterCaught()
    {
        state = State.Caught;
        caughtTimer = caughtDuration;
        agent.SetDestination(transform.position);
    }

    // 환경 장치 연동 — 퍼즐 시스템에서 호출 (전력 차단, 문 잠금 등)
    public void ForceDisengage()
    {
        if (state == State.Chase) EnterLost();
    }

    // ── 감지 ──────────────────────────────────────────────

    // LOS 없이 반경만 체크 — 매복 위치는 씬 배치 단계에서 조정
    private bool DetectByRadius() =>
        player != null &&
        Vector3.Distance(transform.position, player.position) <= detectionRadius;

    // ── 디버그 시각화 ─────────────────────────────────────

    private void InitRing()
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
    }

    private void UpdateRing()
    {
        if (ring == null) return;
        Color c = state == State.Caught  ? Color.magenta :
                  state == State.Chase   ? Color.red :
                  state == State.Lost    ? Color.gray :
                  DetectByRadius()       ? Color.red : new Color(0.5f, 0f, 0.5f);
        ring.startColor = c;
        ring.endColor   = c;

        float r = detectionRadius;
        for (int i = 0; i < 33; i++)
        {
            float angle = i * 2f * Mathf.PI / 32f;
            ring.SetPosition(i, transform.position + new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0f));
        }
    }

    private void OnGUI()
    {
        if (Camera.main == null) return;
        var sp = Camera.main.WorldToScreenPoint(transform.position);
        sp.y = Screen.height - sp.y;
        string extra = state == State.Caught ? $"\n★ CAUGHT ({caughtTimer:F1}s)" : "";
        GUI.Label(new Rect(sp.x - 40, sp.y - 50, 180, 70),
            $"[Stalker] {state}\nR: {detectionRadius}  Break: {breakRadius}{extra}");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, breakRadius);
    }
}
