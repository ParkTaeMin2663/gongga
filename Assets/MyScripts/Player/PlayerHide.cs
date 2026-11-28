using UnityEngine;
using UnityEngine.InputSystem;

// 장롱(HideSpot)에 숨고 나오는 기능. 숨어 있는 동안 괴물 발소리와 "멀어졌다" 안내.
public class PlayerHide : MonoBehaviour
{
    [Header("E 누르는 시간")]
    [Tooltip("숨으려면 E를 몇 초 누르고 있어야 하는지")]
    public float enterHoldTime = 1f;

    [Tooltip("나오려면 E를 몇 초 누르고 있어야 하는지. 0이면 누르자마자 나온다")]
    public float exitHoldTime = 0f;

    [Header("괴물이 멀어지면 알려 주기 (11/24)")]
    [Tooltip("숨어 있는 동안 괴물이 가까이 왔다가 멀어지면 화면 아래에 띄울 글")]
    public string goneMessage = "발소리가 멀어졌다. 이제 나가도 될 것 같다";
    [Tooltip("괴물이 이 거리(m) 안으로 오면 \"가까이 왔다\"로 본다")]
    public float monsterNearDistance = 8f;
    [Tooltip("가까이 왔던 괴물이 이 거리(m)보다 멀어지거나 사라지면 글을 띄운다")]
    public float monsterGoneDistance = 14f;

    [Header("연결 (비워 두면 자동으로 찾음)")]
    public PlayerInteract playerInteract;
    public PlayerMovement playerMovement;
    public PlayerLook playerLook;

    public bool IsHidden { get; private set; }

    public bool IsInsideSpot { get { return this.currentSpot != null; } }

    public HideSpot CurrentSpot { get { return this.currentSpot; } }

    public float CurrentYaw { get { return this.playerLook != null ? this.playerLook.Yaw : this.transform.eulerAngles.y; } }

    private Rigidbody rb;
    private HideSpot currentSpot;

    private bool isInteractHeld;
    private float holdTimer;
    private Texture2D whiteTexture;

    private bool monsterWasNear;
    private float watchTimer;
    private MonsterAI watchedMonster;

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

    private void OnInteract(InputValue value)   // 강의 02장: Player Input 메시지 함수 OnInteract
    {
        this.isInteractHeld = value.isPressed;   // 강의 02장: value.isPressed (Press And Release)
        this.holdTimer = 0f;
    }

    private void Update()
    {
        this.WatchMonster();

        if (this.isInteractHeld == false || (this.currentSpot != null && this.currentSpot.IsBusy))
        {
            this.holdTimer = 0f;
            return;
        }

        if (this.IsHidden)
        {
            this.holdTimer = this.holdTimer + Time.deltaTime;   // 강의 03장: Time.deltaTime
            if (this.holdTimer >= this.exitHoldTime)
            {
                this.isInteractHeld = false;
                this.StartExit();
            }
        }
        else if (this.CanEnterNow())
        {
            this.holdTimer = this.holdTimer + Time.deltaTime;
            if (this.holdTimer >= this.enterHoldTime)
            {
                this.isInteractHeld = false;
                this.StartEnter(this.playerInteract.LookHideSpot);
            }
        }
        else
        {
            this.holdTimer = 0f;
        }
    }

    private void WatchMonster()
    {
        if (this.IsHidden == false)
        {
            this.monsterWasNear = false;
            this.watchedMonster = null;
            return;
        }

        this.watchTimer = this.watchTimer - Time.deltaTime;
        if (this.watchTimer > 0f)
        {
            return;
        }
        this.watchTimer = 0.5f;

        if (this.watchedMonster == null || this.watchedMonster.isActiveAndEnabled == false)
        {
            this.watchedMonster = FindAnyObjectByType<MonsterAI>();
        }

        bool found = this.watchedMonster != null && this.watchedMonster.isActiveAndEnabled;
        float distance = 999f;
        if (found)
        {
            Vector3 diff = this.watchedMonster.transform.position - this.transform.position;
            if (Mathf.Abs(diff.y) < 1.5f)
            {
                diff.y = 0f;
                distance = diff.magnitude;
            }
        }

        if (distance < this.monsterNearDistance)
        {
            this.monsterWasNear = true;
        }
        else if (this.monsterWasNear && distance > this.monsterGoneDistance)
        {
            this.monsterWasNear = false;
            PlayerKeys.ShowMessage(this.goneMessage);
        }
    }

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
        this.IsHidden = false;
        this.SetLookLocked(true);
        this.currentSpot.BeginExit();
    }

    public void OnHideFinished()
    {
        this.IsHidden = true;
        this.SetLookLocked(false);
    }

    public void OnInside()
    {
        this.IsHidden = true;
    }

    public void OnExitFinished()
    {
        this.currentSpot = null;
        this.Freeze(false);
    }

    public void SetYaw(float angle)
    {
        if (this.playerLook != null)
        {
            this.playerLook.SetYaw(angle);
        }
    }

    private void Freeze(bool frozen)
    {
        if (this.rb != null)
        {
            if (frozen)
            {
                this.rb.linearVelocity = Vector3.zero;
            }
            this.rb.isKinematic = frozen;   // 강의 03장: Rigidbody Is Kinematic
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

    private void OnGUI()   // 강의 04장: OnGUI
    {
        if (this.currentSpot != null && this.currentSpot.FadeAmount > 0f)
        {
            GUI.color = new Color(0f, 0f, 0f, this.currentSpot.FadeAmount);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), this.whiteTexture);
        }

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

        UIStyle.Prompt(message);

        float needTime = this.IsHidden ? this.exitHoldTime : this.enterHoldTime;
        if (this.holdTimer > 0f && needTime > 0f)
        {
            float rate = Mathf.Clamp01(this.holdTimer / needTime);
            Rect prompt = UIStyle.PromptRect();
            float width = UIStyle.Size(140);
            float height = Mathf.Max(2f, UIStyle.Size(4));
            Rect bar = new Rect(Screen.width / 2f - width / 2f, prompt.yMax + UIStyle.Size(4), width, height);
            GUI.color = new Color(1f, 1f, 1f, 0.2f);
            GUI.DrawTexture(bar, this.whiteTexture);
            GUI.color = UIStyle.yellow;
            GUI.DrawTexture(new Rect(bar.x, bar.y, bar.width * rate, bar.height), this.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
