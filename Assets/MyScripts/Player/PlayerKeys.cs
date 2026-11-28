using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어가 가진 열쇠 꾸러미 + 짧은 안내 문구 ("보일러실 열쇠를 얻었다", "잠겨 있다"). (11/20)
///
/// [붙이는 곳] Player 프리팹 (이미 붙어 있음)
/// [쓰는 곳]
///   - ItemPickup(종류 Key)  : 주우면 PlayerKeys.Add("보일러실 열쇠")
///   - DraggableDoor         : 잠긴 문을 잡을 때 PlayerKeys.Has(requiredKey)면 열린다
///   - Checkpoint            : 지나갈 때 PlayerKeys.SaveForCheckpoint() → 죽어도 열쇠를 잃지 않음
///   - 아무 스크립트          : PlayerKeys.ShowMessage("글자") 로 화면 아래에 2초 동안 안내
///
/// [방식] 강의 07장 public static (씬을 다시 불러와도 남는 값 — Checkpoint와 같은 방식),
///        강의 04장 OnGUI + 공통 스타일 UIStyle.
///        열쇠 이름 목록은 C# List&lt;string&gt;(배열처럼 쓰되 크기가 늘어나는 목록)를 쓴다. [강의 외 — 작은 보조 기능]
/// </summary>
public class PlayerKeys : MonoBehaviour
{
    [Header("안내 문구")]
    [Tooltip("글자 크기 (1080p 기준)")]
    public int messageSize = 30;

    // ----- 게임 전체가 공유하는 값 (강의 07장 public static) -----
    /// <summary>지금 가진 열쇠 이름들</summary>
    public static List<string> held = new List<string>();      // [강의 외] List
    /// <summary>마지막 체크포인트를 지날 때 가지고 있던 열쇠들 (죽으면 이걸로 되돌림)</summary>
    public static List<string> saved = new List<string>();

    private static string message = "";
    private static float messageEndTime = -100f;

    private void Awake()
    {
        // PlayerDeath.Start가 isRespawning을 끄기 전에 읽어야 하므로 Awake (강의 05장 실행 순서)
        if (Checkpoint.isRespawning)
        {
            held = new List<string>(saved);   // 체크포인트 때 가진 열쇠로 되돌림
        }
        else
        {
            held.Clear();                     // 새 게임 (처음 시작, 엔딩 뒤 "처음으로")
            saved.Clear();
        }
        message = "";
    }

    /// <summary>열쇠를 얻는다</summary>
    public static void Add(string keyName)
    {
        if (held.Contains(keyName) == false)
        {
            held.Add(keyName);
        }
    }

    /// <summary>이 열쇠를 가졌는지</summary>
    public static bool Has(string keyName)
    {
        return held.Contains(keyName);
    }

    /// <summary>체크포인트가 부른다: 지금 가진 열쇠를 기억</summary>
    public static void SaveForCheckpoint()
    {
        saved = new List<string>(held);
    }

    /// <summary>화면 아래에 잠깐 안내 문구를 띄운다</summary>
    public static void ShowMessage(string text)
    {
        message = text;
        messageEndTime = Time.time + 2.2f;
    }

    // 강의 04장 OnGUI
    private void OnGUI()
    {
        float left = messageEndTime - Time.time;
        if (left <= 0f || message == "")
        {
            return;
        }
        // 마지막 0.5초 동안 서서히 사라짐
        float alpha = Mathf.Clamp01(left / 0.5f);
        int size = UIStyle.Text(this.messageSize);   // 흰 안내 글자 배율
        float height = UIStyle.Size(size) * 2f;
        Rect rect = new Rect(0f, Screen.height * 0.72f, Screen.width, height);
        UIStyle.Label(rect, message, UIStyle.Strong(size, UIStyle.white), alpha);
    }
}
