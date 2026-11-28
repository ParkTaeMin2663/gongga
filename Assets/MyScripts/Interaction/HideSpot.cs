using UnityEngine;

// 플레이어가 들어가 숨을 수 있는 곳 (장롱). 괴물은 여기 숨은 플레이어를 찾지 못한다.
public class HideSpot : MonoBehaviour
{
    [Header("위치")]
    [Tooltip("숨었을 때 플레이어 발 위치. 자식 빈 오브젝트 HidePoint를 드래그")]
    public Transform hidePoint;

    [Tooltip("나왔을 때 플레이어 발 위치. 자식 빈 오브젝트 ExitPoint를 드래그")]
    public Transform exitPoint;

    [Header("문 (없어도 됨)")]
    [Tooltip("왼쪽 문짝 (기준점이 경첩에 있는 오브젝트)")]
    public Transform doorLeft;

    [Tooltip("오른쪽 문짝")]
    public Transform doorRight;

    [Tooltip("체크하면 게임 시작 때 문을 자동으로 닫는다. (모델이 문 열린 상태로 만들어져 있을 때)")]
    public bool closeDoorsOnStart = true;

    [Tooltip("문이 열리는 각도(도)")]
    public float doorOpenAngle = 100f;

    [Tooltip("숨어 있는 동안 문을 살짝 열어 두는 각도(도). 이 틈으로 밖을 엿본다. 0이면 꽉 닫힘")]
    public float peekAngle = 12f;

    [Tooltip("문이 열리고 닫히는 데 걸리는 시간(초). 문이 없으면 화면이 어두워지고 밝아지는 시간")]
    public float doorTime = 0.4f;

    [Tooltip("플레이어가 들어가고 나오는 데 걸리는 시간(초)")]
    public float moveTime = 0.6f;

    [Header("소리 (없어도 됨)")]
    [Tooltip("문이 열릴 때 소리. 이 오브젝트에 Audio Source를 붙이고 드래그")]
    public AudioSource doorSound;

    [Tooltip("숨고 나올 때 나는 소리 크기 = 괴물이 듣는 거리(m). 아주 가까이 있을 때만 들리게 작게.")]
    public float noiseLoudness = 3f;

    public bool IsOccupied { get; private set; }

    public float FadeAmount { get; private set; }

    private bool UseFade { get { return this.doorLeft == null && this.doorRight == null; } }

    public bool IsBusy { get { return this.step != Step.None; } }

    private enum Step { None, OpenIn, MoveIn, CloseIn, OpenOut, MoveOut, CloseOut }
    private Step step = Step.None;
    private float stepTime;

    private PlayerHide player;
    private Vector3 moveFrom;
    private float yawFrom;

    private Vector3 leftClosedPos, rightClosedPos;
    private Quaternion leftClosedRot, rightClosedRot;
    private float leftSign, rightSign;
    private Vector3 leftHinge, rightHinge;

    private NoiseSystem noiseSystem;

    private void Start()
    {
        if (this.hidePoint == null || this.exitPoint == null)
        {
            Debug.LogError(this.name + " : Hide Point / Exit Point가 비어 있습니다.");
        }

        if (this.doorLeft != null)
        {
            this.PrepareDoor(this.doorLeft, out this.leftHinge, out this.leftClosedPos, out this.leftClosedRot, out this.leftSign);
        }
        if (this.doorRight != null)
        {
            this.PrepareDoor(this.doorRight, out this.rightHinge, out this.rightClosedPos, out this.rightClosedRot, out this.rightSign);
        }

        GameObject noiseObject = GameObject.Find("NoiseSystem");   // 강의 04장: GameObject.Find (Start에서 한 번)
        if (noiseObject != null)
        {
            this.noiseSystem = noiseObject.GetComponent<NoiseSystem>();
        }
    }

    public void BeginEnter(PlayerHide who)
    {
        this.player = who;
        this.ChangeStep(Step.OpenIn);
        this.MakeSound("장롱 문 열림");
    }

    public void BeginExit()
    {
        this.IsOccupied = false;
        this.ChangeStep(Step.OpenOut);
        this.MakeSound("장롱 문 열림");
    }

    private void Update()
    {
        if (this.step == Step.None)
        {
            return;
        }

        this.stepTime = this.stepTime + Time.deltaTime;   // 강의 03장: Time.deltaTime

        float doorRate = this.doorTime > 0f ? Mathf.Clamp01(this.stepTime / this.doorTime) : 1f;
        float moveRate = this.moveTime > 0f ? Mathf.Clamp01(this.stepTime / this.moveTime) : 1f;

        switch (this.step)
        {
            case Step.OpenIn:
                if (this.UseFade)
                {
                    this.FadeAmount = doorRate;
                    if (doorRate >= 1f)
                    {
                        this.Teleport(this.hidePoint, Step.CloseIn);
                        this.SetInside();
                    }
                }
                else
                {
                    this.SetDoors(doorRate);
                    if (doorRate >= 1f) this.StartMove(Step.MoveIn);
                }
                break;

            case Step.MoveIn:
                this.MovePlayer(this.hidePoint, moveRate);
                if (moveRate >= 1f)
                {
                    this.ChangeStep(Step.CloseIn);
                    this.SetInside();
                }
                break;

            case Step.CloseIn:
                this.SetDoors(Mathf.Lerp(1f, this.PeekRate, doorRate));
                if (this.UseFade) this.FadeAmount = 1f - doorRate;
                if (doorRate >= 1f)
                {
                    this.ChangeStep(Step.None);
                    this.player.OnHideFinished();
                }
                break;

            case Step.OpenOut:
                if (this.UseFade)
                {
                    this.FadeAmount = doorRate;
                    if (doorRate >= 1f) this.Teleport(this.exitPoint, Step.CloseOut);
                }
                else
                {
                    this.SetDoors(Mathf.Lerp(this.PeekRate, 1f, doorRate));
                    if (doorRate >= 1f) this.StartMove(Step.MoveOut);
                }
                break;

            case Step.MoveOut:
                this.MovePlayer(this.exitPoint, moveRate);
                if (moveRate >= 1f) this.ChangeStep(Step.CloseOut);
                break;

            case Step.CloseOut:
                this.SetDoors(1f - doorRate);
                if (this.UseFade) this.FadeAmount = 1f - doorRate;
                if (doorRate >= 1f)
                {
                    this.ChangeStep(Step.None);
                    this.player.OnExitFinished();
                    this.player = null;
                }
                break;
        }
    }

    private void ChangeStep(Step next)
    {
        this.step = next;
        this.stepTime = 0f;
    }

    private void StartMove(Step next)
    {
        this.moveFrom = this.player.transform.position;
        this.yawFrom = this.player.CurrentYaw;
        this.ChangeStep(next);
    }

    private void SetInside()
    {
        this.IsOccupied = true;
        this.player.OnInside();
    }

    private void Teleport(Transform target, Step next)
    {
        this.StartMove(next);
        this.MovePlayer(target, 1f);
    }

    private void MovePlayer(Transform target, float rate)
    {
        this.player.transform.position = Vector3.Lerp(this.moveFrom, target.position, rate);
        float yaw = Mathf.LerpAngle(this.yawFrom, target.eulerAngles.y, rate);
        this.player.SetYaw(yaw);
    }

    private float PeekRate { get { return this.doorOpenAngle != 0f ? this.peekAngle / this.doorOpenAngle : 0f; } }

    private void SetDoors(float rate)
    {
        float angle = this.doorOpenAngle * rate;
        if (this.doorLeft != null)
        {
            this.TurnDoor(this.doorLeft, this.leftHinge, this.leftClosedPos, this.leftClosedRot, this.leftSign * angle);
        }
        if (this.doorRight != null)
        {
            this.TurnDoor(this.doorRight, this.rightHinge, this.rightClosedPos, this.rightClosedRot, this.rightSign * angle);
        }
    }

    private void TurnDoor(Transform door, Vector3 hinge, Vector3 closedPos, Quaternion closedRot, float angle)
    {
        door.position = closedPos;
        door.rotation = closedRot;
        door.RotateAround(hinge, this.transform.up, angle);   // 강의 07장: transform.RotateAround
    }

    private void PrepareDoor(Transform door, out Vector3 hinge, out Vector3 closedPos, out Quaternion closedRot, out float sign)
    {
        Renderer doorRenderer = door.GetComponentInChildren<Renderer>();

        Vector3 min = new Vector3(999f, 999f, 999f);
        Vector3 max = new Vector3(-999f, -999f, -999f);
        if (doorRenderer != null)
        {
            Bounds b = doorRenderer.bounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = new Vector3(i % 2 == 0 ? b.min.x : b.max.x,
                                             (i / 2) % 2 == 0 ? b.min.y : b.max.y,
                                             i / 4 == 0 ? b.min.z : b.max.z);
                Vector3 local = this.transform.InverseTransformPoint(corner);
                min = Vector3.Min(min, local);
                max = Vector3.Max(max, local);
            }
        }
        else
        {
            min = max = this.transform.InverseTransformPoint(door.position);
        }
        float hingeX = Mathf.Abs(min.x) > Mathf.Abs(max.x) ? min.x : max.x;
        hinge = this.transform.TransformPoint(new Vector3(hingeX, (min.y + max.y) / 2f, min.z + 0.015f));

        Vector3 center = doorRenderer != null ? doorRenderer.bounds.center : door.position;
        Vector3 along = Vector3.ProjectOnPlane(center - hinge, this.transform.up).normalized;

        if (this.closeDoorsOnStart)
        {
            Vector3 target = hingeX < 0f ? this.transform.right : -this.transform.right;
            // [강의 외] Vector3.SignedAngle: 장롱 문이 닫힌 정도(각도)
            float closeAngle = Vector3.SignedAngle(along, target, this.transform.up);
            door.RotateAround(hinge, this.transform.up, closeAngle);
            along = target;
        }

        closedPos = door.position;
        closedRot = door.rotation;

        sign = Vector3.Dot(Vector3.Cross(this.transform.up, along), this.transform.forward) >= 0f ? 1f : -1f;
    }

    private void MakeSound(string sourceName)
    {
        if (this.doorSound != null)
        {
            this.doorSound.Play();   // 강의 03장: AudioSource.Play
        }

        if (this.noiseSystem != null)
        {
            this.noiseSystem.Emit(this.transform.position, this.noiseLoudness, sourceName);
        }
    }
}
