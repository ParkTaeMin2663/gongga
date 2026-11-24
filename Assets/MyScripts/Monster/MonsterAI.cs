using UnityEngine;
using UnityEngine.AI;              // 강의 07장: NavMeshAgent
using UnityEngine.SceneManagement; // 강의 04장: SceneManager.LoadScene

/// <summary>
/// 괴물 「집주인」의 AI. 눈이 없어서 소리(NoiseSystem)와 아주 가까운 기척으로만 플레이어를 찾는다.
///
/// [상태]  순찰 → (소리 들음) 확인하러 감 → (플레이어가 가까움) 추격 → (놓침) 수색 → 순찰
///                                                            → (장롱에 숨음) 장롱 앞 서성이기 → 순찰
///   - 순찰(Patrol)      : patrolPoints를 차례로 천천히 돈다. 비어 있으면 제자리에 있다.
///   - 확인(Investigate) : 들은 소리 위치로 간다. 도착하면 수색.
///   - 추격(Chase)       : 플레이어에게 달려간다. 잡으면 재시작.
///   - 수색(Search)      : 그 자리에서 searchTime초 동안 두리번거린 뒤 순찰로 돌아간다.
///   - 서성이기(Lurk)    : 쫓던 플레이어가 장롱에 숨으면 그 장롱 앞을 천천히 왔다 갔다 하며 가끔 장롱을 쳐다본다.
///                         lurkTime초가 지나면 순찰로. 안에서 헐떡이면(소리) 시간이 처음부터 다시.
///   - 문 부수기(BashDoor): 앞에 닫힌 문(DraggableDoor)이 있으면
///         · 추격 중이거나 잠긴 문 → 쾅쾅 두드리다(bashTime초) 경첩째 뜯어 날린다
///         · 그 밖에는 문을 밀어서 연다. 밀어도 안 열리면(반대 방향 등) 두드려 부순다
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
    public enum State { Patrol, Investigate, Chase, Search, Lurk, BashDoor }

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
    [Tooltip("(11/24) 귀 밝기. 소리 크기(들리는 거리)에 이 값을 곱한 거리까지 듣는다. 1 = 소리 크기 그대로, 1.5 = 1.5배 멀리서도 들음 (예: 달리기 6m → 9m)")]
    public float hearingMultiplier = 1.5f;

    [Header("수색")]
    [Tooltip("도착한 뒤 두리번거리는 시간(초)")]
    public float searchTime = 4f;
    [Tooltip("두리번거릴 때 도는 속도(도/초)")]
    public float searchTurnSpeed = 60f;

    [Header("장롱 앞 서성이기")]
    [Tooltip("숨은 장롱 앞을 서성이는 시간(초)")]
    public float lurkTime = 8f;
    [Tooltip("장롱 앞 몇 m 안에서 서성일지")]
    public float lurkRadius = 1.8f;
    [Tooltip("서성일 때 걷는 속도(m/s)")]
    public float lurkSpeed = 0.9f;
    [Tooltip("한 자리에 멈춰 장롱을 쳐다보는 시간(초) 최소~최대")]
    public float lurkPauseMin = 0.8f;
    public float lurkPauseMax = 2f;

    [Header("문")]
    [Tooltip("앞으로 몇 m 안에 닫힌 문이 있으면 반응할지")]
    public float doorCheckDistance = 1.2f;
    [Tooltip("문을 밀 때 힘 (문 무게 20 기준)")]
    public float doorPushImpulse = 40f;
    [Tooltip("문을 밀고 이 시간(초)이 지나도 안 열리면 부수기 시작")]
    public float doorPushWait = 1.5f;
    [Tooltip("두드리는 간격(초)")]
    public float knockInterval = 0.7f;
    [Tooltip("두드리기 시작해서 부술 때까지 시간(초)")]
    public float bashTime = 3f;
    [Tooltip("문을 날려 버리는 힘")]
    public float doorBreakImpulse = 120f;
    [Tooltip("체크하면 순찰 중에도 문을 밀지 않고 항상 쾅쾅 두드려 부순다 (2층 연출용, Floor2Event가 켬)")]
    public bool alwaysBashDoors = false;

    [Header("순찰 지점에서 멈추기 (예: 2층 201호 장롱 앞에서 킁킁)")]
    [Tooltip("이 번호의 순찰 지점에 도착하면 pauseTime초 동안 멈춰 있는다. -1이면 안 멈춤")]
    public int pausePointIndex = -1;
    [Tooltip("멈춰 있는 시간(초)")]
    public float pauseTime = 4f;
    [Tooltip("멈춰 있는 동안 쳐다볼 곳 (예: 장롱). 비워 두면 그냥 서 있는다")]
    public Transform pauseLookAt;

    [Header("한 번만 지나가기 (2층 연출용)")]
    [Tooltip("체크하면 순찰 지점을 처음부터 끝까지 한 번만 지나가고, 마지막 지점에 도착하면 사라진다 (3층 괴물은 끔)")]
    public bool patrolOnce = false;

    [Header("발소리")]
    [Tooltip("괴물 몸에 붙은 Audio Source (3D). 비워 두면 발소리 없음")]
    public AudioSource footstepSound;
    [Tooltip("발소리 파일들 (Assets/Audio/Monster/monster_step_0~3). 걸음마다 무작위로 하나")]
    public AudioClip[] footstepClips;
    [Tooltip("몇 m 걸을 때마다 한 걸음 소리를 낼지 (클수록 느릿느릿 무거운 걸음)")]
    public float stepDistance = 1.1f;
    [Tooltip("천천히 걸을 때(순찰) 볼륨 / 달릴 때(추격) 볼륨")]
    [Range(0f, 1f)] public float walkStepVolume = 0.7f;
    [Range(0f, 1f)] public float chaseStepVolume = 1f;

    [Header("목소리 (11/18)")]
    [Tooltip("괴물 몸에 붙은 두 번째 Audio Source (3D, Loop). 숨소리·킁킁을 계속 낸다")]
    public AudioSource voiceSource;
    [Tooltip("평소 숨소리·그르렁 (반복)")]
    public AudioClip breathLoopClip;
    [Tooltip("멈춰서 냄새 맡을 때·두리번거릴 때 킁킁 (반복)")]
    public AudioClip sniffLoopClip;
    [Tooltip("숨소리 볼륨 / 킁킁 볼륨 (장롱 앞 킁킁은 크게)")]
    [Range(0f, 1f)] public float breathVolume = 0.7f;
    [Range(0f, 1f)] public float sniffVolume = 1f;
    [Tooltip("추격을 시작할 때 포효 (한 번)")]
    public AudioClip roarClip;
    [Tooltip("문을 부수기 시작할 때 으르렁 (한 번)")]
    public AudioClip bashGrowlClip;
    [Tooltip("포효를 다시 내기까지 최소 시간(초). 너무 자주 울부짖지 않게")]
    public float roarCooldown = 8f;
    [Tooltip("추격하는 동안 긴장 BGM을 튼다 (Player의 TensionMusic)")]
    public bool chaseMusic = true;

    [Header("애니메이션 ([강의 외] Animator, 11/18 사용자 허락)")]
    [Tooltip("괴물 모델의 Animator (비워 두면 자식에서 자동으로 찾음). 상태: Idle, Walk, Run, LookAround, BashDoor, Scream")]
    public Animator animator;
    [Tooltip("걷기 동작이 이 속도(m/s)일 때 발이 자연스럽다. 실제 속도에 맞춰 재생 속도를 바꾼다")]
    public float walkAnimSpeed = 1.0f;
    [Tooltip("뛰기 동작이 이 속도(m/s)일 때 발이 자연스럽다")]
    public float runAnimSpeed = 3.5f;
    [Tooltip("이 속도(m/s)보다 빠르면 뛰기 동작")]
    public float runThreshold = 2.6f;

    [Header("이동")]
    [Tooltip("목표까지 이 거리(m) 안이면 도착한 것으로 본다")]
    public float arriveDistance = 0.6f;

    [Header("디버그")]
    [Tooltip("체크하면 화면 왼쪽 위에 괴물 상태를 표시한다")]
    public bool showDebug = true;

    /// <summary>지금 상태. B의 연출이나 소리(괴물 울음 등)에서 읽을 수 있다.</summary>
    public State CurrentState { get; private set; }

    /// <summary>patrolOnce일 때 마지막 지점까지 가서 사라졌는지. 2층 연출(Floor2Event)이 이걸 보고 셔터를 연다.</summary>
    public bool IsRouteFinished { get; private set; }

    // ----- Start()에서 한 번만 찾아 저장 (강의 04장) -----
    private NavMeshAgent agent;
    private Transform player;
    private PlayerHide playerHide;
    private PlayerDeath playerDeath;
    private NoiseSystem noiseSystem;

    private int lastNoiseCount;      // 마지막으로 확인한 소리 번호 (바뀌면 새 소리)
    private Vector3 targetPosition;  // 지금 가려는 곳
    private int patrolIndex;         // 다음 순찰 지점 번호
    private float searchTimer;       // 수색한 시간 / 서성인 시간

    // 문
    private DraggableDoor targetDoor;   // 밀거나 부수는 중인 문
    private float doorTimer;            // 민 뒤 / 두드리기 시작한 뒤 지난 시간
    private float knockTimer;           // 다음 두드림까지
    private State stateBeforeDoor;      // 문 일이 끝나면 돌아갈 상태

    // 서성이기
    private Vector3 lurkCenter;      // 장롱 앞 (ExitPoint)
    private Vector3 lurkLookAt;      // 쳐다볼 곳 (장롱)
    private float lurkPauseTimer;    // 남은 멈춤 시간
    private bool lurkMoving;         // 지금 걷는 중인지

    // 순찰 지점에서 멈추기
    private bool isPausing;          // 지금 멈춰 있는 중인지
    private float pauseTimer;        // 멈춘 시간

    // 발소리
    private Vector3 lastStepPosition;  // 마지막 발소리를 낸 자리
    private float stepMoved;           // 그 뒤로 걸은 거리

    // 애니메이션
    private string currentAnim = "";   // 지금 재생 중인 동작 이름

    // 목소리·음악
    private float lastRoarTime = -100f;
    private TensionMusic tensionMusic;
    private bool isChaseMusicOn;

    private void Start()
    {
        this.agent = this.GetComponent<NavMeshAgent>();
        if (this.agent == null)
        {
            Debug.LogError("[MonsterAI] Nav Mesh Agent가 없습니다. Add Component로 추가해 주세요.");
        }

        this.lastStepPosition = this.transform.position;

        if (this.animator == null)
        {
            this.animator = this.GetComponentInChildren<Animator>();   // 강의 04장 계층구조: 자식 모델의 Animator
        }
        if (this.animator != null)
        {
            this.animator.applyRootMotion = false;   // 위치는 NavMeshAgent가 정한다 (동작은 제자리에서만)
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
            this.playerDeath = playerObject.GetComponent<PlayerDeath>();
            this.tensionMusic = playerObject.GetComponent<TensionMusic>();
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
        this.CheckDoorAhead();
        this.UpdateFootsteps();
        this.UpdateAnimation();
        this.UpdateVoice();

        switch (this.CurrentState)
        {
            case State.Patrol:      this.UpdatePatrol(); break;
            case State.Investigate: this.UpdateInvestigate(); break;
            case State.Chase:       this.UpdateChase(); break;
            case State.Search:      this.UpdateSearch(); break;
            case State.Lurk:        this.UpdateLurk(); break;
            case State.BashDoor:    this.UpdateBashDoor(); break;
        }

        // 어느 상태든 플레이어가 바로 옆에 있으면(숨지 않았으면) 기척을 느끼고 추격
        if (this.CurrentState != State.Chase && this.CurrentState != State.BashDoor && this.CanSensePlayer(this.senseDistance))
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

        // 멈춰 있는 중 (예: 장롱 앞에서 킁킁)
        if (this.isPausing)
        {
            this.Stop();
            if (this.pauseLookAt != null)
            {
                // 강의 07장 LookAt: 높이를 맞춰서 고개가 숙여지지 않게
                Vector3 look = this.pauseLookAt.position;
                look.y = this.transform.position.y;
                this.transform.LookAt(look);
            }
            this.pauseTimer = this.pauseTimer + Time.deltaTime;
            if (this.pauseTimer >= this.pauseTime)
            {
                this.isPausing = false;
                this.NextPatrolPoint();
            }
            return;
        }

        Transform point = this.patrolPoints[this.patrolIndex];
        if (this.MoveTowards(point.position, this.patrolSpeed))
        {
            if (this.patrolIndex == this.pausePointIndex)
            {
                // 이 지점에서는 잠깐 멈춘다
                this.isPausing = true;
                this.pauseTimer = 0f;
                return;
            }
            this.NextPatrolPoint();
        }
    }

    // 다음 순찰 지점으로. 마지막이면 처음으로 (patrolOnce면 사라진다)
    private void NextPatrolPoint()
    {
        if (this.patrolOnce && this.patrolIndex >= this.patrolPoints.Length - 1)
        {
            if (this.showDebug) Debug.Log("[괴물] 길 끝까지 감 → 사라짐");
            this.IsRouteFinished = true;
            this.Stop();
            this.gameObject.SetActive(false);   // 강의 04장 SetActive
            return;
        }
        this.patrolIndex = (this.patrolIndex + 1) % this.patrolPoints.Length;
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
        // 장롱에 숨었으면 놓친다 → 그 장롱 앞을 서성인다
        if (this.IsPlayerHidden())
        {
            this.StartLurk();
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

    // ---------------- 문 ----------------

    // 걸어가는 방향 바로 앞에 닫힌 문이 있는지 본다 (강의 07장 Physics.Raycast)
    private void CheckDoorAhead()
    {
        if (this.CurrentState == State.BashDoor || this.CurrentState == State.Search || this.agent.velocity.magnitude < 0.1f)
        {
            return;
        }

        RaycastHit hit;
        Vector3 origin = this.transform.position + Vector3.up * 1.2f;   // 가슴 높이
        if (Physics.Raycast(origin, this.transform.forward, out hit, this.doorCheckDistance) == false)
        {
            return;
        }

        DraggableDoor door = hit.collider.GetComponentInParent<DraggableDoor>();
        if (door == null || door.IsBroken || door.IsClosed == false)
        {
            return;
        }

        if (this.CurrentState == State.Chase || door.isLocked || this.alwaysBashDoors)
        {
            this.StartDoor(door, true);    // 쫓는 중이거나 잠겼거나 2층 연출이면 바로 부수기
        }
        else
        {
            this.StartDoor(door, false);   // 평소에는 밀어서 열기
        }
    }

    private void StartDoor(DraggableDoor door, bool bash)
    {
        this.stateBeforeDoor = this.CurrentState;
        this.targetDoor = door;
        this.ChangeState(State.BashDoor);
        this.doorTimer = bash ? 0f : -this.doorPushWait;   // 음수 = 아직 밀어 보는 중
        this.knockTimer = 0f;

        if (bash && this.bashGrowlClip != null)
        {
            AudioSource.PlayClipAtPoint(this.bashGrowlClip, this.transform.position + Vector3.up * 1.6f);   // 강의 07장: 으르렁
        }

        if (bash == false)
        {
            door.PushOpen(this.transform.forward, this.doorPushImpulse);
        }
    }

    private void UpdateBashDoor()
    {
        this.Stop();

        // 문이 사라졌거나 열렸으면(밀어서 열림, 플레이어가 엶) → 하던 일로
        if (this.targetDoor == null || this.targetDoor.IsBroken || this.targetDoor.IsClosed == false)
        {
            this.FinishDoor();
            return;
        }

        // 문 쪽을 본다 (강의 07장 LookAt, 높이 고정)
        Vector3 look = this.targetDoor.transform.position;
        look.y = this.transform.position.y;
        this.transform.LookAt(look);

        this.doorTimer = this.doorTimer + Time.deltaTime;
        if (this.doorTimer < 0f)
        {
            return;   // 밀어 놓고 열리길 기다리는 중
        }

        // 쾅… 쾅… (knockInterval마다)
        this.knockTimer = this.knockTimer - Time.deltaTime;
        if (this.knockTimer <= 0f)
        {
            this.knockTimer = this.knockInterval;
            this.targetDoor.Knock();
        }

        // 다 두드렸으면 부순다
        if (this.doorTimer >= this.bashTime)
        {
            this.targetDoor.Break(this.transform.forward, this.doorBreakImpulse);
            if (this.noiseSystem != null)
            {
                this.lastNoiseCount = this.noiseSystem.NoiseCount;   // 자기가 낸 부서지는 소리는 무시
            }
            this.FinishDoor();
        }
    }

    private void FinishDoor()
    {
        this.targetDoor = null;
        State back = this.stateBeforeDoor == State.BashDoor ? State.Patrol : this.stateBeforeDoor;
        this.ChangeState(back);
    }

    // 쫓던 플레이어가 숨은 장롱 앞으로 가서 서성이기 시작
    private void StartLurk()
    {
        HideSpot spot = this.playerHide != null ? this.playerHide.CurrentSpot : null;
        if (spot == null || spot.exitPoint == null)
        {
            this.ChangeState(State.Investigate);   // 장롱을 모르면 예전처럼 마지막 위치 확인
            return;
        }

        this.lurkCenter = spot.exitPoint.position;
        this.lurkLookAt = spot.transform.position;
        this.targetPosition = this.lurkCenter;   // 처음엔 장롱 바로 앞으로
        this.lurkMoving = true;
        this.ChangeState(State.Lurk);
    }

    private void UpdateLurk()
    {
        this.searchTimer = this.searchTimer + Time.deltaTime;
        if (this.searchTimer >= this.lurkTime)
        {
            this.ChangeState(State.Patrol);   // 포기하고 순찰로
            return;
        }

        if (this.lurkMoving)
        {
            // 처음 한 번은 조금 빠르게 장롱 앞까지, 그다음부터는 천천히
            float speed = this.searchTimer < 2f ? this.investigateSpeed : this.lurkSpeed;
            if (this.MoveTowards(this.targetPosition, speed))
            {
                // 도착 → 멈춰서 장롱을 쳐다본다 (강의 03장 Random.Range)
                this.lurkMoving = false;
                this.lurkPauseTimer = Random.Range(this.lurkPauseMin, this.lurkPauseMax);
            }
        }
        else
        {
            this.Stop();

            // 강의 07장 LookAt: 높이를 맞춰서 고개가 숙여지지 않게
            Vector3 look = this.lurkLookAt;
            look.y = this.transform.position.y;
            this.transform.LookAt(look);

            this.lurkPauseTimer = this.lurkPauseTimer - Time.deltaTime;
            if (this.lurkPauseTimer <= 0f)
            {
                // 장롱 앞 lurkRadius 안의 아무 곳으로 몇 걸음 (강의 03장 Random.Range)
                Vector3 offset = new Vector3(Random.Range(-this.lurkRadius, this.lurkRadius), 0f, Random.Range(-this.lurkRadius, this.lurkRadius));
                this.targetPosition = this.lurkCenter + offset;
                this.lurkMoving = true;
            }
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

        if (this.CurrentState == State.BashDoor)
        {
            return;   // 문 부수는 중에는 다른 소리에 신경 쓰지 않는다
        }

        // 다른 층에서 난 소리는 무시한다 (괴물은 자기 층에서만 움직인다)
        // (11/24) 소리 크기 × 귀 밝기(hearingMultiplier) 안이면 들린다 (강의 03장 Vector3 거리)
        float hearDistance = this.noiseSystem.LastLoudness * this.hearingMultiplier;
        bool canHear = Vector3.Distance(this.transform.position, this.noiseSystem.LastPosition) <= hearDistance;
        if (canHear == false || this.IsSameFloor(this.noiseSystem.LastPosition) == false)
        {
            return;
        }

        if (this.CurrentState == State.Chase)
        {
            return;   // 이미 쫓는 중
        }

        // 서성이는 중에 장롱 쪽에서 소리(헐떡임 등)가 나면 → 더 오래 서성인다
        if (this.CurrentState == State.Lurk && Vector3.Distance(this.noiseSystem.LastPosition, this.lurkCenter) < 3f)
        {
            this.searchTimer = 0f;
            if (this.showDebug) Debug.Log("[괴물] 장롱 쪽에서 소리… 더 머문다");
            return;
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
        // 추격 시작: 포효 + 긴장 BGM / 추격이 끝나 순찰로 돌아가면 BGM을 서서히 줄인다
        if (next == State.Chase && this.CurrentState != State.Chase)
        {
            if (this.roarClip != null && Time.time - this.lastRoarTime > this.roarCooldown)
            {
                this.lastRoarTime = Time.time;
                AudioSource.PlayClipAtPoint(this.roarClip, this.transform.position + Vector3.up * 1.6f);   // 강의 07장
            }
            if (this.chaseMusic && this.tensionMusic != null && this.isChaseMusicOn == false)
            {
                this.isChaseMusicOn = true;
                this.tensionMusic.PlayChase();
            }
        }
        if (next == State.Patrol && this.isChaseMusicOn)
        {
            this.isChaseMusicOn = false;
            this.tensionMusic.StopChase();
        }

        this.CurrentState = next;
        this.searchTimer = 0f;
        this.isPausing = false;   // 멈춰 있다가 소리를 들으면 바로 움직인다 (돌아오면 그 지점에서 다시 멈춤)
    }

    // 숨소리/킁킁: 멈춰서 냄새 맡는 상태면 킁킁, 아니면 숨소리 (강의 03장 AudioSource clip·Loop·Play)
    private void UpdateVoice()
    {
        if (this.voiceSource == null)
        {
            return;
        }
        bool sniffing = this.CurrentState == State.Search || this.CurrentState == State.Lurk || this.isPausing;
        AudioClip want = sniffing && this.sniffLoopClip != null ? this.sniffLoopClip : this.breathLoopClip;
        if (want != null && (this.voiceSource.clip != want || this.voiceSource.isPlaying == false))
        {
            this.voiceSource.clip = want;
            this.voiceSource.loop = true;
            // 킁킁은 처음부터 바로 들리게, 숨소리는 매번 다른 곳부터 (같은 소리 반복처럼 안 들리게)
            this.voiceSource.time = sniffing ? 0f : Random.Range(0f, want.length * 0.8f);
            this.voiceSource.volume = sniffing ? this.sniffVolume : this.breathVolume;
            this.voiceSource.Play();
        }
    }

    // [강의 외] Animator: 상태와 실제 속도를 보고 동작을 고른다. 같은 동작이면 다시 틀지 않는다.
    private void UpdateAnimation()
    {
        if (this.animator == null)
        {
            return;
        }

        float speed = this.agent.velocity.magnitude;
        string next;
        float playSpeed = 1f;

        if (this.CurrentState == State.BashDoor && this.doorTimer >= 0f)
        {
            next = "BashDoor";   // 쾅쾅 두드리는 중
        }
        else if (speed > this.runThreshold)
        {
            next = "Run";
            playSpeed = speed / this.runAnimSpeed;
        }
        else if (speed > 0.15f)
        {
            next = "Walk";
            playSpeed = speed / this.walkAnimSpeed;
        }
        else if (this.CurrentState == State.Search || this.CurrentState == State.Lurk || this.isPausing)
        {
            next = "LookAround";   // 두리번, 장롱 앞 킁킁
        }
        else
        {
            next = "Idle";
        }

        this.animator.speed = Mathf.Clamp(playSpeed, 0.5f, 1.8f);
        this.PlayAnim(next);
    }

    // [강의 외] Animator.CrossFade: 0.25초 동안 부드럽게 다음 동작으로 넘어간다
    private void PlayAnim(string animName)
    {
        if (this.animator == null || animName == this.currentAnim)
        {
            return;
        }
        this.currentAnim = animName;
        this.animator.CrossFade(animName, 0.25f);
    }

    // 걸은 거리가 stepDistance를 넘을 때마다 쿵 (강의 03장 AudioSource.Play, Pitch, Random.Range)
    private void UpdateFootsteps()
    {
        if (this.footstepSound == null || this.footstepClips == null || this.footstepClips.Length == 0)
        {
            return;
        }

        Vector3 moved = this.transform.position - this.lastStepPosition;
        moved.y = 0f;
        this.stepMoved = this.stepMoved + moved.magnitude;
        this.lastStepPosition = this.transform.position;

        if (this.stepMoved < this.stepDistance)
        {
            return;
        }
        this.stepMoved = 0f;

        bool fast = this.CurrentState == State.Chase || this.CurrentState == State.Investigate;
        this.footstepSound.clip = this.footstepClips[Random.Range(0, this.footstepClips.Length)];
        this.footstepSound.volume = fast ? this.chaseStepVolume : this.walkStepVolume;
        this.footstepSound.pitch = Random.Range(0.9f, 1.1f);
        this.footstepSound.Play();
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

    // 잡았다 → 괴물은 멈추고, 플레이어 사망 연출(PlayerDeath) 후 체크포인트에서 다시 시작
    private void CatchPlayer()
    {
        // 확인용: 잡힌 순간 플레이어가 장롱에 들어가는 중이었는지
        bool entering = this.playerHide != null && this.playerHide.IsInsideSpot;
        Debug.Log("[괴물] 플레이어를 잡았다! 거리 " + this.FlatDistance(this.player.position).ToString("0.00") + "m, 장롱 동작 중: " + entering);
        this.Stop();
        if (this.animator != null)
        {
            this.animator.speed = 1f;
            this.PlayAnim("Scream");   // 잡는 순간 비명 동작 (플레이어가 이쪽으로 고개를 돌림)
        }
        if (this.voiceSource != null)
        {
            this.voiceSource.Stop();   // 비명은 PlayerDeath가 낸다
        }
        this.enabled = false;   // 더 이상 움직이지 않음 (씬이 다시 열리면 처음 상태로)

        if (this.playerDeath != null)
        {
            this.playerDeath.Die(this.transform);
        }
        else
        {
            // PlayerDeath가 없으면 예전처럼 바로 재시작 (강의 04장)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
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
