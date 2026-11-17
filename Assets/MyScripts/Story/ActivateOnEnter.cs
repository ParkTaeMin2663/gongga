using UnityEngine;

/// <summary>
/// 플레이어가 이 구역에 들어오면 꺼 둔 오브젝트들을 켠다. (예: 3층 301호에 들어오면 괴물 Monster를 켠다 = 3장 시작)
///
/// [붙이는 곳] 빈 오브젝트 + Box Collider(Is Trigger 체크, 강의 05·07장). 예: 3층 301호 안 "Chapter3Start"
/// [플레이어] Player의 Tag가 "Player" (강의 05장 기본 태그)
/// [다시 시작] 체크포인트 activeFromCheckpoint번 이상을 이미 지났으면 씬이 열리자마자 켠다.
///            (죽고 3층 체크포인트에서 다시 시작하면 구역 안에서 시작해서 OnTriggerEnter가 안 불릴 수 있기 때문)
/// [나타날 위치] (11/17 A 추가) enterPositions를 채우면, 구역에 들어와서 켤 때만 그 위치·방향으로 옮긴 뒤 켠다.
///            예: 1층으로 내려갔던 괴물이 2층 계단참에서 나타나 3층으로 올라온다.
///            체크포인트에서 다시 시작할 때는 옮기지 않는다 → 씬에 놓인 원래 자리(304호)에서 시작.
/// </summary>
public class ActivateOnEnter : MonoBehaviour
{
    [Header("켤 오브젝트")]
    [Tooltip("처음엔 꺼 두고(Inspector 맨 위 체크 해제) 플레이어가 들어오면 켤 오브젝트들. 예: Monster")]
    public GameObject[] targets;

    [Tooltip("(없어도 됨) 들어와서 켤 때 targets를 옮길 위치 (빈 오브젝트). targets와 같은 순서. 비워 둔 칸은 안 옮긴다")]
    public Transform[] enterPositions;

    [Header("다시 시작")]
    [Tooltip("이 번호 이상의 체크포인트를 지났으면 처음부터 켠다 (3 = 3층 체크포인트). 0이면 안 씀")]
    public int activeFromCheckpoint = 3;

    [Header("소리 (없어도 됨)")]
    [Tooltip("들어올 때 한 번 나는 소리 (예: 아래층에서 쿵)")]
    public AudioClip enterSound;

    private bool isDone;

    private void Start()
    {
        if (this.activeFromCheckpoint > 0 && Checkpoint.savedOrder >= this.activeFromCheckpoint)
        {
            this.Activate(false);   // 다시 시작: 원래 자리에서
        }
    }

    // 강의 05·07장 OnTriggerEnter
    private void OnTriggerEnter(Collider other)
    {
        if (this.isDone || other.CompareTag("Player") == false)
        {
            return;
        }
        if (this.enterSound != null)
        {
            AudioSource.PlayClipAtPoint(this.enterSound, this.transform.position);   // 강의 07장
        }
        this.Activate(true);   // 처음 들어옴: 나타날 위치로 옮겨서
    }

    private void Activate(bool useEnterPositions)
    {
        this.isDone = true;
        for (int i = 0; i < this.targets.Length; i++)
        {
            if (this.targets[i] == null)
            {
                continue;
            }

            // 꺼져 있는 동안 옮겨 두면 켜질 때 그 자리에서 시작한다 (NavMeshAgent도 켜지면서 그 자리 NavMesh에 붙음)
            if (useEnterPositions && this.enterPositions != null && i < this.enterPositions.Length && this.enterPositions[i] != null)
            {
                this.targets[i].transform.position = this.enterPositions[i].position;   // 강의 03장
                this.targets[i].transform.rotation = this.enterPositions[i].rotation;
            }

            this.targets[i].SetActive(true);   // 강의 04장 SetActive
        }
    }
}
