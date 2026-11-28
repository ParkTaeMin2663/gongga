using UnityEngine;

// 플레이어 숨소리. 평소에는 작고 고른 숨, 뛰면 점점 헐떡이고, 멈추면 천천히 가라앉는다.
public class PlayerBreath : MonoBehaviour
{
    [Header("소리")]
    [Tooltip("평소 숨소리 (Loop)")]
    public AudioSource calmBreath;
    [Tooltip("헐떡이는 숨소리 (Loop)")]
    public AudioSource heavyBreath;

    [Header("볼륨 (0~1)")]
    [Range(0f, 1f)] public float calmVolume = 0.25f;
    [Range(0f, 1f)] public float heavyVolume = 0.8f;
    [Tooltip("장롱에 숨어 있을 때 평소 숨소리를 몇 배로 (좁은 곳이라 크게 들림)")]
    public float hiddenCalmMultiplier = 1.6f;

    [Header("숨참")]
    [Tooltip("달릴 때 1초에 오르는 양. 0.2면 5초 달리면 최대로 헐떡임")]
    public float exertionUpSpeed = 0.2f;
    [Tooltip("안 달릴 때 1초에 내려가는 양. 0.1이면 최대에서 10초 쉬면 가라앉음")]
    public float exertionDownSpeed = 0.1f;

    [Header("괴물에게 들리는 숨소리")]
    [Tooltip("숨어 있을 때 숨참이 이 값보다 크면 헐떡임이 괴물에게 들린다")]
    [Range(0f, 1f)] public float noiseThreshold = 0.5f;
    [Tooltip("헐떡임 소리 크기 = 괴물이 듣는 거리(m)")]
    public float noiseLoudness = 2.5f;
    [Tooltip("몇 초마다 소리를 낼지")]
    public float noiseInterval = 1.5f;

    public float Exertion { get; private set; }

    private PlayerMovement playerMovement;
    private PlayerHide playerHide;
    private NoiseSystem noiseSystem;
    private float noiseTimer;

    private void Start()
    {
        this.playerMovement = this.GetComponent<PlayerMovement>();
        this.playerHide = this.GetComponent<PlayerHide>();

        GameObject noiseObject = GameObject.Find("NoiseSystem");   // 강의 04장: GameObject.Find (Start에서 한 번)
        if (noiseObject != null)
        {
            this.noiseSystem = noiseObject.GetComponent<NoiseSystem>();
        }

        if (this.calmBreath != null)
        {
            this.calmBreath.loop = true;
            this.calmBreath.Play();
        }
        if (this.heavyBreath != null)
        {
            this.heavyBreath.loop = true;
            this.heavyBreath.volume = 0f;
            this.heavyBreath.Play();
        }
    }

    private void Update()
    {
        bool isSprinting = this.playerMovement != null && this.playerMovement.enabled && this.playerMovement.IsSprinting;
        if (isSprinting)
        {
            this.Exertion = Mathf.Min(1f, this.Exertion + this.exertionUpSpeed * Time.deltaTime);   // 강의 03장: Time.deltaTime
        }
        else
        {
            this.Exertion = Mathf.Max(0f, this.Exertion - this.exertionDownSpeed * Time.deltaTime);
        }

        bool isHidden = this.playerHide != null && this.playerHide.IsHidden;
        float calm = this.calmVolume * (1f - this.Exertion * 0.8f);
        if (isHidden)
        {
            calm = calm * this.hiddenCalmMultiplier;
        }
        if (this.calmBreath != null)
        {
            this.calmBreath.volume = calm;
        }
        if (this.heavyBreath != null)
        {
            this.heavyBreath.volume = this.heavyVolume * this.Exertion;
        }

        if (isHidden && this.Exertion > this.noiseThreshold)
        {
            this.noiseTimer = this.noiseTimer - Time.deltaTime;
            if (this.noiseTimer <= 0f)
            {
                this.noiseTimer = this.noiseInterval;
                if (this.noiseSystem != null)
                {
                    this.noiseSystem.Emit(this.transform.position, this.noiseLoudness, "헐떡임");
                }
            }
        }
        else
        {
            this.noiseTimer = 0f;
        }
    }
}
