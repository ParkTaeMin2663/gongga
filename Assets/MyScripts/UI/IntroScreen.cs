using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// 인트로 자막: "시작하기"를 누르면 검은 화면에 이야기를 한 줄씩 보여 준 뒤 게임을 시작한다.
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

    public static bool isPlaying = false;

    private TitleScreen titleScreen;
    private bool waitingForTitle;
    private float[] lineAlpha;
    private float screenAlpha;
    private float textAlpha;
    private float startTime;
    private bool skipped;

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

        bool titleWillShow = this.titleScreen != null && this.titleScreen.showTitle && TitleScreen.hasStarted == false;
        if (this.showIntro && titleWillShow)
        {
            this.waitingForTitle = true;
        }
    }

    private void Update()
    {
        if (this.waitingForTitle && TitleScreen.hasStarted)
        {
            this.waitingForTitle = false;
            this.StartCoroutine(this.PlayIntro());
        }

        if (isPlaying == false)
        {
            return;
        }

        this.SetPlayerFrozen(true);

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

    private IEnumerator PlayIntro()
    {
        isPlaying = true;
        this.startTime = Time.time;
        this.screenAlpha = 1f;
        this.textAlpha = 1f;

        for (int i = 0; i < this.lines.Length && this.skipped == false; i++)
        {
            while (this.lineAlpha[i] < 1f && this.skipped == false)
            {
                this.lineAlpha[i] = this.lineAlpha[i] + Time.deltaTime / this.lineFadeTime;
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

        while (this.textAlpha > 0f)
        {
            this.textAlpha = this.textAlpha - Time.deltaTime / 1f;
            yield return null;
        }
        this.textAlpha = 0f;

        isPlaying = false;
        this.SetPlayerFrozen(false);
        while (this.screenAlpha > 0f)
        {
            this.screenAlpha = this.screenAlpha - Time.deltaTime / this.fadeOutTime;
            yield return null;
        }
        this.screenAlpha = 0f;
    }

    private void SetPlayerFrozen(bool frozen)
    {
        if (this.playerMovement != null) this.playerMovement.enabled = !frozen;
        if (this.playerInteract != null) this.playerInteract.enabled = !frozen;
        if (this.playerPickup != null) this.playerPickup.enabled = !frozen;
        if (this.playerLook != null) this.playerLook.IsLocked = frozen;
    }

    private void OnGUI()
    {
        if (this.screenAlpha <= 0f)
        {
            return;
        }

        GUI.depth = -10;

        GUI.color = new Color(0f, 0f, 0f, this.screenAlpha);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (this.textAlpha <= 0f)
        {
            return;
        }

        float gap = UIStyle.Size(UIStyle.Text(this.lineGap));
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

        GUIStyle small = UIStyle.Make(UIStyle.regular, UIStyle.Text(26), UIStyle.gray, TextAnchor.LowerRight);
        float m = UIStyle.Size(40);
        UIStyle.Label(new Rect(0f, 0f, Screen.width - m, Screen.height - m), "아무 키나 누르면 건너뜁니다", small, 0.7f * this.textAlpha);
    }
}
