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
/// [괴물] MonsterAI가 부른다.
///   - PushOpen : 문을 밀어서 연다 (순찰·확인 중)
///   - Knock    : 쾅! 두드린다 (소리)
///   - Break    : 경첩을 뜯어내고(Destroy, 강의 05장) 문짝을 날려 버린다. 부서진 문은 다시 못 잡는다.
/// </summary>
public class DraggableDoor : MonoBehaviour, IInteractable
{
    [Header("힘")]
    [Tooltip("마우스를 1픽셀 움직일 때 문을 미는 힘. 문이 너무 무거우면 올리고, 너무 휙휙 열리면 내린다.")]
    public float pushStrength = 0.4f;

    [Header("잠금")]
    [Tooltip("체크하면 잠긴 문. 잡아도 열리지 않는다. (나중에 열쇠로 풀 예정)")]
    public bool isLocked = false;

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

    /// <summary>지금 닫혀 있는지 (괴물이 문 앞에서 확인)</summary>
    public bool IsClosed { get { return this.isClosed; } }

    /// <summary>괴물이 부순 문인지. 부서진 문은 경첩이 없고 잡을 수 없다.</summary>
    public bool IsBroken { get; private set; }

    /// <summary>문은 마우스로 밀어야 하므로 잡는 동안 시점을 고정한다.</summary>
    public bool LocksLook { get { return true; } }

    // ----- Start()에서 한 번만 찾아 저장 (강의 04장) -----
    private Rigidbody rb;

    // [강의 외] Hinge Joint의 현재 각도(angle)를 읽어서 열림/닫힘을 판단한다.
    private HingeJoint hinge;

    // 지금 문이 닫힌 상태인지 (열림 → 닫힘이 되는 순간 닫히는 소리)
    private bool isClosed = true;

    // 잡고 있는 동안 플레이어 카메라 (어느 쪽이 "앞"인지 알기 위해)
    private Transform cameraTransform;

    // 괴물이 민 뒤 문이 흔들리다 멈출 때까지 기다렸다가 다시 고정하는 시간
    private float settleTimer;

    // 쿵 소리가 너무 자주 겹치지 않게 (마지막으로 낸 시각)
    private float lastFallSoundTime = -10f;

    private NoiseSystem noiseSystem;

    private void Start()
    {
        this.rb = this.GetComponent<Rigidbody>();
        this.hinge = this.GetComponent<HingeJoint>();

        GameObject noiseObject = GameObject.Find("NoiseSystem");   // 강의 04장: Start에서 한 번만
        if (noiseObject != null)
        {
            this.noiseSystem = noiseObject.GetComponent<NoiseSystem>();
        }

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
        if (this.IsBroken)
        {
            return;   // 부서진 문은 잡지 않는다
        }

        this.cameraTransform = cameraTransform;

        if (this.isLocked)
        {
            Debug.Log(this.name + " : 잠겨 있다.");
            this.PlaySound(this.lockedSound);
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

    private void Update()
    {
        // 괴물이 민 문: 잠시 흔들리게 두었다가 그 각도로 다시 고정 (강의 03장 Is Kinematic)
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

        // 각도가 음수일 수도 있으므로 크기만 본다. (Mathf.Abs = 절댓값)
        float angle = Mathf.Abs(this.hinge.angle);

        // 열림 판정은 closedAngle의 2배로 여유를 둔다. (경계에서 떨리며 소리가 반복되는 것 방지)
        if (this.isClosed && angle > this.closedAngle * 2f)
        {
            // 닫혀 있던 문이 열리기 시작했다
            this.isClosed = false;
            this.PlaySound(this.openSound);
        }
        else if (this.isClosed == false && angle <= this.closedAngle)
        {
            // 열려 있던 문이 닫혔다
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

        // 손을 놓으면 그 각도 그대로 다시 고정한다.
        if (this.rb != null)
        {
            this.rb.isKinematic = true;
        }
    }

    // 강의 03장 OnCollisionEnter: 부서진 문짝이 바닥·벽에 부딪힌 순간 쿵
    private void OnCollisionEnter(Collision coll)
    {
        if (this.IsBroken == false || this.fallSound == null)
        {
            return;
        }

        // 강의 07장 relativeVelocity: 두 물체가 부딪힌 세기. 약하게 스치거나 굴러가는 건 무시
        float speed = coll.relativeVelocity.magnitude;
        if (speed < this.fallSoundMinSpeed || Time.time - this.lastFallSoundTime < 0.15f)
        {
            return;
        }
        this.lastFallSoundTime = Time.time;

        // 세게 부딪힐수록 크게, 음높이는 조금씩 다르게 (강의 03장 Pitch, Random.Range)
        this.fallSound.volume = Mathf.Clamp01((speed - this.fallSoundMinSpeed) / (this.fallSoundMaxSpeed - this.fallSoundMinSpeed)) * 0.7f + 0.3f;
        this.fallSound.pitch = Random.Range(0.85f, 1.05f);
        this.fallSound.Play();
    }

    // ---------------- 괴물이 부르는 함수 ----------------

    /// <summary>괴물이 문을 민다. 잠긴 문이나 부서진 문은 안 움직인다. (열리는 방향 반대에서 밀면 경첩 제한 때문에 안 열림)</summary>
    public void PushOpen(Vector3 direction, float impulse)
    {
        if (this.isLocked || this.IsBroken || this.rb == null)
        {
            return;
        }
        this.rb.isKinematic = false;
        this.rb.AddForce(direction.normalized * impulse, ForceMode.Impulse);   // 강의 03장 AddForce
        this.settleTimer = 2f;
    }

    /// <summary>괴물이 문을 쾅 두드린다. (소리만, 음높이를 조금씩 다르게 — 강의 03장 Pitch, Random.Range)</summary>
    public void Knock()
    {
        if (this.knockSound != null)
        {
            this.knockSound.pitch = Random.Range(0.85f, 1.1f);
            this.knockSound.Play();
        }
    }

    /// <summary>괴물이 문을 부순다: 경첩을 없애고 문짝을 direction 쪽으로 날린다.</summary>
    public void Break(Vector3 direction, float impulse)
    {
        if (this.IsBroken)
        {
            return;
        }
        this.IsBroken = true;
        this.isLocked = false;

        // 강의 05장 Destroy: 경첩(Hinge Joint) 컴포넌트를 없애면 문짝이 자유롭게 떨어져 나간다
        if (this.hinge != null)
        {
            Destroy(this.hinge);
            this.hinge = null;
        }

        if (this.rb != null)
        {
            this.rb.isKinematic = false;
            this.rb.useGravity = true;
            this.rb.AddForce(direction.normalized * impulse, ForceMode.Impulse);                       // 강의 03장 AddForce
            this.rb.AddTorque(new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) * impulse * 0.3f, ForceMode.Impulse);
        }

        // 부서지는 소리는 문짝과 같이 날아가지 않게 그 자리에 떼어 놓고 재생한다
        // (강의 07장 transform.parent = null, 강의 05장 Destroy(물체, 시간)으로 3초 뒤 정리)
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

    // 강의 03장 AudioSource.Play(). 칸이 비어 있으면 아무 소리도 내지 않는다.
    private void PlaySound(AudioSource sound)
    {
        if (sound != null)
        {
            sound.Play();
        }
    }
}
