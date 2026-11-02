using UnityEngine;

/// <summary>
/// 마우스 왼쪽을 누르고 있는 동안 손에 들고 다니는 물건 (암네시아 방식).
/// 누르고 있으면 카메라 앞에 떠서 따라오고, 떼면 그 자리에 떨어진다.
///
/// [붙이는 곳] 집을 수 있는 물건 (예: Cube로 만든 상자, 병, 깡통)
/// [같이 필요한 컴포넌트]
///   - Collider  : Cube/Sphere에 기본으로 있음
///   - Rigidbody : 강의 03장. Collision Detection = Continuous 권장 (빠르게 움직여도 벽을 뚫지 않게)
///
/// [동작] 들고 있는 동안 "카메라 앞 들기 위치"로 가는 속도를 Rigidbody에 매 프레임 준다.
///        (PlayerMovement와 같은 linearVelocity 방식 — 벽에 막히면 물리 엔진이 알아서 멈춘다)
/// </summary>
public class PickupObject : MonoBehaviour, IInteractable
{
    [Header("들기 위치")]
    [Tooltip("카메라 앞 몇 m에 들고 있을지")]
    public float holdDistance = 1.2f;

    [Tooltip("들기 위치로 따라오는 빠르기. 높이면 딱 붙어 따라오고, 낮추면 묵직하게 따라온다.")]
    public float followSpeed = 12f;

    [Tooltip("따라올 때 최고 속도(m/s). 너무 빠르면 벽을 뚫을 수 있다.")]
    public float maxSpeed = 8f;

    [Tooltip("아래를 볼 때 물건이 내려가는 한계. -1이면 발밑까지, 0이면 눈높이에서 멈춤. 발밑으로 오면 플레이어를 밀어 올리는 버그가 생긴다.")]
    [Range(-1f, 0f)]
    public float lowestLook = -0.5f;

    [Header("놓기")]
    [Tooltip("놓는 순간 남는 최고 속도(m/s). 휘두르다 놓아도 멀리 날아가지 않게 한다. (던지기는 따로 만든다)")]
    public float dropMaxSpeed = 2f;

    /// <summary>들고 다니는 동안에도 고개를 돌릴 수 있어야 하므로 시점을 잠그지 않는다.</summary>
    public bool LocksLook { get { return false; } }

    /// <summary>지금 손에 들려 있는지. 나중에 던지기에서 사용한다.</summary>
    public bool IsHeld { get; private set; }

    // ----- Start()에서 한 번만 찾아 저장 (강의 04장) -----
    private Rigidbody rb;

    // 들고 있는 동안 플레이어 카메라
    private Transform cameraTransform;

    // 들기 전 설정을 기억했다가 놓을 때 되돌린다
    private bool originalUseGravity;
    private float originalAngularDamping;

    private void Start()
    {
        this.rb = this.GetComponent<Rigidbody>();

        if (this.rb == null)
        {
            Debug.LogError(this.name + "에 Rigidbody가 없습니다. Add Component로 Rigidbody를 추가해 주세요.");
        }
    }

    public void OnGrabStart(Transform cameraTransform)
    {
        if (this.rb == null)
        {
            return;
        }

        this.cameraTransform = cameraTransform;
        this.IsHeld = true;

        // 강의 03장 Rigidbody 설정을 코드로 바꾼다.
        // 들고 있는 동안은 중력을 끄고(Use Gravity), 빙글빙글 돌지 않게 회전을 강하게 줄인다(Angular Damping).
        this.originalUseGravity = this.rb.useGravity;
        this.originalAngularDamping = this.rb.angularDamping;
        this.rb.useGravity = false;
        this.rb.angularDamping = 10f;
    }

    public void OnGrabUpdate(Vector2 mouseDelta)
    {
        if (this.rb == null || this.cameraTransform == null)
        {
            return;
        }

        // 1) 들기 위치: 카메라 정면 holdDistance(m) 앞
        //    너무 아래를 보면 발밑으로 들어가지 않게 아래쪽 각도를 lowestLook까지만 허용한다.
        Vector3 direction = this.cameraTransform.forward;
        if (direction.y < this.lowestLook)
        {
            direction.y = this.lowestLook;
            direction = direction.normalized; // 강의 03장 normalized
        }
        Vector3 holdPoint = this.cameraTransform.position + direction * this.holdDistance;

        // 2) 지금 위치에서 들기 위치까지 남은 거리만큼 속도를 준다. (멀면 빠르게, 가까우면 천천히)
        //    속도(1초 기준)를 주는 것이라 Time.deltaTime을 곱하지 않는다. (PlayerMovement와 같은 방식)
        Vector3 velocity = (holdPoint - this.rb.position) * this.followSpeed;
        if (velocity.magnitude > this.maxSpeed)
        {
            velocity = velocity.normalized * this.maxSpeed;
        }
        this.rb.linearVelocity = velocity;
    }

    public void OnGrabEnd()
    {
        this.cameraTransform = null;
        this.IsHeld = false;

        if (this.rb == null)
        {
            return;
        }

        // 원래 Rigidbody 설정으로 되돌린다 → 중력을 받아 떨어진다.
        this.rb.useGravity = this.originalUseGravity;
        this.rb.angularDamping = this.originalAngularDamping;

        // 휘두르던 속도가 그대로 남아 멀리 날아가지 않게 줄인다.
        Vector3 velocity = this.rb.linearVelocity;
        if (velocity.magnitude > this.dropMaxSpeed)
        {
            this.rb.linearVelocity = velocity.normalized * this.dropMaxSpeed;
        }
    }
}
