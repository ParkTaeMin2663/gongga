using UnityEngine;

/// <summary>
/// 플레이어 숨소리. 평소에는 작고 고른 숨, 뛰면 점점 헐떡이고, 멈추면 천천히 가라앉는다.
/// 장롱에 숨어 있을 때 헐떡이면 괴물에게 들릴 수 있다 (작은 소리).
///
/// [붙이는 곳] Player 오브젝트 (Player 프리팹)
/// [같이 필요한 컴포넌트] Audio Source 2개 (강의 03장)
///   - 평소 숨소리 : Loop 체크, Play On Awake 끔 → Calm Breath 칸에
///   - 헐떡임      : Loop 체크, Play On Awake 끔 → Heavy Breath 칸에
///   (발소리용 Audio Source와는 따로)
///
/// [동작] "숨참(exertion)" 값 0~1
///   - 달리는 동안 올라가고(강의 03장 Time.deltaTime), 안 달리면 내려간다.
///   - 평소 숨소리는 숨참이 클수록 작아지고, 헐떡임은 커진다 (두 소리를 섞음).
/// </summary>
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

    /// <summary>지금 숨찬 정도 (0 = 평온, 1 = 최대로 헐떡임)</summary>
    public float Exertion { get; private set; }

    // ----- Start()에서 한 번만 찾아 저장 (강의 04장) -----
    private PlayerMovement playerMovement;
    private PlayerHide playerHide;
    private NoiseSystem noiseSystem;
    private float noiseTimer;

    private void Start()
    {
        this.playerMovement = this.GetComponent<PlayerMovement>();
        this.playerHide = this.GetComponent<PlayerHide>();

        GameObject noiseObject = GameObject.Find("NoiseSystem");
        if (noiseObject != null)
        {
            this.noiseSystem = noiseObject.GetComponent<NoiseSystem>();
        }

        // 두 숨소리를 처음부터 계속 반복 재생해 두고, 볼륨만 바꾼다 (강의 03장 Loop, Play)
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
        // 1) 숨참: 달리면 오르고, 아니면 내려간다
        bool isSprinting = this.playerMovement != null && this.playerMovement.enabled && this.playerMovement.IsSprinting;
        if (isSprinting)
        {
            this.Exertion = Mathf.Min(1f, this.Exertion + this.exertionUpSpeed * Time.deltaTime);
        }
        else
        {
            this.Exertion = Mathf.Max(0f, this.Exertion - this.exertionDownSpeed * Time.deltaTime);
        }

        // 2) 두 소리 섞기
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

        // 3) 숨어서 헐떡이면 괴물에게 들린다 (밖에서 뛸 때는 발소리가 이미 소리를 낸다)
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
