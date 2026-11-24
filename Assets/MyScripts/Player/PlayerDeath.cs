using System.Collections;          // 강의 05장: 코루틴(IEnumerator)
using UnityEngine;
using UnityEngine.AI;              // 강의 07장: NavMeshAgent (덮칠 때 잠깐 끈다)
using UnityEngine.SceneManagement; // 강의 04장: SceneManager.LoadScene

/// <summary>
/// 플레이어 사망과 다시 시작.
///   - 괴물에게 잡히면 Die() → 몸이 굳고 괴물 쪽으로 고개가 홱 돌아간다
///   - 점프스케어(11/18): 괴물이 얼굴 앞으로 덮치며 비명(Scream 동작은 MonsterAI가 튼다) + 화면 흔들림 → 뚝 까매짐
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
            this.menuAlpha = Mathf.Min(1f, this.menuAlpha + Time.deltaTime * 1.5f);
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

    // 괴물을 카메라 바로 앞으로 끌어와 얼굴을 들이민다 + 비명 + 화면 흔들림
    private IEnumerator JumpscareRoutine(Transform killer)
    {
        // 괴물은 NavMeshAgent가 위치를 잡고 있으므로 잠깐 끈다 (강의 07장 NavMeshAgent, 04장 enabled)
        NavMeshAgent agent = killer.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }

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

        if (this.jumpscareSound != null)
        {
            AudioSource.PlayClipAtPoint(this.jumpscareSound, camPos, 1f);   // 강의 07장: 귀 바로 앞에서 비명
        }

        // 덮쳐 온다 (아주 빠르게)
        float time = 0f;
        while (time < this.lungeTime)
        {
            time = time + Time.deltaTime;
            killer.position = Vector3.Lerp(startPos, endPos, time / this.lungeTime);
            yield return null;
        }
        killer.position = endPos;

        // 비명 지르는 동안 화면이 덜덜 떨린다 (강의 03장 Random.Range, 04장 localPosition)
        Vector3 camLocal = this.cameraTransform.localPosition;
        time = 0f;
        while (time < this.screamTime)
        {
            time = time + Time.deltaTime;
            float amount = this.shakeAmount * (1f - time / this.screamTime * 0.5f);
            this.cameraTransform.localPosition = camLocal + new Vector3(
                Random.Range(-amount, amount), Random.Range(-amount, amount), 0f);
            this.redAmount = Mathf.Clamp01(time / this.screamTime) * 0.6f;   // 붉게 번짐
            yield return null;
        }
        this.cameraTransform.localPosition = camLocal;
    }

    // "다시 시작 / 그만하기" 화면을 띄운다. 마우스 커서를 보이게 한다 ([강의 외] Cursor, PlayerLook과 같은 방식)
    private void ShowMenu()
    {
        this.redAmount = 0f;
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
        if (this.blackAmount > 0f)
        {
            GUI.color = new Color(0f, 0f, 0f, this.blackAmount);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), this.whiteTexture);
        }
        GUI.color = Color.white;

        if (this.showMenu)
        {
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

    // 까만 화면 가운데: 큰 빨간 제목 + 버튼 두 개 (공통 UI 스타일, 마우스를 올리면 노란색)
    private void DrawMenu()
    {
        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;

        // 제목
        float titleHeight = UIStyle.Size(110);
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(200), Screen.width, titleHeight), "잡혔다", UIStyle.Warning(90), this.menuAlpha);

        // 체크포인트 안내 (흐린 글자)
        string where = Checkpoint.savedOrder > 0 ? "마지막 체크포인트에서 다시 시작합니다" : "처음부터 다시 시작합니다";
        GUIStyle small = UIStyle.Make(UIStyle.regular, 22, UIStyle.gray, TextAnchor.MiddleCenter);
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(80), Screen.width, UIStyle.Size(40)), where, small, this.menuAlpha);

        if (this.menuAlpha < 0.5f)
        {
            return;   // 반쯤 나타난 뒤부터 누를 수 있다 (실수로 바로 누르지 않게)
        }

        // 버튼 두 개: 글자만 보이고, 마우스를 올리면 노란색
        float width = UIStyle.Size(320);
        float height = UIStyle.Size(64);
        if (this.MenuButton(new Rect(cx - width / 2f, cy + UIStyle.Size(10), width, height), "다시 시작"))
        {
            this.Restart();
        }
        if (this.MenuButton(new Rect(cx - width / 2f, cy + UIStyle.Size(90), width, height), "그만하기"))
        {
            this.Quit();
        }
    }

    // 글자 버튼: 마우스가 위에 있으면(Rect.Contains) 노란색으로 그리고, 투명한 GUI.Button으로 클릭을 받는다
    private bool MenuButton(Rect rect, string text)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        UIStyle.Label(rect, text, UIStyle.Strong(40, hover ? UIStyle.yellow : UIStyle.white), this.menuAlpha);
        return GUI.Button(rect, "", GUIStyle.none);
    }
}
