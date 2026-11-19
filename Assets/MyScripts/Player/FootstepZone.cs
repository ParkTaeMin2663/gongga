using UnityEngine;

/// <summary>
/// 바닥 종류에 따라 발소리를 바꾸는 구역. (예: 단지 바깥 = 눈 밟는 소리)
///   - 플레이어가 이 구역(Is Trigger)에 들어오면 Player의 Audio Source 소리(clip)를 zoneClip으로 바꾼다
///   - 구역을 나가면 원래 소리(콘크리트)로 되돌린다
///   - 걸음마다 소리를 내는 건 PlayerMovement가 그대로 한다 (이 스크립트는 소리 파일만 바꿈)
///
/// [붙이는 곳] 빈 오브젝트 "Zone_Snow" + Box Collider(Is Trigger 체크, 강의 05·07장) — 단지 바깥 전체를 덮게
/// [같이 쓰는 것] Player의 Audio Source (PlayerMovement의 발소리)
/// </summary>
public class FootstepZone : MonoBehaviour
{
    [Header("이 구역의 발소리")]
    [Tooltip("구역 안에서 쓸 발소리 (예: Assets/Audio/Footsteps/footstep_snow_000)")]
    public AudioClip zoneClip;

    private AudioSource playerAudio;   // Player의 발소리 Audio Source
    private AudioClip originalClip;    // 원래 발소리 (구역을 나가면 되돌림)

    // 강의 04장: Find는 Start()에서 한 번만
    private void Start()
    {
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            this.playerAudio = playerObject.GetComponent<AudioSource>();
        }
        if (this.playerAudio != null)
        {
            this.originalClip = this.playerAudio.clip;
        }
    }

    // 강의 05·07장 Is Trigger: 구역에 들어왔다 → 발소리를 바꾼다
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && this.playerAudio != null && this.zoneClip != null)
        {
            this.playerAudio.clip = this.zoneClip;
        }
    }

    // 구역을 나갔다 → 원래 발소리로
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && this.playerAudio != null)
        {
            this.playerAudio.clip = this.originalClip;
        }
    }
}
