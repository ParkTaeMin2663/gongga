using UnityEngine;

/// <summary>
/// 배경음 관리. (11/18)
///   - 으스스한 배경음(ambient): 게임 시작부터 계속 작게 깔린다.
///   - 긴장 음악 2종: "숨어라"(숨기 — 심장 소리) / 추격(쫓김 — 빠른 음악). 켜지면 배경음은 작아진다.
///   - 둘 다 켜지면 추격 음악이 우선.
///
/// [붙이는 곳] Player 프리팹 (이미 붙어 있음) + 같은 Player의 Audio Source 2개 (둘 다 Loop, 2D, Play On Awake 끔)
/// [쓰는 곳]   Floor2Event: PlayHide() / StopHide()  ("숨어라" → 괴물이 사라질 때까지)
///             MonsterAI  : PlayChase() / StopChase() (추격 시작 → 순찰로 돌아갈 때까지)
///             → playerObject.GetComponent<TensionMusic>()로 찾는다
/// [방식] 강의 03장 AudioSource(clip, Play, Loop, volume) + Time.deltaTime으로 볼륨을 조금씩 바꿔 서서히 커지고 작아진다.
/// </summary>
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

    private int hideCount;    // "숨어라" 음악을 원하는 곳 수
    private int chaseCount;   // 추격 음악을 원하는 곳 수 (괴물이 여럿일 수 있음)
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
            this.ambientSource.volume = 0f;   // 서서히 커지며 시작
            this.ambientSource.Play();        // 강의 03장
        }
    }

    public void PlayHide() { this.hideCount = this.hideCount + 1; }
    public void StopHide() { this.hideCount = Mathf.Max(0, this.hideCount - 1); }
    public void PlayChase() { this.chaseCount = this.chaseCount + 1; }
    public void StopChase() { this.chaseCount = Mathf.Max(0, this.chaseCount - 1); }

    private void Update()
    {
        // 지금 틀어야 할 음악: 추격 > 숨어라 > 없음
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
                // 다른 음악으로 바꿀 때: 아직 크게 나오고 있으면 먼저 줄인 뒤 바꾼다
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
            this.musicSource.volume = Mathf.MoveTowards(this.musicSource.volume, target, speed * Time.deltaTime);   // 강의 03장 Time.deltaTime
            if (want == null && this.musicSource.volume <= 0f && this.musicSource.isPlaying)
            {
                this.musicSource.Stop();
                this.musicSource.clip = null;
            }
        }

        // 배경음: 긴장 음악이 나오면 작아지고, 끝나면 다시 커진다
        if (this.ambientSource != null)
        {
            float ambientTarget = want != null ? this.ambientDuckVolume : this.ambientVolume;
            this.ambientSource.volume = Mathf.MoveTowards(this.ambientSource.volume, ambientTarget, Time.deltaTime / this.fadeInTime);
        }
    }
}
