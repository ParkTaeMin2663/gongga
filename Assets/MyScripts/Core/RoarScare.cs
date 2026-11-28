using UnityEngine;

// 깜짝 포효. "숨어라"가 시작될 때 괴물이 한 번 크게 울부짖고, 그 뒤로는 약 3분마다 어디선가 포효가 들린다.
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

        if (Floor2Event.reachedHidePhase || Checkpoint.savedOrder >= 3)
        {
            this.isActive = true;
            this.ResetTimer();
        }
    }

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

        this.timer = this.timer - Time.deltaTime;
        if (this.timer > 0f)
        {
            return;
        }

        MonsterAI monster = FindAnyObjectByType<MonsterAI>();
        if (monster != null && monster.enabled && monster.CurrentState == MonsterAI.State.Chase)
        {
            this.timer = 20f;
            return;
        }

        Vector3 from;
        if (monster != null && monster.enabled)
        {
            from = monster.transform.position;
        }
        else
        {
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

    private void PlayRoar(AudioClip clip, Vector3 from)
    {
        if (clip == null)
        {
            return;
        }
        Vector3 toSource = from - this.transform.position;
        float distance = Mathf.Min(toSource.magnitude, this.nearDistance);
        Vector3 position = this.transform.position + toSource.normalized * distance;
        AudioSource.PlayClipAtPoint(clip, position, this.volume);
    }

    private void ResetTimer()
    {
        this.timer = this.interval + Random.Range(-this.randomRange, this.randomRange);
    }
}
