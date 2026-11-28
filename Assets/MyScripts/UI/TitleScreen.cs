using System.Collections;          // 강의 05장: 코루틴(IEnumerator)
using UnityEngine;

/// <summary>
/// 타이틀 화면. (게임은 SampleScene에서 시작해야 하므로 별도 씬 없이 SampleScene 위에 그린다)
///   - Play를 누르면 불투명한 배경 그림(눈 오는 밤, 302호만 불 켜진 아파트) + 큰 빨간 "공가" + "시작하기 / 조작법 / 그만하기"
///   - 배경 그림은 아주 천천히 커졌다 작아진다 (은은하게 움직이는 느낌)
///   - "조작법" → 키 안내 표가 뜨고 "돌아가기"로 닫는다
///   - 마우스 커서가 보이고, 플레이어는 움직이거나 둘러볼 수 없다
///   - "시작하기" → 막이 서서히 걷히며 게임 시작 (커서 숨김)
///   - 죽어서 다시 시작할 때는 다시 뜨지 않는다 (hasStarted = public static, 강의 07장)
///   - 엔딩에서 "처음으로"를 누르면 EndingScreen이 hasStarted를 false로 → 다시 타이틀부터
///
/// [붙이는 곳] SampleScene의 빈 오브젝트 "TitleScreen"
/// [같이 쓰는 것] UIStyle.cs (Player 프리팹에 붙어 있음 — 글꼴·색), PlayerMovement/PlayerLook/PlayerInteract (Player)
/// [테스트 팁] 매번 타이틀을 보기 귀찮으면 Inspector에서 Show Title 체크를 끈다 (제출 전에 다시 켜기!)
/// </summary>
public class TitleScreen : MonoBehaviour
{
    [Header("켜기/끄기")]
    [Tooltip("체크를 끄면 타이틀 없이 바로 게임 시작 (테스트용). 제출 전에 꼭 다시 켠다")]
    public bool showTitle = true;

    [Header("글자")]
    [Tooltip("큰 제목")]
    public string gameTitle = "공가";
    [Tooltip("제목 아래 작은 글자")]
    public string subTitle = "空家";
    [Tooltip("한 줄 소개 (흐린 글자)")]
    public string tagline = "철거 일주일 전, 모두 비었어야 할 단지에 불 켜진 집이 하나 있다";

    [Header("조작법 화면 (강의 05장 public 배열 — Inspector에서 줄을 고치거나 늘릴 수 있다)")]
    [Tooltip("왼쪽 칸: 키 (노란색)")]
    public string[] controlKeys = { "W A S D", "Shift", "마우스", "F", "E", "E 1초 꾹", "마우스 왼쪽 (누른 채 끌기)", "마우스 오른쪽", "Q / E" };
    [Tooltip("오른쪽 칸: 설명 (controlKeys와 같은 순서·개수)")]
    public string[] controlDescs = { "이동", "달리기 (소리가 커서 괴물이 듣는다)", "둘러보기", "손전등 켜기·끄기 (배터리가 닳는다)", "줍기 · 단서 읽기", "장롱에 숨기 / 한 번 더 누르면 나오기", "문 · 서랍 · 레버 끌기, 물건 들기", "들고 있는 물건 던지기 (괴물 유인)", "단서 읽을 때 이전 장 / 다음 장" };

    [Header("배경 그림")]
    [Tooltip("타이틀 뒤에 깔 그림 (title_background). 비워 두면 그냥 검은 화면")]
    public Texture2D backgroundImage;
    [Tooltip("배경 그림 밝기 (1 = 원래 밝기). 글자가 잘 보이게 은은하게")]
    [Range(0f, 1f)] public float backgroundBrightness = 0.85f;
    [Tooltip("배경이 천천히 커지는 정도 (0.04 = 4%까지). 0이면 멈춰 있음")]
    public float zoomAmount = 0.04f;
    [Tooltip("한 번 커졌다 작아지는 데 걸리는 시간(초)")]
    public float zoomTime = 40f;

    [Header("크기 (1080p 기준)")]
    public int titleSize = 150;
    public int buttonSize = 40;

    [Header("연출")]
    [Tooltip("시작을 누른 뒤 어두운 막이 걷히는 시간(초)")]
    public float fadeTime = 1.5f;

    // 강의 07장 public static: 씬을 다시 불러와도(죽어서 다시 시작) 값이 남는다 → 타이틀을 다시 띄우지 않는다
    public static bool hasStarted = false;

    private bool isShowing;          // 타이틀이 떠 있는지 (버튼을 누를 수 있는지)
    private float alpha;             // 화면 전체 투명도 (1 = 다 보임 → 0 = 사라짐)
    private bool showingControls;    // 조작법 표가 떠 있는지

    // 강의 04장: Find는 Start()에서 한 번만
    private PlayerMovement playerMovement;
    private PlayerLook playerLook;
    private PlayerInteract playerInteract;
    private PlayerPickup playerPickup;

    private void Start()
    {
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            this.playerMovement = playerObject.GetComponent<PlayerMovement>();
            this.playerLook = playerObject.GetComponent<PlayerLook>();
            this.playerInteract = playerObject.GetComponent<PlayerInteract>();
            this.playerPickup = playerObject.GetComponent<PlayerPickup>();
        }
        else
        {
            Debug.LogError("[TitleScreen] 씬에서 'Player'를 찾지 못했습니다.");
        }

        if (this.showTitle == false || hasStarted)
        {
            hasStarted = true;
            this.alpha = 0f;
            return;   // 바로 게임
        }

        this.isShowing = true;
        this.alpha = 1f;
        this.SetPlayerFrozen(true);
    }

    private void Update()
    {
        if (this.isShowing)
        {
            // PlayerLook.Start가 커서를 숨기므로, 타이틀이 떠 있는 동안은 매 프레임 다시 보이게 한다
            // ([강의 외] Cursor — PlayerLook, PlayerDeath와 같은 방식)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // 플레이어 걷기·시점·잡기·줍기를 멈추거나 다시 켠다 (강의 04장 enabled)
    private void SetPlayerFrozen(bool frozen)
    {
        if (this.playerMovement != null) this.playerMovement.enabled = !frozen;
        if (this.playerInteract != null) this.playerInteract.enabled = !frozen;
        if (this.playerPickup != null) this.playerPickup.enabled = !frozen;
        if (this.playerLook != null) this.playerLook.IsLocked = frozen;
    }

    // "시작하기"를 눌렀을 때
    private void StartGame()
    {
        this.isShowing = false;
        hasStarted = true;
        Cursor.lockState = CursorLockMode.Locked;   // [강의 외] Cursor
        Cursor.visible = false;
        this.SetPlayerFrozen(false);
        this.StartCoroutine(this.FadeOut());
    }

    // 강의 05장 코루틴: 막이 서서히 걷힌다
    private IEnumerator FadeOut()
    {
        while (this.alpha > 0f)
        {
            this.alpha = this.alpha - Time.deltaTime / this.fadeTime;   // 강의 03장 Time.deltaTime
            yield return null;
        }
        this.alpha = 0f;
    }

    // 그만하기: 게임 종료 ([강의 외] Application.Quit — PlayerDeath와 같은 방식, 11/18 허락)
    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    // 강의 04장 OnGUI
    private void OnGUI()
    {
        if (this.alpha <= 0f)
        {
            return;
        }

        // 불투명한 검은 바탕 (뒤의 게임 화면이 비치지 않게)
        GUI.color = new Color(0f, 0f, 0f, this.alpha);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);

        // 배경 그림: 화면을 꽉 채우고(ScaleAndCrop) 아주 천천히 커졌다 작아진다
        if (this.backgroundImage != null)
        {
            // Mathf.PingPong: 0 → 1 → 0 을 반복 (강의 03장 Time과 같은 방식으로 시간에 따라 값이 변함)
            float zoom = 1f + this.zoomAmount * Mathf.PingPong(Time.time / this.zoomTime * 2f, 1f);
            float w = Screen.width * zoom;
            float h = Screen.height * zoom;
            Rect bgRect = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
            float b = this.backgroundBrightness;
            GUI.color = new Color(b, b, b, this.alpha);
            GUI.DrawTexture(bgRect, this.backgroundImage, ScaleMode.ScaleAndCrop);
        }
        GUI.color = Color.white;

        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;

        if (this.showingControls)
        {
            this.DrawControls(cx, cy);
            return;
        }

        // 큰 빨간 제목 + 한자
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(300), Screen.width, UIStyle.Size(180)), this.gameTitle, UIStyle.Title(this.titleSize), this.alpha);
        GUIStyle subStyle = UIStyle.Hanja(36, UIStyle.white);   // 空家: 동해독도에 한자가 없어 본명조 (11/24)
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(125), Screen.width, UIStyle.Size(50)), this.subTitle, subStyle, this.alpha);

        // 한 줄 소개
        GUIStyle small = UIStyle.Make(UIStyle.regular, UIStyle.Text(28), UIStyle.gray, TextAnchor.MiddleCenter);   // (11/28) 작아서 키움
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(75), Screen.width, UIStyle.Size(60)), this.tagline, small, this.alpha);

        if (this.isShowing == false)
        {
            return;   // 사라지는 중에는 버튼을 누를 수 없다
        }

        // 버튼 세 개 (PlayerDeath의 "다시 시작 / 그만하기"와 같은 모양)
        float width = UIStyle.Size(320);
        float height = UIStyle.Size(64);
        if (this.MenuButton(new Rect(cx - width / 2f, cy + UIStyle.Size(40), width, height), "시작하기"))
        {
            this.StartGame();
        }
        if (this.MenuButton(new Rect(cx - width / 2f, cy + UIStyle.Size(120), width, height), "조작법"))
        {
            this.showingControls = true;
        }
        if (this.MenuButton(new Rect(cx - width / 2f, cy + UIStyle.Size(200), width, height), "그만하기"))
        {
            this.Quit();
        }
    }

    // 조작법 표: 왼쪽 키(노란색) / 오른쪽 설명, 맨 아래 "돌아가기"
    private void DrawControls(float cx, float cy)
    {
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(380), Screen.width, UIStyle.Size(90)), "조작법", UIStyle.Title(70), this.alpha);

        GUIStyle keyStyle = UIStyle.Make(UIStyle.bold, UIStyle.Text(28), UIStyle.yellow, TextAnchor.MiddleRight);
        GUIStyle descStyle = UIStyle.Make(UIStyle.regular, UIStyle.Text(28), UIStyle.white, TextAnchor.MiddleLeft);
        float lineHeight = UIStyle.Size(UIStyle.Text(46));   // 글자가 커져서 줄 간격은 살짝 좁혀 아래 "돌아가기"와 안 겹치게
        float top = cy - UIStyle.Size(260);
        float gap = UIStyle.Size(40);   // 키와 설명 사이 간격
        int count = Mathf.Min(this.controlKeys.Length, this.controlDescs.Length);
        for (int i = 0; i < count; i++)
        {
            float y = top + lineHeight * i;
            UIStyle.Label(new Rect(0f, y, cx - gap / 2f, lineHeight), this.controlKeys[i], keyStyle, this.alpha);
            UIStyle.Label(new Rect(cx + gap / 2f, y, Screen.width - cx, lineHeight), this.controlDescs[i], descStyle, this.alpha);
        }

        GUIStyle small = UIStyle.Make(UIStyle.regular, UIStyle.Text(28), UIStyle.gray, TextAnchor.MiddleCenter);   // (11/28) 작아서 키움
        UIStyle.Label(new Rect(0f, top + lineHeight * count + UIStyle.Size(10), Screen.width, UIStyle.Size(100)),
            "괴물은 눈이 없다. 소리를 내지 마라.\n괴물은 빛을 보지 못한다.", small, this.alpha);   // (11/28) 두 줄

        float width = UIStyle.Size(320);
        float height = UIStyle.Size(64);
        if (this.MenuButton(new Rect(cx - width / 2f, Screen.height - UIStyle.Size(150), width, height), "돌아가기"))
        {
            this.showingControls = false;
        }
    }

    // 글자 버튼: 마우스가 위에 있으면(Rect.Contains) 노란색, 투명한 GUI.Button으로 클릭을 받는다 (PlayerDeath와 같은 방식)
    private bool MenuButton(Rect rect, string text)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        UIStyle.Label(rect, text, UIStyle.Strong(this.buttonSize, hover ? UIStyle.yellow : UIStyle.white), this.alpha);
        return GUI.Button(rect, "", GUIStyle.none);
    }
}
