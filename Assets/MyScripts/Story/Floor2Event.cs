using System.Collections;          // 강의 05장: 코루틴
using UnityEngine;

/// <summary>
/// 2장(2층) 연출. 차단기를 올리면 순서대로 진행된다. (괴물 AI 없이 정해진 연출만)
///
///   1) 차단기 레버(DraggableLever.IsOn)가 올라감
///   2) 방화 셔터가 "끼이익" 올라가다가 중간에 걸려 멈춘다 (아직 못 지나감)
///   3) 센서등이 계단 쪽부터 복도 끝 쪽으로 하나씩 켜짐 → 복도 끝(204호 앞)에 집주인 실루엣
///   4) 손전등이 깜빡이다 꺼지고(PlayerFlashlight.FlickerOut) 센서등도 꺼짐, 기어 오는 소리
///   5) 화면에 "숨어라" → 실루엣이 복도를 따라 천천히 기어 온다
///      · 숨지 않고 가까이 있으면 잡힌다 (PlayerDeath.Die → 체크포인트에서 다시 시작)
///      · 201호 장롱에 숨으면(PlayerHide.IsHidden) 앞을 지나가 계단 쪽으로 사라진다
///   6) 실루엣이 사라지면 셔터가 끝까지 올라감 → 3층으로 갈 수 있다
///
/// [붙이는 곳] SampleScene의 빈 오브젝트 "Floor2Event" (위치 상관없음)
/// [Inspector 연결] 아래 칸들. 비어 있는 칸은 그 단계를 건너뛴다(에러 없음).
/// [소리] NoiseSystem.Emit(위치, 20, "셔터") — 20m 안의 괴물이 듣는다 (CLAUDE.md 소리 규칙)
/// [다시 시작] Checkpoint.savedOrder가 skipFromCheckpoint 이상이면 연출을 끝난 상태로 시작한다 (셔터 열림, 실루엣 없음)
/// </summary>
public class Floor2Event : MonoBehaviour
{
    [Header("시작 조건")]
    [Tooltip("2층 분전함(Breaker 프리팹)의 자식 Lever를 드래그. 이 레버가 올라가면 연출이 시작된다")]
    public DraggableLever lever;

    [Tooltip("이 번호 이상의 체크포인트를 이미 지났으면 연출을 건너뛴다 (3 = 3층 체크포인트)")]
    public int skipFromCheckpoint = 3;

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

    [Header("실루엣 (집주인, 연출용 — MonsterAI 없음)")]
    [Tooltip("복도 끝에 나타나는 집주인 오브젝트. 처음엔 꺼 둔다(체크 해제)")]
    public Transform silhouette;
    [Tooltip("실루엣이 기어 갈 길 (빈 오브젝트들). 0번 = 처음 나타나는 자리(204호 앞) → … → 마지막 = 사라지는 자리(계단)")]
    public Transform[] silhouettePath;
    [Tooltip("기어 오는 속도(m/초). 플레이어 걷기 3보다 훨씬 느리게")]
    public float crawlSpeed = 1.0f;
    [Tooltip("이 번호의 지점에 도착하면 잠깐 멈춰 킁킁거린다 (예: 201호 앞). -1이면 안 멈춤")]
    public int sniffPointIndex = 2;
    [Tooltip("킁킁거리며 멈춰 있는 시간(초)")]
    public float sniffTime = 4f;
    [Tooltip("숨지 않은 플레이어가 이 거리(m) 안이면 잡힌다")]
    public float catchDistance = 1.3f;
    [Tooltip("기어 오는 소리 (실루엣에 붙인 Audio Source, Loop 체크, Play On Awake 끔)")]
    public AudioSource crawlSound;

    [Header("손전등")]
    [Tooltip("손전등이 깜빡이다 꺼지는 시간(초)")]
    public float flickerTime = 2.5f;

    [Header("안내 문구")]
    [Tooltip("화면에 띄울 힌트")]
    public string hideHint = "숨어라";

    [Header("시간 (초)")]
    [Tooltip("셔터가 걸린 뒤 센서등이 켜지기 시작할 때까지")]
    public float waitBeforeLights = 1.5f;
    [Tooltip("실루엣이 보인 뒤 손전등이 깜빡이기 시작할 때까지 (플레이어가 실루엣을 볼 시간)")]
    public float waitBeforeFlicker = 2.5f;
    [Tooltip("손전등이 꺼진 뒤 실루엣이 기어 오기 시작할 때까지")]
    public float waitBeforeCrawl = 1.5f;

    /// <summary>연출이 끝났는지 (셔터가 다 열림)</summary>
    public bool IsFinished { get; private set; }

    private bool isStarted;
    private bool showHint;
    private bool hasCaught;   // 실루엣이 플레이어를 잡았는지
    private Vector3 shutterClosedPosition;
    private Vector3 shutterClosedScale;
    private GUIStyle hintStyle;

    // 강의 04장: Find는 Start()에서 한 번만
    private Transform player;
    private PlayerHide playerHide;
    private PlayerFlashlight playerFlashlight;
    private PlayerDeath playerDeath;
    private NoiseSystem noiseSystem;

    private void Start()
    {
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            this.player = playerObject.transform;
            this.playerHide = playerObject.GetComponent<PlayerHide>();
            this.playerFlashlight = playerObject.GetComponent<PlayerFlashlight>();
            this.playerDeath = playerObject.GetComponent<PlayerDeath>();
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

        // 처음 상태: 센서등 꺼짐, 실루엣 숨김
        this.SetLights(false);
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
        this.ShowSilhouette();
        yield return new WaitForSeconds(this.waitBeforeFlicker);

        // 4) 손전등이 깜빡이다 꺼지고, 센서등도 꺼진다
        if (this.playerFlashlight != null && this.playerFlashlight.hasFlashlight)
        {
            this.playerFlashlight.FlickerOut(this.flickerTime);
        }
        yield return new WaitForSeconds(this.flickerTime);
        this.SetLights(false);
        if (this.crawlSound != null)
        {
            this.crawlSound.Play();   // 강의 03장
        }
        yield return new WaitForSeconds(this.waitBeforeCrawl);

        // 5) "숨어라" + 실루엣이 기어 온다
        this.showHint = true;
        yield return this.StartCoroutine(this.CrawlRoutine());
        if (this.hasCaught)
        {
            yield break;   // 잡혔으면 여기서 끝 (PlayerDeath가 씬을 다시 불러온다)
        }

        // 6) 실루엣이 사라지면 셔터가 끝까지 열린다
        this.showHint = false;
        if (this.crawlSound != null)
        {
            this.crawlSound.Stop();
        }
        if (this.silhouette != null)
        {
            this.silhouette.gameObject.SetActive(false);
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

    private void ShowSilhouette()
    {
        if (this.silhouette == null)
        {
            return;
        }
        if (this.silhouettePath.Length > 0 && this.silhouettePath[0] != null)
        {
            this.silhouette.position = this.silhouettePath[0].position;
        }
        this.silhouette.gameObject.SetActive(true);
    }

    // 실루엣이 길(silhouettePath)을 따라 기어 간다. 숨지 않은 플레이어가 가까우면 잡는다.
    private IEnumerator CrawlRoutine()
    {
        if (this.silhouette == null)
        {
            yield break;
        }

        for (int i = 1; i < this.silhouettePath.Length; i++)
        {
            if (this.silhouettePath[i] == null)
            {
                continue;
            }
            Vector3 target = this.silhouettePath[i].position;

            // 다음 지점을 향해 고개를 돌린다 (강의 07장 LookAt, y 고정해서 숙여지지 않게)
            Vector3 lookPoint = new Vector3(target.x, this.silhouette.position.y, target.z);
            this.silhouette.LookAt(lookPoint);

            while (Vector3.Distance(this.silhouette.position, target) > 0.05f)
            {
                this.silhouette.position = Vector3.MoveTowards(this.silhouette.position, target, this.crawlSpeed * Time.deltaTime);
                if (this.TryCatchPlayer())
                {
                    yield break;
                }
                yield return null;
            }

            // 킁킁 (201호 앞 등에서 잠깐 멈춤)
            if (i == this.sniffPointIndex)
            {
                float time = 0f;
                while (time < this.sniffTime)
                {
                    time = time + Time.deltaTime;
                    if (this.TryCatchPlayer())
                    {
                        yield break;
                    }
                    yield return null;
                }
            }
        }
    }

    // 숨지 않은 플레이어가 가까우면 잡는다. 잡았으면 true
    private bool TryCatchPlayer()
    {
        if (this.player == null || this.playerDeath == null)
        {
            return false;
        }
        if (this.playerHide != null && this.playerHide.IsHidden)
        {
            return false;
        }
        if (Vector3.Distance(this.player.position, this.silhouette.position) > this.catchDistance)
        {
            return false;
        }

        this.hasCaught = true;
        this.showHint = false;
        if (this.crawlSound != null)
        {
            this.crawlSound.Stop();
        }
        this.playerDeath.Die(this.silhouette);   // 잡힘 → 체크포인트에서 다시 시작
        return true;
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
        if (this.showHint == false || (this.playerHide != null && this.playerHide.IsHidden))
        {
            return;
        }

        // 글자 모양은 처음 한 번만 만든다 (GUI.skin은 OnGUI 안에서만 쓸 수 있음)
        if (this.hintStyle == null)
        {
            this.hintStyle = new GUIStyle(GUI.skin.label);
            this.hintStyle.fontSize = 36;
            this.hintStyle.alignment = TextAnchor.MiddleCenter;
        }
        float alpha = 0.55f + 0.45f * Mathf.Sin(Time.time * 3f);
        GUI.color = new Color(0.85f, 0.1f, 0.1f, alpha);
        GUI.Label(new Rect(0f, Screen.height * 0.25f, Screen.width, 60f), this.hideHint, this.hintStyle);
    }
}
