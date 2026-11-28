using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 탈출 엔딩: 비상구에 들어오면 화면이 어두워지고 엔딩 글과 메뉴를 보여 준다.
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

    public static bool isEnding = false;   // 강의 07장: public static 변수

    private bool started;
    private float blackAmount;
    private string currentLine;
    private float lineAlpha;
    private bool showMenu;
    private float menuAlpha;

    private PlayerMovement playerMovement;
    private PlayerLook playerLook;
    private PlayerInteract playerInteract;
    private PlayerPickup playerPickup;
    private TensionMusic tensionMusic;
    private Rigidbody playerBody;

    private void Start()
    {
        isEnding = false;
        GameObject playerObject = GameObject.Find("Player");   // 강의 04장: GameObject.Find (Start에서 한 번)
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

    private void OnTriggerEnter(Collider other)   // 강의 05장: Is Trigger + OnTriggerEnter
    {
        if (this.started || other.CompareTag("Player") == false)   // 강의 07장: CompareTag
        {
            return;
        }
        this.started = true;
        isEnding = true;
        this.StartCoroutine(this.EndingRoutine());   // 강의 05장: 코루틴 StartCoroutine
    }

    private void Update()
    {
        if (this.showMenu)
        {
            // [강의 외] Cursor.lockState / Cursor.visible: 메뉴에서 마우스 커서 보이기
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private IEnumerator EndingRoutine()
    {
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
            this.endingMusic.Play();
        }

        float musicStart = music != null ? music.volume : 0f;
        float ambientStart = ambient != null ? ambient.volume : 0f;
        while (this.blackAmount < 1f)
        {
            this.blackAmount = Mathf.Min(1f, this.blackAmount + Time.deltaTime / this.fadeTime);   // 강의 03장: Time.deltaTime
            if (music != null) music.volume = Mathf.Lerp(musicStart, 0f, this.blackAmount);
            if (ambient != null) ambient.volume = Mathf.Lerp(ambientStart, 0f, this.blackAmount);
            if (this.endingMusic != null) this.endingMusic.volume = this.blackAmount * 0.6f;
            yield return null;
        }
        yield return new WaitForSeconds(1f);   // 강의 05장: WaitForSeconds

        for (int i = 0; i < this.lines.Length; i++)
        {
            this.currentLine = this.lines[i];
            yield return this.StartCoroutine(this.FadeLine(0f, 1f, 0.8f));
            yield return new WaitForSeconds(this.lineTime);
            yield return this.StartCoroutine(this.FadeLine(1f, 0f, 0.8f));
            yield return new WaitForSeconds(0.4f);
        }
        this.currentLine = null;

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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);   // 강의 04장: SceneManager.LoadScene
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
        if (this.blackAmount <= 0f)
        {
            return;
        }

        GUI.color = new Color(0f, 0f, 0f, this.blackAmount);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        UIStyle.MenuBackdrop(this.blackAmount, new Color(0.06f, 0.08f, 0.14f, 1f));

        if (this.currentLine != null)
        {
            GUIStyle lineStyle = UIStyle.Make(UIStyle.regular, UIStyle.Text(this.lineSize), UIStyle.white, TextAnchor.MiddleCenter);
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
        GUIStyle subStyle = UIStyle.Hanja(36, UIStyle.white);
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(125), Screen.width, UIStyle.Size(50)), this.endSubTitle, subStyle, this.menuAlpha);
        GUIStyle small = UIStyle.Make(UIStyle.regular, UIStyle.Text(28), UIStyle.gray, TextAnchor.MiddleCenter);
        UIStyle.Label(new Rect(0f, cy - UIStyle.Size(75), Screen.width, UIStyle.Size(60)), this.thanks, small, this.menuAlpha);

        if (this.menuAlpha < 0.5f)
        {
            return;
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

    private bool MenuButton(Rect rect, string text)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        UIStyle.Label(rect, text, UIStyle.Strong(40, hover ? UIStyle.yellow : UIStyle.white), this.menuAlpha);
        return GUI.Button(rect, "", GUIStyle.none);
    }
}
