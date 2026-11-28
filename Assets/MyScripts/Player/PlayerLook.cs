using UnityEngine;
using UnityEngine.InputSystem;

// 1인칭 마우스 시점. 마우스를 좌우로 움직이면 좌우로, 위아래로 움직이면 위아래로 본다.
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

    private float yaw;
    private float pitch;

    private Vector2 lookDelta;

    public bool IsLocked { get; set; }

    private void Start()
    {
        if (this.cameraTransform == null)
        {
            Debug.LogError("[PlayerLook] Camera Transform이 비어 있습니다. Inspector에서 Main Camera를 드래그해 주세요.");
            return;
        }

        this.yaw = this.cameraTransform.eulerAngles.y;
        this.pitch = 0f;   // 강의 03장: AudioSource Pitch

        // [강의 외] Cursor.lockState / Cursor.visible: 커서를 화면 가운데 고정하고 숨기기
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetYaw(float angle)
    {
        this.SetLook(angle, 0f);
    }

    public void SetLook(float yawAngle, float pitchAngle)
    {
        this.yaw = yawAngle;
        this.pitch = Mathf.Clamp(pitchAngle, this.minPitch, this.maxPitch);
        this.lookDelta = Vector2.zero;

        if (this.cameraTransform != null)
        {
            this.cameraTransform.rotation = Quaternion.Euler(this.pitch, this.yaw, 0f);   // 강의 03장: Quaternion.Euler
        }
    }

    public float Yaw { get { return this.yaw; } }

    public float Pitch { get { return this.pitch; } }

    private void OnLook(InputValue value)   // 강의 02장: Player Input 메시지 함수 OnLook
    {
        this.lookDelta += value.Get<Vector2>();
    }

    private void Update()
    {
        if (this.cameraTransform == null)
        {
            return;
        }

        if (this.IsLocked)
        {
            this.lookDelta = Vector2.zero;
            return;
        }

        this.yaw += this.lookDelta.x * this.mouseSensitivity;
        this.pitch -= this.lookDelta.y * this.mouseSensitivity;

        this.pitch = Mathf.Clamp(this.pitch, this.minPitch, this.maxPitch);

        this.cameraTransform.rotation = Quaternion.Euler(this.pitch, this.yaw, 0f);

        this.lookDelta = Vector2.zero;
    }
}
