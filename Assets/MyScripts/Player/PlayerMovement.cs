using UnityEngine;
using UnityEngine.InputSystem; // Input System Package를 쓰기 위해 반드시 포함 (강의 03장)

/// <summary>
/// 1인칭 플레이어 이동 (WASD 걷기, Shift 달리기). 중력은 Rigidbody가 처리한다.
///
/// [붙이는 곳] Player 오브젝트
/// [같이 필요한 컴포넌트] (강의 03장 Rigidbody, 02장 Player Input)
///   - Rigidbody      : Freeze Rotation X/Y/Z 체크 (넘어지지 않게)
///   - Capsule Collider: 플레이어 몸의 충돌 영역
///   - Player Input   : Actions = InputSystem_Actions, Default Map = Player, Behavior = Send Messages
///
/// [입력] 강의 02장 방식 — Player Input이 보내는 메시지 함수
///   - OnMove  : WASD
///   - OnSprint: Shift. Sprint 액션에 Interaction "Press"(Trigger Behavior = Press And Release)를
///               넣어야 "누른 순간"과 "뗀 순간" 둘 다 이 함수가 불린다.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("연결")]
    [Tooltip("Player의 자식 Main Camera를 여기에 드래그. 카메라가 보는 방향으로 걷는다.")]
    public Transform cameraTransform;

    [Header("이동 속도")]
    [Tooltip("걸을 때 속도 (1초에 몇 m 움직이는지)")]
    public float walkSpeed = 3f;

    [Tooltip("Shift를 누르고 달릴 때 속도. 달리면 나중에 괴물이 소리를 듣게 된다.")]
    public float sprintSpeed = 5.5f;

    // ----- Start()에서 한 번만 찾아 저장해 두는 컴포넌트 (강의 04장: 매 프레임 찾지 않기) -----
    private Rigidbody rb;

    // ----- 입력 값 -----
    private Vector2 moveInput;   // WASD 입력. 왼쪽 (-1, 0), 오른쪽 (1, 0), 앞 (0, 1), 뒤 (0, -1)
    private bool isSprintHeld;   // Shift를 누르고 있는지

    /// <summary>지금 달리고 있는지. 나중에 소리 시스템(괴물이 듣는 소리)에서 사용한다.</summary>
    public bool IsSprinting { get; private set; }

    private void Start()
    {
        this.rb = this.GetComponent<Rigidbody>();

        if (this.rb == null)
        {
            Debug.LogError("[PlayerMovement] Player에 Rigidbody가 없습니다. Add Component로 Rigidbody를 추가해 주세요.");
        }
    }

    // WASD를 누르거나 뗄 때마다 Player Input이 호출한다. (키를 다 떼면 (0, 0))
    private void OnMove(InputValue value)
    {
        this.moveInput = value.Get<Vector2>();
    }

    // Shift를 누른 순간 isPressed = true, 뗀 순간 isPressed = false
    // (Sprint 액션에 Press And Release를 설정해야 뗀 순간에도 호출된다 — 강의 02장 Interactions)
    private void OnSprint(InputValue value)
    {
        this.isSprintHeld = value.isPressed;
    }

    private void Update()
    {
        if (this.rb == null)
        {
            return;
        }

        // 1) 달리기 여부: Shift를 누르고 있고, 실제로 움직이는 중일 때만 달리는 것으로 본다.
        bool isMoving = this.moveInput != Vector2.zero;
        this.IsSprinting = this.isSprintHeld && isMoving;

        float speed = this.IsSprinting ? this.sprintSpeed : this.walkSpeed;

        // 2) 이동 방향: 카메라가 바라보는 방향 기준
        //    카메라의 좌우 각도(yaw)만 쓰고 위아래 각도는 버린다. → 하늘을 봐도 땅을 따라 걷는다.
        Transform basis = this.cameraTransform != null ? this.cameraTransform : this.transform;
        Quaternion yawOnly = Quaternion.Euler(0f, basis.eulerAngles.y, 0f);
        Vector3 forward = yawOnly * Vector3.forward; // 앞쪽 (강의 03장 Vector3.forward)
        Vector3 right = yawOnly * Vector3.right;     // 오른쪽
        Vector3 direction = right * this.moveInput.x + forward * this.moveInput.y;

        // 대각선(W+D)으로 갈 때 더 빨라지지 않도록 길이를 1로 맞춘다 (강의 03장 normalized)
        if (direction.magnitude > 1f)
        {
            direction = direction.normalized;
        }

        // 3) Rigidbody의 속도를 정한다.
        //    linearVelocity = Rigidbody의 "1초당 이동 속도" (Unity 6 이름, 예전 이름은 velocity)
        //    속도(1초 기준)를 주는 것이라 Time.deltaTime을 곱하지 않는다. 프레임과 상관없이 일정한 속도가 된다.
        //    위아래(y) 속도는 그대로 둬야 중력으로 자연스럽게 떨어진다.
        Vector3 velocity = direction * speed;
        velocity.y = this.rb.linearVelocity.y;
        this.rb.linearVelocity = velocity;
    }
}
