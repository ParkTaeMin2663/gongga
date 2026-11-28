using UnityEngine;

// 마우스로 잡고 밀어서 여닫는 문. 괴물이 두드리다 부술 수 있다.
public class DraggableDoor : MonoBehaviour, IInteractable
{
    [Header("힘")]
    [Tooltip("마우스를 1픽셀 움직일 때 문을 미는 힘. 문이 너무 무거우면 올리고, 너무 휙휙 열리면 내린다.")]
    public float pushStrength = 0.4f;

    [Header("잠금")]
    [Tooltip("체크하면 잠긴 문. 잡아도 열리지 않는다.")]
    public bool isLocked = false;

    [Tooltip("(11/20) 이 열쇠를 가지고 잡으면 잠금이 풀린다. 비워 두면 열쇠로 못 여는 문. 줍는 열쇠(ItemPickup)의 Key Name과 똑같이. 예: 보일러실 열쇠")]
    public string requiredKey = "";

    [Tooltip("(11/25) 잠겨 있을 때 띄울 글. 비워 두면 \"잠겨 있다\" (열쇠 문이면 \"잠겨 있다. ○○가 필요하다\")")]
    public string lockedMessage = "";

    [Tooltip("열쇠로 열 때 찰칵 소리 (없어도 됨). 예: Assets/Audio/Furniture/metalLatch")]
    public AudioClip unlockClip;

    [Header("소리 (없어도 됨)")]
    [Tooltip("닫힌 문이 열리기 시작할 때 끼이익. 문에 Audio Source를 붙이고 여기로 드래그")]
    public AudioSource openSound;

    [Tooltip("열린 문이 다시 닫혔을 때 쾅. 두 번째 Audio Source")]
    public AudioSource closeSound;

    [Tooltip("잠긴 문을 잡았을 때 덜컹. 세 번째 Audio Source")]
    public AudioSource lockedSound;

    [Tooltip("괴물이 두드릴 때 쾅 소리. 네 번째 Audio Source")]
    public AudioSource knockSound;

    [Tooltip("괴물이 부술 때 와장창 소리. 자식 오브젝트 BreakSoundPoint의 Audio Source (부서질 때 그 자리에 남는다)")]
    public AudioSource breakSound;

    [Tooltip("부서진 문짝이 바닥·벽에 부딪힐 때 쿵. 여섯 번째 Audio Source (문짝에 붙어 있어 떨어진 자리에서 들린다)")]
    public AudioSource fallSound;

    [Tooltip("부서진 문짝이 이 속도(m/s)보다 세게 부딪혀야 쿵 소리가 난다")]
    public float fallSoundMinSpeed = 1f;

    [Tooltip("이 속도(m/s) 이상이면 최대 볼륨")]
    public float fallSoundMaxSpeed = 6f;

    [Tooltip("이 각도(도)보다 적게 열려 있으면 '닫힌 문'으로 본다.")]
    public float closedAngle = 3f;

    [Header("괴물")]
    [Tooltip("문이 부서질 때 나는 소리 크기 = 괴물이 듣는 거리(m)")]
    public float breakNoiseLoudness = 12f;

    public bool IsClosed { get { return this.isClosed; } }

    public bool IsBroken { get; private set; }

    public bool LocksLook { get { return true; } }

    private Rigidbody rb;

    private HingeJoint hinge;

    private bool isClosed = true;

    private Transform cameraTransform;

    private float settleTimer;

    private float lastFallSoundTime = -10f;

    private NoiseSystem noiseSystem;

    private void Start()
    {
        this.rb = this.GetComponent<Rigidbody>();
        this.hinge = this.GetComponent<HingeJoint>();

        GameObject noiseObject = GameObject.Find("NoiseSystem");
        if (noiseObject != null)
        {
            this.noiseSystem = noiseObject.GetComponent<NoiseSystem>();
        }

        if (this.rb == null)
        {
            Debug.LogError(this.name + "에 Rigidbody가 없습니다.");
            return;
        }

        this.rb.isKinematic = true;
    }

    public void OnGrabStart(Transform cameraTransform)
    {
        if (this.IsBroken)
        {
            return;
        }

        this.cameraTransform = cameraTransform;

        if (this.isLocked && this.requiredKey != "" && PlayerKeys.Has(this.requiredKey))
        {
            this.isLocked = false;
            PlayerKeys.ShowMessage(this.requiredKey + "로 문을 열었다");
            if (this.unlockClip != null)
            {
                AudioSource.PlayClipAtPoint(this.unlockClip, this.transform.position);
            }
        }

        if (this.isLocked)
        {
            this.PlaySound(this.lockedSound);
            if (this.lockedMessage != "")
            {
                PlayerKeys.ShowMessage(this.lockedMessage);
            }
            else
            {
                PlayerKeys.ShowMessage(this.requiredKey != "" ? "잠겨 있다. " + this.requiredKey + "가 필요하다" : "잠겨 있다");
            }
            return;
        }

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

        Vector3 forward = this.cameraTransform.forward;
        forward.y = 0f;
        forward = forward.normalized;

        Vector3 right = this.cameraTransform.right;
        right.y = 0f;
        right = right.normalized;

        Vector3 push = forward * mouseDelta.y + right * mouseDelta.x;

        this.rb.AddForce(push * this.pushStrength, ForceMode.Impulse);
    }

    private void Update()
    {
        if (this.settleTimer > 0f)
        {
            this.settleTimer = this.settleTimer - Time.deltaTime;
            if (this.settleTimer <= 0f && this.cameraTransform == null && this.IsBroken == false && this.rb != null)
            {
                this.rb.isKinematic = true;
            }
        }

        if (this.hinge == null)
        {
            return;
        }

        float angle = Mathf.Abs(this.hinge.angle);

        if (this.isClosed && angle > this.closedAngle * 2f)
        {
            this.isClosed = false;
            this.PlaySound(this.openSound);
        }
        else if (this.isClosed == false && angle <= this.closedAngle)
        {
            this.isClosed = true;
            this.PlaySound(this.closeSound);
        }
    }

    public void OnGrabEnd()
    {
        this.cameraTransform = null;
        if (this.IsBroken)
        {
            return;
        }

        if (this.rb != null)
        {
            this.rb.isKinematic = true;
        }
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (this.IsBroken == false || this.fallSound == null)
        {
            return;
        }

        float speed = coll.relativeVelocity.magnitude;
        if (speed < this.fallSoundMinSpeed || Time.time - this.lastFallSoundTime < 0.15f)
        {
            return;
        }
        this.lastFallSoundTime = Time.time;

        this.fallSound.volume = Mathf.Clamp01((speed - this.fallSoundMinSpeed) / (this.fallSoundMaxSpeed - this.fallSoundMinSpeed)) * 0.7f + 0.3f;
        this.fallSound.pitch = Random.Range(0.85f, 1.05f);
        this.fallSound.Play();
    }

    public void PushOpen(Vector3 direction, float impulse)
    {
        if (this.isLocked || this.IsBroken || this.rb == null)
        {
            return;
        }
        this.rb.isKinematic = false;
        this.rb.AddForce(direction.normalized * impulse, ForceMode.Impulse);
        this.settleTimer = 2f;
    }

    public void Knock()
    {
        if (this.knockSound != null)
        {
            this.knockSound.pitch = Random.Range(0.85f, 1.1f);
            this.knockSound.Play();
        }
    }

    public void Break(Vector3 direction, float impulse)
    {
        if (this.IsBroken)
        {
            return;
        }
        this.IsBroken = true;
        this.isLocked = false;

        if (this.hinge != null)
        {
            Destroy(this.hinge);
            this.hinge = null;
        }

        if (this.rb != null)
        {
            this.rb.isKinematic = false;
            this.rb.useGravity = true;
            this.rb.AddForce(direction.normalized * impulse, ForceMode.Impulse);
            this.rb.AddTorque(new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) * impulse * 0.3f, ForceMode.Impulse);
        }

        if (this.breakSound != null)
        {
            if (this.breakSound.gameObject != this.gameObject)
            {
                this.breakSound.transform.parent = null;
                Destroy(this.breakSound.gameObject, 3f);
            }
            this.breakSound.Play();
        }
        if (this.noiseSystem != null)
        {
            this.noiseSystem.Emit(this.transform.position, this.breakNoiseLoudness, "문 부서짐");
        }
    }

    private void PlaySound(AudioSource sound)
    {
        if (sound != null)
        {
            sound.Play();
        }
    }
}
