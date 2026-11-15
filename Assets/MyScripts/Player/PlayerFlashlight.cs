using System.Collections;          // 강의 05장: 코루틴
using UnityEngine;
using UnityEngine.InputSystem; // Input System Package를 쓰기 위해 반드시 포함 (강의 03장)

/// <summary>
/// 손전등 켜고 끄기 (F 키) + 배터리.
///
/// [붙이는 곳] Player 오브젝트 (Player Input이 있는 오브젝트에 붙여야 OnFlashlight 메시지를 받는다)
/// [Inspector 연결] Flashlight 칸에 Main Camera의 자식 Spotlight를 드래그
///
/// [설계] 손전등은 Main Camera의 자식 Spotlight다. (강의 02장 Spotlight, 04장 계층구조)
///        자식이라서 카메라가 돌면 손전등도 같이 돌아 → 보는 곳을 비춘다.
///
/// [입력] 강의 04장 "새 Input Action 만들기" 방식
///   - Player 맵에 Flashlight 액션 추가, Path = Keyboard F, Interaction = Press (Press Only)
///   - 액션 이름이 Flashlight라서 Player Input이 OnFlashlight 함수를 불러준다.
///
/// [배터리]
///   - 켜져 있는 동안 drainPerSecond씩 줄어든다 (강의 03장 Time.deltaTime). 0이면 꺼지고 다시 못 켠다.
///   - lowBattery 아래로 내려가면 빛이 약해지고 가끔 깜빡인다 (강의 03장 Random.Range).
///   - 건전지(BatteryPickup)를 주우면 AddBattery()로 충전.
/// [연출용] FlickerOut(초): 깜빡깜빡하다가 꺼진다 (B의 2층 연출에서 부른다). 강의 05장 코루틴.
/// </summary>
public class PlayerFlashlight : MonoBehaviour
{
    [Header("연결")]
    [Tooltip("Main Camera의 자식으로 만든 Spotlight를 여기에 드래그")]
    public Light flashlight;

    [Tooltip("왼손에 든 손전등 모델. 손전등을 얻기 전에는 숨긴다 (Main Camera > HeldFlashlight 안의 모델)")]
    public GameObject heldModel;

    [Header("시작 상태")]
    [Tooltip("체크하면 게임 시작할 때 손전등이 켜진 상태. 1장에서는 관리사무소에서 얻기 전이라 꺼 둔다.")]
    public bool isOnAtStart = false;

    [Tooltip("손전등을 가지고 있는지. 끄면 F를 눌러도 안 켜진다 (관리사무소에서 얻을 때 HasFlashlight = true)")]
    public bool hasFlashlight = true;

    [Header("배터리 (0~100)")]
    [Range(0f, 100f)] public float battery = 100f;
    [Tooltip("켜져 있을 때 1초에 줄어드는 양. 0.5면 100 → 0까지 200초(3분 20초)")]
    public float drainPerSecond = 0.5f;
    [Tooltip("이 값 아래면 빛이 약해지고 깜빡인다")]
    public float lowBattery = 20f;
    [Tooltip("배터리가 바닥에 가까울 때 밝기 비율 (1 = 원래 밝기)")]
    [Range(0f, 1f)] public float lowIntensityRate = 0.45f;

    [Header("소리 (없어도 됨)")]
    [Tooltip("켜고 끌 때 딸깍. Player에 Audio Source를 하나 더 붙이고 드래그")]
    public AudioSource clickSound;

    [Header("화면 표시")]
    [Tooltip("켜져 있을 때 왼쪽 아래에 배터리 막대를 보여 준다")]
    public bool showBatteryBar = true;

    /// <summary>지금 켜져 있는지</summary>
    public bool IsOn { get { return this.flashlight != null && this.flashlight.enabled; } }

    private float baseIntensity;     // 처음 밝기 (Start에서 저장)
    private float flickerTimer;      // 다음 깜빡임까지
    private bool isScripted;         // FlickerOut 연출 중이면 true (플레이어 입력·배터리 깜빡임 무시)
    private Texture2D whiteTexture;

    private void Start()
    {
        this.whiteTexture = Texture2D.whiteTexture;

        if (this.flashlight == null)
        {
            Debug.LogError("[PlayerFlashlight] Flashlight 칸이 비어 있습니다. Inspector에서 Spotlight를 드래그해 주세요.");
            return;
        }

        this.baseIntensity = this.flashlight.intensity;

        // 시작할 때 켤지 끌지 정한다. enabled = 컴포넌트 켜기/끄기 (Inspector의 체크박스와 같다)
        this.flashlight.enabled = this.isOnAtStart && this.hasFlashlight && this.battery > 0f;
    }

    // F를 누른 순간 Player Input이 호출한다.
    private void OnFlashlight(InputValue value)
    {
        if (this.flashlight == null || this.isScripted || this.hasFlashlight == false)
        {
            return;
        }

        // 배터리가 없으면 켜지지 않는다 (딸깍 소리만)
        bool turnOn = !this.flashlight.enabled;
        if (turnOn && this.battery <= 0f)
        {
            this.PlayClick();
            return;
        }

        // 켜져 있으면 끄고, 꺼져 있으면 켠다. (! = 반대로)
        this.flashlight.enabled = turnOn;
        this.PlayClick();
    }

    private void Update()
    {
        // 손전등을 가졌을 때만 왼손에 보인다 (강의 04장: SetActive로 오브젝트 켜고 끄기)
        if (this.heldModel != null && this.heldModel.activeSelf != this.hasFlashlight)
        {
            this.heldModel.SetActive(this.hasFlashlight);
        }

        if (this.flashlight == null || this.isScripted || this.flashlight.enabled == false)
        {
            return;
        }

        // 1) 배터리 소모
        this.battery = Mathf.Max(0f, this.battery - this.drainPerSecond * Time.deltaTime);
        if (this.battery <= 0f)
        {
            this.flashlight.enabled = false;
            return;
        }

        // 2) 밝기: lowBattery 아래에서 점점 어두워진다
        float rate = 1f;
        if (this.battery < this.lowBattery)
        {
            rate = Mathf.Lerp(this.lowIntensityRate, 1f, this.battery / this.lowBattery);

            // 3) 가끔 짧게 깜빡 (강의 03장 Random.Range)
            this.flickerTimer = this.flickerTimer - Time.deltaTime;
            if (this.flickerTimer <= 0f)
            {
                this.flickerTimer = Random.Range(0.3f, 2.5f) * (this.battery / this.lowBattery + 0.2f);
                rate = rate * Random.Range(0f, 0.3f);   // 이번 프레임만 거의 꺼짐
            }
        }
        this.flashlight.intensity = this.baseIntensity * rate;
    }

    /// <summary>건전지를 주웠을 때. amount만큼 충전 (최대 100)</summary>
    public void AddBattery(float amount)
    {
        this.battery = Mathf.Min(100f, this.battery + amount);
    }

    /// <summary>손전등을 얻었을 때 (관리사무소 등). 바로 켜진다.</summary>
    public void GiveFlashlight()
    {
        this.hasFlashlight = true;
        if (this.flashlight != null && this.battery > 0f)
        {
            this.flashlight.enabled = true;
            this.PlayClick();
        }
    }

    /// <summary>
    /// 연출용: duration초 동안 불규칙하게 깜빡이다가 꺼진다. 배터리는 줄지 않는다.
    /// 꺼진 뒤에는 플레이어가 F로 다시 켤 수 있다. (예: 2층 실루엣 등장 때)
    /// </summary>
    public void FlickerOut(float duration)
    {
        if (this.flashlight == null)
        {
            return;
        }
        this.StopAllCoroutines();
        this.StartCoroutine(this.FlickerOutRoutine(duration));   // 강의 05장
    }

    private IEnumerator FlickerOutRoutine(float duration)
    {
        this.isScripted = true;
        this.flashlight.enabled = true;

        float time = 0f;
        while (time < duration)
        {
            // 켜졌다 꺼졌다를 짧게 반복. 끝으로 갈수록 꺼져 있는 시간이 길어진다
            float progress = time / duration;
            this.flashlight.enabled = Random.Range(0f, 1f) > progress * 0.8f;
            this.flashlight.intensity = this.baseIntensity * Random.Range(0.3f, 1f);
            float wait = Random.Range(0.04f, 0.15f);
            time = time + wait;
            yield return new WaitForSeconds(wait);
        }

        this.flashlight.enabled = false;
        this.flashlight.intensity = this.baseIntensity;
        this.isScripted = false;
    }

    private void PlayClick()
    {
        if (this.clickSound != null)
        {
            this.clickSound.Play();   // 강의 03장
        }
    }

    // 강의 04장 OnGUI: 켜져 있거나 배터리가 적을 때 왼쪽 아래에 배터리 막대
    private void OnGUI()
    {
        if (this.showBatteryBar == false || this.hasFlashlight == false)
        {
            return;
        }
        if (this.IsOn == false && this.battery >= this.lowBattery)
        {
            return;
        }

        float rate = this.battery / 100f;
        Rect back = new Rect(20f, Screen.height - 34f, 90f, 10f);
        GUI.color = new Color(1f, 1f, 1f, 0.25f);
        GUI.DrawTexture(back, this.whiteTexture);
        GUI.color = this.battery < this.lowBattery ? new Color(0.9f, 0.25f, 0.2f, 0.9f) : new Color(1f, 0.85f, 0.5f, 0.8f);
        GUI.DrawTexture(new Rect(back.x, back.y, back.width * rate, back.height), this.whiteTexture);
    }
}
