using UnityEngine;

/// <summary>
/// 왼손에 든 손전등이 걸을 때 살짝 흔들리게 한다. 뛰면 더 크게, 멈추면 천천히 제자리로.
///
/// [붙이는 곳] Main Camera의 자식 HeldFlashlight (Player 프리팹에 들어 있음)
///   Main Camera
///     └ HeldFlashlight ← 이 스크립트 (왼쪽 아래 위치)
///         ├ pastic_torch_6v (손전등 모델)
///         └ Flashlight (Spotlight, 렌즈 앞) → 손전등과 같이 흔들려서 불빛도 살짝 출렁인다
///
/// [동작] 강의 04장 계층구조: 자식은 부모(카메라) 기준 상대 위치(localPosition)를 가진다.
///        처음 위치를 기억해 두고, 걸음에 맞춰 사인(Sin)·코사인(Cos) 곡선만큼 조금씩 더한다.
/// </summary>
public class FlashlightSway : MonoBehaviour
{
    [Header("연결 (비워 두면 자동으로 찾음)")]
    public PlayerMovement playerMovement;

    [Header("흔들림")]
    [Tooltip("걸을 때 흔들리는 빠르기")]
    public float walkBobSpeed = 8f;
    [Tooltip("뛸 때 흔들리는 빠르기")]
    public float sprintBobSpeed = 13f;
    [Tooltip("걸을 때 흔들리는 폭(m)")]
    public float walkBobAmount = 0.012f;
    [Tooltip("뛸 때 흔들리는 폭(m)")]
    public float sprintBobAmount = 0.028f;
    [Tooltip("멈췄을 때 제자리로 돌아오는 빠르기")]
    public float settleSpeed = 6f;

    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;
    private float bobTimer;       // 걸음 박자
    private float currentAmount;  // 지금 흔들림 폭 (부드럽게 바뀜)

    private void Start()
    {
        this.startLocalPosition = this.transform.localPosition;
        this.startLocalRotation = this.transform.localRotation;

        if (this.playerMovement == null)
        {
            this.playerMovement = this.GetComponentInParent<PlayerMovement>();
        }
    }

    private void Update()
    {
        bool moving = this.playerMovement != null && this.playerMovement.enabled && this.playerMovement.IsMoving;
        bool sprinting = moving && this.playerMovement.IsSprinting;

        // 목표 흔들림 폭으로 부드럽게 (강의 03장 Time.deltaTime)
        float target = moving ? (sprinting ? this.sprintBobAmount : this.walkBobAmount) : 0f;
        this.currentAmount = Mathf.Lerp(this.currentAmount, target, this.settleSpeed * Time.deltaTime);

        if (moving)
        {
            this.bobTimer = this.bobTimer + Time.deltaTime * (sprinting ? this.sprintBobSpeed : this.walkBobSpeed);
        }

        // 좌우로는 한 번, 위아래로는 걸음마다 한 번씩 (∞ 모양으로 출렁)
        Vector3 offset = new Vector3(
            Mathf.Cos(this.bobTimer) * this.currentAmount,
            Mathf.Abs(Mathf.Sin(this.bobTimer)) * this.currentAmount * 1.2f - this.currentAmount * 0.6f,
            0f);
        this.transform.localPosition = this.startLocalPosition + offset;

        // 살짝 기울기도 (강의 03장 Quaternion.Euler)
        float roll = Mathf.Cos(this.bobTimer) * this.currentAmount * 80f;
        this.transform.localRotation = this.startLocalRotation * Quaternion.Euler(0f, 0f, roll);
    }
}
