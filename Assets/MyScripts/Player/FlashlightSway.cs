using UnityEngine;

// 왼손에 든 손전등이 걸을 때 살짝 흔들리게 한다. 뛰면 더 크게, 멈추면 천천히 제자리로.
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
    private float bobTimer;
    private float currentAmount;

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

        float target = moving ? (sprinting ? this.sprintBobAmount : this.walkBobAmount) : 0f;
        this.currentAmount = Mathf.Lerp(this.currentAmount, target, this.settleSpeed * Time.deltaTime);   // 강의 03장: Time.deltaTime

        if (moving)
        {
            this.bobTimer = this.bobTimer + Time.deltaTime * (sprinting ? this.sprintBobSpeed : this.walkBobSpeed);
        }

        Vector3 offset = new Vector3(
            Mathf.Cos(this.bobTimer) * this.currentAmount,
            Mathf.Abs(Mathf.Sin(this.bobTimer)) * this.currentAmount * 1.2f - this.currentAmount * 0.6f,
            0f);
        this.transform.localPosition = this.startLocalPosition + offset;

        float roll = Mathf.Cos(this.bobTimer) * this.currentAmount * 80f;
        this.transform.localRotation = this.startLocalRotation * Quaternion.Euler(0f, 0f, roll);   // 강의 03장: Quaternion.Euler
    }
}
