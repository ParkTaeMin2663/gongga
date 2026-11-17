using UnityEngine;

/// <summary>
/// 게임 전체가 같이 쓰는 UI 스타일 (글꼴·색·글자 크기). A·B 모두 OnGUI에서 이걸 가져다 쓴다.
///
/// [붙이는 곳] Player 프리팹 (이미 붙어 있음, 글꼴 3개도 연결됨) → Player가 있는 씬이면 어디서나 사용 가능
/// [글꼴] Noto Serif KR(본명조) Regular / Bold / Black — Assets/Fonts, SIL OFL 1.1 (Credits.txt)
///
/// [방식] 강의 07장 public static: Awake에서 글꼴을 static 변수에 넣어 두면
///        다른 스크립트가 UIStyle.Prompt(...)처럼 오브젝트를 찾지 않고 바로 쓸 수 있다.
///        강의 05장: Awake는 Start보다 먼저 불리므로 다른 스크립트의 OnGUI보다 항상 먼저 준비된다.
///
/// [사용 예] (OnGUI 안에서)
///   UIStyle.Prompt("[E] 일기장 읽기");                         // 화면 가운데 아래 안내 문구
///   UIStyle.Label(rect, "숨어라", UIStyle.Title(40), UIStyle.red); // 원하는 곳에 그림자 있는 글자
///   GUIStyle s = UIStyle.Body(22, UIStyle.paper);              // 스타일만 받아서 GUI.Label에 직접
/// </summary>
public class UIStyle : MonoBehaviour
{
    [Header("글꼴 (Assets/Fonts)")]
    [Tooltip("본문용: 단서 글, 안내 문구")]
    public Font regularFont;
    [Tooltip("강조용: 버튼, 단서 제목")]
    public Font boldFont;
    [Tooltip("제목용: 타이틀 「공가」, \"숨어라\", 사망 화면")]
    public Font blackFont;

    // ----- 모두가 쓰는 값 (강의 07장 public static) -----
    public static Font regular;
    public static Font bold;
    public static Font black;

    // 색: 차가운 회색/남색 바탕에 빨간 "공가"와 노란 불빛만 튀게 (CLAUDE.md 4장 분위기)
    public static Color white  = new Color(0.86f, 0.85f, 0.82f);        // 기본 글자 (살짝 누런 흰색)
    public static Color yellow = new Color(0.95f, 0.82f, 0.45f);        // 안내·강조 (302호 불빛)
    public static Color red    = new Color(0.72f, 0.12f, 0.10f);        // 경고·제목 (스프레이)
    public static Color paper  = new Color(0.20f, 0.17f, 0.13f);        // 종이 위 글자 (진한 갈색)
    public static Color gray   = new Color(0.55f, 0.58f, 0.64f);        // 흐린 보조 글자
    public static Color shadow = new Color(0f, 0f, 0f, 0.75f);          // 글자 그림자

    private void Awake()
    {
        if (this.regularFont != null) regular = this.regularFont;
        if (this.boldFont != null) bold = this.boldFont;
        if (this.blackFont != null) black = this.blackFont;
    }

    /// <summary>화면 크기에 맞춘 글자 크기. 1080p 기준 크기를 넣으면 화면 높이에 비례해서 커지거나 작아진다.</summary>
    public static int Size(int sizeAt1080)
    {
        return Mathf.Max(8, Mathf.RoundToInt(sizeAt1080 * Screen.height / 1080f));
    }

    /// <summary>글꼴·크기·색·정렬로 GUIStyle 만들기. 글꼴이 없으면(UIStyle이 없는 씬) 기본 글꼴.</summary>
    public static GUIStyle Make(Font font, int sizeAt1080, Color color, TextAnchor alignment)
    {
        GUIStyle style = new GUIStyle();
        style.font = font;                    // null이면 Unity 기본 글꼴
        style.fontSize = Size(sizeAt1080);
        style.normal.textColor = color;
        style.alignment = alignment;
        style.wordWrap = true;
        style.richText = true;                // <b>, <color=#..> 사용 가능
        return style;
    }

    /// <summary>본문 (Regular, 가운데 정렬 아님 — 단서 글처럼 왼쪽 위부터)</summary>
    public static GUIStyle Body(int sizeAt1080, Color color)
    {
        return Make(regular, sizeAt1080, color, TextAnchor.UpperLeft);
    }

    /// <summary>강조 (Bold, 가운데 정렬)</summary>
    public static GUIStyle Strong(int sizeAt1080, Color color)
    {
        return Make(bold != null ? bold : regular, sizeAt1080, color, TextAnchor.MiddleCenter);
    }

    /// <summary>제목 (Black, 빨간색, 가운데 정렬)</summary>
    public static GUIStyle Title(int sizeAt1080)
    {
        Font font = black != null ? black : (bold != null ? bold : regular);
        return Make(font, sizeAt1080, red, TextAnchor.MiddleCenter);
    }

    /// <summary>
    /// 그림자 있는 글자. 밝은 곳(눈, 손전등 불빛)에서도 읽히게 1px 아래 오른쪽에 검은 글자를 먼저 그린다.
    /// alpha로 서서히 나타나기/사라지기 (0~1).
    /// </summary>
    public static void Label(Rect rect, string text, GUIStyle style, float alpha = 1f)
    {
        Color old = GUI.color;
        float offset = Mathf.Max(1f, style.fontSize / 18f);

        // GUI.color는 글자 색에 곱해진다 → 검은색을 곱하면 어떤 글자든 그림자가 된다
        GUI.color = new Color(0f, 0f, 0f, shadow.a * alpha);
        GUI.Label(new Rect(rect.x + offset, rect.y + offset, rect.width, rect.height), text, style);

        GUI.color = new Color(1f, 1f, 1f, alpha);
        GUI.Label(rect, text, style);

        GUI.color = old;
    }

    /// <summary>화면 가운데 조준점 바로 아래 안내 문구가 들어갈 자리</summary>
    public static Rect PromptRect()
    {
        float height = Size(40);
        return new Rect(0f, Screen.height / 2f + Size(24), Screen.width, height);
    }

    /// <summary>"[E] 손전등 줍기" 같은 안내 문구 (조준점 아래 가운데). [E] 부분은 노란색.</summary>
    public static void Prompt(string text, float alpha = 1f)
    {
        // "[E] 줍기" → "<color=노란색>[E]</color> 줍기"
        if (text.StartsWith("["))
        {
            int end = text.IndexOf(']');
            if (end > 0)
            {
                string key = text.Substring(0, end + 1);
                text = "<color=#" + ColorUtility.ToHtmlStringRGB(yellow) + ">" + key + "</color>" + text.Substring(end + 1);
            }
        }
        Label(PromptRect(), text, Make(regular, 26, white, TextAnchor.MiddleCenter), alpha);
    }
}
