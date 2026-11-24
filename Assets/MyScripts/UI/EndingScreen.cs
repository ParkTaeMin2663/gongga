using System.Collections;          // 강의 05장: 코루틴(IEnumerator)
using UnityEngine;
using UnityEngine.SceneManagement; // 강의 04장: SceneManager.LoadScene

/// <summary>
/// 탈출 엔딩. 지하 비상구 구역(Is Trigger)에 들어오면:
///   1) 괴물을 끄고(더 쫓아오지 않게) 플레이어가 멈춘다
///   2) 화면이 서서히 까매지고 배경음·긴장 음악이 줄어든다
///   3) 엔딩 글이 한 줄씩 나타났다 사라진다
///   4) 큰 빨간 "공가" + "처음으로 / 그만하기" (마우스 커서 보임)
///       · 처음으로: 체크포인트·연출 기록을 모두 지우고 씬을 다시 불러온다 → 타이틀부터
///       · 그만하기: 게임 종료 (에디터에서는 Play 멈춤)
///
/// [붙이는 곳] 지하 비상구 문 바로 바깥의 빈 오브젝트 "ExitTrigger" + Box Collider(Is Trigger 체크, 강의 05·07장)
/// [A 16번] 탈출 = 이 구역에 들어오는 것. 추격 쪽에서 따로 부를 필요 없음. 엔딩 중인지는 EndingScreen.isEnding
/// [같이 쓰는 것] UIStyle.cs, TitleScreen.hasStarted, Checkpoint·Floor2Event·BrotherEncounter의 static 값
/// </summary>
public class EndingScreen : MonoBehaviour
{
    [Header("끝날 때 끌 것")]
    [Tooltip("엔딩이 시작되면 끌 오브젝트 (예: Monster_B1). 비상구 앞에서 잡히지 않게")]
    public GameObject[] stopObjects;

    [Header("엔딩 글")]
    [TextArea(1, 3)]
    public string[] lines = new string[]
    {
        "비상구 철문을 밀자, 차가운 바람이 얼굴을 때렸다.",
        "눈은 그쳐 있었다. 돌아본 단지에는 어디에도 불이 켜져 있지 않았다.",
        "일주일 뒤, 단지는 예정대로 철거되었다.",
        "302호 자리에서는 작은 아이 신발 한 켤레가 나왔다고 한다.",
        "나는 지금도 밤마다 현관 불을 켜 두고 잔다."
    };
    [Tooltip("한 줄이 떠 있는 시간(초)")]
    public float lineTime = 4f;
    [Tooltip("엔딩 글 크기 (1080p 기준)")]
    public int lineSize = 34;

    [Header("마지막 화면")]
    public string endTitle = "공가";
    public string endSubTitle = "空家";
    [Tooltip("제목 아래 흐린 글자")]
    public string thanks = "플레이해 주셔서 감사합니다";

    [Header("연출")]
    [Tooltip("화면이 까매지는 시간(초)")]
    public float fadeTime = 2.5f;
    [Tooltip("(없어도 됨) 엔딩 음악. 이 오브젝트에 Audio Source(Spatial Blend 0, Play On Awake 끔)를 붙이고 Clip을 넣는다")]
    public AudioSource endingMusic;

    // 강의 07장 public static: 엔딩 중인지 (다른 스크립트가 확인할 수 있게)
    public static bool isEnding = false;

    private bool started;
    private float blackAmount;    // 화면 검은 정도 (0~1)
    private string currentLine;
    private float lineAlpha;
    private bool showMenu;
    private float menuAlpha;

    // 강의 04장: Find는 Start()에서 한 번만
    private PlayerMovement playerMovement;
    private PlayerLook playerLook;
    private PlayerInteract playerInteract;
    private PlayerPickup playerPickup;
    private TensionMusic tensionMusic;
    private Rigidbody playerBody;

    private void Start()
    {
        isEnding = false;
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            this.playerMovement = playerObject.GetComponent<PlayerMovement>();
            this.playerLook = playerObject.GetComponent<PlayerLook>();
            this.playerInteract = playerObject.GetComponent<PlayerInteract>();
            this.playerPickup = playerObject.GetComponent<PlayerPickup>();
            this.tensionMusic = playerObject.GetComponent<TensionMusic>();
            this.playerBody = playerObject.GetComponent<Rigidbody>();
        }
    }

    // 강의 05·07장 Is Trigger: 비상구를 나왔다
    private void OnTriggerEnter(Collider other)
    {
        if (this.started || other.CompareTag("Player") == false)
        {
            return;
        }
        this.started = true;
        isEnding = true;
        this.StartCoroutine(this.EndingRoutine());
    }

    private void Update()
    {
        if (this.showMenu)
        {
            // [강의 외] Cursor — 다른 스크립트가 숨기지 못하게 메뉴 동안 계속 보이게
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // 강의 05장 코루틴
    private IEnumerator EndingRoutine()
    {
        // 1) 괴물 끄기, 플레이어 멈추기
        for (int i = 0; i < this.stopObjects.Length; i++)
        {
            if (this.stopObjects[i] != null)
            {
                this.stopObjects[i].SetActive(false);
            }
        }
        if (this.playerMovement != null) this.playerMovement.enabled = false;
        if (this.playerInteract != null) this.playerInteract.enabled = false;
        if (this.playerPickup != null) this.playerPickup.enabled = false;
        if (this.playerLook != null) this.playerLook.IsLocked = true;
        if (this.playerBody != null) this.playerBody.linearVelocity = Vector3.zero;

        // 긴장 음악 스크립트를 멈추고(강의 04장 enabled) 그 소리 두 개를 직접 줄인다
        AudioSource music = null;
        AudioSource ambient = null;
        if (this.tensionMusic != null)
        {
            this.tensionMusic.enabled = false;
            music = this.tensionMusic.musicSource;
            ambient = this.tensionMusic.ambientSource;
        }
        if (this.endingMusic != null)
        {
            this.endingMusic.volume = 0f;
            this.endingMusic.Play();   // 강의 03장
        }

        // 2) 화면이 서서히 까매진다 + 소리 줄이기
        float musicStart = music != null ? music.volume : 0f;
        float ambientStart = ambient != null ? ambient.volume : 0f;
        while (this.blackAmount < 1f)
        {
            this.blackAmount = Mathf.Min(1f, this.blackAmount + Time.deltaTime / this.fadeTime);
            if (music != null) music.volume = Mathf.Lerp(musicStart, 0f, this.blackAmount);
            if (ambient != null) ambient.volume = Mathf.Lerp(ambientStart, 0f, this.blackAmount);
            if (this.endingMusic != null) this.endingMusic.volume = this.blackAmount * 0.6f;
            yield return null;
        }
        yield return new WaitForSeconds(1f);

        // 3) 엔딩 글 한 줄씩
        for (int i = 0; i < this.lines.Length; i++)
        {
            this.currentLine = this.lines[i];
            yield return this.StartCoroutine(this.FadeLine(0f, 1f, 0.8f));
            yield return new WaitForSeconds(this.lineTime);
            yield return this.StartCoroutine(this.FadeLine(1f, 0f, 0.8f));
            yield return new WaitForSeconds(0.4f);
        }
        this.currentLine = null;

        // 4) 마지막 화면
        this.showMenu = true;
        while (this.menuAlpha < 1f)
        {
            this.menuAlpha = Mathf.Min(1f, this.menuAlpha + Time.deltaTime / 1.5f);
            yield return null;
        }
    }

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

    // 처음으로: 저장된 기록을 모두 지우고 씬을 다시 불러온다 → 타이틀부터 (강의 04장 LoadScene, 07장 public static)
    private void BackToTitle()
    {
        Checkpoint.savedOrder = 0;
        Checkpoint.isRespawning = false;
        Checkpoint.savedHasFlashlight = false;
        Checkpoint.savedBattery = 100f;
        Floor2Event.reachedHidePhase = false;
        BrotherEncounter.finished = false;
        TitleScreen.hasStarted = false;
        isEnding = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 그만하기 ([강의 외] Application.Quit — PlayerDeath와 같은 방식, 11/18 허락)
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
        if (this.blackAmount <= 0f)
        {
            return;
        }

        // 까만 화면
        GUI.color = new Color(0f, 0f, 0f, this.blackAmount);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // 엔딩 글 (가운데)
        if (this.currentLine != null)
        {
            GUIStyle lineStyle = UIStyle.Make(UIStyle.regular, this.lineSize, UIStyle.white, TextAnchor.MiddleCenter);
            float margin = UIStyle.Size(160);
            Rect rect = new Rect(margin, Screen.height / 2f - UIStyle.Size(80), Screen.width - margin * 2f, UIStyle.Size(160));
            UIStyle.Label(rect, this.currentLine, lineStyle, this.lineAlpha);
        }

        if (this.showMenu == false)
        {
            return;
        }

        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(300), Screen.width, UIStyle.Size(180)), this.endTitle, UIStyle.Title(150), this.menuAlpha);
        GUIStyle subStyle = UIStyle.Hanja(36, UIStyle.white);   // 空家: 동해독도에 한자가 없어 본명조 (11/24)
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(125), Screen.width, UIStyle.Size(50)), this.endSubTitle, subStyle, this.menuAlpha);
        GUIStyle small = UIStyle.Make(UIStyle.regular, 22, UIStyle.gray, TextAnchor.MiddleCenter);
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(65), Screen.width, UIStyle.Size(40)), this.thanks, small, this.menuAlpha);

        if (this.menuAlpha < 0.5f)
        {
            return;   // 반쯤 나타난 뒤부터 누를 수 있다 (PlayerDeath와 같음)
        }

        float width = UIStyle.Size(320);
        float height = UIStyle.Size(64);
        if (this.MenuButton(new Rect(cx - width / 2f, cy + UIStyle.Size(40), width, height), "처음으로"))
        {
            this.BackToTitle();
        }
        if (this.MenuButton(new Rect(cx - width / 2f, cy + UIStyle.Size(120), width, height), "그만하기"))
        {
            this.Quit();
        }
    }

    // 글자 버튼 (PlayerDeath·TitleScreen과 같은 모양)
    private bool MenuButton(Rect rect, string text)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        UIStyle.Label(rect, text, UIStyle.Strong(40, hover ? UIStyle.yellow : UIStyle.white), this.menuAlpha);
        return GUI.Button(rect, "", GUIStyle.none);
    }
}
