using UnityEngine;
using UnityEngine.InputSystem;

// 단서(ClueItem) 읽기 화면. 단서를 바라보면 "[E] 읽기"가 뜨고, E를 누르면 화면 가운데에 종이·사진을 보여 준다.
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

    public bool IsReading { get { return this.current != null; } }

    private Transform cameraTransform;
    private PlayerMovement playerMovement;
    private PlayerLook playerLook;
    private PlayerInteract playerInteract;
    private PlayerHide playerHide;
    private Rigidbody playerRigidbody;
    private AudioSource audioSource;

    private ClueItem lookClue;
    private ClueItem current;
    private int pageIndex;
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
        bool qPressed = Keyboard.current.qKey.wasPressedThisFrame;

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

        this.lookClue = null;
        if (this.playerHide != null && this.playerHide.IsInsideSpot)
        {
            return;
        }
        RaycastHit hit;
        if (Physics.Raycast(this.cameraTransform.position, this.cameraTransform.forward, out hit, this.reachDistance))
        {
            this.lookClue = hit.collider.GetComponentInParent<ClueItem>();
        }

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
            this.audioSource.Play();
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
            Destroy(this.current.gameObject);
        }
        this.current = null;
        this.SetPlayerFrozen(false);
    }

    private void SetPlayerFrozen(bool frozen)
    {
        if (this.playerMovement != null) this.playerMovement.enabled = !frozen;
        if (this.playerInteract != null) this.playerInteract.enabled = !frozen;
        if (this.playerLook != null) this.playerLook.IsLocked = frozen;

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

    private void OnGUI()
    {
        if (this.current == null)
        {
            if (this.lookClue != null)
            {
                UIStyle.Prompt("[E] " + this.lookClue.title + " " + this.lookClue.ActionName);
            }
            return;
        }

        GUIStyle titleStyle = UIStyle.Title(this.fontSize + 8);
        GUIStyle textStyle = UIStyle.Body(this.fontSize, this.textColor);
        GUIStyle hintStyle = UIStyle.Make(UIStyle.regular, this.fontSize - 4, this.textColor, TextAnchor.MiddleRight);
        float margin = UIStyle.Size(40);

        GUI.color = new Color(0f, 0f, 0f, 0.75f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), this.whiteTexture);

        float paperHeight = Screen.height * 0.8f;
        float paperWidth = paperHeight * 0.75f;
        Rect paper = new Rect((Screen.width - paperWidth) / 2f, (Screen.height - paperHeight) / 2f, paperWidth, paperHeight);
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(paper.x + UIStyle.Size(10), paper.y + UIStyle.Size(14), paper.width, paper.height), this.whiteTexture);
        GUI.color = this.paperColor;
        GUI.DrawTexture(paper, this.whiteTexture);
        GUI.color = Color.white;
        UIStyle.EdgeShade(paper, new Color(0.35f, 0.24f, 0.1f, 0.45f), 0.1f);
        UIStyle.Frame(paper, new Color(0.3f, 0.2f, 0.1f, 0.5f), 1f);

        Rect titleRect = new Rect(paper.x + margin, paper.y + margin * 0.6f, paper.width - margin * 2f, UIStyle.Size(this.fontSize + 8) * 2f);
        GUI.Label(titleRect, this.current.title, titleStyle);

        float textTop = titleRect.yMax + margin * 0.3f;
        if (this.current.image != null)
        {
            Rect imageRect = new Rect(paper.x + margin, textTop, paper.width - margin * 2f, paper.height * 0.5f);
            GUI.DrawTexture(imageRect, this.current.image, ScaleMode.ScaleToFit);
            textTop = imageRect.yMax + margin * 0.3f;
        }

        if (this.current.pages != null && this.current.pages.Length > 0)
        {
            Rect textRect = new Rect(paper.x + margin, textTop, paper.width - margin * 2f, paper.yMax - textTop - margin * 1.5f);
            GUI.Label(textRect, this.current.pages[this.pageIndex], textStyle);
        }

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
