using UnityEngine;

/// <summary>
/// 지하의 동생이 플레이어를 쳐다본다.
///   - 플레이어가 지하(높이 basementY 아래)에 내려와 lookDistance 안에 들어오면
///     동생의 몸이 플레이어 쪽으로 천천히 돌아간다 (좌우만 돈다 — 숙여지지 않게, 강의 07장 LookAt에서 y를 고정한 것과 같은 생각)
///   - 플레이어가 움직이면 계속 따라 돈다 → "계속 보고 있다"는 느낌
///   - 연출이 끝나 동생이 꺼지면(SetActive false) Update도 멈춘다
///   - (11/26 수정) 동생 모델이 부모(Brother)의 원점에서 1.4m쯤 떨어져 있어서, 부모를 그냥 돌리면 모델이 원을 그리며 자리를 옮겼다.
///     → 모델 몸의 가운데(Renderer 범위의 중심)를 축으로 강의 07장 transform.RotateAround로 돌려서 제자리에서 몸만 돈다
///
/// [붙이는 곳] SampleScene의 "Brother" (동생 모델의 부모 빈 오브젝트, (-15, -2.8, 6.9))
/// [같이 쓰는 것] 씬의 "Player" (Start에서 한 번 Find, 강의 04장)
/// </summary>
public class BrotherLook : MonoBehaviour
{
    [Header("언제 쳐다볼지")]
    [Tooltip("플레이어 발 높이가 이 값보다 낮으면 지하에 있는 것으로 본다 (지하 바닥 y -2.8, 1층 바닥 y 0)")]
    public float basementY = -1.5f;
    [Tooltip("이 거리(m) 안에 들어오면 쳐다본다. 지하 전체에서 보게 하려면 40 정도")]
    public float lookDistance = 40f;

    [Header("도는 속도")]
    [Tooltip("1초에 몇 도씩 도는지. 작을수록 천천히, 섬뜩하게 돈다")]
    public float turnSpeed = 45f;
    [Tooltip("모델의 앞이 Z축(파란 화살표)과 다르면 여기에 더해 맞춘다 (예: 90, -90, 180)")]
    public float yawOffset = 0f;

    // 강의 04장: Find는 Start()에서 한 번만
    private Transform player;
    private Vector3 pivot;      // 몸이 도는 축 (모델 몸의 가운데, 제자리)

    private void Start()
    {
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            this.player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("[BrotherLook] 씬에서 'Player'를 찾지 못했습니다.");
        }

        // 도는 축 = 자식 모델이 실제로 보이는 범위의 가운데 (없으면 이 오브젝트 위치)
        this.pivot = this.transform.position;
        Renderer body = this.GetComponentInChildren<Renderer>();
        if (body != null)
        {
            this.pivot = body.bounds.center;
        }
    }

    private void Update()
    {
        if (this.player == null)
        {
            return;
        }

        // 지하에 있고 가까울 때만
        if (this.player.position.y > this.basementY)
        {
            return;
        }
        Vector3 dir = this.player.position - this.pivot;
        dir.y = 0f;                                   // 좌우로만 돈다 (위아래로 숙이지 않게)
        if (dir.magnitude > this.lookDistance || dir.magnitude < 0.01f)
        {
            return;
        }

        // 플레이어 쪽 좌우 각도 (BrotherEncounter의 고개 돌리기와 같은 방법: Mathf.Atan2)
        float targetYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + this.yawOffset;
        float currentYaw = this.transform.eulerAngles.y;

        // 강의 03장 Time.deltaTime: 이번 프레임에 돌 각도 (1초에 turnSpeed도까지)
        float newYaw = Mathf.MoveTowardsAngle(currentYaw, targetYaw, this.turnSpeed * Time.deltaTime);
        float step = Mathf.DeltaAngle(currentYaw, newYaw);

        // 강의 07장 RotateAround(중심점, 축, 각도): 몸 가운데를 축으로 돌려서 자리는 그대로
        this.transform.RotateAround(this.pivot, Vector3.up, step);
    }
}
