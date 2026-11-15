using UnityEngine;
using UnityEngine.InputSystem; // Input System Package (강의 03장)

/// <summary>
/// 장롱(HideSpot)에 숨고 나오는 기능.
///
/// [붙이는 곳] Player 오브젝트 (Player Input이 있는 오브젝트) — Player 프리팹에서 추가
/// [Inspector 연결] 비워 두면 같은 Player에서 자동으로 찾는다.
///   - Player Interact, Player Movement, Player Look
///
/// [입력] 기본으로 있는 Interact 액션(E 키) → OnInteract (강의 02장)
///   - InputSystem_Actions에서 Interact의 Interaction = Press (Press And Release)
///     → 누른 순간 isPressed = true, 뗀 순간 false. 누르고 있는 시간을 직접 잰다.
///   - 숨기: 장롱을 보면서 E를 enterHoldTime초 동안 누르고 있기 (화면에 막대가 차오름)
///   - 나오기: E를 한 번 누르기 (exitHoldTime이 0일 때)
///
/// [동작] E → 걷기·시점을 멈추고(Rigidbody Is Kinematic, 강의 03장) HideSpot에게 "들어가기"를 맡긴다.
///        HideSpot이 문 열기 → 걸어 들어가기 → 문 닫기를 끝내면 OnHideFinished()를 불러 준다.
///        숨어 있는 동안에는 고개만 돌릴 수 있어서 문틈으로 밖을 볼 수 있다.
///        다시 E → 나오기도 같은 방식 (끝나면 OnExitFinished).
/// </summary>
public class PlayerHide : MonoBehaviour
{
    [Header("E 누르는 시간")]
    [Tooltip("숨으려면 E를 몇 초 누르고 있어야 하는지")]
    public float enterHoldTime = 1f;

    [Tooltip("나오려면 E를 몇 초 누르고 있어야 하는지. 0이면 누르자마자 나온다")]
    public float exitHoldTime = 0f;

    [Header("연결 (비워 두면 자동으로 찾음)")]
    public PlayerInteract playerInteract;
    public PlayerMovement playerMovement;
    public PlayerLook playerLook;

    /// <summary>장롱 문이 닫히고 완전히 숨은 상태인지. 괴물은 이 값이 true면 플레이어를 찾지 못한다.</summary>
    public bool IsHidden { get; private set; }

    /// <summary>장롱에 들어가는 중 / 숨어 있음 / 나오는 중이면 true. 이 동안에는 걷기·잡기를 못 한다.</summary>
    public bool IsInsideSpot { get { return this.currentSpot != null; } }

    /// <summary>지금 바라보는 좌우 방향(도). HideSpot이 몸을 돌릴 때 쓴다.</summary>
    public float CurrentYaw { get { return this.playerLook != null ? this.playerLook.Yaw : this.transform.eulerAngles.y; } }

    private Rigidbody rb;
    private HideSpot currentSpot;   // 지금 들어가 있는 장롱

    private bool isInteractHeld;    // E를 누르고 있는지
    private float holdTimer;        // E를 누르고 있은 시간(초)
    private Texture2D whiteTexture; // 화면 덮기·막대 그리기용

    private void Start()
    {
        this.rb = this.GetComponent<Rigidbody>();

        if (this.playerInteract == null)
        {
            this.playerInteract = this.GetComponent<PlayerInteract>();
        }
        if (this.playerMovement == null)
        {
            this.playerMovement = this.GetComponent<PlayerMovement>();
        }
        if (this.playerLook == null)
        {
            this.playerLook = this.GetComponent<PlayerLook>();
        }

        this.whiteTexture = Texture2D.whiteTexture;
    }

    // E 키 (Interact 액션). 누른 순간 true, 뗀 순간 false (Press And Release)
    private void OnInteract(InputValue value)
    {
        this.isInteractHeld = value.isPressed;
        this.holdTimer = 0f;
    }

    private void Update()
    {
        // 누르고 있지 않거나, 들어가고 나오는 동작 중이면 시간을 재지 않는다
        if (this.isInteractHeld == false || (this.currentSpot != null && this.currentSpot.IsBusy))
        {
            this.holdTimer = 0f;
            return;
        }

        if (this.IsHidden)
        {
            // 숨어 있을 때: exitHoldTime만큼 누르면 나온다
            this.holdTimer = this.holdTimer + Time.deltaTime;
            if (this.holdTimer >= this.exitHoldTime)
            {
                this.isInteractHeld = false;   // 다시 숨으려면 E를 새로 눌러야 한다
                this.StartExit();
            }
        }
        else if (this.CanEnterNow())
        {
            // 장롱을 보면서 누르고 있을 때: enterHoldTime만큼 누르면 들어간다 (강의 03장 Time.deltaTime)
            this.holdTimer = this.holdTimer + Time.deltaTime;
            if (this.holdTimer >= this.enterHoldTime)
            {
                this.isInteractHeld = false;
                this.StartEnter(this.playerInteract.LookHideSpot);
            }
        }
        else
        {
            // 장롱에서 눈을 돌리면 처음부터 다시
            this.holdTimer = 0f;
        }
    }

    // 지금 장롱에 들어갈 수 있는 상황인지 (장롱을 보고 있고, 손에 든 게 없음)
    private bool CanEnterNow()
    {
        return this.currentSpot == null && this.playerInteract != null
               && this.playerInteract.LookHideSpot != null && this.playerInteract.IsHolding == false;
    }

    private void StartEnter(HideSpot spot)
    {
        if (spot.hidePoint == null || spot.exitPoint == null)
        {
            return;
        }

        this.currentSpot = spot;
        this.Freeze(true);
        spot.BeginEnter(this);
    }

    private void StartExit()
    {
        this.IsHidden = false;   // 문이 열리는 순간부터는 숨은 게 아니다
        this.SetLookLocked(true);
        this.currentSpot.BeginExit();
    }

    /// <summary>HideSpot이 문을 다 닫은 뒤 부른다 → 이제 숨은 상태. 고개는 돌릴 수 있다.</summary>
    public void OnHideFinished()
    {
        this.IsHidden = true;
        this.SetLookLocked(false);
    }

    /// <summary>HideSpot이 나오기를 다 끝낸 뒤 부른다 → 다시 걸을 수 있다.</summary>
    public void OnExitFinished()
    {
        this.currentSpot = null;
        this.Freeze(false);
    }

    /// <summary>HideSpot이 몸을 돌릴 때 부른다.</summary>
    public void SetYaw(float angle)
    {
        if (this.playerLook != null)
        {
            this.playerLook.SetYaw(angle);
        }
    }

    // true = 걷기·물리·시점 멈춤, false = 원래대로
    private void Freeze(bool frozen)
    {
        if (this.rb != null)
        {
            if (frozen)
            {
                this.rb.linearVelocity = Vector3.zero;
            }
            this.rb.isKinematic = frozen;   // 강의 03장 Is Kinematic
        }
        if (this.playerMovement != null)
        {
            this.playerMovement.enabled = !frozen;
        }
        this.SetLookLocked(frozen);
    }

    private void SetLookLocked(bool locked)
    {
        if (this.playerLook != null)
        {
            this.playerLook.IsLocked = locked;
        }
    }

    // 강의 04장 OnGUI: 화면 어두워짐, 안내 문구, E 누르는 막대
    private void OnGUI()
    {
        // 1) 문 없는 장롱에 들어가고 나올 때 화면을 검게 덮는다
        if (this.currentSpot != null && this.currentSpot.FadeAmount > 0f)
        {
            GUI.color = new Color(0f, 0f, 0f, this.currentSpot.FadeAmount);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), this.whiteTexture);
        }

        // 2) 안내 문구
        string message = "";
        if (this.IsHidden)
        {
            message = "[E] 나가기";
        }
        else if (this.CanEnterNow())
        {
            message = "[E] 꾹 눌러 숨기";
        }

        if (message == "")
        {
            return;
        }

        float x = Screen.width / 2f - 60f;
        float y = Screen.height / 2f + 20f;
        GUI.color = Color.white;
        GUI.Label(new Rect(x, y, 200f, 25f), message);

        // 3) E를 누르고 있는 동안 막대가 차오른다
        float needTime = this.IsHidden ? this.exitHoldTime : this.enterHoldTime;
        if (this.holdTimer > 0f && needTime > 0f)
        {
            float rate = Mathf.Clamp01(this.holdTimer / needTime);
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.DrawTexture(new Rect(x, y + 25f, 120f, 6f), this.whiteTexture);        // 바탕
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(x, y + 25f, 120f * rate, 6f), this.whiteTexture); // 찬 만큼
        }
    }
}
