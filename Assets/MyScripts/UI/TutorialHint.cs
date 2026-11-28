using UnityEngine;

// 1장 조작 안내. 플레이어가 이 구역(Is Trigger)에 처음 들어오면 화면 위쪽에 안내 문구를 잠깐 보여 준다.
public class TutorialHint : MonoBehaviour
{
    [Header("안내 문구")]
    [Tooltip("화면에 보일 글. [W A S D]처럼 대괄호로 감싼 부분은 노란색. 줄바꿈 가능")]
    [TextArea(2, 4)]
    public string message = "[W A S D] 이동    [Shift] 달리기";

    [Tooltip("다 나타난 뒤 보여 주는 시간(초)")]
    public float showTime = 6f;

    [Tooltip("글자 크기 (1080p 기준)")]
    public int fontSize = 30;

    [Header("다시 시작")]
    [Tooltip("이 번호 이상의 체크포인트를 지났으면 안 보여 준다 (2 = 2층 체크포인트). 0이면 항상 보여 줌")]
    public int skipFromCheckpoint = 2;

    public static TutorialHint current;

    private bool isUsed;
    private float timer;
    private float alpha;
    private bool isShowing;

    private void Start()
    {
        if (this.skipFromCheckpoint > 0 && Checkpoint.savedOrder >= this.skipFromCheckpoint)
        {
            this.isUsed = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (this.isUsed || other.CompareTag("Player") == false)
        {
            return;
        }
        this.isUsed = true;
        this.isShowing = true;
        this.timer = 0f;
        current = this;
    }

    private void Update()
    {
        if (this.isShowing == false)
        {
            return;
        }

        if (TitleScreen.hasStarted == false || IntroScreen.isPlaying)
        {
            return;
        }

        if (current != this)
        {
            this.isShowing = false;
            this.alpha = 0f;
            return;
        }

        if (this.timer < this.showTime)
        {
            this.alpha = Mathf.MoveTowards(this.alpha, 1f, Time.deltaTime * 2f);
            if (this.alpha >= 1f)
            {
                this.timer += Time.deltaTime;
            }
        }
        else
        {
            this.alpha = Mathf.MoveTowards(this.alpha, 0f, Time.deltaTime);
            if (this.alpha <= 0f)
            {
                this.isShowing = false;
                current = null;
            }
        }
    }

    private void OnGUI()
    {
        if (this.isShowing == false || this.alpha <= 0f)
        {
            return;
        }

        int size = UIStyle.Text(this.fontSize);
        GUIStyle style = UIStyle.Make(UIStyle.regular, size, UIStyle.white, TextAnchor.MiddleCenter);
        Rect rect = new Rect(0f, Screen.height * 0.10f, Screen.width, UIStyle.Size(size * 3));
        UIStyle.Label(rect, this.ColorKeys(this.message), style, this.alpha);
    }

    private string ColorKeys(string text)
    {
        string yellow = "<color=#" + ColorUtility.ToHtmlStringRGB(UIStyle.yellow) + ">";
        return text.Replace("[", yellow + "[").Replace("]", "]</color>");
    }
}
