using UnityEngine;
using UnityEngine.InputSystem; // Input System Package를 쓰기 위해 반드시 포함 (강의 03장)

/// <summary>
/// 1인칭 마우스 시점. 마우스를 좌우로 움직이면 좌우로, 위아래로 움직이면 위아래로 본다.
///
/// [붙이는 곳] Player 오브젝트 (Player Input이 있는 오브젝트에 붙여야 OnLook 메시지를 받는다)
/// [Inspector 연결] Camera Transform 칸에 Player의 자식인 Main Camera를 드래그 (강의 03장 "변수에 물체 연결")
///
/// [설계] Player(몸)는 Rigidbody라서 직접 돌리면 물리와 부딪쳐 화면이 떨릴 수 있다.
///        그래서 몸은 그대로 두고, 자식인 Main Camera만 회전시킨다. (강의 04장 계층구조)
///        이동 방향은 PlayerMovement가 이 카메라의 방향을 보고 정한다.
/// </summary>
public class PlayerLook : MonoBehaviour
{
    [Header("연결")]
    [Tooltip("Player의 자식 Main Camera를 여기에 드래그")]
    public Transform cameraTransform;

    [Header("감도")]
    [Tooltip("마우스 감도. 마우스를 1픽셀 움직일 때 몇 도 돌지. 너무 빠르면 줄이고, 느리면 늘린다.")]
    public float mouseSensitivity = 0.1f;

    [Header("위아래 각도 제한")]
    [Tooltip("위로 올려다볼 수 있는 최대 각도 (음수 = 위쪽)")]
    public float minPitch = -80f;

    [Tooltip("아래로 내려다볼 수 있는 최대 각도 (양수 = 아래쪽)")]
    public float maxPitch = 80f;

    // ----- 현재 각도 -----
    private float yaw;    // 좌우 각도 (Y축 회전)
    private float pitch;  // 위아래 각도 (X축 회전)

    // ----- 이번 프레임에 들어온 마우스 움직임 -----
    private Vector2 lookDelta;

    private void Start()
    {
        if (this.cameraTransform == null)
        {
            Debug.LogError("[PlayerLook] Camera Transform이 비어 있습니다. Inspector에서 Main Camera를 드래그해 주세요.");
            return;
        }

        // 시작할 때 카메라가 보고 있던 좌우 방향에서 출발
        this.yaw = this.cameraTransform.eulerAngles.y;
        this.pitch = 0f;

        // [강의 외] 마우스 커서를 화면 가운데에 고정하고 숨긴다. (1인칭 게임의 기본)
        // 에디터에서는 Esc를 누르면 커서가 다시 나타나고, Game 창을 클릭하면 다시 고정된다.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 마우스가 움직일 때마다 Player Input이 호출한다. 값 = 이번에 움직인 픽셀 수 (x = 좌우, y = 위아래)
    // 한 프레임에 여러 번 불릴 수 있어서 더해서(+=) 모아 둔다.
    private void OnLook(InputValue value)
    {
        this.lookDelta += value.Get<Vector2>();
    }

    private void Update()
    {
        if (this.cameraTransform == null)
        {
            return;
        }

        // 1) 모아 둔 마우스 움직임을 각도로 바꾼다.
        //    마우스 움직임은 이미 "이번 프레임에 움직인 양"이라 Time.deltaTime을 곱하지 않는다.
        this.yaw += this.lookDelta.x * this.mouseSensitivity;
        this.pitch -= this.lookDelta.y * this.mouseSensitivity; // 마우스를 위로 올리면 위를 보도록 빼기

        // 2) 위아래는 목이 꺾이지 않게 제한
        this.pitch = Mathf.Clamp(this.pitch, this.minPitch, this.maxPitch);

        // 3) 카메라 회전을 직접 지정 (강의 03장: rotation + Quaternion.Euler)
        this.cameraTransform.rotation = Quaternion.Euler(this.pitch, this.yaw, 0f);

        // 4) 사용한 움직임은 비운다. (다음 프레임에 또 모은다)
        this.lookDelta = Vector2.zero;
    }
}
