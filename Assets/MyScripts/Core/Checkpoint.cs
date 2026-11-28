using UnityEngine;

// 보이지 않는 체크포인트. 플레이어가 이 구역을 지나가면 "여기서 다시 시작"할 위치를 기억한다.
public class Checkpoint : MonoBehaviour
{
    [Header("순서")]
    [Tooltip("체크포인트 번호. 게임 진행 순서대로 1, 2, 3, 4 … (뒤로 갈수록 크게)")]
    public int order = 1;

    [Tooltip("화면에 잠깐 \"저장됨\"을 띄울지")]
    public bool showSavedMessage = true;

    public static int savedOrder = 0;   // 강의 07장: public static 변수
    public static Vector3 savedPosition;
    public static float savedYaw;
    public static bool isRespawning = false;
    public static float lastSaveTime = -100f;
    public static bool savedHasFlashlight = false;
    public static float savedBattery = 100f;
    public static bool lastSaveShowsMessage = true;

    private void Start()
    {
        Collider area = this.GetComponent<Collider>();
        if (area == null || area.isTrigger == false)
        {
            Debug.LogWarning(this.name + " : Box Collider의 Is Trigger를 체크해 주세요.");
        }
    }

    private void OnTriggerEnter(Collider other)   // 강의 05장: Is Trigger + OnTriggerEnter
    {
        if (other.CompareTag("Player") == false)   // 강의 07장: CompareTag
        {
            return;
        }

        if (this.order <= savedOrder)
        {
            return;
        }

        savedOrder = this.order;
        savedPosition = this.transform.position;
        savedYaw = this.transform.eulerAngles.y;
        lastSaveTime = Time.time;

        PlayerFlashlight flashlight = other.GetComponent<PlayerFlashlight>();
        if (flashlight != null)
        {
            savedHasFlashlight = flashlight.hasFlashlight;
            savedBattery = flashlight.battery;
        }
        PlayerKeys.SaveForCheckpoint();
        lastSaveShowsMessage = this.showSavedMessage;
        Debug.Log("[체크포인트] " + this.order + "번 저장: " + savedPosition);
    }
}
