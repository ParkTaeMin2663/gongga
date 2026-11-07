using UnityEngine;

/// <summary>
/// 마우스로 잡고 끌어올리는 차단기 레버 (암네시아 방식). 2층 분전함에서 쓴다.
/// 잡은 채로 마우스를 위로 밀면 레버가 올라가고, 끝까지 올리면 "켜짐(IsOn)"이 된다.
///
/// [붙이는 곳] 레버의 회전축 역할을 하는 빈 오브젝트 (Scale 1)
///   Breaker (Empty)                ← 분전함 전체
///     ├ Box    (Cube)              분전함 몸통
///     └ Lever  (Empty) ← 이 스크립트. 회전축(경첩) 위치에 놓고, 파란 화살표(+Z)가 플레이어 쪽을 향하게
///         └ Handle (Cube)          손잡이. Box Collider 필요 (Raycast가 이걸 맞힘)
/// [같이 필요한 컴포넌트]
///   - 없음. (선택) Audio Source : 끝까지 올렸을 때 "딸깍" 소리. Play On Awake는 끈다. (강의 03장)
///
/// [동작] 강의 03장 rotation + Quaternion.Euler, 강의 04장 부모 기준 상대 좌표.
///        처음 회전을 기억해 두고, 부모 기준 X축으로 offAngle ~ onAngle 사이에서만 돌린다.
///        서랍(DraggableDrawer)이 localPosition만 바꾸는 것과 같은 방식이라 Joint가 필요 없다.
///
/// [다른 스크립트에서 쓰기] B의 셔터 연출 등은 Start()에서 레버를 찾아 두고 Update()에서 lever.IsOn을 확인하면 된다.
/// </summary>
public class DraggableLever : MonoBehaviour, IInteractable
{
    [Header("각도")]
    [Tooltip("내려가 있을 때(꺼짐) 각도. 처음 놓인 회전 기준으로 X축을 이만큼 돌린다.")]
    public float offAngle = 45f;

    [Tooltip("올라갔을 때(켜짐) 각도. 레버가 반대로 움직이면 offAngle과 onAngle 값을 서로 바꾼다.")]
    public float onAngle = -45f;

    [Tooltip("마우스를 1픽셀 움직일 때 레버가 올라가는 비율. 1 = 끝까지. 너무 느리면 올린다.")]
    public float dragSensitivity = 0.004f;

    [Header("켜짐")]
    [Tooltip("체크하면 한 번 켜진 뒤에는 다시 내릴 수 없다. (차단기 연출은 한 번만 일어나야 하므로 기본 체크)")]
    public bool stayOnOnceSwitched = true;

    [Tooltip("켜질 때 나는 소리 크기 = 괴물이 듣는 거리(m)")]
    public float noiseLoudness = 15f;

    [Header("잠금")]
    [Tooltip("체크하면 잡아도 움직이지 않는다.")]
    public bool isLocked = false;

    /// <summary>레버도 마우스로 밀어야 하므로 잡는 동안 시점을 고정한다.</summary>
    public bool LocksLook { get { return true; } }

    /// <summary>끝까지 올려서 켜졌는지. 셔터·센서등 연출이 이 값을 확인한다.</summary>
    public bool IsOn { get; private set; }

    /// <summary>올라간 정도. 0 = 완전히 내려감(꺼짐), 1 = 완전히 올라감(켜짐).</summary>
    public float Progress { get; private set; }

    // 처음 놓인 회전 (Start에서 저장). 여기에 offAngle ~ onAngle 회전을 더한다.
    private Quaternion startLocalRotation;

    // 끝까지 올렸을 때 소리 (없어도 됨)
    private AudioSource audioSource;

    // 괴물이 듣는 소리를 보낼 곳 (씬의 NoiseSystem 오브젝트)
    private NoiseSystem noiseSystem;

    private void Start()
    {
        this.startLocalRotation = this.transform.localRotation;
        this.audioSource = this.GetComponent<AudioSource>();

        // 강의 04장: Find는 Start()에서 한 번만
        GameObject noiseObject = GameObject.Find("NoiseSystem");
        if (noiseObject != null)
        {
            this.noiseSystem = noiseObject.GetComponent<NoiseSystem>();
        }

        this.Progress = 0f;
        this.IsOn = false;
        this.ApplyRotation();
    }

    public void OnGrabStart(Transform cameraTransform)
    {
        if (this.isLocked)
        {
            Debug.Log(this.name + " : 움직이지 않는다.");
        }
    }

    public void OnGrabUpdate(Vector2 mouseDelta)
    {
        if (this.isLocked)
        {
            return;
        }

        // 한 번 켜지면 고정 (stayOnOnceSwitched)
        if (this.IsOn && this.stayOnOnceSwitched)
        {
            return;
        }

        // 마우스를 위로 밀면(mouseDelta.y > 0) 올라간다.
        float amount = this.Progress + mouseDelta.y * this.dragSensitivity;

        // 0(꺼짐) ~ 1(켜짐) 사이로 제한
        if (amount < 0f)
        {
            amount = 0f;
        }
        if (amount > 1f)
        {
            amount = 1f;
        }
        this.Progress = amount;

        // 끝까지 올렸으면 켜짐
        if (this.Progress >= 1f && this.IsOn == false)
        {
            this.SwitchOn();
        }
        // 다시 내리면 꺼짐 (stayOnOnceSwitched가 꺼져 있을 때만 여기 온다)
        else if (this.Progress < 1f && this.IsOn)
        {
            this.IsOn = false;
            Debug.Log(this.name + " : 차단기 내려감");
        }

        this.ApplyRotation();
    }

    public void OnGrabEnd()
    {
        // 손을 놓으면 그 각도 그대로 멈춰 있다.
    }

    private void SwitchOn()
    {
        this.IsOn = true;
        Debug.Log(this.name + " : 차단기 올라감");

        // 강의 03장 AudioSource.Play()
        if (this.audioSource != null)
        {
            this.audioSource.Play();
        }

        // 괴물을 부르는 큰 소리
        if (this.noiseSystem != null)
        {
            this.noiseSystem.Emit(this.transform.position, this.noiseLoudness, this.name);
        }
    }

    // Progress(0~1)에 맞는 각도로 회전시킨다.
    private void ApplyRotation()
    {
        // 0이면 offAngle, 1이면 onAngle. 그 사이는 비율대로.
        float angle = this.offAngle + (this.onAngle - this.offAngle) * this.Progress;

        // 강의 03장 Quaternion.Euler. 처음 회전에 X축 회전을 더한다(부모 기준).
        this.transform.localRotation = this.startLocalRotation * Quaternion.Euler(angle, 0f, 0f);
    }
}
