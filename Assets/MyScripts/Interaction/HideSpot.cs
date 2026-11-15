using UnityEngine;

/// <summary>
/// 플레이어가 들어가 숨을 수 있는 곳 (장롱). 괴물은 여기 숨은 플레이어를 찾지 못한다.
///
/// [붙이는 곳] 장롱 전체를 묶는 빈 오브젝트 (Scale 1, 파란 화살표 +Z = 장롱 문 쪽)
///   Wardrobe (Empty) ← 이 스크립트
///     ├ (장롱 몸통 모델 또는 Cube들)
///     │   문짝은 따로 떨어진 오브젝트여야 한다. 경첩 위치는 문짝 모양(Renderer 범위)에서 자동으로 찾는다.
///     │   (장롱 바깥쪽 모서리 = 경첩)
///     ├ HidePoint (Empty)   숨었을 때 플레이어 발 위치. 파란 화살표가 문 쪽을 보게
///     └ ExitPoint (Empty)   나왔을 때 플레이어 발 위치 (장롱 앞 바닥). 파란 화살표가 장롱 반대쪽을 보게
///
/// [동작] 문(doorLeft/doorRight)이 있으면:
///          숨기: 문 열림 → 플레이어가 HidePoint로 걸어 들어감(몸을 돌림) → 문 닫힘 → 숨은 상태
///          나오기: 문 열림 → ExitPoint로 걸어 나옴 → 문 닫힘
///        문이 없으면(통짜 모델): 문 소리 + 화면이 어두워짐 → 그 사이 안/밖으로 옮김 → 화면 밝아짐
///        강의 03장: Time.deltaTime으로 조금씩 움직이기, rotation + Quaternion.Euler
///        발이 장롱 안(HidePoint)에 들어간 순간부터 IsOccupied(숨은 상태)가 된다.
///        문이 열리고 걸어 들어가는 도중(약 1초)에는 아직 잡힐 수 있다.
/// </summary>
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

    /// <summary>플레이어가 장롱 안에 들어가 숨은 상태인지 (문이 닫히기 시작할 때부터). 괴물이 이 값을 본다.</summary>
    public bool IsOccupied { get; private set; }

    /// <summary>화면을 얼마나 어둡게 덮을지 (0 = 안 덮음, 1 = 완전히 검정). 문 없는 장롱에서 PlayerHide가 화면에 그린다.</summary>
    public float FadeAmount { get; private set; }

    // 문이 하나도 연결되어 있지 않으면 화면 어두워짐으로 대신한다
    private bool UseFade { get { return this.doorLeft == null && this.doorRight == null; } }

    /// <summary>들어가거나 나오는 동작 중인지. 이 동안에는 E를 눌러도 무시한다.</summary>
    public bool IsBusy { get { return this.step != Step.None; } }

    // 지금 하고 있는 동작 단계
    private enum Step { None, OpenIn, MoveIn, CloseIn, OpenOut, MoveOut, CloseOut }
    private Step step = Step.None;
    private float stepTime;          // 지금 단계가 시작된 뒤 지난 시간

    private PlayerHide player;       // 들어가고 있는/숨어 있는 플레이어
    private Vector3 moveFrom;        // 걸어가기 시작한 위치
    private float yawFrom;           // 걸어가기 시작할 때 바라보던 방향

    // 닫혀 있을 때 문 위치·회전 (Start에서 저장)과, 바깥쪽으로 열리는 회전 방향(+1 또는 -1)
    private Vector3 leftClosedPos, rightClosedPos;
    private Quaternion leftClosedRot, rightClosedRot;
    private float leftSign, rightSign;
    private Vector3 leftHinge, rightHinge;   // 경첩 위치 (월드 좌표)

    private NoiseSystem noiseSystem;

    private void Start()
    {
        if (this.hidePoint == null || this.exitPoint == null)
        {
            Debug.LogError(this.name + " : Hide Point / Exit Point가 비어 있습니다.");
        }

        // 문 닫힌 상태 기억 (모델이 문 열린 채로 만들어졌으면 먼저 닫는다)
        if (this.doorLeft != null)
        {
            this.PrepareDoor(this.doorLeft, out this.leftHinge, out this.leftClosedPos, out this.leftClosedRot, out this.leftSign);
        }
        if (this.doorRight != null)
        {
            this.PrepareDoor(this.doorRight, out this.rightHinge, out this.rightClosedPos, out this.rightClosedRot, out this.rightSign);
        }

        // 강의 04장: Find는 Start()에서 한 번만
        GameObject noiseObject = GameObject.Find("NoiseSystem");
        if (noiseObject != null)
        {
            this.noiseSystem = noiseObject.GetComponent<NoiseSystem>();
        }
    }

    /// <summary>PlayerHide가 E를 눌렀을 때 부른다. 문을 열고 들어가기 시작한다.</summary>
    public void BeginEnter(PlayerHide who)
    {
        this.player = who;
        this.ChangeStep(Step.OpenIn);
        this.MakeSound("장롱 문 열림");
    }

    /// <summary>숨어 있을 때 E를 누르면 부른다. 문을 열고 나오기 시작한다.</summary>
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

        this.stepTime = this.stepTime + Time.deltaTime;

        // 문: 0초 → doorTime초 동안 0 → 1로 (열기) 또는 1 → 0으로 (닫기)
        float doorRate = this.doorTime > 0f ? Mathf.Clamp01(this.stepTime / this.doorTime) : 1f;
        // 걷기: 0초 → moveTime초 동안 0 → 1로
        float moveRate = this.moveTime > 0f ? Mathf.Clamp01(this.stepTime / this.moveTime) : 1f;

        switch (this.step)
        {
            case Step.OpenIn:
                if (this.UseFade)
                {
                    this.FadeAmount = doorRate;
                    if (doorRate >= 1f)
                    {
                        this.Teleport(this.hidePoint, Step.CloseIn);   // 화면이 까만 동안 안으로
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
                    this.SetInside();   // 발이 장롱 안에 들어갔다 → 이때부터 숨은 것으로 친다
                }
                break;

            case Step.CloseIn:
                // 다 닫지 않고 peekAngle만큼 틈을 남긴다 / 문이 없으면 화면이 밝아진다
                this.SetDoors(Mathf.Lerp(1f, this.PeekRate, doorRate));
                if (this.UseFade) this.FadeAmount = 1f - doorRate;
                if (doorRate >= 1f)
                {
                    // 문이 다 닫혔다 → 이제 숨은 상태
                    this.ChangeStep(Step.None);
                    this.player.OnHideFinished();
                }
                break;

            case Step.OpenOut:
                if (this.UseFade)
                {
                    this.FadeAmount = doorRate;
                    if (doorRate >= 1f) this.Teleport(this.exitPoint, Step.CloseOut);   // 화면이 까만 동안 밖으로
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

    // 걷기 시작: 지금 위치·방향을 기억해 둔다
    private void StartMove(Step next)
    {
        this.moveFrom = this.player.transform.position;
        this.yawFrom = this.player.CurrentYaw;
        this.ChangeStep(next);
    }

    // 장롱 안에 들어갔다: 문이 닫히기 전이라도 괴물은 더 이상 플레이어를 느끼지 못한다
    private void SetInside()
    {
        this.IsOccupied = true;
        this.player.OnInside();
    }

    // 한 번에 target으로 옮기고 다음 단계로 (화면이 까말 때 쓴다)
    private void Teleport(Transform target, Step next)
    {
        this.StartMove(next);
        this.MovePlayer(target, 1f);
    }

    // rate(0~1)만큼 target 쪽으로 옮기고, 몸도 target의 방향으로 돌린다.
    private void MovePlayer(Transform target, float rate)
    {
        this.player.transform.position = Vector3.Lerp(this.moveFrom, target.position, rate);
        float yaw = Mathf.LerpAngle(this.yawFrom, target.eulerAngles.y, rate);
        this.player.SetYaw(yaw);
    }

    // 틈 각도를 0~1 비율로
    private float PeekRate { get { return this.doorOpenAngle != 0f ? this.peekAngle / this.doorOpenAngle : 0f; } }

    // rate 0 = 닫힘, 1 = 활짝 열림.
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

    // 닫힌 상태로 되돌린 뒤, 경첩을 축으로 angle만큼 돌린다.
    private void TurnDoor(Transform door, Vector3 hinge, Vector3 closedPos, Quaternion closedRot, float angle)
    {
        door.position = closedPos;
        door.rotation = closedRot;
        // 강의 07장 RotateAround: 강의 03장 Rotate와 같지만, 자기 중심이 아니라 지정한 점(경첩)을 축으로 돈다.
        door.RotateAround(hinge, this.transform.up, angle);
    }

    // 문을 처음 준비한다: 경첩 위치를 찾고, (필요하면) 닫고, 닫힌 위치·회전과 바깥쪽 방향을 기억한다.
    private void PrepareDoor(Transform door, out Vector3 hinge, out Vector3 closedPos, out Quaternion closedRot, out float sign)
    {
        Renderer doorRenderer = door.GetComponentInChildren<Renderer>();

        // 1) 문짝 범위를 장롱 기준 좌표로 바꿔서 경첩 위치를 찾는다.
        //    경첩 = 장롱 가운데에서 먼 쪽 모서리(X), 문짝 뒷면 조금 앞(Z)
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

        // 2) 문짝이 경첩에서 뻗어 나간 방향 (바닥과 평행하게)
        Vector3 center = doorRenderer != null ? doorRenderer.bounds.center : door.position;
        Vector3 along = Vector3.ProjectOnPlane(center - hinge, this.transform.up).normalized;

        // 3) 열려 있으면 닫기: 닫힌 문은 장롱 앞면과 나란하고 가운데를 향한다
        if (this.closeDoorsOnStart)
        {
            Vector3 target = hingeX < 0f ? this.transform.right : -this.transform.right;
            // [강의 외] SignedAngle: 두 방향 사이의 각도(부호 포함)
            float closeAngle = Vector3.SignedAngle(along, target, this.transform.up);
            door.RotateAround(hinge, this.transform.up, closeAngle);
            along = target;
        }

        closedPos = door.position;
        closedRot = door.rotation;

        // 4) 바깥쪽(장롱 앞 +Z)으로 열리는 회전 방향 (+각도로 돌릴 때 문 끝이 움직이는 방향 = up × along)
        sign = Vector3.Dot(Vector3.Cross(this.transform.up, along), this.transform.forward) >= 0f ? 1f : -1f;
    }

    private void MakeSound(string sourceName)
    {
        // 강의 03장 AudioSource.Play()
        if (this.doorSound != null)
        {
            this.doorSound.Play();
        }

        if (this.noiseSystem != null)
        {
            this.noiseSystem.Emit(this.transform.position, this.noiseLoudness, sourceName);
        }
    }
}
