using System.Collections;          // 강의 05장: 코루틴
using UnityEngine;
using UnityEngine.AI;              // 강의 07장: NavMeshAgent

/// <summary>
/// 2장(2층) 연출. 차단기를 올리면 순서대로 진행된다.
/// (11/17 A 수정) 검은 실루엣 대신 **진짜 Monster 프리팹 사본(Monster_2F)** 이 정해진 길을 한 번 지나간다.
///
///   1) 차단기 레버(DraggableLever.IsOn)가 올라감
///      (11/24) 이때부터 "숨어라"까지 걷기·마우스가 멈추고 고개가 괴물 쪽으로 돌아가 고정된다 (lockViewDuringEvent)
///   2) 방화 셔터가 "끼이익" 올라가다가 중간에 걸려 멈춘다 (아직 못 지나감)
///   3) 센서등이 계단 쪽부터 복도 끝 쪽으로 하나씩 켜짐 → 복도 끝(204호 앞)에 집주인(Monster_2F)이 서 있다 (AI 꺼짐)
///   4) 손전등이 깜빡이다 꺼지고(PlayerFlashlight.FlickerOut) 센서등도 꺼짐
///   5) 화면에 "숨어라" → 괴물 AI(MonsterAI)가 켜져 순찰 지점을 한 번 지나간다 (patrolOnce)
///      202호 앞 → 201호 문 앞(닫혀 있으면 밀고, 안 열리면 부숨) → 201호 안 장롱 앞에서 킁킁(pausePointIndex)
///      → 다시 복도 → 계단에서 사라짐. 진짜 괴물과 똑같이 소리를 듣고, 숨지 않은 플레이어는 쫓아와 잡는다
///   6) 괴물이 사라지면(MonsterAI.IsRouteFinished) 셔터가 끝까지 올라감 → 3층으로 갈 수 있다
///
/// [붙이는 곳] SampleScene의 빈 오브젝트 "Floor2Event" (위치 상관없음)
/// [Inspector 연결] 아래 칸들. 비어 있는 칸은 그 단계를 건너뛴다(에러 없음).
/// [소리] NoiseSystem.Emit(위치, 20, "셔터") — 20m 안의 괴물이 듣는다 (CLAUDE.md 소리 규칙)
/// [다시 시작] Checkpoint.savedOrder가 skipFromCheckpoint 이상이면 연출을 끝난 상태로 시작한다 (셔터 열림, 실루엣 없음)
///            (11/18 A 추가) "숨어라"까지 본 뒤 2층에서 잡혀 체크포인트 2에서 다시 시작하면,
///            레버·센서등을 건너뛰고 셔터가 걸린 상태에서 바로 "숨어라" + 괴물이 다시 출발한다.
/// </summary>
public class Floor2Event : MonoBehaviour
{
    [Header("시작 조건")]
    [Tooltip("2층 분전함(Breaker 프리팹)의 자식 Lever를 드래그. 이 레버가 올라가면 연출이 시작된다")]
    public DraggableLever lever;

    [Tooltip("이 번호 이상의 체크포인트를 이미 지났으면 연출을 건너뛴다 (3 = 3층 체크포인트)")]
    public int skipFromCheckpoint = 3;

    [Tooltip("\"숨어라\"를 본 뒤 잡혀서 이 번호 체크포인트(2층)에서 다시 시작하면 바로 \"숨어라\"부터 다시 한다")]
    public int retryFromCheckpoint = 2;
    [Tooltip("다시 시작한 뒤 \"숨어라\"와 괴물 출발까지 기다리는 시간(초). 화면이 밝아지는 동안")]
    public float retryDelay = 1.5f;

    [Header("방화 셔터")]
    [Tooltip("3층 계단을 막는 셔터 Cube (부모 없이, Scale Y = 셔터 높이). 아래쪽부터 위로 말려 올라간다")]
    public Transform shutter;
    [Tooltip("처음에 올라가다 걸려 멈추는 높이(m) = 바닥과 셔터 사이 틈. 플레이어가 못 지나갈 만큼 낮게")]
    public float shutterJamHeight = 0.4f;
    [Tooltip("끝까지 올라가는 높이(m). 셔터 높이보다 크면 셔터 높이 - 0.1까지만 올라간다")]
    public float shutterOpenHeight = 2.6f;
    [Tooltip("셔터가 올라가는 속도(m/초)")]
    public float shutterSpeed = 0.25f;
    [Tooltip("셔터 끼이익 소리 (셔터에 붙인 Audio Source, Play On Awake 끔)")]
    public AudioSource shutterSound;

    [Header("센서등")]
    [Tooltip("복도 천장의 Point Light들. 계단 쪽 → 복도 끝(204호) 순서로 넣는다. 처음엔 꺼 둔다(Light 체크 해제)")]
    public Light[] sensorLights;
    [Tooltip("센서등이 하나씩 켜지는 간격(초)")]
    public float sensorInterval = 0.7f;
    [Tooltip("센서등이 켜질 때 딸깍 소리 (없어도 됨). 소리 파일을 드래그")]
    public AudioClip sensorClickClip;

    [Header("2층 괴물 (Monster 프리팹 사본)")]
    [Tooltip("2층용 Monster 사본(Monster_2F)을 드래그. 처음엔 꺼 둔다(체크 해제). 순찰 지점·Patrol Once·Pause 설정은 그 괴물의 MonsterAI에서")]
    public MonsterAI monster;

    [Tooltip("체크하면 2층 괴물이 201호 문을 열지 않고 쾅쾅 두드려 부수고 들어온다")]
    public bool monsterBashesDoors = true;

    [Tooltip("(예전 방식, 비워 두기) 움직이지 않는 실루엣. monster가 비어 있을 때만 잠깐 보였다가 사라진다")]
    public Transform silhouette;

    [Header("연출 중 1층으로 못 내려가게 (11/22)")]
    [Tooltip("레버를 올린 순간 켜지고, 괴물이 지나가 셔터가 열릴 때 꺼지는 투명 벽들. 1층으로 내려가는 계단 입구에 Box Collider만 있는 빈 오브젝트(StairBlock_2F)를 처음엔 꺼 두고 여기에 드래그")]
    public GameObject[] stairBlockers;

    [Header("레버를 올린 뒤 시점 고정 (11/24)")]
    [Tooltip("체크하면 레버를 올린 순간부터 \"숨어라\"까지 걷기·마우스가 멈추고, 고개가 괴물 쪽으로 돌아가 고정된다 (지하 동생 연출과 같은 방식)")]
    public bool lockViewDuringEvent = true;
    [Tooltip("바라볼 곳. 비워 두면 2층 괴물(Monster_2F)을 본다")]
    public Transform viewTarget;
    [Tooltip("바라볼 높이(m). 바라볼 곳의 발밑에서 이만큼 위 (괴물 가슴 높이)")]
    public float viewHeight = 1.3f;
    [Tooltip("고개가 괴물 쪽으로 돌아가는 시간(초)")]
    public float viewTurnTime = 1.2f;

    [Header("손전등")]
    [Tooltip("손전등이 깜빡이다 꺼지는 시간(초)")]
    public float flickerTime = 2.5f;

    [Header("안내 문구")]
    [Tooltip("화면에 띄울 힌트")]
    public string hideHint = "숨어라";
    [Tooltip("\"숨어라\" 글자 크기 (1080p 기준, 화면 크기에 맞춰 바뀜)")]
    public int hideHintSize = 120;

    [Header("시간 (초)")]
    [Tooltip("셔터가 걸린 뒤 센서등이 켜지기 시작할 때까지")]
    public float waitBeforeLights = 1.5f;
    [Tooltip("(안 씀, 11/18부터 아래 두 값을 씀) 예전: 괴물이 보인 뒤 손전등이 깜빡이기까지")]
    public float waitBeforeFlicker = 2.5f;
    [Tooltip("(안 씀) 예전: 손전등이 꺼진 뒤 괴물이 움직이기까지 — 지금은 꺼지자마자 \"숨어라\"")]
    public float waitBeforeCrawl = 1.5f;
    [Tooltip("마지막 센서등이 켜진 뒤 괴물이 걸어오기 시작할 때까지 (거의 바로)")]
    public float monsterStartDelay = 0.3f;
    [Tooltip("마지막 센서등이 켜진 뒤 손전등이 깜빡이기 시작할 때까지 (괴물이 다가오는 걸 볼 시간)")]
    public float flickerStartDelay = 1.5f;

    /// <summary>연출이 끝났는지 (셔터가 다 열림)</summary>
    public bool IsFinished { get; private set; }

    // 강의 07장 public static: 씬을 다시 불러와도 값이 남는다. "숨어라"까지 진행했는지 기억
    public static bool reachedHidePhase = false;

    private bool isStarted;
    private bool showHint;
    private bool isRespawn;   // 죽고 다시 시작한 씬인지 (Awake에서 저장)
    private Vector3 shutterClosedPosition;
    private Vector3 shutterClosedScale;
    private GUIStyle hintStyle;

    // 강의 04장: Find는 Start()에서 한 번만
    private Transform player;
    private PlayerHide playerHide;
    private PlayerFlashlight playerFlashlight;
    private PlayerDeath playerDeath;   // 잡혀서 죽는 중이면 "숨어라"를 숨긴다
    private TensionMusic tensionMusic; // "숨어라" 동안 긴장 BGM (Player 프리팹)
    private RoarScare roarScare;       // "숨어라" 때 포효 + 그 뒤 3분마다 포효 (Player 프리팹)
    private NoiseSystem noiseSystem;
    private PlayerMovement playerMovement;   // 시점 고정 동안 걷기 멈춤 (11/24)
    private PlayerLook playerLook;
    private PlayerInteract playerInteract;
    private Rigidbody playerBody;
    private bool isViewLocked;               // 시점 고정 중인지

    // 강의 05장 실행 순서: Awake는 모든 Start보다 먼저 불린다.
    // PlayerDeath.Start가 Checkpoint.isRespawning을 false로 되돌리기 전에 읽어 둔다.
    private void Awake()
    {
        this.isRespawn = Checkpoint.isRespawning;
        if (this.isRespawn == false)
        {
            reachedHidePhase = false;   // 처음부터 시작하는 게임
        }
    }

    private void Start()
    {
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            this.player = playerObject.transform;
            this.playerHide = playerObject.GetComponent<PlayerHide>();
            this.playerFlashlight = playerObject.GetComponent<PlayerFlashlight>();
            this.playerDeath = playerObject.GetComponent<PlayerDeath>();
            this.tensionMusic = playerObject.GetComponent<TensionMusic>();
            this.roarScare = playerObject.GetComponent<RoarScare>();
            this.playerMovement = playerObject.GetComponent<PlayerMovement>();
            this.playerLook = playerObject.GetComponent<PlayerLook>();
            this.playerInteract = playerObject.GetComponent<PlayerInteract>();
            this.playerBody = playerObject.GetComponent<Rigidbody>();
        }
        GameObject noiseObject = GameObject.Find("NoiseSystem");
        if (noiseObject != null)
        {
            this.noiseSystem = noiseObject.GetComponent<NoiseSystem>();
        }

        if (this.shutter != null)
        {
            this.shutterClosedPosition = this.shutter.position;
            this.shutterClosedScale = this.shutter.localScale;
        }

        // 처음 상태: 센서등 꺼짐, 괴물·실루엣 숨김, 계단 막이 꺼짐
        this.SetLights(false);
        this.SetBlockers(false);
        if (this.monster != null)
        {
            this.monster.gameObject.SetActive(false);
        }
        if (this.silhouette != null)
        {
            this.silhouette.gameObject.SetActive(false);
        }

        // 이미 3층 체크포인트를 지난 뒤 다시 시작한 경우 → 끝난 상태로
        if (Checkpoint.savedOrder >= this.skipFromCheckpoint)
        {
            this.isStarted = true;
            this.IsFinished = true;
            if (this.shutter != null)
            {
                float fullHeight = this.shutterClosedScale.y;
                this.SetShutterRaised(Mathf.Clamp(this.shutterOpenHeight, 0f, fullHeight - 0.1f), fullHeight,
                                      this.shutterClosedPosition.y + fullHeight / 2f);
            }
        }
        // "숨어라"까지 보고 2층에서 잡혀 체크포인트 2에서 다시 시작한 경우 → 바로 "숨어라"부터
        else if (this.isRespawn && reachedHidePhase && Checkpoint.savedOrder >= this.retryFromCheckpoint)
        {
            this.isStarted = true;   // 레버는 다시 안 봄
            if (this.shutter != null)
            {
                float fullHeight = this.shutterClosedScale.y;
                this.SetShutterRaised(Mathf.Clamp(this.shutterJamHeight, 0f, fullHeight - 0.1f), fullHeight,
                                      this.shutterClosedPosition.y + fullHeight / 2f);   // 걸린 셔터
            }
            this.StartCoroutine(this.RetryRoutine());   // 강의 05장
        }
    }

    // 다시 시작: 괴물이 204호 앞에 서 있고, 잠깐 뒤 "숨어라" + 출발
    private IEnumerator RetryRoutine()
    {
        this.SetBlockers(true);   // 다시 시작해도 1층 계단은 막혀 있다
        this.ShowMonster();
        this.StartViewLock();     // (11/24) 다시 시작해도 "숨어라"까지 괴물 쪽을 본다
        yield return new WaitForSeconds(this.retryDelay);
        this.StartMonsterMove();
        yield return this.StartCoroutine(this.HideRoutine());
    }

    private void Update()
    {
        if (this.isStarted || this.lever == null)
        {
            return;
        }

        // 레버가 올라간 순간 한 번만 시작
        if (this.lever.IsOn)
        {
            this.isStarted = true;
            this.StartCoroutine(this.EventRoutine());   // 강의 05장
        }
    }

    // 강의 05장 코루틴: 시간 순서대로 연출
    private IEnumerator EventRoutine()
    {
        // 1-1) 레버를 올린 순간 괴물이 복도 끝 어둠 속에 이미 서 있다 (센서등이 켜지며 드러남, 11/18)
        this.ShowMonster();
        this.SetBlockers(true);   // (11/22) 연출이 끝날 때까지 1층으로 못 내려간다 → 2층에서 숨어야 함
        this.StartViewLock();     // (11/24) "숨어라"까지 걷기·마우스 멈춤 + 괴물 쪽으로 고개 고정

        // 2) 셔터가 올라가다 걸린다
        if (this.shutter != null)
        {
            this.PlayShutterSound();
            this.Emit(this.shutter.position, 20f, "셔터");
            yield return this.StartCoroutine(this.MoveShutter(this.shutterJamHeight));
            this.StopShutterSound();
        }
        yield return new WaitForSeconds(this.waitBeforeLights);

        // 3) 센서등이 하나씩 켜진다 → 마지막 등 아래에 실루엣
        for (int i = 0; i < this.sensorLights.Length; i++)
        {
            if (this.sensorLights[i] == null)
            {
                continue;
            }
            this.sensorLights[i].enabled = true;
            if (this.sensorClickClip != null)
            {
                AudioSource.PlayClipAtPoint(this.sensorClickClip, this.sensorLights[i].transform.position);   // 강의 07장
            }
            yield return new WaitForSeconds(this.sensorInterval);
        }
        if (this.sensorLights.Length > 0 && this.sensorLights[this.sensorLights.Length - 1] != null)
        {
            this.Emit(this.sensorLights[this.sensorLights.Length - 1].transform.position, 20f, "센서등");
        }
        // 3-1) 괴물이 거의 바로 걸어오기 시작한다 (11/18: 가만히 서 있는 시간을 줄임)
        yield return new WaitForSeconds(this.monsterStartDelay);
        this.StartMonsterMove();
        yield return new WaitForSeconds(Mathf.Max(0f, this.flickerStartDelay - this.monsterStartDelay));

        // 4) 손전등이 깜빡이다 꺼지고, 센서등도 꺼진다
        if (this.playerFlashlight != null && this.playerFlashlight.hasFlashlight)
        {
            this.playerFlashlight.FlickerOut(this.flickerTime);
        }
        yield return new WaitForSeconds(this.flickerTime);
        this.SetLights(false);
        if (this.silhouette != null)
        {
            this.silhouette.gameObject.SetActive(false);   // (예전 방식) 어둠 속에서 사라진다
        }
        // 꺼지자마자 바로 "숨어라" (11/18: 빈 시간 없앰)
        yield return this.StartCoroutine(this.HideRoutine());
    }

    // 5)~6) "숨어라" → 괴물이 지나감 → 셔터 열림. 처음 연출과 다시 시작이 같이 쓴다
    private IEnumerator HideRoutine()
    {
        // 5) "숨어라" + 괴물 AI가 켜져 정해진 길을 한 번 지나간다
        reachedHidePhase = true;   // 여기서 잡히면 다음엔 바로 여기부터
        this.isViewLocked = false; // (11/24) "숨어라"와 동시에 다시 움직일 수 있다 (ViewLockRoutine이 풀어 줌)
        this.showHint = true;
        if (this.tensionMusic != null)
        {
            this.tensionMusic.PlayHide();   // 심장 소리 긴장 음악 시작 (서서히 커짐)
        }
        if (this.roarScare != null && this.monster != null)
        {
            this.roarScare.StartScare(this.monster.transform.position);   // 복도 끝에서 크게 포효 → 이후 약 3분마다
        }
        if (this.monster != null)
        {
            // 괴물이 길 끝에서 사라질 때까지 기다린다. 잡히면 씬이 다시 열리므로 여기서 끝난다
            while (this.monster.IsRouteFinished == false)
            {
                yield return null;
            }
        }

        // 6) 괴물이 사라지면 셔터가 끝까지 열린다
        this.showHint = false;
        this.SetBlockers(false);   // 1층 계단도 다시 열림
        if (this.tensionMusic != null)
        {
            this.tensionMusic.StopHide();   // 긴장 음악이 서서히 줄어든다 → "갔나?"
        }
        yield return new WaitForSeconds(1.5f);
        if (this.shutter != null)
        {
            this.PlayShutterSound();
            this.Emit(this.shutter.position, 20f, "셔터");
            yield return this.StartCoroutine(this.MoveShutter(this.shutterOpenHeight));
            this.StopShutterSound();
        }
        this.IsFinished = true;
    }

    // (11/24) 시점 고정 시작. "숨어라"(HideRoutine)에서 isViewLocked = false가 되면 풀린다
    private void StartViewLock()
    {
        if (this.lockViewDuringEvent == false || this.isViewLocked)
        {
            return;
        }
        this.isViewLocked = true;
        this.StartCoroutine(this.ViewLockRoutine());   // 강의 05장
    }

    // 걷기·마우스를 멈추고 고개를 괴물 쪽으로 천천히 돌린 뒤, "숨어라"까지 괴물을 계속 바라본다.
    // (BrotherEncounter의 동생 쪽으로 고개 돌리기와 같은 방식: Mathf.Atan2로 각도 → Mathf.LerpAngle로 천천히)
    private IEnumerator ViewLockRoutine()
    {
        this.SetPlayerFrozen(true);
        yield return null;   // 다시 시작한 경우 플레이어 위치·방향이 먼저 정해지도록 한 프레임 기다린다

        Transform target = this.viewTarget;
        if (target == null && this.monster != null)
        {
            target = this.monster.transform;
        }
        bool canTurn = target != null && this.playerLook != null && this.playerLook.cameraTransform != null;
        float startYaw = canTurn ? this.playerLook.Yaw : 0f;
        float startPitch = canTurn ? this.playerLook.Pitch : 0f;
        float t = 0f;

        while (this.isViewLocked)
        {
            if (this.playerDeath != null && this.playerDeath.IsDead)
            {
                yield break;   // 잡혔으면 PlayerDeath가 시점을 맡는다
            }

            if (canTurn)
            {
                // 괴물 가슴 쪽을 향하는 좌우·위아래 각도 (괴물이 걸어와도 계속 따라 본다)
                Vector3 dir = target.position + Vector3.up * this.viewHeight - this.playerLook.cameraTransform.position;
                float endYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                float flat = Mathf.Sqrt(dir.x * dir.x + dir.z * dir.z);   // 강의 06장 Mathf.Sqrt
                float endPitch = -Mathf.Atan2(dir.y, flat) * Mathf.Rad2Deg;   // 아래에 있으면 양수(아래를 봄)

                t = Mathf.Min(1f, t + Time.deltaTime / this.viewTurnTime);
                this.playerLook.IsLocked = true;   // 다른 스크립트가 풀어도 다시 잠근다
                this.playerLook.SetLook(Mathf.LerpAngle(startYaw, endYaw, t), Mathf.Lerp(startPitch, endPitch, t));
            }

            // 미끄러지지 않게 좌우 속도만 0 (떨어지는 속도는 그대로)
            if (this.playerBody != null)
            {
                Vector3 v = this.playerBody.linearVelocity;
                this.playerBody.linearVelocity = new Vector3(0f, v.y, 0f);
            }
            yield return null;
        }

        this.SetPlayerFrozen(false);
    }

    // 플레이어 걷기·잡기·시점을 멈추거나 다시 켠다 (강의 04장 enabled, BrotherEncounter와 같은 방식)
    private void SetPlayerFrozen(bool frozen)
    {
        if (frozen && this.playerInteract != null)
        {
            this.playerInteract.ReleaseGrab();   // 레버를 잡고 있었다면 놓는다
        }
        if (this.playerMovement != null) this.playerMovement.enabled = !frozen;
        if (this.playerInteract != null) this.playerInteract.enabled = !frozen;
        if (this.playerLook != null) this.playerLook.IsLocked = frozen;
    }

    // 투명 벽 켜기/끄기 (강의 04장 SetActive)
    private void SetBlockers(bool on)
    {
        if (this.stairBlockers == null)
        {
            return;
        }
        for (int i = 0; i < this.stairBlockers.Length; i++)
        {
            if (this.stairBlockers[i] != null)
            {
                this.stairBlockers[i].SetActive(on);
            }
        }
    }

    // 셔터 아래쪽을 height(m)만큼 말아 올린다. 위쪽 끝은 그대로 두고 세로 크기(localScale.y)만 줄인다
    // → 진짜 셔터처럼 천장 셔터함으로 감겨 들어가고, 3층 바닥을 뚫고 나오지 않는다. (강의 03장 localScale, Time.deltaTime)
    private IEnumerator MoveShutter(float height)
    {
        float fullHeight = this.shutterClosedScale.y;
        float target = Mathf.Clamp(height, 0f, fullHeight - 0.1f);   // 0.1m는 셔터함처럼 남겨 둔다
        float raised = fullHeight - this.shutter.localScale.y;
        float topY = this.shutterClosedPosition.y + fullHeight / 2f;   // 셔터 위쪽 끝 (Cube 기준점이 가운데)

        while (Mathf.Abs(raised - target) > 0.005f)
        {
            raised = Mathf.MoveTowards(raised, target, this.shutterSpeed * Time.deltaTime);
            this.SetShutterRaised(raised, fullHeight, topY);
            yield return null;   // 다음 프레임까지
        }
        this.SetShutterRaised(target, fullHeight, topY);
    }

    private void SetShutterRaised(float raised, float fullHeight, float topY)
    {
        float height = fullHeight - raised;
        Vector3 scale = this.shutterClosedScale;
        scale.y = height;
        this.shutter.localScale = scale;
        Vector3 position = this.shutterClosedPosition;
        position.y = topY - height / 2f;
        this.shutter.position = position;
    }

    // 괴물 AI 켜기 → MonsterAI.Start가 불리고 정해진 길로 걸어오기 시작
    private void StartMonsterMove()
    {
        if (this.monster != null)
        {
            this.SetMonsterAgent(true);    // (11/24) 꺼 뒀던 길찾기를 먼저 켜고
            this.SetMonsterPose(false);    // (11/24) 멈춰 둔 동작도 다시 움직이게
            this.monster.enabled = true;   // AI를 켠다 (MonsterAI.Start가 Agent를 쓰므로 이 순서)
        }
    }

    // 복도 끝에 괴물을 보이게 한다. AI는 아직 꺼 둔다 (가만히 서 있기만 함)
    private void ShowMonster()
    {
        if (this.monster != null)
        {
            this.monster.enabled = false;               // 컴포넌트 끄기 (켜지기 전이라 Start도 아직 안 불림)
            this.monster.alwaysBashDoors = this.monsterBashesDoors;   // 201호 문을 부수고 들어오게
            this.SetMonsterAgent(false);                // (11/24) 길찾기도 꺼서 출발 전에 조금도 미끄러지지 않게
            this.monster.gameObject.SetActive(true);    // 강의 04장 SetActive
            this.SetMonsterPose(true);                  // (11/24) 동작(Idle)도 멈춰 동상처럼 서 있게
        }
        else if (this.silhouette != null)
        {
            this.silhouette.gameObject.SetActive(true);
        }
    }

    // (11/24) 괴물의 NavMeshAgent 켜기/끄기 (강의 07장 NavMeshAgent, 04장 enabled)
    // 레버를 올린 순간 AI는 꺼져 있어도 Agent가 켜져 있으면 바닥(NavMesh)에 맞추느라 조금 움직일 수 있다 → 출발 전까지 꺼 둔다
    private void SetMonsterAgent(bool on)
    {
        NavMeshAgent agent = this.monster.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = on;
        }
    }

    // (11/24) 괴물 동작 멈추기/다시 움직이기 ([강의 외] Animator, 11/18 허락 — Animator.speed)
    // 괴물 모델의 기본 Idle 동작은 몸이 앞뒤로 조금 움직이는 동작이라, AI가 꺼져 있어도 걸어오다 멈추는 것처럼 보였다
    // → 출발 전까지 Animator 재생 속도를 0으로 해서 첫 자세 그대로 서 있게 한다. 출발하면 MonsterAI가 속도를 다시 정한다
    private void SetMonsterPose(bool freeze)
    {
        Animator animator = this.monster.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.speed = freeze ? 0f : 1f;
        }
    }

    private void SetLights(bool on)
    {
        for (int i = 0; i < this.sensorLights.Length; i++)
        {
            if (this.sensorLights[i] != null)
            {
                this.sensorLights[i].enabled = on;
            }
        }
    }

    private void PlayShutterSound()
    {
        if (this.shutterSound != null)
        {
            this.shutterSound.Play();
        }
    }

    private void StopShutterSound()
    {
        if (this.shutterSound != null)
        {
            this.shutterSound.Stop();
        }
    }

    private void Emit(Vector3 position, float loudness, string sourceName)
    {
        if (this.noiseSystem != null)
        {
            this.noiseSystem.Emit(position, loudness, sourceName);
        }
    }

    // 강의 04장 OnGUI: 화면 가운데 위쪽에 "숨어라" (천천히 깜빡임)
    private void OnGUI()
    {
        if (this.showHint == false || (this.playerHide != null && this.playerHide.IsHidden) || (this.playerDeath != null && this.playerDeath.IsDead))
        {
            return;
        }

        // 공통 UI 스타일 (UIStyle.cs): 경고 글꼴(검은고딕, 11/24) + 빨간색, 그림자. 화면 크기가 바뀔 수 있어 매번 만든다
        this.hintStyle = UIStyle.Warning(this.hideHintSize);
        float height = UIStyle.Size(this.hideHintSize) * 1.6f;
        float alpha = 0.55f + 0.45f * Mathf.Sin(Time.time * 3f);
        UIStyle.Label(new Rect(0f, Screen.height * 0.3f - height / 2f, Screen.width, height), this.hideHint, this.hintStyle, alpha);
    }
}
