using UnityEngine;

// 마우스로 잡고 끌어올리는 차단기 레버 (암네시아 방식). 2층 분전함에서 쓴다.
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

    public bool LocksLook { get { return true; } }

    public bool IsOn { get; private set; }

    public float Progress { get; private set; }

    private Quaternion startLocalRotation;

    private AudioSource audioSource;

    private NoiseSystem noiseSystem;

    private void Start()
    {
        this.startLocalRotation = this.transform.localRotation;
        this.audioSource = this.GetComponent<AudioSource>();

        GameObject noiseObject = GameObject.Find("NoiseSystem");   // 강의 04장: GameObject.Find (Start에서 한 번)
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

        if (this.IsOn && this.stayOnOnceSwitched)
        {
            return;
        }

        float amount = this.Progress + mouseDelta.y * this.dragSensitivity;

        if (amount < 0f)
        {
            amount = 0f;
        }
        if (amount > 1f)
        {
            amount = 1f;
        }
        this.Progress = amount;

        if (this.Progress >= 1f && this.IsOn == false)
        {
            this.SwitchOn();
        }
        else if (this.Progress < 1f && this.IsOn)
        {
            this.IsOn = false;
            Debug.Log(this.name + " : 차단기 내려감");
        }

        this.ApplyRotation();
    }

    public void OnGrabEnd()
    {
    }

    private void SwitchOn()
    {
        this.IsOn = true;
        Debug.Log(this.name + " : 차단기 올라감");

        if (this.audioSource != null)
        {
            this.audioSource.Play();   // 강의 03장: AudioSource.Play
        }

        if (this.noiseSystem != null)
        {
            this.noiseSystem.Emit(this.transform.position, this.noiseLoudness, this.name);
        }
    }

    private void ApplyRotation()
    {
        float angle = this.offAngle + (this.onAngle - this.offAngle) * this.Progress;

        this.transform.localRotation = this.startLocalRotation * Quaternion.Euler(angle, 0f, 0f);   // 강의 03장: Quaternion.Euler
    }
}
