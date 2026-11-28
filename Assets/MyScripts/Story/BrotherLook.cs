using UnityEngine;

// 지하의 동생이 플레이어를 쳐다본다.
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

    private Transform player;
    private Vector3 pivot;

    private void Start()
    {
        GameObject playerObject = GameObject.Find("Player");   // 강의 04장: GameObject.Find (Start에서 한 번)
        if (playerObject != null)
        {
            this.player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("[BrotherLook] 씬에서 'Player'를 찾지 못했습니다.");
        }

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

        if (this.player.position.y > this.basementY)
        {
            return;
        }
        Vector3 dir = this.player.position - this.pivot;
        dir.y = 0f;
        if (dir.magnitude > this.lookDistance || dir.magnitude < 0.01f)
        {
            return;
        }

        float targetYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + this.yawOffset;
        float currentYaw = this.transform.eulerAngles.y;

        float newYaw = Mathf.MoveTowardsAngle(currentYaw, targetYaw, this.turnSpeed * Time.deltaTime);   // 강의 03장: Time.deltaTime
        float step = Mathf.DeltaAngle(currentYaw, newYaw);

        this.transform.RotateAround(this.pivot, Vector3.up, step);   // 강의 07장: transform.RotateAround
    }
}
