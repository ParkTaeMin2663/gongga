using UnityEngine;

// 단서 물건 (일기장, 사진, 카세트테이프). 바라보고 E를 누르면 ClueReader가 읽기 화면을 띄운다.
public class ClueItem : MonoBehaviour
{
    public enum ClueType { Diary, Photo, Cassette }

    [Header("단서 내용")]
    [Tooltip("무슨 단서인지")]
    public ClueType type = ClueType.Diary;

    [Tooltip("화면 위쪽 제목 (예: 302호 일기장 — 1994년 12월)")]
    public string title = "일기장";

    [Tooltip("한 칸 = 한 장. E를 누르면 다음 장으로 넘어간다")]
    [TextArea(3, 10)]
    public string[] pages;

    [Tooltip("사진 단서일 때 보여 줄 그림 (Texture). 사진이 아니면 비워 둠")]
    public Texture2D image;

    [Tooltip("카세트 단서일 때 틀 소리 파일. 아니면 비워 둠")]
    public AudioClip audioClip;

    [Header("한 번 읽은 뒤")]
    [Tooltip("체크하면 읽은 뒤 사라진다 (주워 간 것처럼). 보통은 끔")]
    public bool removeAfterRead = false;

    public bool IsRead { get; set; }

    public string ActionName
    {
        get
        {
            if (this.type == ClueType.Photo) return "보기";
            if (this.type == ClueType.Cassette) return "듣기";
            return "읽기";
        }
    }
}
