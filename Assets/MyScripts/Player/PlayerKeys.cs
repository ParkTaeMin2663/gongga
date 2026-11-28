using System.Collections.Generic;
using UnityEngine;

// 플레이어가 가진 열쇠 목록과 짧은 안내 문구("열쇠를 얻었다", "잠겨 있다").
public class PlayerKeys : MonoBehaviour
{
    [Header("안내 문구")]
    [Tooltip("글자 크기 (1080p 기준)")]
    public int messageSize = 30;

    // [강의 외] List<string>: 크기가 늘어나는 열쇠 목록
    public static List<string> held = new List<string>();   // 강의 07장: public static 변수
    public static List<string> saved = new List<string>();

    private static string message = "";
    private static float messageEndTime = -100f;

    private void Awake()   // 강의 05장: Awake
    {
        if (Checkpoint.isRespawning)
        {
            held = new List<string>(saved);
        }
        else
        {
            held.Clear();
            saved.Clear();
        }
        message = "";
    }

    public static void Add(string keyName)
    {
        if (held.Contains(keyName) == false)
        {
            held.Add(keyName);
        }
    }

    public static bool Has(string keyName)
    {
        return held.Contains(keyName);
    }

    public static void SaveForCheckpoint()
    {
        saved = new List<string>(held);
    }

    public static void ShowMessage(string text)
    {
        message = text;
        messageEndTime = Time.time + 2.2f;
    }

    private void OnGUI()   // 강의 04장: OnGUI
    {
        float left = messageEndTime - Time.time;
        if (left <= 0f || message == "")
        {
            return;
        }
        float alpha = Mathf.Clamp01(left / 0.5f);
        int size = UIStyle.Text(this.messageSize);
        float height = UIStyle.Size(size) * 2f;
        Rect rect = new Rect(0f, Screen.height * 0.72f, Screen.width, height);
        UIStyle.Label(rect, message, UIStyle.Strong(size, UIStyle.white), alpha);
    }
}
