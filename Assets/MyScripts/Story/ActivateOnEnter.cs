using UnityEngine;

/// <summary>
/// 플레이어가 이 구역에 들어오면 꺼 둔 오브젝트들을 켠다. (예: 3층에 올라오면 괴물 Monster를 켠다 = 3장 시작)
///
/// [붙이는 곳] 빈 오브젝트 + Box Collider(Is Trigger 체크, 강의 05·07장). 예: 3층 계단 위 "Chapter3Start"
/// [플레이어] Player의 Tag가 "Player" (강의 05장 기본 태그)
/// [다시 시작] 체크포인트 activeFromCheckpoint번 이상을 이미 지났으면 씬이 열리자마자 켠다.
///            (죽고 3층 체크포인트에서 다시 시작하면 구역 안에서 시작해서 OnTriggerEnter가 안 불릴 수 있기 때문)
/// </summary>
public class ActivateOnEnter : MonoBehaviour
{
    [Header("켤 오브젝트")]
    [Tooltip("처음엔 꺼 두고(Inspector 맨 위 체크 해제) 플레이어가 들어오면 켤 오브젝트들. 예: Monster")]
    public GameObject[] targets;

    [Header("다시 시작")]
    [Tooltip("이 번호 이상의 체크포인트를 지났으면 처음부터 켠다 (3 = 3층 체크포인트). 0이면 안 씀")]
    public int activeFromCheckpoint = 3;

    [Header("소리 (없어도 됨)")]
    [Tooltip("들어올 때 한 번 나는 소리 (예: 위층에서 쿵)")]
    public AudioClip enterSound;

    private bool isDone;

    private void Start()
    {
        if (this.activeFromCheckpoint > 0 && Checkpoint.savedOrder >= this.activeFromCheckpoint)
        {
            this.Activate();
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
        this.Activate();
    }

    private void Activate()
    {
        this.isDone = true;
        for (int i = 0; i < this.targets.Length; i++)
        {
            if (this.targets[i] != null)
            {
                this.targets[i].SetActive(true);   // 강의 04장 SetActive
            }
        }
    }
}
