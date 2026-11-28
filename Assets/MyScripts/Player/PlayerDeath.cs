using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// 플레이어 사망 연출(점프스케어)과 마지막 체크포인트에서 다시 시작.
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

    public bool IsDead { get; private set; }

    private float redAmount;
    private float flashAmount;
    private float edgeAmount;
    private float closeAmount;
    private float strobeBlack;
    private float noiseAmount;
    private string whisper;
    private string hint;
    private float menuTime;
    private float blackAmount;
    private bool showMenu;
    private float menuAlpha;
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
            Checkpoint.isRespawning = false;
            this.transform.position = Checkpoint.savedPosition;
            if (this.playerLook != null)
            {
                this.playerLook.SetYaw(Checkpoint.savedYaw);
            }
            this.blackAmount = 1f;

            PlayerFlashlight flashlight = this.GetComponent<PlayerFlashlight>();
            if (flashlight != null)
            {
                flashlight.hasFlashlight = Checkpoint.savedHasFlashlight;
                flashlight.battery = Checkpoint.savedBattery;
            }
        }
        else
        {
            Checkpoint.savedOrder = 0;
            Checkpoint.lastSaveTime = -100f;
        }
    }

    private void Update()
    {
        if (this.IsDead == false && this.blackAmount > 0f)
        {
            this.blackAmount = Mathf.Max(0f, this.blackAmount - Time.deltaTime / this.fadeTime);   // 강의 03장: Time.deltaTime
        }

        if (this.showMenu)
        {
            this.menuAlpha = Mathf.Min(1f, this.menuAlpha + Time.deltaTime * 0.8f);
            this.menuTime = this.menuTime + Time.deltaTime;
        }
    }

    public void Die(Transform killer)
    {
        if (this.IsDead)
        {
            return;
        }
        this.IsDead = true;

        if (this.playerMovement != null) this.playerMovement.enabled = false;
        if (this.playerInteract != null) this.playerInteract.enabled = false;
        if (this.playerLook != null) this.playerLook.IsLocked = true;
        Rigidbody rb = this.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;   // 강의 03장: Rigidbody Is Kinematic
        }

        if (this.deathSound != null)
        {
            this.deathSound.Play();   // 강의 03장: AudioSource.Play
        }

        if (this.deathWhispers != null && this.deathWhispers.Length > 0)
        {
            this.whisper = this.deathWhispers[Random.Range(0, this.deathWhispers.Length)];   // 강의 03장: Random.Range
        }
        if (this.floorHints != null && this.floorHints.Length >= 4)
        {
            float y = this.transform.position.y;
            int floor = y < -1.5f ? 3 : (y > 4.5f ? 2 : (y > 1.5f ? 1 : 0));
            this.hint = this.floorHints[floor];
        }

        this.StartCoroutine(this.DeathRoutine(killer));   // 강의 05장: 코루틴 StartCoroutine
    }

    private IEnumerator DeathRoutine(Transform killer)
    {
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
            yield return null;
        }
        if (this.playerLook != null)
        {
            this.playerLook.SetYaw(targetYaw);
        }

        if (this.useJumpscare && killer != null && this.cameraTransform != null)
        {
            yield return this.StartCoroutine(this.JumpscareRoutine(killer));
            this.blackAmount = 1f;
        }
        else
        {
            time = 0f;
            while (time < this.fadeTime)
            {
                time = time + Time.deltaTime;
                float rate = time / this.fadeTime;
                this.redAmount = Mathf.Clamp01(rate * 2f);
                this.blackAmount = Mathf.Clamp01(rate * 2f - 1f);
                yield return null;
            }
        }

        yield return new WaitForSeconds(this.blackTime);   // 강의 05장: WaitForSeconds
        this.ShowMenu();
    }

    private IEnumerator JumpscareRoutine(Transform killer)
    {
        NavMeshAgent agent = killer.GetComponent<NavMeshAgent>();   // 강의 07장: NavMeshAgent
        if (agent != null)
        {
            agent.enabled = false;
        }

        float yaw = this.playerLook != null ? this.playerLook.Yaw : this.cameraTransform.eulerAngles.y;

        Vector3 forward = this.cameraTransform.forward;
        forward.y = 0f;
        forward = forward.normalized;
        Vector3 camPos = this.cameraTransform.position;
        Vector3 endPos = camPos + forward * this.jumpscareDistance + Vector3.down * this.monsterFaceHeight;
        Vector3 startPos = killer.position;

        Vector3 lookPoint = new Vector3(camPos.x, killer.position.y, camPos.z);
        killer.LookAt(lookPoint);   // 강의 07장: transform.LookAt

        float time = 0f;
        while (time < this.freezeTime)
        {
            time = time + Time.deltaTime;
            float k = Mathf.Clamp01(time / this.freezeTime);
            this.cameraTransform.rotation = Quaternion.Euler(Mathf.Lerp(0f, this.lookUpPitch, k), yaw, 0f);   // 강의 03장: Quaternion.Euler
            this.edgeAmount = k * 0.3f;
            yield return null;
        }

        if (this.jumpscareSound != null)
        {
            AudioSource.PlayClipAtPoint(this.jumpscareSound, camPos, 1f);   // 강의 07장: AudioSource.PlayClipAtPoint
        }

        time = 0f;
        while (time < this.lungeTime)
        {
            time = time + Time.deltaTime;
            float k = Mathf.Clamp01(time / this.lungeTime);
            killer.position = Vector3.Lerp(startPos, endPos, k * k);
            yield return null;
        }
        killer.position = endPos;

        this.flashAmount = this.impactFlash;
        this.noiseAmount = this.staticAmount;
        for (int i = 0; i < this.strobeCount; i++)
        {
            this.strobeBlack = 1f;
            yield return new WaitForSeconds(0.04f + i * 0.02f);
            this.strobeBlack = 0f;
            this.flashAmount = this.flashAmount * 0.5f;
            yield return new WaitForSeconds(0.06f);
        }

        Vector3 camLocal = this.cameraTransform.localPosition;
        time = 0f;
        while (time < this.screamTime)
        {
            time = time + Time.deltaTime;
            float k = Mathf.Clamp01(time / this.screamTime);
            float amount = 1f - k * 0.6f;
            this.cameraTransform.localPosition = camLocal + new Vector3(
                Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * this.shakeAmount * amount;
            this.cameraTransform.rotation = Quaternion.Euler(
                this.lookUpPitch + Random.Range(-1f, 1f) * this.shakeAngle * amount,
                yaw + Random.Range(-1f, 1f) * this.shakeAngle * amount,
                Random.Range(-1f, 1f) * this.shakeAngle * 1.5f * amount);
            this.flashAmount = Mathf.Max(0f, this.flashAmount - Time.deltaTime * 5f);
            this.edgeAmount = 0.3f + 0.7f * Mathf.Clamp01(k * 2f);
            this.redAmount = k * 0.35f;
            this.noiseAmount = this.staticAmount * (1f - k * 0.7f);
            yield return null;
        }
        this.flashAmount = 0f;

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
            float fall = 1f - (1f - k) * (1f - k);
            this.cameraTransform.localPosition = Vector3.Lerp(fallStart, fallEnd, fall);
            this.noiseAmount = this.staticAmount * 0.3f * (1f - k);
            this.cameraTransform.rotation = Quaternion.Euler(
                Mathf.Lerp(this.lookUpPitch, this.collapsePitch, fall), yaw, Mathf.Lerp(0f, this.collapseRoll, fall));
            yield return null;
        }

        time = 0f;
        while (time < this.lyingTime)
        {
            time = time + Time.deltaTime;
            this.closeAmount = Mathf.Clamp01(time / this.lyingTime);
            yield return null;
        }
    }

    // [강의 외] Cursor.lockState / Cursor.visible: 메뉴에서 마우스 커서 보이기
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

    private void Restart()
    {
        Checkpoint.isRespawning = Checkpoint.savedOrder > 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);   // 강의 04장: SceneManager.LoadScene
    }

    // [강의 외] Application.Quit: 게임 종료
    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnGUI()   // 강의 04장: OnGUI
    {
        if (this.redAmount > 0f)
        {
            GUI.color = new Color(0.5f, 0f, 0f, this.redAmount * 0.7f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), this.whiteTexture);
        }
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
            UIStyle.MenuBackdrop(this.menuAlpha, new Color(0.22f, 0.02f, 0.02f, 1f));
            this.DrawMenu();
            return;
        }

        float ago = Time.time - Checkpoint.lastSaveTime;
        if (Checkpoint.lastSaveShowsMessage && ago >= 0f && ago < 2f)
        {
            GUIStyle style = UIStyle.Make(UIStyle.regular, UIStyle.Text(26), UIStyle.gray, TextAnchor.LowerRight);
            Rect rect = new Rect(0f, 0f, Screen.width - UIStyle.Size(32), Screen.height - UIStyle.Size(28));
            UIStyle.Label(rect, "저장됨", style, 1f - ago / 2f);
        }
    }

    private void DrawMenu()
    {
        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;
        Rect screen = new Rect(0f, 0f, Screen.width, Screen.height);

        float beatSpeed = Mathf.Lerp(2.2f, 0.9f, Mathf.Clamp01(this.menuTime / 6f));
        float phase = (this.menuTime * beatSpeed) % 1f;
        float beat = Mathf.Max(0f, 1f - Mathf.Abs(phase - 0.1f) * 12f) + 0.6f * Mathf.Max(0f, 1f - Mathf.Abs(phase - 0.3f) * 12f);
        UIStyle.EdgeShade(screen, new Color(0.5f, 0f, 0f, (0.25f + 0.45f * beat) * this.menuAlpha), 0.3f);

        float whisperAlpha = Mathf.Clamp01(this.menuTime / 1.5f);
        if (string.IsNullOrEmpty(this.whisper) == false)
        {
            GUIStyle whisperStyle = UIStyle.Make(UIStyle.regular, 40, UIStyle.white, TextAnchor.MiddleCenter);
            float jitter = UIStyle.Size(2) * (1f - whisperAlpha * 0.5f);
            Rect wr = new Rect(Random.Range(-jitter, jitter), cy - UIStyle.Size(150) + Random.Range(-jitter, jitter), Screen.width, UIStyle.Size(60));
            UIStyle.Label(wr, this.whisper, whisperStyle, whisperAlpha * 0.9f);
        }

        float hintAlpha = Mathf.Clamp01((this.menuTime - 1.2f) / 1f);
        if (string.IsNullOrEmpty(this.hint) == false && hintAlpha > 0f)
        {
            GUIStyle hintStyle = UIStyle.Make(UIStyle.regular, 26, UIStyle.yellow, TextAnchor.MiddleCenter);
            UIStyle.Label(new Rect(0f, cy - UIStyle.Size(80), Screen.width, UIStyle.Size(40)), this.hint, hintStyle, hintAlpha * 0.85f);
        }

        float menuShow = Mathf.Clamp01((this.menuTime - this.menuDelay) / 0.8f);
        if (menuShow <= 0f)
        {
            return;
        }

        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(330), Screen.width, UIStyle.Size(110)), "잡혔다", UIStyle.Warning(90), menuShow);

        string where = Checkpoint.savedOrder > 0 ? "마지막 체크포인트에서 다시 시작합니다" : "처음부터 다시 시작합니다";
        GUIStyle small = UIStyle.Make(UIStyle.regular, UIStyle.Text(28), UIStyle.gray, TextAnchor.MiddleCenter);
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(30), Screen.width, UIStyle.Size(60)), where, small, menuShow);

        if (menuShow < 0.5f)
        {
            return;
        }

        float width = UIStyle.Size(320);
        float height = UIStyle.Size(64);
        float oldAlpha = this.menuAlpha;
        this.menuAlpha = menuShow;
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

    private bool MenuButton(Rect rect, string text)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        UIStyle.Label(rect, text, UIStyle.Strong(40, hover ? UIStyle.yellow : UIStyle.white), this.menuAlpha);
        return GUI.Button(rect, "", GUIStyle.none);
    }
}
