using UnityEngine;

// 플레이어가 이 구역에 들어오면 꺼 둔 오브젝트(예: 3층 괴물)를 켠다.
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
            this.Activate(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (this.isDone || other.CompareTag("Player") == false)
        {
            return;
        }
        if (this.enterSound != null)
        {
            AudioSource.PlayClipAtPoint(this.enterSound, this.transform.position);
        }
        this.Activate(true);
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

            if (useEnterPositions && this.enterPositions != null && i < this.enterPositions.Length && this.enterPositions[i] != null)
            {
                this.targets[i].transform.position = this.enterPositions[i].position;
                this.targets[i].transform.rotation = this.enterPositions[i].rotation;
            }

            this.targets[i].SetActive(true);
        }
    }
}
