using UnityEngine;

/// <summary>
/// 게임 전체가 같이 쓰는 UI 스타일 (글꼴·색·글자 크기). A·B 모두 OnGUI에서 이걸 가져다 쓴다.
///
/// [붙이는 곳] Player 프리팹 (이미 붙어 있음, 글꼴 3개도 연결됨) → Player가 있는 씬이면 어디서나 사용 가능
/// [글꼴] (11/24 변경) 본문·강조·제목 = East Sea Dokdo(동해독도), 경고 문구 = Black Han Sans(검은고딕),
///        한자(空家) = Noto Serif KR(본명조, 동해독도에 한자가 없어서) — 모두 Assets/Fonts, SIL OFL 1.1 (Credits.txt)
///
/// [방식] 강의 07장 public static: Awake에서 글꼴을 static 변수에 넣어 두면
///        다른 스크립트가 UIStyle.Prompt(...)처럼 오브젝트를 찾지 않고 바로 쓸 수 있다.
///        강의 05장: Awake는 Start보다 먼저 불리므로 다른 스크립트의 OnGUI보다 항상 먼저 준비된다.
///
/// [사용 예] (OnGUI 안에서)
///   UIStyle.Prompt("[E] 일기장 읽기");                         // 화면 가운데 아래 안내 문구
///   UIStyle.Label(rect, "숨어라", UIStyle.Warning(40));     // 원하는 곳에 그림자 있는 글자
///   GUIStyle s = UIStyle.Body(22, UIStyle.paper);              // 스타일만 받아서 GUI.Label에 직접
/// </summary>
public class UIStyle : MonoBehaviour
{
    [Header("글꼴 (Assets/Fonts)")]
    [Tooltip("본문용: 단서 글, 안내 문구")]
    public Font regularFont;
    [Tooltip("강조용: 버튼, 단서 제목")]
    public Font boldFont;
    [Tooltip("제목용: 타이틀 「공가」, 단서 제목, 엔딩 제목")]
    public Font blackFont;
    [Tooltip("경고 문구용: \"숨어라\", \"잡혔다\", \"도망쳐\" (비워 두면 제목 글꼴)")]
    public Font warningFont;
    [Tooltip("한자용: 타이틀·엔딩의 \"空家\" (비워 두면 본문 글꼴)")]
    public Font hanjaFont;

    [Header("글자 크기 (11/24)")]
    [Tooltip("본문·강조·제목 글꼴(동해독도) 글자 크기 배율. 손글씨 글꼴이라 작아 보여서 키운다. 1 = 그대로. 경고(검은고딕)·한자 글꼴에는 안 곱함")]
    public float mainFontScale = 1.3f;

    // ----- 모두가 쓰는 값 (강의 07장 public static) -----
    public static Font regular;
    public static Font bold;
    public static Font black;
    public static Font warning;
    public static Font hanja;
    public static float mainScale = 1.3f;

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
        if (this.warningFont != null) warning = this.warningFont;
        if (this.hanjaFont != null) hanja = this.hanjaFont;
        mainScale = this.mainFontScale;
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
        // (11/24) 본문·강조·제목 글꼴이면 배율을 곱한다 (경고·한자 글꼴은 그대로)
        bool isMainFont = font != null && (font == regular || font == bold || font == black) && font != warning && font != hanja;
        float scale = isMainFont ? mainScale : 1f;
        style.fontSize = Size(Mathf.RoundToInt(sizeAt1080 * scale));
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

    /// <summary>경고 문구 (검은고딕, 빨간색, 가운데 정렬) — "숨어라", "잡혔다", "도망쳐"</summary>
    public static GUIStyle Warning(int sizeAt1080)
    {
        if (warning == null)
        {
            return Title(sizeAt1080);   // 경고 글꼴이 없으면 예전처럼 제목 글꼴
        }
        return Make(warning, sizeAt1080, red, TextAnchor.MiddleCenter);
    }

    /// <summary>한자 글자 ("空家"). 한자 글꼴이 없으면 본문 글꼴</summary>
    public static GUIStyle Hanja(int sizeAt1080, Color color)
    {
        return Make(hanja != null ? hanja : regular, sizeAt1080, color, TextAnchor.MiddleCenter);
    }

    /// <summary>
    /// 그림자 있는 글자. 밝은 곳(눈, 손전등 불빛)이나 가는 손글씨 글꼴(동해독도)에서도 읽히게
    /// ① 글자 둘레에 얇은 검은 테두리(상하좌우 대각선 4방향) ② 오른쪽 아래로 진한 그림자를 먼저 그리고 그 위에 글자를 그린다. (11/24 강화)
    /// alpha로 서서히 나타나기/사라지기 (0~1).
    /// </summary>
    public static void Label(Rect rect, string text, GUIStyle style, float alpha = 1f)
    {
        Color old = GUI.color;
        float outline = Mathf.Max(1f, style.fontSize / 30f);   // 테두리 두께
        float offset = Mathf.Max(2f, style.fontSize / 14f);    // 그림자가 떨어진 거리

        // GUI.color는 글자 색에 곱해진다 → 검은색을 곱하면 어떤 글자든 그림자가 된다
        // (11/24 A) 단, <color=#..>로 색을 정한 부분([E] 같은 노란 키)은 검은색이 안 곱해져서 테두리·그림자까지 노랗게 번졌다
        //           → 테두리·그림자에는 색 태그를 뺀 글을 쓴다 (글자 모양은 같고 색만 기본색 → 검게 곱해짐)
        string plain = StripColorTags(text);

        // ① 테두리: 4방향 대각선으로 조금씩 밀어서 그림
        GUI.color = new Color(0f, 0f, 0f, 0.8f * alpha);
        GUI.Label(new Rect(rect.x - outline, rect.y - outline, rect.width, rect.height), plain, style);
        GUI.Label(new Rect(rect.x + outline, rect.y - outline, rect.width, rect.height), plain, style);
        GUI.Label(new Rect(rect.x - outline, rect.y + outline, rect.width, rect.height), plain, style);
        GUI.Label(new Rect(rect.x + outline, rect.y + outline, rect.width, rect.height), plain, style);

        // ② 오른쪽 아래 그림자
        GUI.color = new Color(0f, 0f, 0f, shadow.a * alpha);
        GUI.Label(new Rect(rect.x + offset, rect.y + offset, rect.width, rect.height), plain, style);

        // ③ 원래 글자
        GUI.color = new Color(1f, 1f, 1f, alpha);
        GUI.Label(rect, text, style);

        GUI.color = old;
    }

    /// <summary>(11/24 A) "&lt;color=#FFCC00&gt;[E]&lt;/color&gt; 줍기" → "[E] 줍기" (색 태그만 뺀다. 문자열 IndexOf·Substring)</summary>
    private static string StripColorTags(string text)
    {
        if (text.IndexOf("<color") < 0)
        {
            return text;   // 색 태그가 없으면 그대로
        }
        string result = text.Replace("</color>", "");
        int start = result.IndexOf("<color");
        while (start >= 0)
        {
            int end = result.IndexOf('>', start);
            if (end < 0)
            {
                break;
            }
            result = result.Substring(0, start) + result.Substring(end + 1);
            start = result.IndexOf("<color");
        }
        return result;
    }

    // ---------------- (11/26 A) 화면 효과: 흰 텍스처(Texture2D.whiteTexture)를 색·투명도만 바꿔 여러 번 그린다 ----------------

    /// <summary>
    /// rect 가장자리를 안쪽으로 갈수록 옅게 칠한다 (비네트). 가장자리 = color, 안쪽으로 갈수록 투명.
    /// thickness = 짧은 변에 대한 번짐 두께 비율 (0~0.5)
    /// </summary>
    public static void EdgeShade(Rect rect, Color color, float thickness = 0.3f)
    {
        Color old = GUI.color;
        int steps = 20;
        float total = Mathf.Min(rect.width, rect.height) * thickness;
        float step = total / steps;
        for (int i = 0; i < steps; i++)
        {
            float k = 1f - (float)i / steps;   // 가장자리 1 → 안쪽 0
            GUI.color = new Color(color.r, color.g, color.b, color.a * k * k);
            float inset = step * i;
            Rect r = new Rect(rect.x + inset, rect.y + inset, rect.width - inset * 2f, rect.height - inset * 2f);
            // 겹치지 않는 네 변 띠 (위·아래·왼쪽·오른쪽)
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, step), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.yMax - step, r.width, step), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y + step, step, r.height - step * 2f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.xMax - step, r.y + step, step, r.height - step * 2f), Texture2D.whiteTexture);
        }
        GUI.color = old;
    }

    /// <summary>
    /// 메뉴 화면 배경 (사망·엔딩): 까만 화면 위에 가운데만 아주 옅게 색(tint)을 깔고, 가장자리는 다시 까맣게 → 비네트.
    /// 완전히 까만 화면에는 비네트가 안 보이므로 가운데를 살짝 밝힌다.
    /// </summary>
    public static void MenuBackdrop(float alpha, Color tint)
    {
        Rect screen = new Rect(0f, 0f, Screen.width, Screen.height);
        Color old = GUI.color;
        GUI.color = new Color(tint.r, tint.g, tint.b, tint.a * alpha);
        GUI.DrawTexture(screen, Texture2D.whiteTexture);
        GUI.color = old;
        EdgeShade(screen, new Color(0f, 0f, 0f, alpha), 0.45f);
    }

    /// <summary>테두리 선 (두께 thickness 픽셀)</summary>
    public static void Frame(Rect rect, Color color, float thickness)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), Texture2D.whiteTexture);
        GUI.color = old;
    }

    /// <summary>
    /// 경고 문구 연출 ("숨어라"·"도망쳐"): 화면 가장자리가 붉게 물들고, 글자가 살짝 떨린다.
    /// rect = 글자 자리, alpha = 깜빡임·사라짐 투명도
    /// </summary>
    public static void WarningText(Rect rect, string text, GUIStyle style, float alpha)
    {
        EdgeShade(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.35f, 0f, 0f, 0.55f * alpha), 0.3f);
        float shake = Size(3);
        Rect shaken = new Rect(rect.x + Random.Range(-shake, shake), rect.y + Random.Range(-shake, shake), rect.width, rect.height);   // 강의 03장 Random.Range
        Label(shaken, text, style, alpha);
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
