using System.Collections;
using UnityEngine;

// 타이틀 화면. (게임은 SampleScene에서 시작해야 하므로 별도 씬 없이 SampleScene 위에 그린다)
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

    public static bool hasStarted = false;   // 강의 07장: public static 변수

    private bool isShowing;
    private float alpha;
    private bool showingControls;

    private PlayerMovement playerMovement;
    private PlayerLook playerLook;
    private PlayerInteract playerInteract;
    private PlayerPickup playerPickup;

    private void Start()
    {
        GameObject playerObject = GameObject.Find("Player");   // 강의 04장: GameObject.Find (Start에서 한 번)
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
            return;
        }

        this.isShowing = true;
        this.alpha = 1f;
        this.SetPlayerFrozen(true);
    }

    private void Update()
    {
        if (this.isShowing)
        {
            // [강의 외] Cursor.lockState / Cursor.visible: 메뉴에서 마우스 커서 보이기
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void SetPlayerFrozen(bool frozen)
    {
        if (this.playerMovement != null) this.playerMovement.enabled = !frozen;
        if (this.playerInteract != null) this.playerInteract.enabled = !frozen;
        if (this.playerPickup != null) this.playerPickup.enabled = !frozen;
        if (this.playerLook != null) this.playerLook.IsLocked = frozen;
    }

    private void StartGame()
    {
        this.isShowing = false;
        hasStarted = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        this.SetPlayerFrozen(false);
        this.StartCoroutine(this.FadeOut());   // 강의 05장: 코루틴 StartCoroutine
    }

    private IEnumerator FadeOut()
    {
        while (this.alpha > 0f)
        {
            this.alpha = this.alpha - Time.deltaTime / this.fadeTime;   // 강의 03장: Time.deltaTime
            yield return null;
        }
        this.alpha = 0f;
    }

    // [강의 외] Application.Quit: 게임 종료
    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    private void OnGUI()   // 강의 04장: OnGUI
    {
        if (this.alpha <= 0f)
        {
            return;
        }

        GUI.color = new Color(0f, 0f, 0f, this.alpha);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);

        if (this.backgroundImage != null)
        {
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

        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(300), Screen.width, UIStyle.Size(180)), this.gameTitle, UIStyle.Title(this.titleSize), this.alpha);
        GUIStyle subStyle = UIStyle.Hanja(36, UIStyle.white);
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(125), Screen.width, UIStyle.Size(50)), this.subTitle, subStyle, this.alpha);

        GUIStyle small = UIStyle.Make(UIStyle.regular, UIStyle.Text(28), UIStyle.gray, TextAnchor.MiddleCenter);
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(75), Screen.width, UIStyle.Size(60)), this.tagline, small, this.alpha);

        if (this.isShowing == false)
        {
            return;
        }

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

    private void DrawControls(float cx, float cy)
    {
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(380), Screen.width, UIStyle.Size(90)), "조작법", UIStyle.Title(70), this.alpha);

        GUIStyle keyStyle = UIStyle.Make(UIStyle.bold, UIStyle.Text(28), UIStyle.yellow, TextAnchor.MiddleRight);
        GUIStyle descStyle = UIStyle.Make(UIStyle.regular, UIStyle.Text(28), UIStyle.white, TextAnchor.MiddleLeft);
        float lineHeight = UIStyle.Size(UIStyle.Text(46));
        float top = cy - UIStyle.Size(260);
        float gap = UIStyle.Size(40);
        int count = Mathf.Min(this.controlKeys.Length, this.controlDescs.Length);
        for (int i = 0; i < count; i++)
        {
            float y = top + lineHeight * i;
            UIStyle.Label(new Rect(0f, y, cx - gap / 2f, lineHeight), this.controlKeys[i], keyStyle, this.alpha);
            UIStyle.Label(new Rect(cx + gap / 2f, y, Screen.width - cx, lineHeight), this.controlDescs[i], descStyle, this.alpha);
        }

        GUIStyle small = UIStyle.Make(UIStyle.regular, UIStyle.Text(28), UIStyle.gray, TextAnchor.MiddleCenter);
        UIStyle.Label(new Rect(0f, top + lineHeight * count + UIStyle.Size(10), Screen.width, UIStyle.Size(100)),
            "괴물은 눈이 없다. 소리를 내지 마라.\n괴물은 빛을 보지 못한다.", small, this.alpha);

        float width = UIStyle.Size(320);
        float height = UIStyle.Size(64);
        if (this.MenuButton(new Rect(cx - width / 2f, Screen.height - UIStyle.Size(150), width, height), "돌아가기"))
        {
            this.showingControls = false;
        }
    }

    private bool MenuButton(Rect rect, string text)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        UIStyle.Label(rect, text, UIStyle.Strong(this.buttonSize, hover ? UIStyle.yellow : UIStyle.white), this.alpha);
        return GUI.Button(rect, "", GUIStyle.none);
    }
}
