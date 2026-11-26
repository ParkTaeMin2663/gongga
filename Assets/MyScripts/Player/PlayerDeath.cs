using System.Collections;          // 강의 05장: 코루틴(IEnumerator)
using UnityEngine;
using UnityEngine.AI;              // 강의 07장: NavMeshAgent (덮칠 때 잠깐 끈다)
using UnityEngine.SceneManagement; // 강의 04장: SceneManager.LoadScene

/// <summary>
/// 플레이어 사망과 다시 시작.
///   - 괴물에게 잡히면 Die() → 몸이 굳고 괴물 쪽으로 고개가 홱 돌아간다
///   - 점프스케어: 괴물이 얼굴 앞으로 덮치며 비명(Scream 동작은 MonsterAI가 튼다) + 화면 흔들림 → 뚝 까매짐
///   - 멈칫 → 덮침 → 하얀 번쩍·기울며 흔들림·붉은 테 → 바닥으로 쓰러져 괴물을 올려다봄 → 가장자리부터 까매짐
///   - 까만 화면에 "다시 시작 / 그만하기" 선택 (마우스 커서가 보인다)
///       · 다시 시작: 씬 다시 불러오기 → 마지막 체크포인트에서 시작 (Checkpoint.savedPosition)
///       · 그만하기 : 게임 종료 (에디터에서는 Play 멈춤)
///   - 처음 시작할 때(죽어서 다시 연 게 아닐 때)는 체크포인트 기록을 지운다.
///
/// [붙이는 곳] Player 오브젝트 (Player 프리팹)
/// [Inspector 연결] 비워 두면 같은 Player에서 자동으로 찾는다.
/// [같이 쓰는 것] Checkpoint.cs (public static 저장값), UIStyle.cs (글꼴·색)
/// </summary>
public class PlayerDeath : MonoBehaviour
{
    [Header("사망 연출 (초)")]
    [Tooltip("괴물 쪽으로 고개가 돌아가는 시간")]
    public float turnTime = 0.15f;
    [Tooltip("화면이 붉어졌다가 까맣게 되는 시간 (점프스케어를 끄면 이 방식)")]
    public float fadeTime = 1.2f;
    [Tooltip("까만 화면으로 기다리는 시간 (선택 화면이 뜨기 전)")]
    public float blackTime = 0.6f;

    [Header("점프스케어")]
    [Tooltip("체크하면 괴물이 얼굴 앞으로 덮친다. 끄면 예전처럼 붉어졌다가 까매진다")]
    public bool useJumpscare = true;
    [Tooltip("괴물이 덮쳐 오는 시간(초). 짧을수록 깜짝 놀란다")]
    public float lungeTime = 0.12f;
    [Tooltip("덮친 괴물 얼굴이 카메라에서 떨어진 거리(m)")]
    public float jumpscareDistance = 0.55f;
    [Tooltip("괴물 발에서 얼굴까지 높이(m). 얼굴이 화면 가운데 오게")]
    public float monsterFaceHeight = 1.6f;
    [Tooltip("덮친 뒤 비명을 지르는 동안 (화면 흔들림) 시간(초)")]
    public float screamTime = 1.0f;
    [Tooltip("화면 흔들림 크기(m)")]
    public float shakeAmount = 0.04f;
    [Tooltip("덮칠 때 비명 소리 (없어도 됨). 카메라 바로 앞에서 크게 재생")]
    public AudioClip jumpscareSound;

    [Header("사망 연출 강화")]
    [Tooltip("괴물을 본 순간 덮치기 전에 멈칫하는 시간(초). 정적이 있어야 더 놀란다")]
    public float freezeTime = 0.35f;
    [Tooltip("멈칫하는 동안 괴물 얼굴 쪽으로 고개를 드는 각도 (음수 = 위)")]
    public float lookUpPitch = -12f;
    [Tooltip("덮친 순간 화면이 하얗게 번쩍이는 세기 (0 = 없음)")]
    [Range(0f, 1f)] public float impactFlash = 0.8f;
    [Tooltip("비명 동안 화면이 기울며 흔들리는 각도(도)")]
    public float shakeAngle = 4f;
    [Tooltip("쓰러지는 시간(초): 카메라가 바닥으로 떨어지며 옆으로 눕고 괴물을 올려다본다")]
    public float collapseTime = 0.9f;
    [Tooltip("쓰러진 뒤 카메라 높이(m, Player 발 기준)")]
    public float collapseHeight = 0.3f;
    [Tooltip("쓰러질 때 옆으로 눕는 각도(도)")]
    public float collapseRoll = 75f;
    [Tooltip("쓰러진 뒤 올려다보는 각도 (음수 = 위)")]
    public float collapsePitch = -35f;
    [Tooltip("쓰러진 뒤 까매지기 전까지 괴물을 올려다보는 시간(초)")]
    public float lyingTime = 0.6f;
    [Tooltip("쓰러질 때 쿵 소리 (없어도 됨)")]
    public AudioClip fallSound;

    [Header("잡힌 직후 화면 (암네시아·아웃라스트 참고)")]
    [Tooltip("덮친 순간 화면이 까맣게 끊겼다 켜졌다 하는 횟수 (필름이 튀는 느낌)")]
    public int strobeCount = 3;
    [Tooltip("덮친 뒤 지지직 잡음(점·가로 줄)이 보이는 세기 (0 = 없음)")]
    [Range(0f, 1f)] public float staticAmount = 0.8f;
    [Tooltip("사망 화면에서 속삭임이 먼저 나오고, 버튼은 이 시간(초) 뒤에 나타난다")]
    public float menuDelay = 2.5f;
    [TextArea]
    [Tooltip("사망 화면 속삭임 (하나를 무작위로) — 동생·집주인의 목소리처럼")]
    public string[] deathWhispers =
    {
        "…형, 또 나만 두고 가는 거야?",
        "집주인은 아직 배가 고프다.",
        "30년 전에도, 아무도 오지 않았어.",
        "불은 켜 둘게. 다시 와."
    };
    [Tooltip("층별 도움말: 바깥·1층 / 2층 / 3층 / 지하 (잡힌 높이로 고름)")]
    public string[] floorHints =
    {
        "뛰면 발소리가 크게 난다. 괴물은 소리로 사냥한다.",
        "괴물은 눈이 없다. 숨어서 숨을 죽여라.",
        "장롱 안에 숨으면 괴물은 찾지 못한다. 물건을 던져 따돌려라.",
        "멈추면 잡힌다. 보일러를 돌아 비상구로 달려라."
    };

    [Header("소리 (없어도 됨)")]
    [Tooltip("잡혔을 때 소리. Player에 Audio Source를 하나 더 붙이고 드래그 (발소리용과 따로)")]
    public AudioSource deathSound;

    [Header("연결 (비워 두면 자동으로 찾음)")]
    public PlayerMovement playerMovement;
    public PlayerLook playerLook;
    public PlayerInteract playerInteract;
    [Tooltip("Player의 자식 Main Camera (흔들림용)")]
    public Transform cameraTransform;

    /// <summary>죽는 중인지. 다른 스크립트(괴물 등)가 두 번 잡지 않도록 확인한다.</summary>
    public bool IsDead { get; private set; }

    private float redAmount;     // 화면 붉은 정도 (0~1)
    private float flashAmount;   // 덮친 순간 하얀 번쩍임 (0~1)
    private float edgeAmount;    // 화면 가장자리 붉은 테 (0~1)
    private float closeAmount;   // 가장자리부터 까맣게 닫힘 (0~1)
    private float strobeBlack;   // 덮친 순간 까맣게 끊기는 화면 (0 또는 1)
    private float noiseAmount;   // 지지직 잡음 세기 (0~1)
    private string whisper;      // 사망 화면 속삭임 (잡힐 때 하나 고름)
    private string hint;         // 사망 화면 도움말 (잡힌 층)
    private float menuTime;      // 사망 화면이 뜬 뒤 지난 시간
    private float blackAmount;   // 화면 검은 정도 (0~1)
    private bool showMenu;       // "다시 시작 / 그만하기" 화면
    private float menuAlpha;     // 선택 화면이 서서히 나타남 (0~1)
    private Texture2D whiteTexture;

    private void Start()
    {
        this.whiteTexture = Texture2D.whiteTexture;

        if (this.playerMovement == null) this.playerMovement = this.GetComponent<PlayerMovement>();
        if (this.playerLook == null) this.playerLook = this.GetComponent<PlayerLook>();
        if (this.playerInteract == null) this.playerInteract = this.GetComponent<PlayerInteract>();
        if (this.cameraTransform == null && this.playerInteract != null) this.cameraTransform = this.playerInteract.cameraTransform;

        if (Checkpoint.isRespawning)
        {
            // 죽어서 다시 열린 씬 → 마지막 체크포인트에서 시작
            Checkpoint.isRespawning = false;
            this.transform.position = Checkpoint.savedPosition;
            if (this.playerLook != null)
            {
                this.playerLook.SetYaw(Checkpoint.savedYaw);
            }
            this.blackAmount = 1f;   // 까만 화면에서 서서히 밝아지며 시작

            // 체크포인트 때의 손전등 상태로 되돌린다 (주운 손전등을 잃지 않게)
            PlayerFlashlight flashlight = this.GetComponent<PlayerFlashlight>();
            if (flashlight != null)
            {
                flashlight.hasFlashlight = Checkpoint.savedHasFlashlight;
                flashlight.battery = Checkpoint.savedBattery;
            }
        }
        else
        {
            // 새로 시작한 게임 → 예전 기록 지우기
            Checkpoint.savedOrder = 0;
            Checkpoint.lastSaveTime = -100f;
        }
    }

    private void Update()
    {
        // 다시 시작했을 때 까만 화면이 서서히 걷힌다 (강의 03장 Time.deltaTime)
        if (this.IsDead == false && this.blackAmount > 0f)
        {
            this.blackAmount = Mathf.Max(0f, this.blackAmount - Time.deltaTime / this.fadeTime);
        }

        if (this.showMenu)
        {
            this.menuAlpha = Mathf.Min(1f, this.menuAlpha + Time.deltaTime * 0.8f);   // 천천히 나타남
            this.menuTime = this.menuTime + Time.deltaTime;
        }
    }

    /// <summary>괴물이 잡았을 때 부른다. killer = 잡은 괴물 (그쪽을 바라보게)</summary>
    public void Die(Transform killer)
    {
        if (this.IsDead)
        {
            return;
        }
        this.IsDead = true;

        // 몸을 굳힌다: 걷기·잡기·시점 멈춤, 물리 멈춤 (강의 03장 Is Kinematic)
        if (this.playerMovement != null) this.playerMovement.enabled = false;
        if (this.playerInteract != null) this.playerInteract.enabled = false;
        if (this.playerLook != null) this.playerLook.IsLocked = true;
        Rigidbody rb = this.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (this.deathSound != null)
        {
            this.deathSound.Play();   // 강의 03장
        }

        // 사망 화면 글: 속삭임은 무작위(강의 03장 Random.Range), 도움말은 잡힌 높이(층)로 고른다
        if (this.deathWhispers != null && this.deathWhispers.Length > 0)
        {
            this.whisper = this.deathWhispers[Random.Range(0, this.deathWhispers.Length)];
        }
        if (this.floorHints != null && this.floorHints.Length >= 4)
        {
            float y = this.transform.position.y;           // 지하 -2.8 / 1층 0 / 2층 2.8 / 3층 5.6
            int floor = y < -1.5f ? 3 : (y > 4.5f ? 2 : (y > 1.5f ? 1 : 0));
            this.hint = this.floorHints[floor];
        }

        // 강의 05장: 코루틴으로 시간 순서대로 연출
        this.StartCoroutine(this.DeathRoutine(killer));
    }

    private IEnumerator DeathRoutine(Transform killer)
    {
        // 1) 괴물 쪽으로 고개가 홱 돌아간다
        float startYaw = this.playerLook != null ? this.playerLook.Yaw : 0f;
        Vector3 toKiller = killer != null ? killer.position - this.transform.position : this.transform.forward;
        float targetYaw = Mathf.Atan2(toKiller.x, toKiller.z) * Mathf.Rad2Deg;
        float time = 0f;
        while (time < this.turnTime)
        {
            time = time + Time.deltaTime;
            if (this.playerLook != null)
            {
                this.playerLook.SetYaw(Mathf.LerpAngle(startYaw, targetYaw, time / this.turnTime));
            }
            yield return null;   // 다음 프레임까지 기다림
        }
        if (this.playerLook != null)
        {
            this.playerLook.SetYaw(targetYaw);   // 정면(수평)으로 고정
        }

        if (this.useJumpscare && killer != null && this.cameraTransform != null)
        {
            // 2) 점프스케어: 괴물이 얼굴 앞으로 덮친다
            yield return this.StartCoroutine(this.JumpscareRoutine(killer));
            this.blackAmount = 1f;   // 비명 끝에 뚝 끊기듯 까맣게
        }
        else
        {
            // 2) (예전 방식) 화면이 붉어졌다가 까맣게
            time = 0f;
            while (time < this.fadeTime)
            {
                time = time + Time.deltaTime;
                float rate = time / this.fadeTime;
                this.redAmount = Mathf.Clamp01(rate * 2f);          // 앞 절반 동안 붉어짐
                this.blackAmount = Mathf.Clamp01(rate * 2f - 1f);   // 뒤 절반 동안 까매짐
                yield return null;
            }
        }

        // 3) 잠깐 까만 화면 → 선택 화면
        yield return new WaitForSeconds(this.blackTime);
        this.ShowMenu();
    }

    // 괴물을 카메라 바로 앞으로 끌어와 얼굴을 들이민다 + 비명 + 화면 흔들림 + 쓰러짐
    //   ① 멈칫(정적, 고개를 들어 괴물 얼굴을 봄) → ② 점점 빨라지며 덮침 → ③ 하얀 번쩍 + 기울며 덜덜 떨림 + 붉은 테
    //   → ④ 바닥으로 쓰러져 옆으로 누운 채 괴물을 올려다봄 → ⑤ 가장자리부터 까맣게 닫힘
    private IEnumerator JumpscareRoutine(Transform killer)
    {
        // 괴물은 NavMeshAgent가 위치를 잡고 있으므로 잠깐 끈다 (강의 07장 NavMeshAgent, 04장 enabled)
        NavMeshAgent agent = killer.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }

        float yaw = this.playerLook != null ? this.playerLook.Yaw : this.cameraTransform.eulerAngles.y;

        // 도착할 자리: 카메라 앞 jumpscareDistance, 얼굴 높이가 카메라 높이 (강의 03장 Vector3)
        Vector3 forward = this.cameraTransform.forward;
        forward.y = 0f;
        forward = forward.normalized;
        Vector3 camPos = this.cameraTransform.position;
        Vector3 endPos = camPos + forward * this.jumpscareDistance + Vector3.down * this.monsterFaceHeight;
        Vector3 startPos = killer.position;

        // 괴물이 카메라를 정면으로 본다 (강의 07장 LookAt, y 고정)
        Vector3 lookPoint = new Vector3(camPos.x, killer.position.y, camPos.z);
        killer.LookAt(lookPoint);

        // ① 멈칫: 소리 없이 고개를 들어 괴물 얼굴을 본다 (강의 03장 rotation + Quaternion.Euler)
        float time = 0f;
        while (time < this.freezeTime)
        {
            time = time + Time.deltaTime;
            float k = Mathf.Clamp01(time / this.freezeTime);
            this.cameraTransform.rotation = Quaternion.Euler(Mathf.Lerp(0f, this.lookUpPitch, k), yaw, 0f);
            this.edgeAmount = k * 0.3f;   // 가장자리가 살짝 어두워지며 붉어짐
            yield return null;
        }

        if (this.jumpscareSound != null)
        {
            AudioSource.PlayClipAtPoint(this.jumpscareSound, camPos, 1f);   // 강의 07장: 귀 바로 앞에서 비명
        }

        // ② 덮쳐 온다: 처음엔 천천히, 끝에 확 (k * k)
        time = 0f;
        while (time < this.lungeTime)
        {
            time = time + Time.deltaTime;
            float k = Mathf.Clamp01(time / this.lungeTime);
            killer.position = Vector3.Lerp(startPos, endPos, k * k);
            yield return null;
        }
        killer.position = endPos;

        // ③-1 부딪친 순간: 하얗게 번쩍 → 화면이 까맣게 끊겼다 켜졌다 (필름이 튀듯)
        this.flashAmount = this.impactFlash;
        this.noiseAmount = this.staticAmount;
        for (int i = 0; i < this.strobeCount; i++)
        {
            this.strobeBlack = 1f;
            yield return new WaitForSeconds(0.04f + i * 0.02f);   // 강의 05장 WaitForSeconds
            this.strobeBlack = 0f;
            this.flashAmount = this.flashAmount * 0.5f;
            yield return new WaitForSeconds(0.06f);
        }

        // ③-2 비명 동안 화면이 기울며 덜덜 떨림 + 붉은 테 + 잡음이 점점 줄어듦 (강의 03장 Random.Range)
        Vector3 camLocal = this.cameraTransform.localPosition;
        time = 0f;
        while (time < this.screamTime)
        {
            time = time + Time.deltaTime;
            float k = Mathf.Clamp01(time / this.screamTime);
            float amount = 1f - k * 0.6f;   // 처음이 가장 세고 점점 약해짐
            this.cameraTransform.localPosition = camLocal + new Vector3(
                Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * this.shakeAmount * amount;
            this.cameraTransform.rotation = Quaternion.Euler(
                this.lookUpPitch + Random.Range(-1f, 1f) * this.shakeAngle * amount,
                yaw + Random.Range(-1f, 1f) * this.shakeAngle * amount,
                Random.Range(-1f, 1f) * this.shakeAngle * 1.5f * amount);
            this.flashAmount = Mathf.Max(0f, this.flashAmount - Time.deltaTime * 5f);   // 0.2초쯤에 사라짐
            this.edgeAmount = 0.3f + 0.7f * Mathf.Clamp01(k * 2f);                       // 붉은 테가 번짐
            this.redAmount = k * 0.35f;                                                   // 화면 전체는 살짝만
            this.noiseAmount = this.staticAmount * (1f - k * 0.7f);                        // 잡음이 점점 줄어듦
            yield return null;
        }
        this.flashAmount = 0f;

        // ④ 쓰러진다: 카메라가 바닥으로 떨어지며 옆으로 눕고, 위의 괴물을 올려다본다
        if (this.fallSound != null)
        {
            AudioSource.PlayClipAtPoint(this.fallSound, camPos, 1f);
        }
        Vector3 fallStart = this.cameraTransform.localPosition;
        Vector3 fallEnd = new Vector3(camLocal.x, this.collapseHeight, camLocal.z);
        time = 0f;
        while (time < this.collapseTime)
        {
            time = time + Time.deltaTime;
            float k = Mathf.Clamp01(time / this.collapseTime);
            float fall = 1f - (1f - k) * (1f - k);   // 처음에 빠르게 떨어지고 바닥에서 느려짐
            this.cameraTransform.localPosition = Vector3.Lerp(fallStart, fallEnd, fall);
            this.noiseAmount = this.staticAmount * 0.3f * (1f - k);
            this.cameraTransform.rotation = Quaternion.Euler(
                Mathf.Lerp(this.lookUpPitch, this.collapsePitch, fall), yaw, Mathf.Lerp(0f, this.collapseRoll, fall));
            yield return null;
        }

        // ⑤ 누운 채 잠깐 올려다보다가 가장자리부터 까맣게 닫힌다
        time = 0f;
        while (time < this.lyingTime)
        {
            time = time + Time.deltaTime;
            this.closeAmount = Mathf.Clamp01(time / this.lyingTime);
            yield return null;
        }
        // 다시 시작하면 씬을 새로 불러오므로 카메라 위치·회전은 되돌리지 않아도 된다
    }

    // "다시 시작 / 그만하기" 화면을 띄운다. 마우스 커서를 보이게 한다 ([강의 외] Cursor, PlayerLook과 같은 방식)
    private void ShowMenu()
    {
        this.redAmount = 0f;
        this.edgeAmount = 0f;
        this.flashAmount = 0f;
        this.closeAmount = 0f;
        this.noiseAmount = 0f;
        this.strobeBlack = 0f;
        this.menuTime = 0f;
        this.blackAmount = 1f;
        this.showMenu = true;
        this.menuAlpha = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 다시 시작: 체크포인트를 지났으면 거기서, 아니면 처음부터 (강의 04장: 현재 Scene 다시 로드)
    private void Restart()
    {
        Checkpoint.isRespawning = Checkpoint.savedOrder > 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 그만하기: 게임 종료 ([강의 외] Application.Quit — 빌드한 게임에서만 동작, 에디터에서는 Play를 멈춘다)
    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 강의 04장 OnGUI: 붉은/검은 화면, 선택 화면, "저장됨" 표시
    private void OnGUI()
    {
        if (this.redAmount > 0f)
        {
            GUI.color = new Color(0.5f, 0f, 0f, this.redAmount * 0.7f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), this.whiteTexture);
        }
        // 가장자리 붉은 테 (A의 UIStyle.EdgeShade), 하얀 번쩍임, 가장자리부터 까맣게 닫힘
        Rect screen = new Rect(0f, 0f, Screen.width, Screen.height);
        if (this.edgeAmount > 0f)
        {
            UIStyle.EdgeShade(screen, new Color(0.45f, 0f, 0f, this.edgeAmount * 0.9f), 0.35f);
        }
        if (this.flashAmount > 0f)
        {
            GUI.color = new Color(1f, 0.92f, 0.88f, this.flashAmount);
            GUI.DrawTexture(screen, this.whiteTexture);
        }
        if (this.noiseAmount > 0f)
        {
            this.DrawStatic(screen, this.noiseAmount);
        }
        if (this.strobeBlack > 0f)
        {
            GUI.color = Color.black;
            GUI.DrawTexture(screen, this.whiteTexture);
        }
        if (this.closeAmount > 0f)
        {
            UIStyle.EdgeShade(screen, new Color(0f, 0f, 0f, 1f), 0.5f * this.closeAmount + 0.05f);
            GUI.color = new Color(0f, 0f, 0f, this.closeAmount * this.closeAmount);
            GUI.DrawTexture(screen, this.whiteTexture);
        }
        if (this.blackAmount > 0f)
        {
            GUI.color = new Color(0f, 0f, 0f, this.blackAmount);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), this.whiteTexture);
        }
        GUI.color = Color.white;

        if (this.showMenu)
        {
            UIStyle.MenuBackdrop(this.menuAlpha, new Color(0.22f, 0.02f, 0.02f, 1f));   // 가운데 검붉게, 가장자리 까맣게 (비네트)
            this.DrawMenu();
            return;
        }

        // 체크포인트를 지난 뒤 2초 동안 오른쪽 아래에 "저장됨"
        float ago = Time.time - Checkpoint.lastSaveTime;
        if (Checkpoint.lastSaveShowsMessage && ago >= 0f && ago < 2f)
        {
            // 공통 UI 스타일 (UIStyle.cs): 오른쪽 아래, 흐린 글자로 서서히 사라짐
            GUIStyle style = UIStyle.Make(UIStyle.regular, 22, UIStyle.gray, TextAnchor.LowerRight);
            Rect rect = new Rect(0f, 0f, Screen.width - UIStyle.Size(32), Screen.height - UIStyle.Size(28));
            UIStyle.Label(rect, "저장됨", style, 1f - ago / 2f);
        }
    }

    // 잡힌 직후 화면 — 암네시아처럼 까만 화면에 목소리(속삭임)가 먼저 천천히 떠오르고,
    // 도움말 한 줄 → 마지막에 "잡혔다"와 버튼. 화면 가장자리는 심장 박동처럼 붉게 뛰다가 느려진다.
    private void DrawMenu()
    {
        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;
        Rect screen = new Rect(0f, 0f, Screen.width, Screen.height);

        // 심장 박동: 처음엔 빠르게 → 점점 느려짐. 두 번 뛰는 "쿵쿵" 모양 (강의 03장 Time)
        float beatSpeed = Mathf.Lerp(2.2f, 0.9f, Mathf.Clamp01(this.menuTime / 6f));
        float phase = (this.menuTime * beatSpeed) % 1f;
        float beat = Mathf.Max(0f, 1f - Mathf.Abs(phase - 0.1f) * 12f) + 0.6f * Mathf.Max(0f, 1f - Mathf.Abs(phase - 0.3f) * 12f);
        UIStyle.EdgeShade(screen, new Color(0.5f, 0f, 0f, (0.25f + 0.45f * beat) * this.menuAlpha), 0.3f);

        // 1) 속삭임 — 제일 먼저, 아주 천천히 (흰 글자, 화면 가운데 조금 위)
        float whisperAlpha = Mathf.Clamp01(this.menuTime / 1.5f);
        if (string.IsNullOrEmpty(this.whisper) == false)
        {
            GUIStyle whisperStyle = UIStyle.Make(UIStyle.regular, 40, UIStyle.white, TextAnchor.MiddleCenter);
            // 글자가 아주 살짝 떨린다 (속삭이는 느낌)
            float jitter = UIStyle.Size(2) * (1f - whisperAlpha * 0.5f);
            Rect wr = new Rect(Random.Range(-jitter, jitter), cy - UIStyle.Size(150) + Random.Range(-jitter, jitter), Screen.width, UIStyle.Size(60));
            UIStyle.Label(wr, this.whisper, whisperStyle, whisperAlpha * 0.9f);
        }

        // 2) 도움말 — 1.2초 뒤 (노란 글자)
        float hintAlpha = Mathf.Clamp01((this.menuTime - 1.2f) / 1f);
        if (string.IsNullOrEmpty(this.hint) == false && hintAlpha > 0f)
        {
            GUIStyle hintStyle = UIStyle.Make(UIStyle.regular, 26, UIStyle.yellow, TextAnchor.MiddleCenter);
            UIStyle.Label(new Rect(0f, cy - UIStyle.Size(80), Screen.width, UIStyle.Size(40)), this.hint, hintStyle, hintAlpha * 0.85f);
        }

        // 3) "잡혔다" + 버튼 — menuDelay초 뒤 (실수로 바로 누르지 않게)
        float menuShow = Mathf.Clamp01((this.menuTime - this.menuDelay) / 0.8f);
        if (menuShow <= 0f)
        {
            return;
        }

        // 제목은 위쪽에 (속삭임을 가리지 않게)
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(330), Screen.width, UIStyle.Size(110)), "잡혔다", UIStyle.Warning(90), menuShow);

        string where = Checkpoint.savedOrder > 0 ? "마지막 체크포인트에서 다시 시작합니다" : "처음부터 다시 시작합니다";
        GUIStyle small = UIStyle.Make(UIStyle.regular, 22, UIStyle.gray, TextAnchor.MiddleCenter);
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(20), Screen.width, UIStyle.Size(40)), where, small, menuShow);

        if (menuShow < 0.5f)
        {
            return;
        }

        float width = UIStyle.Size(320);
        float height = UIStyle.Size(64);
        float oldAlpha = this.menuAlpha;
        this.menuAlpha = menuShow;   // 버튼도 같이 서서히
        if (this.MenuButton(new Rect(cx - width / 2f, cy + UIStyle.Size(50), width, height), "다시 시작"))
        {
            this.Restart();
        }
        if (this.MenuButton(new Rect(cx - width / 2f, cy + UIStyle.Size(130), width, height), "그만하기"))
        {
            this.Quit();
        }
        this.menuAlpha = oldAlpha;
    }

    // 지지직 잡음: 작은 회색 점들 + 가로로 긴 줄 몇 개 (강의 03장 Random.Range, 04장 OnGUI DrawTexture)
    private void DrawStatic(Rect screen, float amount)
    {
        int dots = (int)(400 * amount);
        float dotSize = UIStyle.Size(3);
        for (int i = 0; i < dots; i++)
        {
            float g = Random.Range(0.3f, 1f);
            GUI.color = new Color(g, g, g, Random.Range(0.1f, 0.45f) * amount);
            GUI.DrawTexture(new Rect(Random.Range(0f, screen.width), Random.Range(0f, screen.height), dotSize, dotSize), this.whiteTexture);
        }
        int lines = (int)(6 * amount);
        for (int i = 0; i < lines; i++)
        {
            float h = Random.Range(1f, UIStyle.Size(6));
            bool dark = Random.Range(0, 2) == 0;
            GUI.color = dark ? new Color(0f, 0f, 0f, 0.5f * amount) : new Color(0.8f, 0.75f, 0.75f, 0.18f * amount);
            GUI.DrawTexture(new Rect(0f, Random.Range(0f, screen.height), screen.width, h), this.whiteTexture);
        }
        GUI.color = Color.white;
    }

    // 글자 버튼: 마우스가 위에 있으면(Rect.Contains) 노란색으로 그리고, 투명한 GUI.Button으로 클릭을 받는다
    private bool MenuButton(Rect rect, string text)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        UIStyle.Label(rect, text, UIStyle.Strong(40, hover ? UIStyle.yellow : UIStyle.white), this.menuAlpha);
        return GUI.Button(rect, "", GUIStyle.none);
    }
}
