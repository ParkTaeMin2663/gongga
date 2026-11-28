using UnityEngine;
using UnityEngine.InputSystem;

// 1인칭 플레이어 이동 (WASD 걷기, Shift 달리기). 중력은 Rigidbody가 처리한다.
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

    [Header("발소리 (실제 소리)")]
    [Tooltip("걸을 때 한 걸음 간격(초)")]
    public float walkStepInterval = 0.55f;

    [Tooltip("달릴 때 한 걸음 간격(초)")]
    public float sprintStepInterval = 0.35f;

    [Tooltip("걸을 때 발소리 볼륨 (0~1)")]
    [Range(0f, 1f)]
    public float walkVolume = 0.35f;

    [Tooltip("달릴 때 발소리 볼륨 (0~1)")]
    [Range(0f, 1f)]
    public float sprintVolume = 0.8f;

    [Tooltip("걸음마다 Pitch를 1 ± 이 값 사이에서 무작위로 바꿔 똑같은 소리 반복처럼 안 들리게 한다. (강의 03장 Pitch, Random.Range)")]
    [Range(0f, 0.3f)]
    public float pitchVariation = 0.1f;

    [Header("발소리 (괴물이 듣는 소리)")]
    [Tooltip("달리는 발소리 크기 = 괴물이 듣는 거리(m). 걷기는 괴물에게 들리지 않는다.")]
    public float sprintNoiseLoudness = 6f;

    private Rigidbody rb;
    private AudioSource footstepAudio;
    private NoiseSystem noiseSystem;
    private float stepTimer;

    private Vector2 moveInput;
    private bool isSprintHeld;

    public bool IsSprinting { get; private set; }

    public bool IsMoving { get; private set; }

    private void Start()
    {
        this.rb = this.GetComponent<Rigidbody>();

        if (this.rb == null)
        {
            Debug.LogError("[PlayerMovement] Player에 Rigidbody가 없습니다. Add Component로 Rigidbody를 추가해 주세요.");
        }

        this.footstepAudio = this.GetComponent<AudioSource>();
        if (this.footstepAudio == null)
        {
            Debug.LogWarning("[PlayerMovement] Player에 Audio Source가 없어서 발소리가 나지 않습니다.");
        }

        GameObject noiseObject = GameObject.Find("NoiseSystem");   // 강의 04장: GameObject.Find (Start에서 한 번)
        if (noiseObject != null)
        {
            this.noiseSystem = noiseObject.GetComponent<NoiseSystem>();
        }
        else
        {
            Debug.LogWarning("[PlayerMovement] 씬에 NoiseSystem 오브젝트가 없습니다. 괴물이 소리를 못 듣습니다.");
        }
    }

    private void OnMove(InputValue value)   // 강의 02장: Player Input 메시지 함수 OnMove
    {
        this.moveInput = value.Get<Vector2>();
    }

    private void OnSprint(InputValue value)   // 강의 02장: Player Input 메시지 함수 OnSprint
    {
        this.isSprintHeld = value.isPressed;   // 강의 02장: value.isPressed (Press And Release)
    }

    private void Update()
    {
        if (this.rb == null)
        {
            return;
        }

        bool isMoving = this.moveInput != Vector2.zero;
        this.IsMoving = isMoving;
        this.IsSprinting = this.isSprintHeld && isMoving;

        float speed = this.IsSprinting ? this.sprintSpeed : this.walkSpeed;

        if (isMoving)
        {
            this.stepTimer = this.stepTimer - Time.deltaTime;   // 강의 03장: Time.deltaTime
            if (this.stepTimer <= 0f)
            {
                this.stepTimer = this.IsSprinting ? this.sprintStepInterval : this.walkStepInterval;
                this.PlayFootstep();
            }
        }
        else
        {
            this.stepTimer = 0f;
        }

        Transform basis = this.cameraTransform != null ? this.cameraTransform : this.transform;
        Quaternion yawOnly = Quaternion.Euler(0f, basis.eulerAngles.y, 0f);   // 강의 03장: Quaternion.Euler
        Vector3 forward = yawOnly * Vector3.forward;
        Vector3 right = yawOnly * Vector3.right;
        Vector3 direction = right * this.moveInput.x + forward * this.moveInput.y;

        if (direction.magnitude > 1f)
        {
            direction = direction.normalized;
        }

        Vector3 velocity = direction * speed;
        velocity.y = this.rb.linearVelocity.y;
        this.rb.linearVelocity = velocity;
    }

    private void PlayFootstep()
    {
        if (this.footstepAudio != null)
        {
            this.footstepAudio.volume = this.IsSprinting ? this.sprintVolume : this.walkVolume;
            this.footstepAudio.pitch = Random.Range(1f - this.pitchVariation, 1f + this.pitchVariation);   // 강의 03장: Random.Range, AudioSource Pitch
            this.footstepAudio.Play();   // 강의 03장: AudioSource.Play
        }

        if (this.IsSprinting && this.noiseSystem != null)
        {
            this.noiseSystem.Emit(this.transform.position, this.sprintNoiseLoudness, "달리기");
        }
    }
}
