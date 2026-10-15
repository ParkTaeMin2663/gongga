using UnityEngine;

/// <summary>
/// 마우스로 잡고 끌어서 여닫는 문 (암네시아 방식).
/// 마우스를 앞으로 밀면 문을 밀고, 뒤로 당기면 문을 당긴다.
///
/// [붙이는 곳] 문 오브젝트 (Cube)
/// [같이 필요한 컴포넌트]
///   - Box Collider : Cube에 기본으로 있음
///   - Rigidbody    : 강의 03장. 힘을 받아 움직이는 몸
///   - Hinge Joint  : [강의 외] 문 한쪽 끝을 경첩처럼 고정하고, 0~90도만 돌게 제한
///
/// [동작] 잡고 있는 동안 마우스 움직임만큼 AddForce로 문을 민다. (강의 03장 AddForce)
///        실제로 어느 방향으로 얼마나 돌지는 Hinge Joint와 물리 엔진이 정한다.
/// </summary>
public class DraggableDoor : MonoBehaviour, IInteractable
{
    [Header("힘")]
    [Tooltip("마우스를 1픽셀 움직일 때 문을 미는 힘. 문이 너무 무거우면 올리고, 너무 휙휙 열리면 내린다.")]
    public float pushStrength = 0.4f;

    [Header("잠금")]
    [Tooltip("체크하면 잠긴 문. 잡아도 열리지 않는다. (나중에 열쇠로 풀 예정)")]
    public bool isLocked = false;

    // ----- Start()에서 한 번만 찾아 저장 (강의 04장) -----
    private Rigidbody rb;

    // 잡고 있는 동안 플레이어 카메라 (어느 쪽이 "앞"인지 알기 위해)
    private Transform cameraTransform;

    private void Start()
    {
        this.rb = this.GetComponent<Rigidbody>();

        if (this.rb == null)
        {
            Debug.LogError(this.name + "에 Rigidbody가 없습니다.");
            return;
        }

        // 손으로 잡기 전에는 문을 그 자리에 고정한다. (강의 03장 Is Kinematic)
        // Is Kinematic = 물리(힘, 충돌)로 움직이지 않는 상태 → 몸으로 밀어도 안 열리고, 혼자 닫히지도 않는다.
        this.rb.isKinematic = true;
    }

    public void OnGrabStart(Transform cameraTransform)
    {
        this.cameraTransform = cameraTransform;

        if (this.isLocked)
        {
            // 나중에 덜컹거리는 소리를 여기서 재생한다.
            Debug.Log(this.name + " : 잠겨 있다.");
            return;
        }

        // 잡은 동안만 물리로 움직일 수 있게 푼다.
        if (this.rb != null)
        {
            this.rb.isKinematic = false;
        }
    }

    public void OnGrabUpdate(Vector2 mouseDelta)
    {
        if (this.isLocked || this.rb == null || this.cameraTransform == null)
        {
            return;
        }

        // 카메라 기준 앞/오른쪽 방향. 위아래(y)는 버려서 땅과 평행하게 만든다.
        Vector3 forward = this.cameraTransform.forward;
        forward.y = 0f;
        forward = forward.normalized; // 강의 03장 normalized

        Vector3 right = this.cameraTransform.right;
        right.y = 0f;
        right = right.normalized;

        // 마우스 앞뒤(y) = 밀기/당기기, 마우스 좌우(x) = 옆으로 밀기
        Vector3 push = forward * mouseDelta.y + right * mouseDelta.x;

        // 강의 03장 AddForce.
        // [강의 외] ForceMode.Impulse: 한 번에 "툭" 미는 힘. 마우스를 움직인 거리만큼만 밀어서
        //           프레임 수(컴퓨터 속도)와 상관없이 같은 느낌이 나게 한다.
        this.rb.AddForce(push * this.pushStrength, ForceMode.Impulse);
    }

    public void OnGrabEnd()
    {
        this.cameraTransform = null;

        // 손을 놓으면 그 각도 그대로 다시 고정한다.
        if (this.rb != null)
        {
            this.rb.isKinematic = true;
        }
    }
}
