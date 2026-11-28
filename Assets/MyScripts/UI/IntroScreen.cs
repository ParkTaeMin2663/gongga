using System.Collections;          // 강의 05장: 코루틴(IEnumerator)
using UnityEngine;
using UnityEngine.InputSystem;     // 강의 06장 Mouse.current처럼 Keyboard.current로 키 입력을 직접 읽는다

/// <summary>
/// 인트로 자막. 타이틀에서 "시작하기"를 누르면 검은 화면에 한 줄씩 자막이 나타나고,
/// 다 보여 준 뒤 서서히 밝아지며 단지 입구(정면에 302호 불빛)에서 게임이 시작된다.
///   - 누구·왜·뭘 할지만 알려 준다 (30년 전 사건·반전은 단서로) — PROGRESS "A·B 상의할 것" ① 안
///   - 아무 키나 마우스 클릭 → 바로 건너뛰기
///   - 죽어서 다시 시작할 때는 나오지 않는다 (타이틀이 뜰 때만 같이 나옴)
///   - 자막이 나오는 동안 플레이어는 움직이거나 둘러볼 수 없다
///
/// [붙이는 곳] SampleScene의 빈 오브젝트 "IntroScreen"
/// [같이 쓰는 것] TitleScreen (hasStarted, showTitle), UIStyle (글꼴·그림자), PlayerMovement/PlayerLook/PlayerInteract/PlayerPickup
/// [테스트 팁] TitleScreen의 Show Title을 끄면 인트로도 같이 안 나온다. 인트로만 끄려면 Show Intro 체크 해제
/// </summary>
public class IntroScreen : MonoBehaviour
{
    [Header("켜기/끄기")]
    [Tooltip("체크를 끄면 인트로 자막 없이 바로 게임 (테스트용)")]
    public bool showIntro = true;

    [Header("자막 (강의 05장 public 배열 — 줄을 고치거나 늘릴 수 있다)")]
    [TextArea]
    [Tooltip("위에서부터 한 줄씩 나타난다")]
    public string[] lines =
    {
        "2024년 12월.",
        "철거를 일주일 앞둔 복도식 아파트 단지.",
        "나는 빈집 확인 용역이다.\n오늘 밤 마지막 점검만 끝내면 된다.",
        "주민은 모두 떠났고, 전기도 끊겼다.",
        "…이 동네, 어릴 때 와 본 것 같기도 하다.",
        "그런데 302호에 불이 켜져 있다."
    };
    [Tooltip("마지막 줄만 빨간색으로 (302호 불빛 강조)")]
    public bool lastLineRed = true;

    [Header("시간 (초)")]
    [Tooltip("한 줄이 서서히 나타나는 시간")]
    public float lineFadeTime = 1.0f;
    [Tooltip("한 줄이 다 나타난 뒤 다음 줄까지 기다리는 시간")]
    public float lineWaitTime = 1.6f;
    [Tooltip("마지막 줄까지 다 나온 뒤 기다리는 시간")]
    public float endWaitTime = 2.5f;
    [Tooltip("검은 화면이 걷히며 게임이 보이는 시간")]
    public float fadeOutTime = 2.5f;

    [Header("크기 (1080p 기준)")]
    public int fontSize = 40;
    public int lineGap = 90;

    // 강의 07장 public static: 다른 스크립트(TutorialHint)가 "인트로 중인지" 알 수 있게
    public static bool isPlaying = false;

    private TitleScreen titleScreen;
    private bool waitingForTitle;    // 타이틀에서 "시작하기"를 기다리는 중
    private float[] lineAlpha;       // 줄마다 투명도 (0 → 1)
    private float screenAlpha;       // 검은 화면 투명도 (1 = 까맘 → 0 = 게임이 보임)
    private float textAlpha;         // 자막 전체 투명도 (건너뛰거나 끝날 때 같이 사라짐)
    private float startTime;         // 인트로가 시작된 시각 (시작 버튼 클릭이 바로 건너뛰기로 읽히지 않게)
    private bool skipped;

    // 강의 04장: Find는 Start()에서 한 번만
    private PlayerMovement playerMovement;
    private PlayerLook playerLook;
    private PlayerInteract playerInteract;
    private PlayerPickup playerPickup;

    private void Start()
    {
        isPlaying = false;
        this.lineAlpha = new float[this.lines.Length];

        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            this.playerMovement = playerObject.GetComponent<PlayerMovement>();
            this.playerLook = playerObject.GetComponent<PlayerLook>();
            this.playerInteract = playerObject.GetComponent<PlayerInteract>();
            this.playerPickup = playerObject.GetComponent<PlayerPickup>();
        }

        GameObject titleObject = GameObject.Find("TitleScreen");
        if (titleObject != null)
        {
            this.titleScreen = titleObject.GetComponent<TitleScreen>();
        }

        // 타이틀이 뜨는 처음 시작일 때만 인트로를 보여 준다 (죽어서 다시 시작하면 hasStarted가 이미 true)
        bool titleWillShow = this.titleScreen != null && this.titleScreen.showTitle && TitleScreen.hasStarted == false;
        if (this.showIntro && titleWillShow)
        {
            this.waitingForTitle = true;
        }
    }

    private void Update()
    {
        // 타이틀에서 "시작하기"를 누른 순간 → 인트로 시작
        if (this.waitingForTitle && TitleScreen.hasStarted)
        {
            this.waitingForTitle = false;
            this.StartCoroutine(this.PlayIntro());
        }

        if (isPlaying == false)
        {
            return;
        }

        // TitleScreen이 "시작하기"에서 플레이어를 풀어 주므로, 인트로 동안은 매 프레임 다시 멈춘다
        this.SetPlayerFrozen(true);

        // 아무 키나 클릭 → 건너뛰기 (시작 버튼을 누른 클릭이 그대로 읽히지 않게 0.5초 뒤부터)
        if (this.skipped == false && Time.time - this.startTime > 0.5f)
        {
            bool keyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
            bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            if (keyPressed || mousePressed)
            {
                this.skipped = true;
            }
        }
    }

    // 강의 05장 코루틴: 한 줄씩 나타나기 → 기다리기 → 밝아지며 게임 시작
    private IEnumerator PlayIntro()
    {
        isPlaying = true;
        this.startTime = Time.time;
        this.screenAlpha = 1f;
        this.textAlpha = 1f;

        // 한 줄씩 나타나기 (건너뛰면 바로 끝으로)
        for (int i = 0; i < this.lines.Length && this.skipped == false; i++)
        {
            while (this.lineAlpha[i] < 1f && this.skipped == false)
            {
                this.lineAlpha[i] = this.lineAlpha[i] + Time.deltaTime / this.lineFadeTime;   // 강의 03장 Time.deltaTime
                yield return null;
            }
            this.lineAlpha[i] = 1f;

            float wait = (i == this.lines.Length - 1) ? this.endWaitTime : this.lineWaitTime;
            float t = 0f;
            while (t < wait && this.skipped == false)
            {
                t = t + Time.deltaTime;
                yield return null;
            }
        }

        // 자막이 먼저 사라지고 (1초)
        while (this.textAlpha > 0f)
        {
            this.textAlpha = this.textAlpha - Time.deltaTime / 1f;
            yield return null;
        }
        this.textAlpha = 0f;

        // 플레이어를 풀어 주고 검은 화면이 서서히 걷힌다
        isPlaying = false;
        this.SetPlayerFrozen(false);
        while (this.screenAlpha > 0f)
        {
            this.screenAlpha = this.screenAlpha - Time.deltaTime / this.fadeOutTime;
            yield return null;
        }
        this.screenAlpha = 0f;
    }

    // 플레이어 걷기·시점·잡기·줍기를 멈추거나 다시 켠다 (TitleScreen과 같은 방식, 강의 04장 enabled)
    private void SetPlayerFrozen(bool frozen)
    {
        if (this.playerMovement != null) this.playerMovement.enabled = !frozen;
        if (this.playerInteract != null) this.playerInteract.enabled = !frozen;
        if (this.playerPickup != null) this.playerPickup.enabled = !frozen;
        if (this.playerLook != null) this.playerLook.IsLocked = frozen;
    }

    // 강의 04장 OnGUI
    private void OnGUI()
    {
        if (this.screenAlpha <= 0f)
        {
            return;
        }

        // 다른 화면(타이틀이 사라지는 중인 것 등)보다 위에 그린다 (GUI.depth가 작을수록 위)
        GUI.depth = -10;

        // 검은 바탕
        GUI.color = new Color(0f, 0f, 0f, this.screenAlpha);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (this.textAlpha <= 0f)
        {
            return;
        }

        // 자막: 화면 가운데에 줄들을 세로로 늘어놓는다
        float gap = UIStyle.Size(UIStyle.Text(this.lineGap));   // 흰 안내 글자 배율만큼 줄 간격도
        float totalHeight = gap * this.lines.Length;
        float y = Screen.height / 2f - totalHeight / 2f;
        for (int i = 0; i < this.lines.Length; i++)
        {
            if (this.lineAlpha[i] > 0f)
            {
                bool isLast = i == this.lines.Length - 1;
                Color color = (isLast && this.lastLineRed) ? UIStyle.red : UIStyle.white;
                GUIStyle style = UIStyle.Make(UIStyle.regular, UIStyle.Text(this.fontSize), color, TextAnchor.MiddleCenter);
                UIStyle.Label(new Rect(0f, y, Screen.width, gap), this.lines[i], style, this.lineAlpha[i] * this.textAlpha);
            }
            y = y + gap;
        }

        // 오른쪽 아래 건너뛰기 안내
        GUIStyle small = UIStyle.Make(UIStyle.regular, UIStyle.Text(26), UIStyle.gray, TextAnchor.LowerRight);   // (11/28) 작아서 키움
        float m = UIStyle.Size(40);
        UIStyle.Label(new Rect(0f, 0f, Screen.width - m, Screen.height - m), "아무 키나 누르면 건너뜁니다", small, 0.7f * this.textAlpha);
    }
}
