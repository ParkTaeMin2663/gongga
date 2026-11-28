using UnityEngine;

// 마우스 왼쪽을 누르고 있는 동안 손에 들고 다니는 물건 (암네시아 방식).
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
    [Tooltip("놓는 순간 남는 최고 속도(m/s). 휘두르다 놓아도 멀리 날아가지 않게 한다.")]
    public float dropMaxSpeed = 2f;

    [Header("던지기")]
    [Tooltip("던지는 힘. Mass 1 기준 400이면 1초에 약 8m 날아가는 속도. 무거운 물건(Mass 큼)일수록 덜 날아간다.")]
    public float throwForce = 400f;

    [Header("소리")]
    [Tooltip("이 속도(m/s)보다 세게 부딪히면 소리가 난다. 살짝 내려놓을 때는 안 나게.")]
    public float noiseMinImpactSpeed = 2f;

    [Tooltip("떨어지거나 던져져 부딪힐 때 소리 크기 = 괴물이 듣는 거리(m)")]
    public float noiseLoudness = 8f;

    public bool LocksLook { get { return false; } }

    public bool IsHeld { get; private set; }

    private Rigidbody rb;
    private AudioSource audioSource;
    private NoiseSystem noiseSystem;

    private Transform cameraTransform;

    private bool originalUseGravity;
    private float originalAngularDamping;

    private void Start()
    {
        this.rb = this.GetComponent<Rigidbody>();

        if (this.rb == null)
        {
            Debug.LogError(this.name + "에 Rigidbody가 없습니다. Add Component로 Rigidbody를 추가해 주세요.");
        }

        this.audioSource = this.GetComponent<AudioSource>();

        GameObject noiseObject = GameObject.Find("NoiseSystem");
        if (noiseObject != null)
        {
            this.noiseSystem = noiseObject.GetComponent<NoiseSystem>();
        }
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (this.IsHeld)
        {
            return;
        }

        if (Time.time < 1f)
        {
            return;
        }

        float impactSpeed = coll.relativeVelocity.magnitude;
        if (impactSpeed < this.noiseMinImpactSpeed)
        {
            return;
        }

        if (this.audioSource != null)
        {
            this.audioSource.Play();
        }

        if (this.noiseSystem != null)
        {
            this.noiseSystem.Emit(this.transform.position, this.noiseLoudness, this.name + " 떨어짐");
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

        Vector3 direction = this.cameraTransform.forward;
        if (direction.y < this.lowestLook)
        {
            direction.y = this.lowestLook;
            direction = direction.normalized;
        }
        Vector3 holdPoint = this.cameraTransform.position + direction * this.holdDistance;

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

        this.rb.useGravity = this.originalUseGravity;
        this.rb.angularDamping = this.originalAngularDamping;

        Vector3 velocity = this.rb.linearVelocity;
        if (velocity.magnitude > this.dropMaxSpeed)
        {
            this.rb.linearVelocity = velocity.normalized * this.dropMaxSpeed;
        }
    }

    public void Throw(Vector3 direction)
    {
        if (this.rb == null)
        {
            return;
        }

        this.rb.AddForce(direction.normalized * this.throwForce);
    }
}
