using UnityEngine;

// 배경음 관리: 평소 배경음, "숨어라" 긴장 음악, 추격 음악을 상황에 맞게 바꿔 튼다.
public class TensionMusic : MonoBehaviour
{
    [Header("연결 (Player의 Audio Source 2개)")]
    [Tooltip("긴장 음악을 재생할 Audio Source (Loop, Spatial Blend 0 = 2D)")]
    public AudioSource musicSource;
    [Tooltip("으스스한 배경음을 재생할 Audio Source (Loop, 2D)")]
    public AudioSource ambientSource;

    [Header("음악 파일 (Assets/Audio/Music)")]
    [Tooltip("숨어라 — 심장 소리 긴장 음악")]
    public AudioClip hideClip;
    [Tooltip("추격 — 빠르고 긴박한 음악")]
    public AudioClip chaseClip;
    [Tooltip("게임 내내 깔리는 으스스한 배경음")]
    public AudioClip ambientClip;

    [Header("볼륨")]
    [Range(0f, 1f)] public float hideVolume = 0.5f;
    [Range(0f, 1f)] public float chaseVolume = 0.55f;
    [Tooltip("평소 배경음 볼륨")]
    [Range(0f, 1f)] public float ambientVolume = 0.35f;
    [Tooltip("긴장 음악이 나올 때 배경음 볼륨 (작아짐)")]
    [Range(0f, 1f)] public float ambientDuckVolume = 0.08f;
    [Tooltip("켤 때 커지는 데 걸리는 시간(초)")]
    public float fadeInTime = 1.2f;
    [Tooltip("끌 때 작아지는 데 걸리는 시간(초)")]
    public float fadeOutTime = 4f;

    private int hideCount;
    private int chaseCount;
    private AudioClip wantClip;

    private void Start()
    {
        if (this.musicSource != null)
        {
            this.musicSource.loop = true;
            this.musicSource.volume = 0f;
        }
        if (this.ambientSource != null && this.ambientClip != null)
        {
            this.ambientSource.clip = this.ambientClip;
            this.ambientSource.loop = true;
            this.ambientSource.volume = 0f;
            this.ambientSource.Play();   // 강의 03장: AudioSource.Play
        }
    }

    public void PlayHide() { this.hideCount = this.hideCount + 1; }
    public void StopHide() { this.hideCount = Mathf.Max(0, this.hideCount - 1); }
    public void PlayChase() { this.chaseCount = this.chaseCount + 1; }
    public void StopChase() { this.chaseCount = Mathf.Max(0, this.chaseCount - 1); }

    private void Update()
    {
        AudioClip want = null;
        float wantVolume = 0f;
        if (this.chaseCount > 0 && this.chaseClip != null)
        {
            want = this.chaseClip;
            wantVolume = this.chaseVolume;
        }
        else if (this.hideCount > 0 && this.hideClip != null)
        {
            want = this.hideClip;
            wantVolume = this.hideVolume;
        }

        if (this.musicSource != null)
        {
            if (want != null && this.musicSource.clip != want)
            {
                if (this.musicSource.isPlaying && this.musicSource.volume > 0.05f)
                {
                    wantVolume = 0f;
                }
                else
                {
                    this.musicSource.clip = want;
                    this.musicSource.Play();
                }
            }

            float target = want != null ? wantVolume : 0f;
            float speed = target > this.musicSource.volume ? 1f / this.fadeInTime : 1f / this.fadeOutTime;
            this.musicSource.volume = Mathf.MoveTowards(this.musicSource.volume, target, speed * Time.deltaTime);   // 강의 03장: Time.deltaTime
            if (want == null && this.musicSource.volume <= 0f && this.musicSource.isPlaying)
            {
                this.musicSource.Stop();
                this.musicSource.clip = null;
            }
        }

        if (this.ambientSource != null)
        {
            float ambientTarget = want != null ? this.ambientDuckVolume : this.ambientVolume;
            this.ambientSource.volume = Mathf.MoveTowards(this.ambientSource.volume, ambientTarget, Time.deltaTime / this.fadeInTime);
        }
    }
}
