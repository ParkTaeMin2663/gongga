using System.Collections;          // 강의 05장: 코루틴(IEnumerator)
using UnityEngine;
using UnityEngine.SceneManagement; // 강의 04장: SceneManager.LoadScene

/// <summary>
/// 플레이어 사망과 다시 시작.
///   - 괴물에게 잡히면 Die() → 몸이 굳고, 괴물 쪽으로 고개가 돌아가며, 화면이 붉게 번지다가 까맣게 → 씬 다시 불러오기
///   - 씬이 다시 열리면 Start()에서 마지막 체크포인트(Checkpoint.savedPosition)로 옮긴다.
///   - 처음 시작할 때(죽어서 다시 연 게 아닐 때)는 체크포인트 기록을 지운다.
///
/// [붙이는 곳] Player 오브젝트 (Player 프리팹)
/// [Inspector 연결] 비워 두면 같은 Player에서 자동으로 찾는다.
/// [같이 쓰는 것] Checkpoint.cs (public static 저장값)
/// </summary>
public class PlayerDeath : MonoBehaviour
{
    [Header("사망 연출 (초)")]
    [Tooltip("괴물 쪽으로 고개가 돌아가는 시간")]
    public float turnTime = 0.25f;
    [Tooltip("화면이 붉어졌다가 까맣게 되는 시간")]
    public float fadeTime = 1.2f;
    [Tooltip("까만 화면으로 기다리는 시간")]
    public float blackTime = 0.6f;

    [Header("소리 (없어도 됨)")]
    [Tooltip("잡혔을 때 소리. Player에 Audio Source를 하나 더 붙이고 드래그 (발소리용과 따로)")]
    public AudioSource deathSound;

    [Header("연결 (비워 두면 자동으로 찾음)")]
    public PlayerMovement playerMovement;
    public PlayerLook playerLook;
    public PlayerInteract playerInteract;

    /// <summary>죽는 중인지. 다른 스크립트(괴물 등)가 두 번 잡지 않도록 확인한다.</summary>
    public bool IsDead { get; private set; }

    private float redAmount;     // 화면 붉은 정도 (0~1)
    private float blackAmount;   // 화면 검은 정도 (0~1)
    private Texture2D whiteTexture;

    private void Start()
    {
        this.whiteTexture = Texture2D.whiteTexture;

        if (this.playerMovement == null) this.playerMovement = this.GetComponent<PlayerMovement>();
        if (this.playerLook == null) this.playerLook = this.GetComponent<PlayerLook>();
        if (this.playerInteract == null) this.playerInteract = this.GetComponent<PlayerInteract>();

        if (Checkpoint.isRespawning)
        {
            // 죽어서 다시 열린 씬 → 마지막 체크포인트에서 시작
            Checkpoint.isRespawning = false;
            this.transform.position = Checkpoint.savedPosition;
            if (this.playerLook != null)
            {
                this.playerLook.SetYaw(Checkpoint.savedYaw);
            }
            this.blackAmount = 1f;   // 까만 화면에서 서서히 밝아지며 시작

            // 체크포인트 때의 손전등 상태로 되돌린다 (주운 손전등을 잃지 않게)
            PlayerFlashlight flashlight = this.GetComponent<PlayerFlashlight>();
            if (flashlight != null)
            {
                flashlight.hasFlashlight = Checkpoint.savedHasFlashlight;
                flashlight.battery = Checkpoint.savedBattery;
            }
        }
        else
        {
            // 새로 시작한 게임 → 예전 기록 지우기
            Checkpoint.savedOrder = 0;
            Checkpoint.lastSaveTime = -100f;
        }
    }

    private void Update()
    {
        // 다시 시작했을 때 까만 화면이 서서히 걷힌다 (강의 03장 Time.deltaTime)
        if (this.IsDead == false && this.blackAmount > 0f)
        {
            this.blackAmount = Mathf.Max(0f, this.blackAmount - Time.deltaTime / this.fadeTime);
        }
    }

    /// <summary>괴물이 잡았을 때 부른다. killer = 잡은 괴물 (그쪽을 바라보게)</summary>
    public void Die(Transform killer)
    {
        if (this.IsDead)
        {
            return;
        }
        this.IsDead = true;

        // 몸을 굳힌다: 걷기·잡기·시점 멈춤, 물리 멈춤 (강의 03장 Is Kinematic)
        if (this.playerMovement != null) this.playerMovement.enabled = false;
        if (this.playerInteract != null) this.playerInteract.enabled = false;
        if (this.playerLook != null) this.playerLook.IsLocked = true;
        Rigidbody rb = this.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (this.deathSound != null)
        {
            this.deathSound.Play();   // 강의 03장
        }

        // 강의 05장: 코루틴으로 시간 순서대로 연출
        this.StartCoroutine(this.DeathRoutine(killer));
    }

    private IEnumerator DeathRoutine(Transform killer)
    {
        // 1) 괴물 쪽으로 고개가 홱 돌아간다
        float startYaw = this.playerLook != null ? this.playerLook.Yaw : 0f;
        Vector3 toKiller = killer != null ? killer.position - this.transform.position : this.transform.forward;
        float targetYaw = Mathf.Atan2(toKiller.x, toKiller.z) * Mathf.Rad2Deg;
        float time = 0f;
        while (time < this.turnTime)
        {
            time = time + Time.deltaTime;
            if (this.playerLook != null)
            {
                this.playerLook.SetYaw(Mathf.LerpAngle(startYaw, targetYaw, time / this.turnTime));
            }
            yield return null;   // 다음 프레임까지 기다림
        }

        // 2) 화면이 붉어졌다가 까맣게
        time = 0f;
        while (time < this.fadeTime)
        {
            time = time + Time.deltaTime;
            float rate = time / this.fadeTime;
            this.redAmount = Mathf.Clamp01(rate * 2f);          // 앞 절반 동안 붉어짐
            this.blackAmount = Mathf.Clamp01(rate * 2f - 1f);   // 뒤 절반 동안 까매짐
            yield return null;
        }

        // 3) 잠깐 까만 화면
        yield return new WaitForSeconds(this.blackTime);

        // 4) 체크포인트를 지났으면 거기서, 아니면 처음부터 (강의 04장: 현재 Scene 다시 로드)
        Checkpoint.isRespawning = Checkpoint.savedOrder > 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 강의 04장 OnGUI: 붉은/검은 화면, "저장됨" 표시
    private void OnGUI()
    {
        if (this.redAmount > 0f)
        {
            GUI.color = new Color(0.5f, 0f, 0f, this.redAmount * 0.7f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), this.whiteTexture);
        }
        if (this.blackAmount > 0f)
        {
            GUI.color = new Color(0f, 0f, 0f, this.blackAmount);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), this.whiteTexture);
        }

        // 체크포인트를 지난 뒤 2초 동안 오른쪽 아래에 "저장됨"
        float ago = Time.time - Checkpoint.lastSaveTime;
        if (Checkpoint.lastSaveShowsMessage && ago >= 0f && ago < 2f)
        {
            GUI.color = new Color(1f, 1f, 1f, 1f - ago / 2f);
            GUI.Label(new Rect(Screen.width - 110f, Screen.height - 40f, 100f, 25f), "저장됨");
        }
    }
}
