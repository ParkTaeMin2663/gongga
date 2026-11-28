using UnityEngine;

// 게임 안의 "큰 소리"를 모아 두는 곳. 괴물(집주인)은 눈이 없어서 여기 기록된 소리로만 플레이어를 찾는다.
public class NoiseSystem : MonoBehaviour
{
    [Header("디버그")]
    [Tooltip("체크하면 화면 왼쪽 위에 마지막 소리를 글자로 보여 주고 Console에도 찍는다. 다 만들면 끈다.")]
    public bool showDebug = true;

    public Vector3 LastPosition { get; private set; }

    public float LastLoudness { get; private set; }

    public float LastTime { get; private set; }

    public int NoiseCount { get; private set; }

    private string lastSourceName = "";

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

    public bool CanHear(Vector3 listenerPosition)
    {
        if (this.NoiseCount == 0)
        {
            return false;
        }

        float distance = Vector3.Distance(listenerPosition, this.LastPosition);
        return distance <= this.LastLoudness;
    }

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
