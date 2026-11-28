using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// 괴물 「집주인」의 AI. 눈이 없어서 소리(NoiseSystem)와 아주 가까운 기척으로만 플레이어를 찾는다.
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

    [Header("계속 쫓기 (지하 최종 추격용, 11/26)")]
    [Tooltip("체크하면 소리를 내지 않아도 같은 층에 있는 플레이어를 계속 쫓는다 (멀어져도 놓치지 않음). 장롱에 숨으면 그때만 놓친다. 지하 Monster_B1만 켠다")]
    public bool alwaysChase = false;
    [Tooltip("계속 쫓기일 때 이 거리(m) 안이면 쫓는다. 지하 전체를 덮게 크게")]
    public float alwaysChaseDistance = 60f;

    [Header("계단실로 안 나가기 (3층 괴물용)")]
    [Tooltip("체크하면 이 x보다 동쪽(계단실 x 16~20)으로는 쫓아가지 않는다. 플레이어가 계단으로 도망치면 계단 앞에서 멈춰 두리번거리다 순찰로 돌아간다. 3층 Monster_3F만 켠다 (2층 괴물은 계단으로 사라져야 하므로 끔)")]
    public bool stayOffStairs = false;
    [Tooltip("괴물이 갈 수 있는 가장 동쪽 x. 복도 끝이 16이라 조금 안쪽")]
    public float stairsX = 15f;

    [Header("다음 구역에 가면 끄기 (3층 괴물용)")]
    [Tooltip("이 번호 이상의 체크포인트에 도착하면(또는 거기서 다시 시작하면) 괴물이 꺼진다. 지하 체크포인트가 4라서 Monster_3F는 4. 0이면 안 끔. 지하에서 3층 괴물이 문 부수는 소리가 들리던 문제")]
    public int offFromCheckpoint = 0;

    [Header("발소리")]
    [Tooltip("괴물 몸에 붙은 Audio Source (3D). 비워 두면 발소리 없음")]
    public AudioSource footstepSound;
    [Tooltip("발소리 파일들 (Assets/Audio/Monster/monster_step_0~3). 걸음마다 무작위로 하나")]
    public AudioClip[] footstepClips;
    [Tooltip("(11/24) 플레이어가 장롱에 숨어 있을 때 발소리 Min Distance(m). 크게 하면 멀리서도 또렷하게 들린다 (평소 값은 Audio Source의 Min Distance)")]
    public float hiddenStepMinDistance = 10f;
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

    [Header("애니메이션")]
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

    public State CurrentState { get; private set; }

    public bool IsRouteFinished { get; private set; }

    private NavMeshAgent agent;   // 강의 07장: NavMeshAgent
    private Transform player;
    private PlayerHide playerHide;
    private PlayerDeath playerDeath;
    private NoiseSystem noiseSystem;

    private int lastNoiseCount;
    private Vector3 targetPosition;
    private int patrolIndex;
    private float searchTimer;

    private DraggableDoor targetDoor;
    private float doorTimer;
    private float knockTimer;
    private State stateBeforeDoor;

    private Vector3 lurkCenter;
    private Vector3 lurkLookAt;
    private float lurkPauseTimer;
    private bool lurkMoving;

    private bool isPausing;
    private float pauseTimer;

    private Vector3 lastStepPosition;
    private float stepMoved;

    private string currentAnim = "";

    private float lastRoarTime = -100f;
    private TensionMusic tensionMusic;
    private bool isChaseMusicOn;
    private float normalStepMinDistance = -1f;

    private void OnDisable()
    {
        if (this.isChaseMusicOn && this.tensionMusic != null)
        {
            this.isChaseMusicOn = false;
            this.tensionMusic.StopChase();
        }
    }

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
            this.animator = this.GetComponentInChildren<Animator>();
        }
        if (this.animator != null)
        {
            this.animator.applyRootMotion = false;
        }

        Rigidbody rb = this.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;   // 강의 03장: Rigidbody Is Kinematic
        }

        GameObject playerObject = GameObject.Find("Player");   // 강의 04장: GameObject.Find (Start에서 한 번)
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
            this.lastNoiseCount = this.noiseSystem.NoiseCount;
        }
        else
        {
            Debug.LogWarning("[MonsterAI] 씬에 NoiseSystem이 없어서 소리를 못 듣습니다.");
        }

        this.ChangeState(State.Patrol);
    }

    private void Update()
    {
        if (this.offFromCheckpoint > 0 && Checkpoint.savedOrder >= this.offFromCheckpoint)
        {
            this.gameObject.SetActive(false);
            return;
        }

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

        float senseRange = this.alwaysChase ? this.alwaysChaseDistance : this.senseDistance;
        if (this.CurrentState != State.Chase && this.CurrentState != State.BashDoor && (this.alwaysChase == false || this.CurrentState != State.Lurk) && this.CanSensePlayer(senseRange))
        {
            this.ChangeState(State.Chase);
        }
    }

    private void UpdatePatrol()
    {
        if (this.patrolPoints == null || this.patrolPoints.Length == 0)
        {
            this.Stop();
            return;
        }

        if (this.isPausing)
        {
            this.Stop();
            if (this.pauseLookAt != null)
            {
                Vector3 look = this.pauseLookAt.position;
                look.y = this.transform.position.y;
                this.transform.LookAt(look);   // 강의 07장: transform.LookAt
            }
            this.pauseTimer = this.pauseTimer + Time.deltaTime;   // 강의 03장: Time.deltaTime
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
                this.isPausing = true;
                this.pauseTimer = 0f;
                return;
            }
            this.NextPatrolPoint();
        }
    }

    private void NextPatrolPoint()
    {
        if (this.patrolOnce && this.patrolIndex >= this.patrolPoints.Length - 1)
        {
            if (this.showDebug) Debug.Log("[괴물] 길 끝까지 감 → 사라짐");
            this.IsRouteFinished = true;
            this.Stop();
            this.gameObject.SetActive(false);
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
        if (this.IsPlayerHidden())
        {
            this.StartLurk();
            return;
        }

        float distance = this.FlatDistance(this.player.position);

        if (distance <= this.catchDistance && this.IsSameFloor(this.player.position))
        {
            this.CatchPlayer();
            return;
        }

        float lose = this.alwaysChase ? this.alwaysChaseDistance : this.loseDistance;
        if (distance > lose || this.IsSameFloor(this.player.position) == false || this.IsInStairs(this.player.position))
        {
            this.ChangeState(State.Investigate);
            return;
        }

        this.targetPosition = this.player.position;
        this.MoveTowards(this.targetPosition, this.chaseSpeed);
    }

    private void UpdateSearch()
    {
        this.Stop();

        this.transform.Rotate(0f, this.searchTurnSpeed * Time.deltaTime, 0f);   // 강의 03장: transform.Rotate

        this.searchTimer = this.searchTimer + Time.deltaTime;
        if (this.searchTimer >= this.searchTime)
        {
            this.ChangeState(State.Patrol);
        }
    }

    private void CheckDoorAhead()
    {
        if (this.CurrentState == State.BashDoor || this.CurrentState == State.Search || this.agent.velocity.magnitude < 0.1f)
        {
            return;
        }

        RaycastHit hit;
        Vector3 origin = this.transform.position + Vector3.up * 1.2f;
        if (Physics.Raycast(origin, this.transform.forward, out hit, this.doorCheckDistance) == false)   // 강의 07장: Physics.Raycast
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
            this.StartDoor(door, true);
        }
        else
        {
            this.StartDoor(door, false);
        }
    }

    private void StartDoor(DraggableDoor door, bool bash)
    {
        this.stateBeforeDoor = this.CurrentState;
        this.targetDoor = door;
        this.ChangeState(State.BashDoor);
        this.doorTimer = bash ? 0f : -this.doorPushWait;
        this.knockTimer = 0f;

        if (bash && this.bashGrowlClip != null)
        {
            AudioSource.PlayClipAtPoint(this.bashGrowlClip, this.transform.position + Vector3.up * 1.6f);   // 강의 07장: AudioSource.PlayClipAtPoint
        }

        if (bash == false)
        {
            door.PushOpen(this.transform.forward, this.doorPushImpulse);
        }
    }

    private void UpdateBashDoor()
    {
        this.Stop();

        if (this.targetDoor == null || this.targetDoor.IsBroken || this.targetDoor.IsClosed == false)
        {
            this.FinishDoor();
            return;
        }

        Vector3 look = this.targetDoor.transform.position;
        look.y = this.transform.position.y;
        this.transform.LookAt(look);

        this.doorTimer = this.doorTimer + Time.deltaTime;
        if (this.doorTimer < 0f)
        {
            return;
        }

        this.knockTimer = this.knockTimer - Time.deltaTime;
        if (this.knockTimer <= 0f)
        {
            this.knockTimer = this.knockInterval;
            this.targetDoor.Knock();
        }

        if (this.doorTimer >= this.bashTime)
        {
            this.targetDoor.Break(this.transform.forward, this.doorBreakImpulse);
            if (this.noiseSystem != null)
            {
                this.lastNoiseCount = this.noiseSystem.NoiseCount;
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

    private void StartLurk()
    {
        HideSpot spot = this.playerHide != null ? this.playerHide.CurrentSpot : null;
        if (spot == null || spot.exitPoint == null)
        {
            this.ChangeState(State.Investigate);
            return;
        }

        this.lurkCenter = spot.exitPoint.position;
        this.lurkLookAt = spot.transform.position;
        this.targetPosition = this.lurkCenter;
        this.lurkMoving = true;
        this.ChangeState(State.Lurk);
    }

    private void UpdateLurk()
    {
        this.searchTimer = this.searchTimer + Time.deltaTime;
        if (this.searchTimer >= this.lurkTime)
        {
            this.ChangeState(State.Patrol);
            return;
        }

        if (this.lurkMoving)
        {
            float speed = this.searchTimer < 2f ? this.investigateSpeed : this.lurkSpeed;
            if (this.MoveTowards(this.targetPosition, speed))
            {
                this.lurkMoving = false;
                this.lurkPauseTimer = Random.Range(this.lurkPauseMin, this.lurkPauseMax);   // 강의 03장: Random.Range
            }
        }
        else
        {
            this.Stop();

            Vector3 look = this.lurkLookAt;
            look.y = this.transform.position.y;
            this.transform.LookAt(look);

            this.lurkPauseTimer = this.lurkPauseTimer - Time.deltaTime;
            if (this.lurkPauseTimer <= 0f)
            {
                Vector3 offset = new Vector3(Random.Range(-this.lurkRadius, this.lurkRadius), 0f, Random.Range(-this.lurkRadius, this.lurkRadius));
                this.targetPosition = this.lurkCenter + offset;
                this.lurkMoving = true;
            }
        }
    }

    private void ListenForNoise()
    {
        if (this.noiseSystem == null || this.noiseSystem.NoiseCount == this.lastNoiseCount)
        {
            return;
        }
        this.lastNoiseCount = this.noiseSystem.NoiseCount;

        if (this.CurrentState == State.BashDoor)
        {
            return;
        }

        float hearDistance = this.noiseSystem.LastLoudness * this.hearingMultiplier;
        bool canHear = Vector3.Distance(this.transform.position, this.noiseSystem.LastPosition) <= hearDistance;
        if (canHear == false || this.IsSameFloor(this.noiseSystem.LastPosition) == false)
        {
            return;
        }

        if (this.CurrentState == State.Chase)
        {
            return;
        }

        if (this.CurrentState == State.Lurk && Vector3.Distance(this.noiseSystem.LastPosition, this.lurkCenter) < 3f)
        {
            this.searchTimer = 0f;
            if (this.showDebug) Debug.Log("[괴물] 장롱 쪽에서 소리… 더 머문다");
            return;
        }

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

    private void ChangeState(State next)
    {
        if (this.showDebug && next != this.CurrentState)
        {
            Debug.Log("[괴물] " + this.CurrentState + " → " + next);
        }
        if (next == State.Chase && this.CurrentState != State.Chase)
        {
            if (this.roarClip != null && Time.time - this.lastRoarTime > this.roarCooldown)
            {
                this.lastRoarTime = Time.time;
                AudioSource.PlayClipAtPoint(this.roarClip, this.transform.position + Vector3.up * 1.6f);
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
        this.isPausing = false;
    }

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
            this.voiceSource.time = sniffing ? 0f : Random.Range(0f, want.length * 0.8f);
            this.voiceSource.volume = sniffing ? this.sniffVolume : this.breathVolume;
            this.voiceSource.Play();   // 강의 03장: AudioSource.Play
        }
    }

    // [강의 외] Animator: 상태와 속도에 맞는 동작 고르기
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
            next = "BashDoor";
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
            next = "LookAround";
        }
        else
        {
            next = "Idle";
        }

        this.animator.speed = Mathf.Clamp(playSpeed, 0.5f, 1.8f);
        this.PlayAnim(next);
    }

    // [강의 외] Animator.CrossFade: 동작을 부드럽게 바꾸기
    private void PlayAnim(string animName)
    {
        if (this.animator == null || animName == this.currentAnim)
        {
            return;
        }
        this.currentAnim = animName;
        this.animator.CrossFade(animName, 0.25f);
    }

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
        if (this.normalStepMinDistance < 0f)
        {
            this.normalStepMinDistance = this.footstepSound.minDistance;
        }
        this.footstepSound.minDistance = this.IsPlayerHidden() ? this.hiddenStepMinDistance : this.normalStepMinDistance;
        this.footstepSound.clip = this.footstepClips[Random.Range(0, this.footstepClips.Length)];
        this.footstepSound.volume = fast ? this.chaseStepVolume : this.walkStepVolume;
        this.footstepSound.pitch = Random.Range(0.9f, 1.1f);   // 강의 03장: AudioSource Pitch
        this.footstepSound.Play();
    }

    private bool MoveTowards(Vector3 target, float speed)
    {
        this.agent.isStopped = false;
        this.agent.speed = speed;

        if (this.stayOffStairs && target.x > this.stairsX)
        {
            target.x = this.stairsX;
        }

        if ((this.agent.destination - target).magnitude > 0.3f || this.agent.hasPath == false)
        {
            this.agent.SetDestination(target);   // 강의 07장: NavMeshAgent.SetDestination
        }

        if (this.agent.pathPending)
        {
            return false;
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
               && this.IsInStairs(this.player.position) == false
               && this.FlatDistance(this.player.position) <= distance;
    }

    private bool IsPlayerHidden()
    {
        return this.playerHide != null && this.playerHide.IsHidden;
    }

    private bool IsInStairs(Vector3 position)
    {
        return this.stayOffStairs && position.x > this.stairsX;
    }

    private bool IsSameFloor(Vector3 position)
    {
        return Mathf.Abs(position.y - this.transform.position.y) < this.sameFloorHeight;
    }

    private float FlatDistance(Vector3 position)
    {
        Vector3 diff = position - this.transform.position;
        diff.y = 0f;
        return diff.magnitude;
    }

    private void CatchPlayer()
    {
        bool entering = this.playerHide != null && this.playerHide.IsInsideSpot;
        Debug.Log("[괴물] 플레이어를 잡았다! 거리 " + this.FlatDistance(this.player.position).ToString("0.00") + "m, 장롱 동작 중: " + entering);
        this.Stop();
        if (this.animator != null)
        {
            this.animator.speed = 1f;
            this.PlayAnim("Scream");
        }
        if (this.voiceSource != null)
        {
            this.voiceSource.Stop();
        }
        this.enabled = false;

        if (this.playerDeath != null)
        {
            this.playerDeath.Die(this.transform);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);   // 강의 04장: SceneManager.LoadScene
        }
    }

    private void OnGUI()   // 강의 04장: OnGUI
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
