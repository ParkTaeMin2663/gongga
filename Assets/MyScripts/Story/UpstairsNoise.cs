using System.Collections;
using UnityEngine;

// 1장 연출: 1층에 있는 동안 위층에서 괴물 발소리·으르렁 소리가 가끔 들린다.
public class UpstairsNoise : MonoBehaviour
{
    [Header("소리")]
    [Tooltip("위층 발소리 (예: Assets/Audio/Monster/monster_step_0~3). repeat번 이어서 쿵, 쿵, 쿵")]
    public AudioClip[] stepClips;

    [Tooltip("가끔 섞을 다른 소리 (예: monster_growl, door_knock). 한 번만 난다. 비워 둬도 됨")]
    public AudioClip[] otherClips;

    [Tooltip("다른 소리(otherClips)가 날 확률 (0~1). 0.3 = 열 번에 세 번")]
    public float otherChance = 0.3f;

    [Tooltip("소리 크기 (0~1)")]
    public float volume = 1f;

    [Header("간격 (초)")]
    [Tooltip("구역에 들어오고 첫 소리까지 기다리는 시간")]
    public float firstDelay = 4f;
    [Tooltip("소리 사이 가장 짧은 간격")]
    public float minDelay = 10f;
    [Tooltip("소리 사이 가장 긴 간격")]
    public float maxDelay = 20f;

    [Header("위치")]
    [Tooltip("플레이어 머리 위로 얼마나 높이 (위층 바닥 = 2.8)")]
    public float height = 3.2f;
    [Tooltip("앞뒤좌우로 이만큼까지 무작위로 옮김 (m)")]
    public float spread = 6f;

    [Header("한 번에 여러 번 (발소리처럼)")]
    [Tooltip("소리를 몇 번 이어서 낼지 (예: 3 = 쿵, 쿵, 쿵). 1이면 한 번")]
    public int repeat = 3;
    [Tooltip("이어서 낼 때 간격(초)")]
    public float repeatGap = 0.6f;

    [Header("다시 시작")]
    [Tooltip("이 번호 이상의 체크포인트를 지났으면 소리를 내지 않는다 (2 = 2층 체크포인트)")]
    public int stopFromCheckpoint = 2;

    private Transform player;
    private bool isInside;
    private Coroutine loop;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            this.player = playerObject.transform;
        }
        if (this.stepClips == null || this.stepClips.Length == 0)
        {
            Debug.LogWarning("[UpstairsNoise] Step Clips가 비어 있어서 위층 발소리가 나지 않습니다.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") == false || this.isInside)
        {
            return;
        }
        if (this.stopFromCheckpoint > 0 && Checkpoint.savedOrder >= this.stopFromCheckpoint)
        {
            return;
        }
        this.isInside = true;
        this.loop = this.StartCoroutine(this.NoiseLoop());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") == false)
        {
            return;
        }
        this.isInside = false;
        if (this.loop != null)
        {
            this.StopCoroutine(this.loop);
            this.loop = null;
        }
    }

    private IEnumerator NoiseLoop()
    {
        yield return new WaitForSeconds(this.firstDelay);

        while (this.isInside)
        {
            Vector3 spot = this.RandomSpot();
            bool hasOther = this.otherClips != null && this.otherClips.Length > 0;

            if (hasOther && Random.value < this.otherChance)
            {
                this.PlayRandomClip(this.otherClips, spot);
            }
            else
            {
                Vector3 step = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-0.5f, 0.5f)).normalized * 0.8f;
                for (int i = 0; i < this.repeat; i++)
                {
                    this.PlayRandomClip(this.stepClips, spot + step * i);
                    yield return new WaitForSeconds(this.repeatGap);
                }
            }

            yield return new WaitForSeconds(Random.Range(this.minDelay, this.maxDelay));
        }
    }

    private Vector3 RandomSpot()
    {
        Vector3 center = this.player != null ? this.player.position : this.transform.position;
        return center + new Vector3(Random.Range(-this.spread, this.spread), this.height, Random.Range(-this.spread, this.spread));
    }

    private void PlayRandomClip(AudioClip[] list, Vector3 position)
    {
        if (list == null || list.Length == 0)
        {
            return;
        }
        AudioClip clip = list[Random.Range(0, list.Length)];
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position, this.volume);
        }
    }
}
