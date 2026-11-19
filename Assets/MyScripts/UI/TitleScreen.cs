using System.Collections;          // 강의 05장: 코루틴(IEnumerator)
using UnityEngine;

/// <summary>
/// 타이틀 화면. (게임은 SampleScene에서 시작해야 하므로 별도 씬 없이 SampleScene 위에 그린다)
///   - Play를 누르면 게임 화면 위에 어두운 막 + 큰 빨간 "공가" + "시작하기 / 그만하기"
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
    [Tooltip("맨 아래 조작법 안내")]
    public string controls = "WASD 이동   Shift 달리기   F 손전등   마우스로 문·서랍 끌기   E 조사·숨기";

    [Header("크기 (1080p 기준)")]
    public int titleSize = 150;
    public int buttonSize = 40;

    [Header("연출")]
    [Tooltip("시작을 누른 뒤 어두운 막이 걷히는 시간(초)")]
    public float fadeTime = 1.5f;
    [Tooltip("어두운 막의 진하기 (0 = 투명, 1 = 새까맣게). 뒤로 게임 화면(302호 불빛)이 살짝 보이게")]
    [Range(0f, 1f)] public float darkness = 0.6f;

    // 강의 07장 public static: 씬을 다시 불러와도(죽어서 다시 시작) 값이 남는다 → 타이틀을 다시 띄우지 않는다
    public static bool hasStarted = false;

    private bool isShowing;          // 타이틀이 떠 있는지 (버튼을 누를 수 있는지)
    private float alpha;             // 화면 전체 투명도 (1 = 다 보임 → 0 = 사라짐)

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

        // 어두운 막
        GUI.color = new Color(0f, 0f, 0f, this.darkness * this.alpha);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;

        // 큰 빨간 제목 + 한자
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(300), Screen.width, UIStyle.Size(180)), this.gameTitle, UIStyle.Title(this.titleSize), this.alpha);
        GUIStyle subStyle = UIStyle.Make(UIStyle.regular, 36, UIStyle.white, TextAnchor.MiddleCenter);
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(125), Screen.width, UIStyle.Size(50)), this.subTitle, subStyle, this.alpha);

        // 한 줄 소개
        GUIStyle small = UIStyle.Make(UIStyle.regular, 22, UIStyle.gray, TextAnchor.MiddleCenter);
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(65), Screen.width, UIStyle.Size(40)), this.tagline, small, this.alpha);

        // 조작법 (맨 아래)
        UIStyle.Label(new Rect(0f, Screen.height - UIStyle.Size(80), Screen.width, UIStyle.Size(40)), this.controls, small, this.alpha);

        if (this.isShowing == false)
        {
            return;   // 사라지는 중에는 버튼을 누를 수 없다
        }

        // 버튼 두 개 (PlayerDeath의 "다시 시작 / 그만하기"와 같은 모양)
        float width = UIStyle.Size(320);
        float height = UIStyle.Size(64);
        if (this.MenuButton(new Rect(cx - width / 2f, cy + UIStyle.Size(40), width, height), "시작하기"))
        {
            this.StartGame();
        }
        if (this.MenuButton(new Rect(cx - width / 2f, cy + UIStyle.Size(120), width, height), "그만하기"))
        {
            this.Quit();
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
