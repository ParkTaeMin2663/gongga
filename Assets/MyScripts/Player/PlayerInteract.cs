using UnityEngine;
using UnityEngine.InputSystem; // Input System Package를 쓰기 위해 반드시 포함 (강의 03장)

/// <summary>
/// 바라보는 물체를 찾고, 마우스 왼쪽으로 잡아서 끄는 기능. 화면 가운데에 조준점도 그린다.
///
/// [붙이는 곳] Player 오브젝트 (Player Input이 있는 오브젝트)
/// [Inspector 연결]
///   - Camera Transform : Player의 자식 Main Camera
///   - Player Look      : Player에 붙어 있는 PlayerLook (잡는 동안 시점을 멈추기 위해)
///
/// [입력] 강의 04장 Fire처럼 새 액션 Grab 추가
///   - Path = Mouse Left Button, Interaction = Press (Press And Release)
///   - 누른 순간 OnGrab(isPressed = true), 뗀 순간 OnGrab(isPressed = false)
///   - OnLook은 PlayerLook과 이 스크립트가 둘 다 받는다. 잡는 동안에는 이쪽이 문을 미는 데 쓴다.
/// </summary>
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

    // ----- 상태 -----
    private IInteractable lookTarget;   // 지금 바라보고 있는 잡을 수 있는 물체 (없으면 null)
    private IInteractable heldTarget;   // 지금 잡고 있는 물체 (없으면 null)
    private Transform heldTransform;    // 잡고 있는 물체의 위치 (거리 확인용)
    private Vector2 lookDelta;          // 이번 프레임 마우스 움직임

    // 조준점 그리기용 흰색 그림 (Unity가 기본으로 제공하는 1x1 흰색)
    private Texture2D dotTexture;

    private void Start()
    {
        if (this.cameraTransform == null)
        {
            Debug.LogError("[PlayerInteract] Camera Transform이 비어 있습니다. Main Camera를 드래그해 주세요.");
        }

        this.dotTexture = Texture2D.whiteTexture;
    }

    // 마우스가 움직일 때마다 호출 (PlayerLook도 같은 메시지를 받는다)
    private void OnLook(InputValue value)
    {
        this.lookDelta += value.Get<Vector2>();
    }

    // 마우스 왼쪽을 누른 순간 / 뗀 순간 (Press And Release)
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

    private void Update()
    {
        if (this.cameraTransform == null)
        {
            return;
        }

        if (this.heldTarget != null)
        {
            // 잡고 있는 동안: 마우스 움직임을 물체에 전달
            this.heldTarget.OnGrabUpdate(this.lookDelta);

            // 너무 멀어지면 놓친다
            float distance = Vector3.Distance(this.cameraTransform.position, this.heldTransform.position);
            if (distance > this.releaseDistance)
            {
                this.EndGrab();
            }
        }
        else
        {
            // 잡고 있지 않을 때: 무엇을 보고 있는지 찾는다
            this.FindLookTarget();
        }

        this.lookDelta = Vector2.zero;
    }

    private void FindLookTarget()
    {
        this.lookTarget = null;

        // [강의 외] Physics.Raycast: 카메라에서 정면으로 보이지 않는 선을 쏴서 처음 맞은 물체를 알려준다.
        //           reachDistance(m)까지만 쏜다. 벽에 먼저 맞으면 벽 너머 물체는 찾지 않는다.
        RaycastHit hit;
        if (Physics.Raycast(this.cameraTransform.position, this.cameraTransform.forward, out hit, this.reachDistance))
        {
            // 맞은 물체(또는 그 부모)에 IInteractable이 붙어 있는지 확인
            this.lookTarget = hit.collider.GetComponentInParent<IInteractable>();
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

        // 잡는 동안에는 고개를 고정 (마우스는 물체를 미는 데 쓴다)
        if (this.playerLook != null)
        {
            this.playerLook.IsLocked = true;
        }
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

    // 강의 04장 OnGUI: 화면 가운데 조준점
    // 평소에는 작은 점, 잡을 수 있는 물체를 보거나 잡고 있으면 큰 점
    private void OnGUI()
    {
        bool isActive = this.lookTarget != null || this.heldTarget != null;
        float size = isActive ? 10f : 4f;

        GUI.color = isActive ? Color.white : new Color(1f, 1f, 1f, 0.5f);
        Rect rect = new Rect(Screen.width / 2f - size / 2f, Screen.height / 2f - size / 2f, size, size);
        GUI.DrawTexture(rect, this.dotTexture);
    }
}
