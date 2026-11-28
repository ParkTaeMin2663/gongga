using UnityEngine;
using UnityEngine.InputSystem;

// 바라보는 물체를 찾고, 마우스 왼쪽으로 잡아서 끄는 기능. 화면 가운데에 조준점도 그린다.
public class PlayerInteract : MonoBehaviour
{
    [Header("연결")]
    [Tooltip("Player의 자식 Main Camera를 여기에 드래그")]
    public Transform cameraTransform;

    [Tooltip("Player에 붙어 있는 PlayerLook을 드래그 (잡는 동안 고개가 안 돌아가게)")]
    public PlayerLook playerLook;

    [Header("거리")]
    [Tooltip("손이 닿는 거리(m). 이 안에 있는 물체만 잡을 수 있다.")]
    public float reachDistance = 2f;

    [Tooltip("잡은 물체에서 이만큼(m) 멀어지면 자동으로 놓는다.")]
    public float releaseDistance = 3f;

    private IInteractable lookTarget;
    private IInteractable heldTarget;
    private Transform heldTransform;
    private Vector2 lookDelta;

    private PlayerHide playerHide;

    public HideSpot LookHideSpot { get; private set; }

    public ItemPickup LookItem { get; private set; }

    public bool IsHolding { get { return this.heldTarget != null; } }

    private Texture2D dotTexture;

    private void Start()
    {
        if (this.cameraTransform == null)
        {
            Debug.LogError("[PlayerInteract] Camera Transform이 비어 있습니다. Main Camera를 드래그해 주세요.");
        }

        this.dotTexture = Texture2D.whiteTexture;
        this.playerHide = this.GetComponent<PlayerHide>();
    }

    private void OnLook(InputValue value)
    {
        this.lookDelta += value.Get<Vector2>();
    }

    private void OnGrab(InputValue value)
    {
        if (value.isPressed)
        {
            this.StartGrab();
        }
        else
        {
            this.EndGrab();
        }
    }

    private void OnThrow(InputValue value)
    {
        PickupObject pickup = this.heldTarget as PickupObject;
        if (pickup == null)
        {
            return;
        }

        this.EndGrab();
        pickup.Throw(this.cameraTransform.forward);
    }

    private void Update()
    {
        if (this.cameraTransform == null)
        {
            return;
        }

        if (this.playerHide != null && this.playerHide.IsInsideSpot)
        {
            this.lookTarget = null;
            this.LookHideSpot = null;
            this.LookItem = null;
            this.lookDelta = Vector2.zero;
            return;
        }

        if (this.heldTarget != null && this.heldTransform == null)
        {
            this.heldTarget = null;
            if (this.playerLook != null)
            {
                this.playerLook.IsLocked = false;
            }
        }

        if (this.heldTarget != null)
        {
            this.heldTarget.OnGrabUpdate(this.lookDelta);

            float distance = Vector3.Distance(this.cameraTransform.position, this.heldTransform.position);
            if (distance > this.releaseDistance)
            {
                this.EndGrab();
            }
        }
        else
        {
            this.FindLookTarget();
        }

        this.lookDelta = Vector2.zero;
    }

    private void FindLookTarget()
    {
        this.lookTarget = null;
        this.LookHideSpot = null;
        this.LookItem = null;

        RaycastHit hit;
        if (Physics.Raycast(this.cameraTransform.position, this.cameraTransform.forward, out hit, this.reachDistance))
        {
            this.lookTarget = hit.collider.GetComponentInParent<IInteractable>();

            this.LookHideSpot = hit.collider.GetComponentInParent<HideSpot>();

            this.LookItem = hit.collider.GetComponentInParent<ItemPickup>();
        }
    }

    private void StartGrab()
    {
        if (this.lookTarget == null)
        {
            return;
        }

        this.heldTarget = this.lookTarget;
        this.heldTransform = ((MonoBehaviour)this.lookTarget).transform;
        this.heldTarget.OnGrabStart(this.cameraTransform);

        if (this.playerLook != null && this.heldTarget.LocksLook)
        {
            this.playerLook.IsLocked = true;
        }
    }

    public void ReleaseGrab()
    {
        this.EndGrab();
    }

    private void EndGrab()
    {
        if (this.heldTarget != null)
        {
            this.heldTarget.OnGrabEnd();
        }

        this.heldTarget = null;
        this.heldTransform = null;

        if (this.playerLook != null)
        {
            this.playerLook.IsLocked = false;
        }
    }

    private void OnGUI()
    {
        bool isActive = this.lookTarget != null || this.heldTarget != null || this.LookHideSpot != null || this.LookItem != null;
        float size = isActive ? 10f : 4f;

        GUI.color = isActive ? Color.white : new Color(1f, 1f, 1f, 0.5f);
        Rect rect = new Rect(Screen.width / 2f - size / 2f, Screen.height / 2f - size / 2f, size, size);
        GUI.DrawTexture(rect, this.dotTexture);
    }
}
