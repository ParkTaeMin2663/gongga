using UnityEngine;

/// <summary>
/// 1장 조작 안내. 플레이어가 이 구역(Is Trigger)에 처음 들어오면 화면 위쪽에 안내 문구를 잠깐 보여 준다.
///   - 한 번만 보여 준다 (다시 들어와도 안 뜸)
///   - 서서히 나타났다가 showTime초 뒤 서서히 사라진다
///   - 다른 안내가 새로 뜨면 이전 안내는 바로 사라진다 (안내 두 개가 겹치지 않게)
///   - 타이틀 화면이 떠 있는 동안에는 시간이 가지 않는다 (시작하기를 누른 뒤부터 보임)
///   - 죽어서 skipFromCheckpoint번 이상 체크포인트에서 다시 시작하면 안 뜬다 (이미 배운 조작)
///
/// [붙이는 곳] 빈 오브젝트 "Hint_OOO" + Box Collider(Is Trigger 체크, 강의 05·07장)
/// [글자] UIStyle.cs (본명조, 강의 04장 OnGUI). [W] 처럼 대괄호로 감싼 키는 노란색으로 보인다.
/// </summary>
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

    // 강의 07장 public static: 지금 화면에 떠 있는 안내 (새 안내가 뜨면 이전 것은 사라지게)
    public static TutorialHint current;

    private bool isUsed;     // 이미 한 번 보여 줬나
    private float timer;     // 다 나타난 뒤 지난 시간
    private float alpha;     // 0 = 안 보임, 1 = 다 보임
    private bool isShowing;

    private void Start()
    {
        // 이미 지나간 체크포인트에서 다시 시작했으면 안내는 필요 없다
        if (this.skipFromCheckpoint > 0 && Checkpoint.savedOrder >= this.skipFromCheckpoint)
        {
            this.isUsed = true;
        }
    }

    // 강의 05·07장 OnTriggerEnter: 구역에 처음 들어왔다 → 안내 시작
    private void OnTriggerEnter(Collider other)
    {
        if (this.isUsed || other.CompareTag("Player") == false)
        {
            return;
        }
        this.isUsed = true;
        this.isShowing = true;
        this.timer = 0f;
        current = this;   // 이전 안내는 OnGUI에서 current가 아니게 되어 사라진다
    }

    private void Update()
    {
        if (this.isShowing == false)
        {
            return;
        }

        // 타이틀 화면이나 인트로 자막이 떠 있으면 기다린다 (시작 지점 안내가 그 뒤에서 지나가 버리지 않게)
        if (TitleScreen.hasStarted == false || IntroScreen.isPlaying)
        {
            return;
        }

        // 다른 안내가 새로 떴으면 바로 끝
        if (current != this)
        {
            this.isShowing = false;
            this.alpha = 0f;
            return;
        }

        // 강의 03장 Time.deltaTime: 0.5초 동안 나타남 → showTime초 보여 줌 → 1초 동안 사라짐
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

    // 강의 04장 OnGUI: 화면 위쪽 가운데에 안내 문구
    private void OnGUI()
    {
        if (this.isShowing == false || this.alpha <= 0f)
        {
            return;
        }

        int size = UIStyle.Text(this.fontSize);   // 흰 안내 글자 배율
        GUIStyle style = UIStyle.Make(UIStyle.regular, size, UIStyle.white, TextAnchor.MiddleCenter);
        Rect rect = new Rect(0f, Screen.height * 0.10f, Screen.width, UIStyle.Size(size * 3));
        UIStyle.Label(rect, this.ColorKeys(this.message), style, this.alpha);
    }

    /// <summary>"[E] 줍기" → [E] 부분만 노란색으로 (UIStyle.Prompt와 같은 방식, 여러 개도 됨)</summary>
    private string ColorKeys(string text)
    {
        string yellow = "<color=#" + ColorUtility.ToHtmlStringRGB(UIStyle.yellow) + ">";
        return text.Replace("[", yellow + "[").Replace("]", "]</color>");
    }
}
