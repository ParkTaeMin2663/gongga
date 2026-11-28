using UnityEngine;

// 마우스로 잡고 끌어서 여닫는 서랍 (암네시아 방식).
public class DraggableDrawer : MonoBehaviour, IInteractable
{
    [Header("움직임")]
    [Tooltip("최대로 열리는 길이(m). 서랍 깊이보다 조금 짧게 하면 빠지지 않는다.")]
    public float openDistance = 0.35f;

    [Tooltip("마우스를 1픽셀 당길 때 서랍이 움직이는 거리(m). 너무 느리면 올린다.")]
    public float dragSensitivity = 0.002f;

    [Tooltip("부모 기준으로 서랍이 열리는 방향. 기본 (0, 0, 1) = 부모의 앞쪽(+Z). 가져온 모델에서 서랍이 엉뚱한 쪽으로 열리면 (0, 0, -1), (1, 0, 0) 등으로 바꾼다.")]
    public Vector3 openDirection = new Vector3(0f, 0f, 1f);

    [Header("잠금")]
    [Tooltip("체크하면 잠긴 서랍. 잡아도 열리지 않는다.")]
    public bool isLocked = false;

    [Header("소리 (없어도 됨)")]
    [Tooltip("닫힌 상태에서 열리기 시작할 때. 서랍에 Audio Source를 붙이고 여기로 드래그")]
    public AudioSource openSound;

    [Tooltip("끝까지 닫혔을 때. 두 번째 Audio Source를 붙이고 여기로 드래그")]
    public AudioSource closeSound;

    [Tooltip("잠긴 서랍을 잡았을 때 덜컹 소리. 세 번째 Audio Source")]
    public AudioSource lockedSound;

    public bool LocksLook { get { return true; } }

    public float OpenAmount { get; private set; }

    private Vector3 closedLocalPosition;

    private void Start()
    {
        if (this.transform.parent == null)
        {
            Debug.LogError(this.name + " : 서랍은 서랍장(빈 오브젝트)의 자식이어야 합니다.");
        }

        this.closedLocalPosition = this.transform.localPosition;
        this.OpenAmount = 0f;
    }

    public void OnGrabStart(Transform cameraTransform)
    {
        if (this.isLocked)
        {
            Debug.Log(this.name + " : 잠겨 있다.");
            this.PlaySound(this.lockedSound);
        }
    }

    public void OnGrabUpdate(Vector2 mouseDelta)
    {
        if (this.isLocked)
        {
            return;
        }

        float amount = this.OpenAmount - mouseDelta.y * this.dragSensitivity;

        if (amount < 0f)
        {
            amount = 0f;
        }
        if (amount > this.openDistance)
        {
            amount = this.openDistance;
        }

        if (this.OpenAmount == 0f && amount > 0f)
        {
            this.PlaySound(this.openSound);
        }
        else if (this.OpenAmount > 0f && amount == 0f)
        {
            this.PlaySound(this.closeSound);
        }

        this.OpenAmount = amount;

        this.transform.localPosition = this.closedLocalPosition + this.openDirection.normalized * this.OpenAmount;
    }

    public void OnGrabEnd()
    {
    }

    private void PlaySound(AudioSource sound)
    {
        if (sound != null)
        {
            sound.Play();
        }
    }
}
