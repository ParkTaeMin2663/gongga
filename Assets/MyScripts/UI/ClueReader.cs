using UnityEngine;
using UnityEngine.InputSystem; // Input System Package (강의 03장)

/// <summary>
/// 단서(ClueItem) 읽기 화면. 단서를 바라보면 "[E] 읽기"가 뜨고, E를 누르면 화면 가운데에 종이·사진을 보여 준다.
/// 읽는 동안 플레이어는 걷기·시점·잡기가 멈춘다. E를 누르면 다음 장, 마지막 장에서 E를 누르면 닫힌다. Q = 이전 장 (11/20 A 추가).
///
/// [붙이는 곳] SampleScene의 빈 오브젝트 "ClueReader" (1개)
/// [같이 필요한 컴포넌트] Audio Source (Play On Awake 끔) — 카세트 소리를 튼다. Spatial Blend 0(2D)
/// [씬에 필요한 것] 이름이 "Player"인 플레이어 (자식 Main Camera)
///
/// [방식] 화면 그리기는 강의 04장 OnGUI + 공통 스타일 UIStyle(본명조 글꼴, Player 프리팹에 붙어 있음). 바라보는 단서는 강의 07장 Physics.Raycast.
///        E 키는 Player 프리팹(A 담당)을 고치지 않으려고 Keyboard.current로 직접 읽는다
///        (강의 06장 Mouse.current와 같은 방식).
/// </summary>
public class ClueReader : MonoBehaviour
{
    [Header("거리")]
    [Tooltip("이 거리(m) 안의 단서만 읽을 수 있다 (손 닿는 거리 2와 같게)")]
    public float reachDistance = 2f;

    [Header("화면")]
    [Tooltip("본문 글자 크기 (1080p 기준, 화면 크기에 맞춰 자동으로 바뀜)")]
    public int fontSize = 26;
    [Tooltip("종이 색 (낡은 누런 종이)")]
    public Color paperColor = new Color(0.86f, 0.82f, 0.70f, 0.97f);
    [Tooltip("글자 색 (UIStyle.paper = 진한 갈색)")]
    public Color textColor = new Color(0.20f, 0.17f, 0.13f, 1f);

    [Header("소리 (없어도 됨)")]
    [Tooltip("종이 넘기는 소리")]
    public AudioClip pageSound;

    /// <summary>지금 단서를 읽고 있는지</summary>
    public bool IsReading { get { return this.current != null; } }

    // 강의 04장: Find는 Start()에서 한 번만
    private Transform cameraTransform;
    private PlayerMovement playerMovement;
    private PlayerLook playerLook;
    private PlayerInteract playerInteract;
    private PlayerHide playerHide;
    private Rigidbody playerRigidbody;
    private AudioSource audioSource;

    private ClueItem lookClue;     // 지금 바라보는 단서
    private ClueItem current;      // 지금 읽고 있는 단서
    private int pageIndex;         // 몇 번째 장
    private Texture2D whiteTexture;

    private void Start()
    {
        this.whiteTexture = Texture2D.whiteTexture;
        this.audioSource = this.GetComponent<AudioSource>();

        GameObject playerObject = GameObject.Find("Player");
        if (playerObject == null)
        {
            Debug.LogError("[ClueReader] 이름이 Player인 오브젝트가 없습니다.");
            return;
        }
        this.playerMovement = playerObject.GetComponent<PlayerMovement>();
        this.playerLook = playerObject.GetComponent<PlayerLook>();
        this.playerInteract = playerObject.GetComponent<PlayerInteract>();
        this.playerHide = playerObject.GetComponent<PlayerHide>();
        this.playerRigidbody = playerObject.GetComponent<Rigidbody>();
        if (this.playerLook != null)
        {
            this.cameraTransform = this.playerLook.cameraTransform;
        }
    }

    private void Update()
    {
        if (this.cameraTransform == null || Keyboard.current == null)
        {
            return;
        }

        bool ePressed = Keyboard.current.eKey.wasPressedThisFrame;
        bool qPressed = Keyboard.current.qKey.wasPressedThisFrame;   // (11/20 A 추가) 이전 장

        // 1) 읽는 중: E = 다음 장 / 마지막 장이면 닫기, Q = 이전 장
        if (this.current != null)
        {
            if (ePressed)
            {
                this.NextPage();
            }
            else if (qPressed)
            {
                this.PrevPage();
            }
            return;
        }

        // 2) 바라보는 단서 찾기 (강의 07장 Physics.Raycast)
        this.lookClue = null;
        if (this.playerHide != null && this.playerHide.IsInsideSpot)
        {
            return;   // 장롱 안에서는 못 읽음
        }
        RaycastHit hit;
        if (Physics.Raycast(this.cameraTransform.position, this.cameraTransform.forward, out hit, this.reachDistance))
        {
            this.lookClue = hit.collider.GetComponentInParent<ClueItem>();
        }

        // 3) E를 누르면 읽기 시작 (물건을 들고 있으면 못 읽음)
        if (ePressed && this.lookClue != null && (this.playerInteract == null || this.playerInteract.IsHolding == false))
        {
            this.Open(this.lookClue);
        }
    }

    private void Open(ClueItem clue)
    {
        this.current = clue;
        this.pageIndex = 0;
        this.lookClue = null;
        this.SetPlayerFrozen(true);

        if (clue.type == ClueItem.ClueType.Cassette && clue.audioClip != null && this.audioSource != null)
        {
            this.audioSource.clip = clue.audioClip;
            this.audioSource.Play();   // 강의 03장
        }
        else
        {
            this.PlayPageSound();
        }
    }

    private void NextPage()
    {
        int pageCount = this.current.pages != null ? this.current.pages.Length : 0;
        if (this.pageIndex < pageCount - 1)
        {
            this.pageIndex = this.pageIndex + 1;
            this.PlayPageSound();
            return;
        }
        this.Close();
    }

    // (11/20 A 추가) Q: 이전 장. 첫 장이면 아무 일도 없다
    private void PrevPage()
    {
        if (this.pageIndex > 0)
        {
            this.pageIndex = this.pageIndex - 1;
            this.PlayPageSound();
        }
    }

    private void Close()
    {
        if (this.audioSource != null)
        {
            this.audioSource.Stop();
        }
        this.current.IsRead = true;
        if (this.current.removeAfterRead)
        {
            Destroy(this.current.gameObject);   // 강의 05장
        }
        this.current = null;
        this.SetPlayerFrozen(false);
    }

    // 읽는 동안 걷기·시점·잡기를 멈춘다 (enabled = Inspector 체크박스와 같다)
    private void SetPlayerFrozen(bool frozen)
    {
        if (this.playerMovement != null) this.playerMovement.enabled = !frozen;
        if (this.playerInteract != null) this.playerInteract.enabled = !frozen;
        if (this.playerLook != null) this.playerLook.IsLocked = frozen;

        // 걷던 속도가 남아 미끄러지지 않게 멈춘다 (강의 03장 Rigidbody)
        if (frozen && this.playerRigidbody != null)
        {
            this.playerRigidbody.linearVelocity = Vector3.zero;
        }
    }

    private void PlayPageSound()
    {
        if (this.pageSound != null && this.audioSource != null)
        {
            this.audioSource.PlayOneShot(this.pageSound);
        }
    }

    // 강의 04장 OnGUI. 글꼴·색은 A·B 공통 스타일 UIStyle.cs (본명조, 1080p 기준 크기 → 화면에 맞춰 바뀜)
    private void OnGUI()
    {
        // 바라보는 중: 조준점 아래 가운데 안내 ("[E]"는 노란색)
        if (this.current == null)
        {
            if (this.lookClue != null)
            {
                UIStyle.Prompt("[E] " + this.lookClue.title + " " + this.lookClue.ActionName);
            }
            return;
        }

        // 화면 크기가 바뀔 수 있어 글자 모양은 매번 만든다 (Floor2Event "숨어라"와 같은 방식)
        GUIStyle titleStyle = UIStyle.Title(this.fontSize + 8);
        GUIStyle textStyle = UIStyle.Body(this.fontSize, this.textColor);
        GUIStyle hintStyle = UIStyle.Make(UIStyle.regular, this.fontSize - 4, this.textColor, TextAnchor.MiddleRight);
        float margin = UIStyle.Size(40);

        // 1) 화면 전체를 어둡게
        GUI.color = new Color(0f, 0f, 0f, 0.75f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), this.whiteTexture);

        // 2) 가운데 종이 (화면 높이의 80%, 가로:세로 = 3:4)
        float paperHeight = Screen.height * 0.8f;
        float paperWidth = paperHeight * 0.75f;
        Rect paper = new Rect((Screen.width - paperWidth) / 2f, (Screen.height - paperHeight) / 2f, paperWidth, paperHeight);
        // (11/26 A) 종이 아래 그림자 → 종이 → 가장자리가 누렇게 바랜 듯 → 얇은 테두리
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(paper.x + UIStyle.Size(10), paper.y + UIStyle.Size(14), paper.width, paper.height), this.whiteTexture);
        GUI.color = this.paperColor;
        GUI.DrawTexture(paper, this.whiteTexture);
        GUI.color = Color.white;
        UIStyle.EdgeShade(paper, new Color(0.35f, 0.24f, 0.1f, 0.45f), 0.1f);
        UIStyle.Frame(paper, new Color(0.3f, 0.2f, 0.1f, 0.5f), 1f);

        // 3) 제목 (본명조 Black, 빨간색 — 스프레이 색)
        Rect titleRect = new Rect(paper.x + margin, paper.y + margin * 0.6f, paper.width - margin * 2f, UIStyle.Size(this.fontSize + 8) * 2f);
        GUI.Label(titleRect, this.current.title, titleStyle);

        // 4) 사진 (있으면 종이 위쪽 절반)
        float textTop = titleRect.yMax + margin * 0.3f;
        if (this.current.image != null)
        {
            Rect imageRect = new Rect(paper.x + margin, textTop, paper.width - margin * 2f, paper.height * 0.5f);
            GUI.DrawTexture(imageRect, this.current.image, ScaleMode.ScaleToFit);
            textTop = imageRect.yMax + margin * 0.3f;
        }

        // 5) 글 (지금 장)
        if (this.current.pages != null && this.current.pages.Length > 0)
        {
            Rect textRect = new Rect(paper.x + margin, textTop, paper.width - margin * 2f, paper.yMax - textTop - margin * 1.5f);
            GUI.Label(textRect, this.current.pages[this.pageIndex], textStyle);
        }

        // 6) 아래 안내: 쪽 번호와 E
        int pageCount = this.current.pages != null ? this.current.pages.Length : 0;
        string hint = this.pageIndex < pageCount - 1 ? "[E] 다음" : "[E] 닫기";
        if (this.pageIndex > 0)
        {
            hint = "[Q] 이전    " + hint;
        }
        if (pageCount > 1)
        {
            hint = (this.pageIndex + 1) + " / " + pageCount + "    " + hint;
        }
        GUI.Label(new Rect(paper.x + margin, paper.yMax - margin * 1.3f, paper.width - margin * 2f, margin), hint, hintStyle);
    }
}
