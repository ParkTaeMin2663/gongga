using UnityEngine;

/// <summary>
/// 마우스로 잡고 끌어서 여닫는 서랍 (암네시아 방식).
/// 잡은 채로 마우스를 뒤로 당기면 열리고, 앞으로 밀면 닫힌다.
///
/// [붙이는 곳] 서랍 오브젝트 (Cube). 반드시 서랍장(빈 오브젝트, Scale 1)의 자식이어야 한다.
///   Dresser (Empty, Scale 1) ← 파란 화살표(+Z)가 서랍장 앞쪽을 향하게 회전
///     ├ Body   (Cube) 서랍장 몸통
///     └ Drawer (Cube) ← 이 스크립트
/// [같이 필요한 컴포넌트]
///   - Box Collider : Cube에 기본으로 있음
///   - Rigidbody    : 강의 03장. Is Kinematic 체크 (힘이 아니라 위치로 움직이므로 물리에 밀리지 않게)
///
/// [동작] 강의 04장: 자식은 부모 기준 상대 좌표(localPosition)를 가진다.
///        부모 기준 openDirection(기본 +Z) 방향으로만 localPosition을 바꾸고, 열린 길이는 0 ~ openDistance로 제한한다.
/// </summary>
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

    /// <summary>서랍은 마우스로 당겨야 하므로 잡는 동안 시점을 고정한다.</summary>
    public bool LocksLook { get { return true; } }

    /// <summary>지금 열린 길이(m). 0 = 완전히 닫힘. 나중에 단서 연출에서 사용할 수 있다.</summary>
    public float OpenAmount { get; private set; }

    // 닫혀 있을 때의 위치 (Start에서 저장). 여기서 +Z로 openDistance까지만 움직인다.
    private Vector3 closedLocalPosition;

    private void Start()
    {
        if (this.transform.parent == null)
        {
            Debug.LogError(this.name + " : 서랍은 서랍장(빈 오브젝트)의 자식이어야 합니다.");
        }

        // 처음 놓인 위치를 "닫힌 위치"로 기억한다.
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

        // 마우스를 뒤로 당기면(mouseDelta.y < 0) 플레이어 쪽(서랍장 앞쪽, +Z)으로 열린다.
        float amount = this.OpenAmount - mouseDelta.y * this.dragSensitivity;

        // 0(닫힘) ~ openDistance(최대 열림) 사이로 제한
        if (amount < 0f)
        {
            amount = 0f;
        }
        if (amount > this.openDistance)
        {
            amount = this.openDistance;
        }

        // 소리: 닫혀 있다가 열리기 시작한 순간 / 열려 있다가 끝까지 닫힌 순간
        if (this.OpenAmount == 0f && amount > 0f)
        {
            this.PlaySound(this.openSound);
        }
        else if (this.OpenAmount > 0f && amount == 0f)
        {
            this.PlaySound(this.closeSound);
        }

        this.OpenAmount = amount;

        // 부모 기준 openDirection 쪽으로 OpenAmount만큼 밀어낸 위치 (강의 03장 normalized로 길이를 1로 맞춤)
        this.transform.localPosition = this.closedLocalPosition + this.openDirection.normalized * this.OpenAmount;
    }

    public void OnGrabEnd()
    {
        // 손을 놓으면 그 위치에 그대로 멈춰 있다. (Is Kinematic이라 혼자 미끄러지지 않음)
    }

    // 강의 03장 AudioSource.Play(). 칸이 비어 있으면 아무 소리도 내지 않는다.
    private void PlaySound(AudioSource sound)
    {
        if (sound != null)
        {
            sound.Play();
        }
    }
}
