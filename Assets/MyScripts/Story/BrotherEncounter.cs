using System.Collections;          // 강의 05장: 코루틴(IEnumerator)
using UnityEngine;

/// <summary>
/// 4장 지하: 제사상 앞에서 동생과 마주하는 연출 → 최종 추격 시작 신호.
///   1) 플레이어가 제사상 앞 구역(Is Trigger)에 들어오면 걷기·시점이 멈추고 고개가 동생 쪽으로 천천히 돌아간다
///   2) 동생의 말(자막)이 한 줄씩 뜬다 (목소리 소리는 없어도 됨)
///   3) 동생이 사라지고 촛불(조명)이 꺼진다 → 빨간 "도망쳐"
///   4) 꺼 둔 괴물(Monster_B1)을 켜고, 0.5초 뒤 플레이어 자리에서 큰 소리 → 괴물이 듣고 달려온다 (NoiseSystem, 느슨한 연결)
///   5) 플레이어는 다시 움직일 수 있다 → 비상구(EndingScreen)로 도망
///
/// [붙이는 곳] 빈 오브젝트 "BrotherEncounter" + Box Collider(Is Trigger 체크, 강의 05·07장) — 제사상 앞
/// [다시 시작] 연출을 끝까지 본 뒤 잡혀서 체크포인트 retryFromCheckpoint(4)번에서 다시 시작하면
///            자막 없이 동생은 이미 없고, retryDelay초 뒤 바로 괴물이 켜져 쫓아온다. (finished = public static, 강의 07장)
/// [A 16번] 추격 쪽에서 "연출이 끝났는지" 알고 싶으면 BrotherEncounter.finished 를 보면 된다.
/// </summary>
public class BrotherEncounter : MonoBehaviour
{
    [Header("동생")]
    [Tooltip("동생 오브젝트 (모델 또는 작은 검은 캡슐). 연출 끝에 사라진다")]
    public GameObject brother;
    [Tooltip("고개를 돌려 바라볼 곳 (동생 오브젝트를 그대로 넣어도 됨)")]
    public Transform lookTarget;
    [Tooltip("고개가 돌아가는 시간(초)")]
    public float turnTime = 1.5f;

    [Header("자막 (동생의 말)")]
    [TextArea(1, 3)]
    public string[] lines = new string[]
    {
        "…형아?",
        "형아 올 때까지 불 켜 놓는다고 했잖아.",
        "아저씨들이 형아 대신 나를 데려왔어. 여기 계속 있었어. 서른 번이나 겨울이 지났어.",
        "근데 형아, 그 아저씨가 아직 배고프대.",
        "…도망가."
    };
    [Tooltip("자막 한 줄이 떠 있는 시간(초)")]
    public float lineTime = 3.5f;
    [Tooltip("자막 글자 크기 (1080p 기준)")]
    public int lineSize = 32;
    [Tooltip("(없어도 됨) 동생 목소리. 연출이 시작될 때 한 번 재생 (이 오브젝트에 Audio Source를 붙이고 Clip은 여기)")]
    public AudioClip voiceClip;

    [Header("끝날 때")]
    [Tooltip("연출 끝에 끌 조명들 (제사상 촛불 Point Light 등)")]
    public Light[] lightsOff;
    [Tooltip("빨간 경고 글자")]
    public string runText = "도망쳐";
    [Tooltip("경고 글자 크기 (1080p 기준, \"숨어라\"와 같은 120)")]
    public int runTextSize = 120;
    [Tooltip("경고 글자가 떠 있는 시간(초)")]
    public float runTextTime = 2f;

    [Header("추격 시작 (A 16번과 연결)")]
    [Tooltip("연출이 끝나면 켤 오브젝트. 처음엔 꺼 둔다(체크 해제). 예: Monster_B1")]
    public GameObject[] activateAfter;
    [Tooltip("괴물을 켠 뒤 플레이어 자리에서 나는 소리의 크기 = 들리는 거리(m). 지하 전체에 들리게 크게")]
    public float noiseLoudness = 40f;
    [Tooltip("(11/24) 체크하면 추격이 시작될 때부터 엔딩까지 쫓기는 음악(TensionMusic 추격 음악)이 계속 나온다. 괴물이 잠깐 놓쳐도 안 꺼짐")]
    public bool chaseMusicUntilEnd = true;

    [Header("다시 시작")]
    [Tooltip("이 번호 이상의 체크포인트에서 다시 시작하면 자막 없이 바로 추격 (4 = 지하 체크포인트)")]
    public int retryFromCheckpoint = 4;
    [Tooltip("다시 시작한 뒤 괴물이 켜질 때까지 시간(초)")]
    public float retryDelay = 2f;

    // 강의 07장 public static: 씬을 다시 불러와도 값이 남는다. 연출을 끝까지 봤는지 기억
    public static bool finished = false;

    private bool isRespawn;       // 죽어서 다시 시작한 게임인지
    private bool isPlaying;       // 연출 중인지 (두 번 시작하지 않게)
    private string currentLine;   // 지금 화면에 띄울 자막
    private float lineAlpha;      // 자막 투명도
    private bool showRunText;
    private float runAlpha;

    // 강의 04장: Find는 Start()에서 한 번만
    private Transform player;
    private PlayerMovement playerMovement;
    private PlayerLook playerLook;
    private PlayerInteract playerInteract;
    private PlayerDeath playerDeath;
    private NoiseSystem noiseSystem;

    // 강의 05장 실행 순서: Awake는 모든 Start보다 먼저 불린다.
    // PlayerDeath.Start가 Checkpoint.isRespawning을 false로 되돌리기 전에 읽어 둔다. (Floor2Event와 같은 방식)
    private void Awake()
    {
        this.isRespawn = Checkpoint.isRespawning;
        if (this.isRespawn == false)
        {
            finished = false;   // 처음부터 시작하는 게임
        }
    }

    private void Start()
    {
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            this.player = playerObject.transform;
            this.playerMovement = playerObject.GetComponent<PlayerMovement>();
            this.playerLook = playerObject.GetComponent<PlayerLook>();
            this.playerInteract = playerObject.GetComponent<PlayerInteract>();
            this.playerDeath = playerObject.GetComponent<PlayerDeath>();
        }
        GameObject noiseObject = GameObject.Find("NoiseSystem");
        if (noiseObject != null)
        {
            this.noiseSystem = noiseObject.GetComponent<NoiseSystem>();
        }
        if (this.lookTarget == null && this.brother != null)
        {
            this.lookTarget = this.brother.transform;
        }

        // 연출을 이미 봤고 지하 체크포인트에서 다시 시작 → 동생 없이 바로 추격
        if (this.isRespawn && finished && Checkpoint.savedOrder >= this.retryFromCheckpoint)
        {
            this.isPlaying = true;   // 구역에 다시 들어가도 연출이 또 나오지 않게
            this.SetEndState();
            this.StartCoroutine(this.RetryRoutine());
        }
    }

    // 강의 05·07장 Is Trigger: 플레이어가 제사상 앞에 들어왔다
    private void OnTriggerEnter(Collider other)
    {
        if (this.isPlaying || other.CompareTag("Player") == false)
        {
            return;
        }
        this.isPlaying = true;
        this.StartCoroutine(this.EncounterRoutine());
    }

    // 강의 05장 코루틴: 시간 순서대로 연출
    private IEnumerator EncounterRoutine()
    {
        this.SetPlayerFrozen(true);

        AudioSource voice = this.GetComponent<AudioSource>();
        if (voice != null && this.voiceClip != null)
        {
            voice.clip = this.voiceClip;
            voice.Play();   // 강의 03장
        }

        // 1) 고개가 동생 쪽으로 천천히 돌아간다
        if (this.lookTarget != null && this.playerLook != null && this.playerLook.cameraTransform != null)
        {
            Vector3 dir = this.lookTarget.position - this.player.position;
            float startYaw = this.playerLook.cameraTransform.eulerAngles.y;
            float endYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;   // 바라볼 좌우 각도
            float t = 0f;
            while (t < 1f)
            {
                t = t + Time.deltaTime / this.turnTime;
                this.playerLook.SetYaw(Mathf.LerpAngle(startYaw, endYaw, t));
                yield return null;
            }
        }

        // 2) 자막 한 줄씩 (서서히 나타났다 사라짐)
        for (int i = 0; i < this.lines.Length; i++)
        {
            this.currentLine = this.lines[i];
            yield return this.StartCoroutine(this.FadeLine(0f, 1f, 0.4f));
            yield return new WaitForSeconds(this.lineTime);
            yield return this.StartCoroutine(this.FadeLine(1f, 0f, 0.4f));
        }
        this.currentLine = null;

        // 3) 동생이 사라지고 촛불이 꺼진다
        this.SetEndState();
        yield return new WaitForSeconds(0.8f);

        // 4) "도망쳐" + 괴물 켜기 + 큰 소리
        finished = true;
        this.showRunText = true;
        this.runAlpha = 1f;
        this.SetPlayerFrozen(false);
        yield return this.StartCoroutine(this.StartChase());

        yield return new WaitForSeconds(this.runTextTime);
        while (this.runAlpha > 0f)
        {
            this.runAlpha = this.runAlpha - Time.deltaTime;
            yield return null;
        }
        this.showRunText = false;
    }

    // 다시 시작했을 때: 잠깐 뒤 바로 추격
    private IEnumerator RetryRoutine()
    {
        yield return new WaitForSeconds(this.retryDelay);
        yield return this.StartCoroutine(this.StartChase());
    }

    // 괴물을 켜고, 0.5초 뒤(괴물의 Start가 끝난 뒤) 플레이어 자리에서 큰 소리를 낸다
    private IEnumerator StartChase()
    {
        for (int i = 0; i < this.activateAfter.Length; i++)
        {
            if (this.activateAfter[i] != null)
            {
                this.activateAfter[i].SetActive(true);
            }
        }
        // (11/24) 지하 탈출 내내 쫓기는 음악. TensionMusic은 "원하는 곳 수"를 세므로 여기서 한 번 켜 두면
        // 괴물이 놓쳤다가 다시 쫓아도(괴물 쪽 PlayChase/StopChase) 꺼지지 않는다. 엔딩 화면(EndingScreen)이 음악을 줄여 끈다.
        if (this.chaseMusicUntilEnd && this.player != null)
        {
            TensionMusic music = this.player.GetComponent<TensionMusic>();
            if (music != null)
            {
                music.PlayChase();
            }
        }
        yield return new WaitForSeconds(0.5f);
        if (this.noiseSystem != null && this.player != null)
        {
            this.noiseSystem.Emit(this.player.position, this.noiseLoudness, "동생 울음");
        }
    }

    // 자막 투명도를 from → to로 바꾼다
    private IEnumerator FadeLine(float from, float to, float time)
    {
        float t = 0f;
        while (t < 1f)
        {
            t = t + Time.deltaTime / time;
            this.lineAlpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
    }

    // 연출이 끝난 모습: 동생 없음, 촛불 꺼짐
    private void SetEndState()
    {
        if (this.brother != null)
        {
            this.brother.SetActive(false);
        }
        for (int i = 0; i < this.lightsOff.Length; i++)
        {
            if (this.lightsOff[i] != null)
            {
                this.lightsOff[i].enabled = false;
            }
        }
    }

    // 플레이어 걷기·시점·잡기를 멈추거나 다시 켠다 (강의 04장 enabled, PlayerDeath와 같은 방식)
    private void SetPlayerFrozen(bool frozen)
    {
        if (this.playerMovement != null) this.playerMovement.enabled = !frozen;
        if (this.playerInteract != null) this.playerInteract.enabled = !frozen;
        if (this.playerLook != null) this.playerLook.IsLocked = frozen;
        if (frozen && this.player != null)
        {
            Rigidbody body = this.player.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;   // 미끄러지지 않게
            }
        }
    }

    // 강의 04장 OnGUI: 자막(아래 가운데)과 "도망쳐"
    private void OnGUI()
    {
        if (this.playerDeath != null && this.playerDeath.IsDead)
        {
            return;
        }

        if (this.currentLine != null)
        {
            GUIStyle lineStyle = UIStyle.Body(this.lineSize, UIStyle.white);
            lineStyle.alignment = TextAnchor.MiddleCenter;
            lineStyle.wordWrap = true;
            float margin = UIStyle.Size(200);
            Rect rect = new Rect(margin, Screen.height - UIStyle.Size(220), Screen.width - margin * 2f, UIStyle.Size(110));
            UIStyle.Label(rect, this.currentLine, lineStyle, this.lineAlpha);
        }

        if (this.showRunText)
        {
            Rect rect = new Rect(0f, Screen.height / 2f - UIStyle.Size(90), Screen.width, UIStyle.Size(180));
            UIStyle.Label(rect, this.runText, UIStyle.Title(this.runTextSize), this.runAlpha);
        }
    }
}
