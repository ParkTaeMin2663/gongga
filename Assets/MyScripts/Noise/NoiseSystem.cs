using UnityEngine;

/// <summary>
/// 게임 안의 "큰 소리"를 모아 두는 곳. 괴물(집주인)은 눈이 없어서 여기 기록된 소리로만 플레이어를 찾는다.
///
/// [붙이는 곳] 씬에 빈 오브젝트 하나 (이름은 반드시 "NoiseSystem"). 씬마다 1개만 둔다.
///
/// [소리 내는 쪽] 강의 04장: GameObject.Find()는 Start()에서 한 번만.
///     private NoiseSystem noiseSystem;
///     void Start() {
///         GameObject obj = GameObject.Find("NoiseSystem");
///         if (obj != null) this.noiseSystem = obj.GetComponent&lt;NoiseSystem&gt;();
///     }
///     // 소리가 날 때
///     if (this.noiseSystem != null) this.noiseSystem.Emit(위치, 크기, "이름");
///
/// [듣는 쪽 — 괴물] Update()에서 NoiseCount가 지난번과 달라졌으면 새 소리가 난 것.
///     CanHear(괴물 위치)가 true면 LastPosition으로 확인하러 간다.
///
/// [크기(loudness)] = 소리가 들리는 거리(m). 8이면 8m 안에 있는 괴물만 듣는다.
///     참고값: 달리기 6 / 물건 떨어짐 8 / 차단기 15 / 셔터·센서등 20 (B 연출)
/// </summary>
public class NoiseSystem : MonoBehaviour
{
    [Header("디버그")]
    [Tooltip("체크하면 화면 왼쪽 위에 마지막 소리를 글자로 보여 주고 Console에도 찍는다. 다 만들면 끈다.")]
    public bool showDebug = true;

    /// <summary>마지막 소리가 난 위치</summary>
    public Vector3 LastPosition { get; private set; }

    /// <summary>마지막 소리의 크기 = 들리는 거리(m)</summary>
    public float LastLoudness { get; private set; }

    /// <summary>마지막 소리가 난 시각 (Time.time, 게임 시작 후 몇 초)</summary>
    public float LastTime { get; private set; }

    /// <summary>지금까지 난 소리 개수. 듣는 쪽은 이 숫자가 바뀌었는지로 "새 소리"를 알아챈다.</summary>
    public int NoiseCount { get; private set; }

    // 디버그 표시용 소리 이름
    private string lastSourceName = "";

    /// <summary>
    /// 소리를 낸다. position = 소리 난 곳, loudness = 들리는 거리(m), sourceName = 디버그용 이름
    /// </summary>
    public void Emit(Vector3 position, float loudness, string sourceName)
    {
        this.LastPosition = position;
        this.LastLoudness = loudness;
        this.LastTime = Time.time;
        this.NoiseCount = this.NoiseCount + 1;
        this.lastSourceName = sourceName;

        if (this.showDebug)
        {
            Debug.Log("[소리] " + sourceName + " / 크기 " + loudness + "m / 위치 " + position);
        }
    }

    /// <summary>
    /// listenerPosition(괴물 위치)에서 마지막 소리가 들리는지. 소리 크기(m)보다 가까우면 들린다.
    /// </summary>
    public bool CanHear(Vector3 listenerPosition)
    {
        if (this.NoiseCount == 0)
        {
            return false;
        }

        float distance = Vector3.Distance(listenerPosition, this.LastPosition);
        return distance <= this.LastLoudness;
    }

    // 강의 04장 OnGUI: 마지막 소리를 화면에 표시 (테스트용)
    private void OnGUI()
    {
        if (this.showDebug == false || this.NoiseCount == 0)
        {
            return;
        }

        float ago = Time.time - this.LastTime;
        GUI.color = Color.yellow;
        GUI.Label(new Rect(10, 10, 500, 25),
            "소리 #" + this.NoiseCount + " : " + this.lastSourceName + " (" + this.LastLoudness + "m) " + ago.ToString("0.0") + "초 전");
    }
}
