using UnityEngine;

// 게임 전체가 같이 쓰는 UI 스타일 (글꼴·색·글자 크기). A·B 모두 OnGUI에서 이걸 가져다 쓴다.
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
    [Tooltip("동해독도 글꼴(본문·제목·경고) 글자 크기 배율. 손글씨 글꼴이라 작아 보여서 키운다. 1 = 그대로. 한자 글꼴에는 안 곱함. Play 중에 바꿔도 바로 보인다")]
    public float mainFontScale = 1.3f;
    [Tooltip("흰색 안내 글자(타이틀 조작법, 인트로 자막, 플레이 중 안내·[E] 문구, 동생·엔딩 자막)만 더 키우는 배율. 1 = 그대로. Play 중에 바꿔도 바로 보인다")]
    public float textScale = 1.25f;

    public static Font regular;   // 강의 07장: public static 변수
    public static Font bold;
    public static Font black;
    public static Font warning;
    public static Font hanja;
    public static float mainScale = 1.3f;
    public static float textScaleNow = 1.25f;

    public static Color white  = new Color(0.86f, 0.85f, 0.82f);
    public static Color yellow = new Color(0.95f, 0.82f, 0.45f);
    public static Color red    = new Color(0.72f, 0.12f, 0.10f);
    public static Color paper  = new Color(0.20f, 0.17f, 0.13f);
    public static Color gray   = new Color(0.55f, 0.58f, 0.64f);
    public static Color shadow = new Color(0f, 0f, 0f, 0.75f);

    private void Awake()   // 강의 05장: Awake
    {
        if (this.regularFont != null) regular = this.regularFont;
        if (this.boldFont != null) bold = this.boldFont;
        if (this.blackFont != null) black = this.blackFont;
        if (this.warningFont != null) warning = this.warningFont;
        if (this.hanjaFont != null) hanja = this.hanjaFont;
        mainScale = this.mainFontScale;
        textScaleNow = this.textScale;
    }

    private void Update()
    {
        mainScale = this.mainFontScale;
        textScaleNow = this.textScale;
    }

    public static int Text(int sizeAt1080)
    {
        return Mathf.RoundToInt(sizeAt1080 * textScaleNow);
    }

    public static int Size(int sizeAt1080)
    {
        return Mathf.Max(8, Mathf.RoundToInt(sizeAt1080 * Screen.height / 1080f));
    }

    public static GUIStyle Make(Font font, int sizeAt1080, Color color, TextAnchor alignment)
    {
        GUIStyle style = new GUIStyle();
        style.font = font;
        bool isMainFont = font != null && (font == regular || font == bold || font == black) && font != hanja;
        float scale = isMainFont ? mainScale : 1f;
        style.fontSize = Size(Mathf.RoundToInt(sizeAt1080 * scale));
        style.normal.textColor = color;
        style.alignment = alignment;
        style.wordWrap = true;
        style.richText = true;
        return style;
    }

    public static GUIStyle Body(int sizeAt1080, Color color)
    {
        return Make(regular, sizeAt1080, color, TextAnchor.UpperLeft);
    }

    public static GUIStyle Strong(int sizeAt1080, Color color)
    {
        return Make(bold != null ? bold : regular, sizeAt1080, color, TextAnchor.MiddleCenter);
    }

    public static GUIStyle Title(int sizeAt1080)
    {
        Font font = black != null ? black : (bold != null ? bold : regular);
        return Make(font, sizeAt1080, red, TextAnchor.MiddleCenter);
    }

    public static GUIStyle Warning(int sizeAt1080)
    {
        if (warning == null)
        {
            return Title(sizeAt1080);
        }
        return Make(warning, sizeAt1080, red, TextAnchor.MiddleCenter);
    }

    public static GUIStyle Hanja(int sizeAt1080, Color color)
    {
        return Make(hanja != null ? hanja : regular, sizeAt1080, color, TextAnchor.MiddleCenter);
    }

    public static void Label(Rect rect, string text, GUIStyle style, float alpha = 1f)
    {
        Color old = GUI.color;
        float outline = Mathf.Max(1f, style.fontSize / 30f);
        float offset = Mathf.Max(2f, style.fontSize / 14f);

        string plain = StripColorTags(text);

        GUI.color = new Color(0f, 0f, 0f, 0.8f * alpha);
        GUI.Label(new Rect(rect.x - outline, rect.y - outline, rect.width, rect.height), plain, style);
        GUI.Label(new Rect(rect.x + outline, rect.y - outline, rect.width, rect.height), plain, style);
        GUI.Label(new Rect(rect.x - outline, rect.y + outline, rect.width, rect.height), plain, style);
        GUI.Label(new Rect(rect.x + outline, rect.y + outline, rect.width, rect.height), plain, style);

        GUI.color = new Color(0f, 0f, 0f, shadow.a * alpha);
        GUI.Label(new Rect(rect.x + offset, rect.y + offset, rect.width, rect.height), plain, style);

        GUI.color = new Color(1f, 1f, 1f, alpha);
        GUI.Label(rect, text, style);

        GUI.color = old;
    }

    private static string StripColorTags(string text)
    {
        if (text.IndexOf("<color") < 0)
        {
            return text;
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

    public static void EdgeShade(Rect rect, Color color, float thickness = 0.3f)
    {
        Color old = GUI.color;
        int steps = 20;
        float total = Mathf.Min(rect.width, rect.height) * thickness;
        float step = total / steps;
        for (int i = 0; i < steps; i++)
        {
            float k = 1f - (float)i / steps;
            GUI.color = new Color(color.r, color.g, color.b, color.a * k * k);
            float inset = step * i;
            Rect r = new Rect(rect.x + inset, rect.y + inset, rect.width - inset * 2f, rect.height - inset * 2f);
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, step), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.yMax - step, r.width, step), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y + step, step, r.height - step * 2f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.xMax - step, r.y + step, step, r.height - step * 2f), Texture2D.whiteTexture);
        }
        GUI.color = old;
    }

    public static void MenuBackdrop(float alpha, Color tint)
    {
        Rect screen = new Rect(0f, 0f, Screen.width, Screen.height);
        Color old = GUI.color;
        GUI.color = new Color(tint.r, tint.g, tint.b, tint.a * alpha);
        GUI.DrawTexture(screen, Texture2D.whiteTexture);
        GUI.color = old;
        EdgeShade(screen, new Color(0f, 0f, 0f, alpha), 0.45f);
    }

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

    public static void WarningText(Rect rect, string text, GUIStyle style, float alpha)
    {
        EdgeShade(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.35f, 0f, 0f, 0.55f * alpha), 0.3f);
        float shake = Size(3);
        Rect shaken = new Rect(rect.x + Random.Range(-shake, shake), rect.y + Random.Range(-shake, shake), rect.width, rect.height);   // 강의 03장: Random.Range
        Label(shaken, text, style, alpha);
    }

    public static Rect PromptRect()
    {
        float height = Size(Text(40));
        return new Rect(0f, Screen.height / 2f + Size(24), Screen.width, height);
    }

    public static void Prompt(string text, float alpha = 1f)
    {
        if (text.StartsWith("["))
        {
            int end = text.IndexOf(']');
            if (end > 0)
            {
                string key = text.Substring(0, end + 1);
                text = "<color=#" + ColorUtility.ToHtmlStringRGB(yellow) + ">" + key + "</color>" + text.Substring(end + 1);
            }
        }
        Label(PromptRect(), text, Make(regular, Text(26), white, TextAnchor.MiddleCenter), alpha);
    }
}
