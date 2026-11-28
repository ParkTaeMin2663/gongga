using System.Collections;
using UnityEngine;

// 4장 지하: 제사상 앞에서 동생과 마주하는 연출, 끝나면 최종 추격을 시작한다.
public class BrotherEncounter : MonoBehaviour
{
    [Header("동생")]
    [Tooltip("동생 오브젝트 (모델 또는 작은 검은 캡슐). 연출 끝에 사라진다")]
    public GameObject brother;
    [Tooltip("고개를 돌려 바라볼 곳 (동생 오브젝트를 그대로 넣어도 됨)")]
    public Transform lookTarget;
    [Tooltip("고개가 돌아가는 시간(초)")]
    public float turnTime = 1.5f;

    [Header("자막 (동생의 말)")]
    [TextArea(1, 3)]
    public string[] lines = new string[]
    {
        "…형아?",
        "형아 올 때까지 불 켜 놓는다고 했잖아.",
        "아저씨들이 형아 대신 나를 데려왔어. 여기 계속 있었어. 서른 번이나 겨울이 지났어.",
        "근데 형아, 그 아저씨가 아직 배고프대.",
        "…도망가."
    };
    [Tooltip("자막 한 줄이 떠 있는 시간(초)")]
    public float lineTime = 3.5f;
    [Tooltip("자막 글자 크기 (1080p 기준)")]
    public int lineSize = 32;
    [Tooltip("(없어도 됨) 동생 목소리. 연출이 시작될 때 한 번 재생 (이 오브젝트에 Audio Source를 붙이고 Clip은 여기)")]
    public AudioClip voiceClip;

    [Header("끝날 때")]
    [Tooltip("연출 끝에 끌 조명들 (제사상 촛불 Point Light 등)")]
    public Light[] lightsOff;
    [Tooltip("빨간 경고 글자")]
    public string runText = "도망쳐";
    [Tooltip("경고 글자 크기 (1080p 기준, \"숨어라\"와 같은 120)")]
    public int runTextSize = 120;
    [Tooltip("경고 글자가 떠 있는 시간(초)")]
    public float runTextTime = 2f;

    [Header("추격 시작 (A 16번과 연결)")]
    [Tooltip("연출이 끝나면 켤 오브젝트. 처음엔 꺼 둔다(체크 해제). 예: Monster_B1")]
    public GameObject[] activateAfter;
    [Tooltip("괴물을 켠 뒤 플레이어 자리에서 나는 소리의 크기 = 들리는 거리(m). 지하 전체에 들리게 크게")]
    public float noiseLoudness = 40f;
    [Tooltip("(11/24) 체크하면 추격이 시작될 때부터 엔딩까지 쫓기는 음악(TensionMusic 추격 음악)이 계속 나온다. 괴물이 잠깐 놓쳐도 안 꺼짐")]
    public bool chaseMusicUntilEnd = true;

    [Header("비상구 잠금 (11/25)")]
    [Tooltip("동생을 만나기 전에는 잠가 두고, 추격이 시작될 때 여는 문의 이름 (씬의 비상구 문). 비워 두면 안 잠금")]
    public string exitDoorName = "Door_Exit";
    [Tooltip("비상구가 잠겨 있을 때 띄울 글")]
    public string exitLockedMessage = "비상구가 잠겨 있다. 아직 나갈 수 없다";

    [Header("다시 시작")]
    [Tooltip("이 번호 이상의 체크포인트에서 다시 시작하면 자막 없이 바로 추격 (4 = 지하 체크포인트)")]
    public int retryFromCheckpoint = 4;
    [Tooltip("다시 시작한 뒤 괴물이 켜질 때까지 시간(초)")]
    public float retryDelay = 2f;

    public static bool finished = false;   // 강의 07장: public static 변수

    private bool isRespawn;
    private bool isPlaying;
    private string currentLine;
    private float lineAlpha;
    private bool showRunText;
    private float runAlpha;

    private Transform player;
    private PlayerMovement playerMovement;
    private PlayerLook playerLook;
    private PlayerInteract playerInteract;
    private PlayerDeath playerDeath;
    private NoiseSystem noiseSystem;
    private DraggableDoor exitDoor;

    private void Awake()   // 강의 05장: Awake
    {
        this.isRespawn = Checkpoint.isRespawning;
        if (this.isRespawn == false)
        {
            finished = false;
        }
    }

    private void Start()
    {
        GameObject playerObject = GameObject.Find("Player");   // 강의 04장: GameObject.Find (Start에서 한 번)
        if (playerObject != null)
        {
            this.player = playerObject.transform;
            this.playerMovement = playerObject.GetComponent<PlayerMovement>();
            this.playerLook = playerObject.GetComponent<PlayerLook>();
            this.playerInteract = playerObject.GetComponent<PlayerInteract>();
            this.playerDeath = playerObject.GetComponent<PlayerDeath>();
        }
        GameObject noiseObject = GameObject.Find("NoiseSystem");
        if (noiseObject != null)
        {
            this.noiseSystem = noiseObject.GetComponent<NoiseSystem>();
        }
        if (this.lookTarget == null && this.brother != null)
        {
            this.lookTarget = this.brother.transform;
        }

        if (this.exitDoorName != "")
        {
            GameObject doorObject = GameObject.Find(this.exitDoorName);
            if (doorObject != null)
            {
                this.exitDoor = doorObject.GetComponentInChildren<DraggableDoor>();
            }
            if (this.exitDoor != null)
            {
                this.exitDoor.isLocked = true;
                this.exitDoor.lockedMessage = this.exitLockedMessage;
            }
            else
            {
                Debug.LogWarning("[BrotherEncounter] 비상구 문(" + this.exitDoorName + ")을 못 찾아서 잠그지 못했습니다.");
            }
        }

        if (this.isRespawn && finished && Checkpoint.savedOrder >= this.retryFromCheckpoint)
        {
            this.isPlaying = true;
            this.SetEndState();
            this.StartCoroutine(this.RetryRoutine());   // 강의 05장: 코루틴 StartCoroutine
        }
    }

    private void OnTriggerEnter(Collider other)   // 강의 05장: Is Trigger + OnTriggerEnter
    {
        if (this.isPlaying || other.CompareTag("Player") == false)   // 강의 07장: CompareTag
        {
            return;
        }
        this.isPlaying = true;
        this.StartCoroutine(this.EncounterRoutine());
    }

    private IEnumerator EncounterRoutine()
    {
        this.SetPlayerFrozen(true);

        AudioSource voice = this.GetComponent<AudioSource>();
        if (voice != null && this.voiceClip != null)
        {
            voice.clip = this.voiceClip;
            voice.Play();
        }

        if (this.lookTarget != null && this.playerLook != null && this.playerLook.cameraTransform != null)
        {
            Vector3 dir = this.lookTarget.position - this.player.position;
            float startYaw = this.playerLook.cameraTransform.eulerAngles.y;
            float endYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            float t = 0f;
            while (t < 1f)
            {
                t = t + Time.deltaTime / this.turnTime;   // 강의 03장: Time.deltaTime
                this.playerLook.SetYaw(Mathf.LerpAngle(startYaw, endYaw, t));
                yield return null;
            }
        }

        for (int i = 0; i < this.lines.Length; i++)
        {
            this.currentLine = this.lines[i];
            yield return this.StartCoroutine(this.FadeLine(0f, 1f, 0.4f));
            yield return new WaitForSeconds(this.lineTime);   // 강의 05장: WaitForSeconds
            yield return this.StartCoroutine(this.FadeLine(1f, 0f, 0.4f));
        }
        this.currentLine = null;

        this.SetEndState();
        yield return new WaitForSeconds(0.8f);

        finished = true;
        this.showRunText = true;
        this.runAlpha = 1f;
        this.SetPlayerFrozen(false);
        yield return this.StartCoroutine(this.StartChase());

        yield return new WaitForSeconds(this.runTextTime);
        while (this.runAlpha > 0f)
        {
            this.runAlpha = this.runAlpha - Time.deltaTime;
            yield return null;
        }
        this.showRunText = false;
    }

    private IEnumerator RetryRoutine()
    {
        yield return new WaitForSeconds(this.retryDelay);
        yield return this.StartCoroutine(this.StartChase());
    }

    private IEnumerator StartChase()
    {
        if (this.exitDoor != null)
        {
            this.exitDoor.isLocked = false;
            this.exitDoor.lockedMessage = "";
        }

        for (int i = 0; i < this.activateAfter.Length; i++)
        {
            if (this.activateAfter[i] != null)
            {
                this.activateAfter[i].SetActive(true);
            }
        }
        if (this.chaseMusicUntilEnd && this.player != null)
        {
            TensionMusic music = this.player.GetComponent<TensionMusic>();
            if (music != null)
            {
                music.PlayChase();
            }
        }
        yield return new WaitForSeconds(0.5f);
        if (this.noiseSystem != null && this.player != null)
        {
            this.noiseSystem.Emit(this.player.position, this.noiseLoudness, "동생 울음");
        }
    }

    private IEnumerator FadeLine(float from, float to, float time)
    {
        float t = 0f;
        while (t < 1f)
        {
            t = t + Time.deltaTime / time;
            this.lineAlpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
    }

    private void SetEndState()
    {
        if (this.brother != null)
        {
            this.brother.SetActive(false);
        }
        for (int i = 0; i < this.lightsOff.Length; i++)
        {
            if (this.lightsOff[i] != null)
            {
                this.lightsOff[i].enabled = false;
            }
        }
    }

    private void SetPlayerFrozen(bool frozen)
    {
        if (this.playerMovement != null) this.playerMovement.enabled = !frozen;
        if (this.playerInteract != null) this.playerInteract.enabled = !frozen;
        if (this.playerLook != null) this.playerLook.IsLocked = frozen;
        if (frozen && this.player != null)
        {
            Rigidbody body = this.player.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
            }
        }
    }

    private void OnGUI()   // 강의 04장: OnGUI
    {
        if (this.playerDeath != null && this.playerDeath.IsDead)
        {
            return;
        }

        if (this.currentLine != null)
        {
            GUIStyle lineStyle = UIStyle.Body(UIStyle.Text(this.lineSize), UIStyle.white);
            lineStyle.alignment = TextAnchor.MiddleCenter;
            lineStyle.wordWrap = true;
            float margin = UIStyle.Size(200);
            Rect rect = new Rect(margin, Screen.height - UIStyle.Size(240), Screen.width - margin * 2f, UIStyle.Size(140));
            UIStyle.Label(rect, this.currentLine, lineStyle, this.lineAlpha);
        }

        if (this.showRunText)
        {
            Rect rect = new Rect(0f, Screen.height / 2f - UIStyle.Size(90), Screen.width, UIStyle.Size(180));
            UIStyle.WarningText(rect, this.runText, UIStyle.Warning(this.runTextSize), this.runAlpha);
        }
    }
}
