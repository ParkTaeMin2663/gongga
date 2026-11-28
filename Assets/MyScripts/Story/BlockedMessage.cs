using UnityEngine;

// 투명 벽에 플레이어가 부딪히면 화면 아래에 짧은 안내를 띄운다.
public class BlockedMessage : MonoBehaviour
{
    [Tooltip("부딪혔을 때 띄울 글")]
    public string message = "지금은 내려갈 수 없다. 어딘가에 숨어야 한다";

    [Tooltip("같은 글을 다시 띄우기까지 기다리는 시간(초)")]
    public float cooldown = 3f;

    private float lastTime = -100f;

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.CompareTag("Player") == false)
        {
            return;
        }
        if (Time.time - this.lastTime < this.cooldown)
        {
            return;
        }
        this.lastTime = Time.time;
        PlayerKeys.ShowMessage(this.message);
    }
}
