using UnityEngine;
using UnityEngine.AI;              // 강의 07장: NavMeshAgent
using UnityEngine.SceneManagement; // 강의 04장: SceneManager.LoadScene

/// <summary>
/// 괴물 「집주인」의 AI. 눈이 없어서 소리(NoiseSystem)와 아주 가까운 기척으로만 플레이어를 찾는다.
///
/// [상태]  순찰 → (소리 들음) 확인하러 감 → (플레이어가 가까움) 추격 → (놓침) 수색 → 순찰
///   - 순찰(Patrol)      : patrolPoints를 차례로 천천히 돈다. 비어 있으면 제자리에 있다.
///   - 확인(Investigate) : 들은 소리 위치로 간다. 도착하면 수색.
///   - 추격(Chase)       : 플레이어에게 달려간다. 잡으면 재시작.
///   - 수색(Search)      : 그 자리에서 searchTime초 동안 두리번거린 뒤 순찰로 돌아간다.
///   - 장롱에 숨은 플레이어(PlayerHide.IsHidden)는 느끼지 못한다.
///
/// [붙이는 곳] 괴물 오브젝트 (기준점이 발밑). Monster 프리팹에 들어 있음
/// [같이 필요한 컴포넌트]
///   - Nav Mesh Agent  : 강의 07장. 길을 찾아 움직인다 (Radius 0.5, Height 2.4)
///   - Rigidbody       : Is Kinematic 체크 (Agent가 움직이므로 물리로 밀리지 않게, 강의 03장)
///   - Capsule Collider
/// [씬에 필요한 것] 이름이 정확히 "Player"인 플레이어, "NoiseSystem" 오브젝트 (Start에서 Find),
///                 바닥에 NavMesh Surface + Bake (강의 07장)
///
/// [이동] 강의 07장 NavMesh: agent.SetDestination(목표)만 하면 벽·장애물을 돌아서 간다.
///        속도는 상태마다 agent.speed를 바꾼다.
/// </summary>
public class MonsterAI : MonoBehaviour
{
    public enum State { Patrol, Investigate, Chase, Search }

    [Header("순찰")]
    [Tooltip("순찰할 지점들 (빈 오브젝트). 순서대로 돈다. 비워 두면 제자리에서 소리만 기다린다")]
    public Transform[] patrolPoints;

    [Header("속도 (m/s) — 플레이어 걷기 3, 달리기 5.5")]
    public float patrolSpeed = 1.2f;
    public float investigateSpeed = 2.2f;
    [Tooltip("추격 속도. 플레이어 달리기(5.5)보다 느려야 도망칠 수 있다. 대신 달리면 소리가 난다")]
    public float chaseSpeed = 3.5f;

    [Header("감지")]
    [Tooltip("이 거리(m) 안에 플레이어가 있으면 소리 없이도 기척을 느끼고 추격한다")]
    public float senseDistance = 2.5f;
    [Tooltip("추격 중 이 거리(m)보다 멀어지고 소리도 안 나면 놓친다")]
    public float loseDistance = 8f;
    [Tooltip("이 거리(m) 안으로 들어오면 잡는다")]
    public float catchDistance = 1.1f;
    [Tooltip("같은 층으로 볼 높이 차이(m). 층 높이가 2.8이라 그보다 작게")]
    public float sameFloorHeight = 1.5f;

    [Header("수색")]
    [Tooltip("도착한 뒤 두리번거리는 시간(초)")]
    public float searchTime = 4f;
    [Tooltip("두리번거릴 때 도는 속도(도/초)")]
    public float searchTurnSpeed = 60f;

    [Header("이동")]
    [Tooltip("목표까지 이 거리(m) 안이면 도착한 것으로 본다")]
    public float arriveDistance = 0.6f;

    [Header("디버그")]
    [Tooltip("체크하면 화면 왼쪽 위에 괴물 상태를 표시한다")]
    public bool showDebug = true;

    /// <summary>지금 상태. B의 연출이나 소리(괴물 울음 등)에서 읽을 수 있다.</summary>
    public State CurrentState { get; private set; }

    // ----- Start()에서 한 번만 찾아 저장 (강의 04장) -----
    private NavMeshAgent agent;
    private Transform player;
    private PlayerHide playerHide;
    private NoiseSystem noiseSystem;

    private int lastNoiseCount;      // 마지막으로 확인한 소리 번호 (바뀌면 새 소리)
    private Vector3 targetPosition;  // 지금 가려는 곳
    private int patrolIndex;         // 다음 순찰 지점 번호
    private float searchTimer;       // 수색한 시간

    private void Start()
    {
        this.agent = this.GetComponent<NavMeshAgent>();
        if (this.agent == null)
        {
            Debug.LogError("[MonsterAI] Nav Mesh Agent가 없습니다. Add Component로 추가해 주세요.");
        }

        // Agent가 위치를 정하므로 물리에 밀리지 않게 한다 (강의 03장 Is Kinematic)
        Rigidbody rb = this.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            this.player = playerObject.transform;
            this.playerHide = playerObject.GetComponent<PlayerHide>();
        }
        else
        {
            Debug.LogError("[MonsterAI] 씬에 Player 오브젝트가 없습니다.");
        }

        GameObject noiseObject = GameObject.Find("NoiseSystem");
        if (noiseObject != null)
        {
            this.noiseSystem = noiseObject.GetComponent<NoiseSystem>();
            this.lastNoiseCount = this.noiseSystem.NoiseCount;   // 시작 전 소리는 무시
        }
        else
        {
            Debug.LogWarning("[MonsterAI] 씬에 NoiseSystem이 없어서 소리를 못 듣습니다.");
        }

        this.ChangeState(State.Patrol);
    }

    private void Update()
    {
        if (this.player == null || this.agent == null)
        {
            return;
        }

        this.ListenForNoise();

        switch (this.CurrentState)
        {
            case State.Patrol:      this.UpdatePatrol(); break;
            case State.Investigate: this.UpdateInvestigate(); break;
            case State.Chase:       this.UpdateChase(); break;
            case State.Search:      this.UpdateSearch(); break;
        }

        // 어느 상태든 플레이어가 바로 옆에 있으면(숨지 않았으면) 기척을 느끼고 추격
        if (this.CurrentState != State.Chase && this.CanSensePlayer(this.senseDistance))
        {
            this.ChangeState(State.Chase);
        }
    }

    // ---------------- 상태별 동작 ----------------

    private void UpdatePatrol()
    {
        if (this.patrolPoints == null || this.patrolPoints.Length == 0)
        {
            this.Stop();
            return;
        }

        Transform point = this.patrolPoints[this.patrolIndex];
        if (this.MoveTowards(point.position, this.patrolSpeed))
        {
            // 도착 → 다음 지점 (마지막이면 처음으로)
            this.patrolIndex = (this.patrolIndex + 1) % this.patrolPoints.Length;
        }
    }

    private void UpdateInvestigate()
    {
        if (this.MoveTowards(this.targetPosition, this.investigateSpeed))
        {
            this.ChangeState(State.Search);
        }
    }

    private void UpdateChase()
    {
        // 장롱에 숨었으면 놓친다 → 마지막으로 있던 곳을 수색
        if (this.IsPlayerHidden())
        {
            this.ChangeState(State.Investigate);
            return;
        }

        float distance = this.FlatDistance(this.player.position);

        // 잡았다
        if (distance <= this.catchDistance && this.IsSameFloor(this.player.position))
        {
            this.CatchPlayer();
            return;
        }

        // 너무 멀어지면 놓친다 (소리가 나면 ListenForNoise가 다시 쫓게 한다)
        if (distance > this.loseDistance || this.IsSameFloor(this.player.position) == false)
        {
            this.ChangeState(State.Investigate);   // targetPosition = 마지막으로 본 위치
            return;
        }

        this.targetPosition = this.player.position;
        this.MoveTowards(this.targetPosition, this.chaseSpeed);
    }

    private void UpdateSearch()
    {
        this.Stop();

        // 제자리에서 천천히 돌며 두리번거린다 (강의 03장 Rotate + Time.deltaTime)
        this.transform.Rotate(0f, this.searchTurnSpeed * Time.deltaTime, 0f);

        this.searchTimer = this.searchTimer + Time.deltaTime;
        if (this.searchTimer >= this.searchTime)
        {
            this.ChangeState(State.Patrol);
        }
    }

    // ---------------- 소리 듣기 ----------------

    // 새 소리가 났고, 들리는 거리 안이면 그쪽으로 간다.
    private void ListenForNoise()
    {
        if (this.noiseSystem == null || this.noiseSystem.NoiseCount == this.lastNoiseCount)
        {
            return;
        }
        this.lastNoiseCount = this.noiseSystem.NoiseCount;

        // 다른 층에서 난 소리는 무시한다 (괴물은 자기 층에서만 움직인다)
        if (this.noiseSystem.CanHear(this.transform.position) == false || this.IsSameFloor(this.noiseSystem.LastPosition) == false)
        {
            return;
        }

        if (this.CurrentState == State.Chase)
        {
            return;   // 이미 쫓는 중
        }

        // 소리가 플레이어 바로 근처에서 났고 숨지 않았으면 → 곧장 추격, 아니면 확인하러 감
        this.targetPosition = this.noiseSystem.LastPosition;
        bool nearPlayer = Vector3.Distance(this.noiseSystem.LastPosition, this.player.position) < 1.5f;
        if (nearPlayer && this.IsPlayerHidden() == false && this.FlatDistance(this.player.position) < this.loseDistance)
        {
            this.ChangeState(State.Chase);
        }
        else
        {
            this.ChangeState(State.Investigate);
        }
    }

    // ---------------- 도움 함수 ----------------

    private void ChangeState(State next)
    {
        if (this.showDebug && next != this.CurrentState)
        {
            Debug.Log("[괴물] " + this.CurrentState + " → " + next);
        }
        this.CurrentState = next;
        this.searchTimer = 0f;
    }

    /// <summary>
    /// target으로 speed(m/s)로 간다. 도착했으면 true.
    /// 강의 07장 NavMesh: SetDestination으로 목표만 알려 주면 Agent가 길을 찾아 움직이고 방향도 돌린다.
    /// 길이 막혀 끝까지 못 가는 곳이면, 갈 수 있는 가장 가까운 곳에 멈췄을 때 도착으로 친다.
    /// </summary>
    private bool MoveTowards(Vector3 target, float speed)
    {
        this.agent.isStopped = false;
        this.agent.speed = speed;

        // 목표가 조금이라도 바뀌었으면 새로 길을 찾는다 (매 프레임 찾지 않게)
        if ((this.agent.destination - target).magnitude > 0.3f || this.agent.hasPath == false)
        {
            this.agent.SetDestination(target);
        }

        if (this.agent.pathPending)
        {
            return false;   // 아직 길 계산 중
        }

        return this.agent.remainingDistance <= this.arriveDistance;
    }

    private void Stop()
    {
        if (this.agent.hasPath)
        {
            this.agent.ResetPath();
        }
        this.agent.isStopped = true;
    }

    private bool CanSensePlayer(float distance)
    {
        return this.IsPlayerHidden() == false
               && this.IsSameFloor(this.player.position)
               && this.FlatDistance(this.player.position) <= distance;
    }

    private bool IsPlayerHidden()
    {
        return this.playerHide != null && this.playerHide.IsHidden;
    }

    private bool IsSameFloor(Vector3 position)
    {
        return Mathf.Abs(position.y - this.transform.position.y) < this.sameFloorHeight;
    }

    // 높이를 빼고 잰 거리
    private float FlatDistance(Vector3 position)
    {
        Vector3 diff = position - this.transform.position;
        diff.y = 0f;
        return diff.magnitude;
    }

    // 잡혔다 → 지금은 바로 재시작 (강의 04장: 현재 Scene 다시 로드). 사망 연출·체크포인트는 4번 작업에서.
    private void CatchPlayer()
    {
        Debug.Log("[괴물] 플레이어를 잡았다!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 강의 04장 OnGUI: 괴물 상태 표시 (NoiseSystem 글자 아래 줄)
    private void OnGUI()
    {
        if (this.showDebug == false)
        {
            return;
        }
        GUI.color = Color.red;
        GUI.Label(new Rect(10, 30, 500, 25),
            "괴물: " + this.CurrentState + "  (거리 " + this.FlatDistance(this.player != null ? this.player.position : this.transform.position).ToString("0.0") + "m)");
    }
}
