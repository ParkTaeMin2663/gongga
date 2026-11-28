using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// 손전등 켜고 끄기(F)와 배터리. 배터리가 적으면 어두워지고 깜빡인다.
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

    public bool IsOn { get { return this.flashlight != null && this.flashlight.enabled; } }

    private float baseIntensity;
    private float flickerTimer;
    private bool isScripted;
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

        this.flashlight.enabled = this.isOnAtStart && this.hasFlashlight && this.battery > 0f;
    }

    private void OnFlashlight(InputValue value)   // 강의 02장: Player Input 메시지 함수 OnFlashlight
    {
        if (this.flashlight == null || this.isScripted || this.hasFlashlight == false)
        {
            return;
        }

        bool turnOn = !this.flashlight.enabled;
        if (turnOn && this.battery <= 0f)
        {
            this.PlayClick();
            return;
        }

        this.flashlight.enabled = turnOn;
        this.PlayClick();
    }

    private void Update()
    {
        if (this.heldModel != null && this.heldModel.activeSelf != this.hasFlashlight)
        {
            this.heldModel.SetActive(this.hasFlashlight);
        }

        if (this.flashlight == null || this.isScripted || this.flashlight.enabled == false)
        {
            return;
        }

        this.battery = Mathf.Max(0f, this.battery - this.drainPerSecond * Time.deltaTime);   // 강의 03장: Time.deltaTime
        if (this.battery <= 0f)
        {
            this.flashlight.enabled = false;
            return;
        }

        float rate = 1f;
        if (this.battery < this.lowBattery)
        {
            rate = Mathf.Lerp(this.lowIntensityRate, 1f, this.battery / this.lowBattery);

            this.flickerTimer = this.flickerTimer - Time.deltaTime;
            if (this.flickerTimer <= 0f)
            {
                this.flickerTimer = Random.Range(0.3f, 2.5f) * (this.battery / this.lowBattery + 0.2f);   // 강의 03장: Random.Range
                rate = rate * Random.Range(0f, 0.3f);
            }
        }
        this.flashlight.intensity = this.baseIntensity * rate;
    }

    public void AddBattery(float amount)
    {
        this.battery = Mathf.Min(100f, this.battery + amount);
    }

    public void GiveFlashlight()
    {
        this.hasFlashlight = true;
        if (this.flashlight != null && this.battery > 0f)
        {
            this.flashlight.enabled = true;
            this.PlayClick();
        }
    }

    public void FlickerOut(float duration)
    {
        if (this.flashlight == null)
        {
            return;
        }
        this.StopAllCoroutines();
        this.StartCoroutine(this.FlickerOutRoutine(duration));   // 강의 05장: 코루틴 StartCoroutine
    }

    private IEnumerator FlickerOutRoutine(float duration)
    {
        this.isScripted = true;
        this.flashlight.enabled = true;

        float time = 0f;
        while (time < duration)
        {
            float progress = time / duration;
            this.flashlight.enabled = Random.Range(0f, 1f) > progress * 0.8f;
            this.flashlight.intensity = this.baseIntensity * Random.Range(0.3f, 1f);
            float wait = Random.Range(0.04f, 0.15f);
            time = time + wait;
            yield return new WaitForSeconds(wait);   // 강의 05장: WaitForSeconds
        }

        this.flashlight.enabled = false;
        this.flashlight.intensity = this.baseIntensity;
        this.isScripted = false;
    }

    private void PlayClick()
    {
        if (this.clickSound != null)
        {
            this.clickSound.Play();   // 강의 03장: AudioSource.Play
        }
    }

    private void OnGUI()   // 강의 04장: OnGUI
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
        GUI.color = Color.white;
    }
}
