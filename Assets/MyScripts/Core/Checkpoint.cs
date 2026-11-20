using UnityEngine;

/// <summary>
/// 보이지 않는 체크포인트. 플레이어가 이 구역을 지나가면 "여기서 다시 시작"할 위치를 기억한다.
/// 잡혀서 씬을 다시 불러오면 PlayerDeath가 이 위치로 플레이어를 옮긴다.
///
/// [붙이는 곳] 빈 오브젝트 (Checkpoint 프리팹). 다시 시작할 자리에 놓고,
///             파란 화살표(+Z)를 다시 시작했을 때 바라볼 방향으로 돌린다.
/// [같이 필요한 컴포넌트]
///   - Box Collider : Is Trigger 체크 (강의 05·07장). 플레이어가 지나갈 만큼 크게
/// [플레이어] Player 오브젝트의 Tag가 "Player"여야 한다 (강의 05장 기본 태그)
///
/// [저장] 강의 07장 public static: 씬을 다시 불러와도 값이 남는다.
///        order가 더 큰 체크포인트만 저장한다 → 뒤로 돌아가도 앞 체크포인트로 덮어쓰지 않는다.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Header("순서")]
    [Tooltip("체크포인트 번호. 게임 진행 순서대로 1, 2, 3, 4 … (뒤로 갈수록 크게)")]
    public int order = 1;

    [Tooltip("화면에 잠깐 \"저장됨\"을 띄울지")]
    public bool showSavedMessage = true;

    // ----- 게임 전체가 공유하는 값 (강의 07장 public static) -----
    /// <summary>지금까지 지나간 가장 큰 체크포인트 번호. 0 = 아직 없음</summary>
    public static int savedOrder = 0;
    /// <summary>다시 시작할 위치와 바라볼 방향</summary>
    public static Vector3 savedPosition;
    public static float savedYaw;
    /// <summary>true면 다음에 씬이 열릴 때 savedPosition에서 시작한다 (죽었을 때 PlayerDeath가 켠다)</summary>
    public static bool isRespawning = false;
    /// <summary>마지막으로 저장한 시각 ("저장됨" 표시용)</summary>
    public static float lastSaveTime = -100f;
    /// <summary>체크포인트를 지날 때의 손전등 상태 (죽고 다시 시작할 때 되돌린다)</summary>
    public static bool savedHasFlashlight = false;
    public static float savedBattery = 100f;
    public static bool lastSaveShowsMessage = true;

    private void Start()
    {
        // 강의 05·07장: Trigger는 Collider에 체크한다
        Collider area = this.GetComponent<Collider>();
        if (area == null || area.isTrigger == false)
        {
            Debug.LogWarning(this.name + " : Box Collider의 Is Trigger를 체크해 주세요.");
        }
    }

    // 강의 05·07장 OnTriggerEnter: 무언가 이 구역에 들어온 순간
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어만 (강의 07장 CompareTag)
        if (other.CompareTag("Player") == false)
        {
            return;
        }

        // 이미 더 뒤의 체크포인트를 지났으면 무시
        if (this.order <= savedOrder)
        {
            return;
        }

        savedOrder = this.order;
        savedPosition = this.transform.position;
        savedYaw = this.transform.eulerAngles.y;
        lastSaveTime = Time.time;

        // 손전등을 가졌는지, 배터리가 얼마였는지도 같이 기억한다
        PlayerFlashlight flashlight = other.GetComponent<PlayerFlashlight>();
        if (flashlight != null)
        {
            savedHasFlashlight = flashlight.hasFlashlight;
            savedBattery = flashlight.battery;
        }
        PlayerKeys.SaveForCheckpoint();   // (11/20) 가진 열쇠도 기억
        lastSaveShowsMessage = this.showSavedMessage;
        Debug.Log("[체크포인트] " + this.order + "번 저장: " + savedPosition);
    }
}
