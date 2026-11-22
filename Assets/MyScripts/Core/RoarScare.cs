using UnityEngine;

/// <summary>
/// 깜짝 포효. "숨어라"가 시작될 때 괴물이 한 번 크게 울부짖고, 그 뒤로는 약 3분마다 어디선가 포효가 들린다. (11/18)
///   - 괴물이 켜져 있으면 괴물이 있는 방향에서, 없으면(2층 괴물이 내려간 뒤 등) 아래층 어딘가에서 들린다.
///   - 멀리 있어도 들리게, 플레이어 쪽으로 가까이(nearDistance) 당겨서 재생한다 (방향은 그대로).
///   - 괴물이 쫓아오는 중이면 건너뛴다 (이미 포효했으므로).
///
/// [붙이는 곳] Player 프리팹 (이미 붙어 있음)
/// [쓰는 곳]   Floor2Event가 "숨어라" 때 StartScare(괴물 위치)를 부른다.
///             죽고 다시 시작해도 "숨어라"를 이미 지났으면(Floor2Event.reachedHidePhase, 체크포인트 3 이상) 계속된다.
/// [방식] 강의 03장 Time.deltaTime 타이머 + Random.Range, 강의 07장 AudioSource.PlayClipAtPoint
/// </summary>
public class RoarScare : MonoBehaviour
{
    [Header("소리")]
    [Tooltip("\"숨어라\" 때 처음 한 번 내는 포효")]
    public AudioClip firstRoarClip;
    [Tooltip("그 뒤로 가끔 들리는 포효들 (무작위로 하나)")]
    public AudioClip[] scareClips;
    [Range(0f, 1f)] public float volume = 1f;

    [Header("간격 (초)")]
    [Tooltip("평균 간격. 180 = 3분")]
    public float interval = 180f;
    [Tooltip("간격이 이만큼 들쭉날쭉 (예측 못 하게)")]
    public float randomRange = 40f;
    [Tooltip("소리를 플레이어에게서 이 거리(m)까지 당겨서 재생한다 (멀어도 잘 들리게)")]
    public float nearDistance = 7f;

    private bool isActive;
    private float timer;
    private PlayerDeath playerDeath;

    private void Start()
    {
        this.playerDeath = this.GetComponent<PlayerDeath>();

        // 다시 시작한 게임에서도 "숨어라"를 이미 지났으면 계속 (강의 07장 public static 값)
        if (Floor2Event.reachedHidePhase || Checkpoint.savedOrder >= 3)
        {
            this.isActive = true;
            this.ResetTimer();
        }
    }

    /// <summary>"숨어라" 시작: 괴물 쪽에서 바로 한 번 포효하고 3분 타이머 시작</summary>
    public void StartScare(Vector3 monsterPosition)
    {
        this.PlayRoar(this.firstRoarClip, monsterPosition);
        this.isActive = true;
        this.ResetTimer();
    }

    private void Update()
    {
        if (this.isActive == false || (this.playerDeath != null && this.playerDeath.IsDead))
        {
            return;
        }

        this.timer = this.timer - Time.deltaTime;   // 강의 03장
        if (this.timer > 0f)
        {
            return;
        }

        // 켜져 있는 괴물 찾기 (3분에 한 번만 찾으므로 괜찮다)
        MonsterAI monster = FindAnyObjectByType<MonsterAI>();   // (11/22) Unity 6.5에서 FindFirstObjectByType이 사라질 예정이라 교체
        if (monster != null && monster.enabled && monster.CurrentState == MonsterAI.State.Chase)
        {
            this.timer = 20f;   // 쫓기는 중이면 조금 뒤에 다시
            return;
        }

        Vector3 from;
        if (monster != null && monster.enabled)
        {
            from = monster.transform.position;
        }
        else
        {
            // 괴물이 안 보일 때: 아래층 어딘가에서 (강의 03장 Random.Range)
            Vector3 dir = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) * Vector3.forward;
            from = this.transform.position + dir * 15f + Vector3.down * 3f;
        }

        AudioClip clip = this.firstRoarClip;
        if (this.scareClips != null && this.scareClips.Length > 0)
        {
            clip = this.scareClips[Random.Range(0, this.scareClips.Length)];
        }
        this.PlayRoar(clip, from);
        this.ResetTimer();
    }

    // from 쪽 방향에서, 플레이어에게서 nearDistance 이내 거리로 당겨 재생
    private void PlayRoar(AudioClip clip, Vector3 from)
    {
        if (clip == null)
        {
            return;
        }
        Vector3 toSource = from - this.transform.position;
        float distance = Mathf.Min(toSource.magnitude, this.nearDistance);
        Vector3 position = this.transform.position + toSource.normalized * distance;
        AudioSource.PlayClipAtPoint(clip, position, this.volume);   // 강의 07장
    }

    private void ResetTimer()
    {
        this.timer = this.interval + Random.Range(-this.randomRange, this.randomRange);
    }
}
