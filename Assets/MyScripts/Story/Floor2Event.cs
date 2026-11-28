using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// 2장(2층) 연출: 차단기를 올리면 셔터·센서등·괴물 등장·손전등 꺼짐·"숨어라"가 순서대로 진행된다.
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

    public bool IsFinished { get; private set; }

    public static bool reachedHidePhase = false;

    private bool isStarted;
    private bool showHint;
    private bool isRespawn;
    private Vector3 shutterClosedPosition;
    private Vector3 shutterClosedScale;
    private GUIStyle hintStyle;

    private Transform player;
    private PlayerHide playerHide;
    private PlayerFlashlight playerFlashlight;
    private PlayerDeath playerDeath;
    private TensionMusic tensionMusic;
    private RoarScare roarScare;
    private NoiseSystem noiseSystem;
    private PlayerMovement playerMovement;
    private PlayerLook playerLook;
    private PlayerInteract playerInteract;
    private Rigidbody playerBody;
    private bool isViewLocked;

    private void Awake()
    {
        this.isRespawn = Checkpoint.isRespawning;
        if (this.isRespawn == false)
        {
            reachedHidePhase = false;
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
        else if (this.isRespawn && reachedHidePhase && Checkpoint.savedOrder >= this.retryFromCheckpoint)
        {
            this.isStarted = true;
            if (this.shutter != null)
            {
                float fullHeight = this.shutterClosedScale.y;
                this.SetShutterRaised(Mathf.Clamp(this.shutterJamHeight, 0f, fullHeight - 0.1f), fullHeight,
                                      this.shutterClosedPosition.y + fullHeight / 2f);
            }
            this.StartCoroutine(this.RetryRoutine());
        }
    }

    private IEnumerator RetryRoutine()
    {
        this.SetBlockers(true);
        this.ShowMonster();
        this.StartViewLock();
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

        if (this.lever.IsOn)
        {
            this.isStarted = true;
            this.StartCoroutine(this.EventRoutine());
        }
    }

    private IEnumerator EventRoutine()
    {
        this.ShowMonster();
        this.SetBlockers(true);
        this.StartViewLock();

        if (this.shutter != null)
        {
            this.PlayShutterSound();
            this.Emit(this.shutter.position, 20f, "셔터");
            yield return this.StartCoroutine(this.MoveShutter(this.shutterJamHeight));
            this.StopShutterSound();
        }
        yield return new WaitForSeconds(this.waitBeforeLights);

        for (int i = 0; i < this.sensorLights.Length; i++)
        {
            if (this.sensorLights[i] == null)
            {
                continue;
            }
            this.sensorLights[i].enabled = true;
            if (this.sensorClickClip != null)
            {
                AudioSource.PlayClipAtPoint(this.sensorClickClip, this.sensorLights[i].transform.position);
            }
            yield return new WaitForSeconds(this.sensorInterval);
        }
        if (this.sensorLights.Length > 0 && this.sensorLights[this.sensorLights.Length - 1] != null)
        {
            this.Emit(this.sensorLights[this.sensorLights.Length - 1].transform.position, 20f, "센서등");
        }
        yield return new WaitForSeconds(this.monsterStartDelay);
        this.StartMonsterMove();
        yield return new WaitForSeconds(Mathf.Max(0f, this.flickerStartDelay - this.monsterStartDelay));

        if (this.playerFlashlight != null && this.playerFlashlight.hasFlashlight)
        {
            this.playerFlashlight.FlickerOut(this.flickerTime);
        }
        yield return new WaitForSeconds(this.flickerTime);
        this.SetLights(false);
        if (this.silhouette != null)
        {
            this.silhouette.gameObject.SetActive(false);
        }
        yield return this.StartCoroutine(this.HideRoutine());
    }

    private IEnumerator HideRoutine()
    {
        reachedHidePhase = true;
        this.isViewLocked = false;
        this.showHint = true;
        if (this.tensionMusic != null)
        {
            this.tensionMusic.PlayHide();
        }
        if (this.roarScare != null && this.monster != null)
        {
            this.roarScare.StartScare(this.monster.transform.position);
        }
        if (this.monster != null)
        {
            while (this.monster.IsRouteFinished == false)
            {
                yield return null;
            }
        }

        this.showHint = false;
        this.SetBlockers(false);
        if (this.tensionMusic != null)
        {
            this.tensionMusic.StopHide();
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

    private void StartViewLock()
    {
        if (this.lockViewDuringEvent == false || this.isViewLocked)
        {
            return;
        }
        this.isViewLocked = true;
        this.StartCoroutine(this.ViewLockRoutine());
    }

    private IEnumerator ViewLockRoutine()
    {
        this.SetPlayerFrozen(true);
        yield return null;

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
                yield break;
            }

            if (canTurn)
            {
                Vector3 dir = target.position + Vector3.up * this.viewHeight - this.playerLook.cameraTransform.position;
                float endYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                float flat = Mathf.Sqrt(dir.x * dir.x + dir.z * dir.z);
                float endPitch = -Mathf.Atan2(dir.y, flat) * Mathf.Rad2Deg;

                t = Mathf.Min(1f, t + Time.deltaTime / this.viewTurnTime);
                this.playerLook.IsLocked = true;
                this.playerLook.SetLook(Mathf.LerpAngle(startYaw, endYaw, t), Mathf.Lerp(startPitch, endPitch, t));
            }

            if (this.playerBody != null)
            {
                Vector3 v = this.playerBody.linearVelocity;
                this.playerBody.linearVelocity = new Vector3(0f, v.y, 0f);
            }
            yield return null;
        }

        this.SetPlayerFrozen(false);
    }

    private void SetPlayerFrozen(bool frozen)
    {
        if (frozen && this.playerInteract != null)
        {
            this.playerInteract.ReleaseGrab();
        }
        if (this.playerMovement != null) this.playerMovement.enabled = !frozen;
        if (this.playerInteract != null) this.playerInteract.enabled = !frozen;
        if (this.playerLook != null) this.playerLook.IsLocked = frozen;
    }

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

    private IEnumerator MoveShutter(float height)
    {
        float fullHeight = this.shutterClosedScale.y;
        float target = Mathf.Clamp(height, 0f, fullHeight - 0.1f);
        float raised = fullHeight - this.shutter.localScale.y;
        float topY = this.shutterClosedPosition.y + fullHeight / 2f;

        while (Mathf.Abs(raised - target) > 0.005f)
        {
            raised = Mathf.MoveTowards(raised, target, this.shutterSpeed * Time.deltaTime);
            this.SetShutterRaised(raised, fullHeight, topY);
            yield return null;
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

    private void StartMonsterMove()
    {
        if (this.monster != null)
        {
            this.SetMonsterAgent(true);
            this.SetMonsterPose(false);
            this.monster.enabled = true;
        }
    }

    private void ShowMonster()
    {
        if (this.monster != null)
        {
            this.monster.enabled = false;
            this.monster.alwaysBashDoors = this.monsterBashesDoors;
            this.SetMonsterAgent(false);
            this.monster.gameObject.SetActive(true);
            this.SetMonsterPose(true);
        }
        else if (this.silhouette != null)
        {
            this.silhouette.gameObject.SetActive(true);
        }
    }

    private void SetMonsterAgent(bool on)
    {
        NavMeshAgent agent = this.monster.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = on;
        }
    }

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

    private void OnGUI()
    {
        if (this.showHint == false || (this.playerHide != null && this.playerHide.IsHidden) || (this.playerDeath != null && this.playerDeath.IsDead))
        {
            return;
        }

        this.hintStyle = UIStyle.Warning(this.hideHintSize);
        float height = UIStyle.Size(this.hideHintSize) * 1.6f;
        float alpha = 0.55f + 0.45f * Mathf.Sin(Time.time * 3f);
        UIStyle.WarningText(new Rect(0f, Screen.height * 0.3f - height / 2f, Screen.width, height), this.hideHint, this.hintStyle, alpha);
    }
}
