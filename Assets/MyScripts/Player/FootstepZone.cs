using UnityEngine;

// 바닥 종류에 따라 발소리를 바꾸는 구역. (예: 단지 바깥 = 눈 밟는 소리)
public class FootstepZone : MonoBehaviour
{
    [Header("이 구역의 발소리")]
    [Tooltip("구역 안에서 쓸 발소리 (예: Assets/Audio/Footsteps/footstep_snow_000)")]
    public AudioClip zoneClip;

    private AudioSource playerAudio;
    private AudioClip originalClip;

    private void Start()
    {
        GameObject playerObject = GameObject.Find("Player");   // 강의 04장: GameObject.Find (Start에서 한 번)
        if (playerObject != null)
        {
            this.playerAudio = playerObject.GetComponent<AudioSource>();
        }
        if (this.playerAudio != null)
        {
            this.originalClip = this.playerAudio.clip;
        }
    }

    private void OnTriggerEnter(Collider other)   // 강의 05장: Is Trigger + OnTriggerEnter
    {
        if (other.CompareTag("Player") && this.playerAudio != null && this.zoneClip != null)   // 강의 07장: CompareTag
        {
            this.playerAudio.clip = this.zoneClip;
        }
    }

    private void OnTriggerExit(Collider other)   // 강의 05장: Is Trigger + OnTriggerExit
    {
        if (other.CompareTag("Player") && this.playerAudio != null)
        {
            this.playerAudio.clip = this.originalClip;
        }
    }
}
