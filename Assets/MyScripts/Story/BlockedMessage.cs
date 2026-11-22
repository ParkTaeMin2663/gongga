using UnityEngine;

/// <summary>
/// 투명 벽에 플레이어가 부딪히면 화면 아래에 짧은 안내를 띄운다. (11/22)
/// 예: 2층 연출 중 1층 계단 입구 → "지금은 내려갈 수 없다. 어딘가에 숨어야 한다"
///
/// [붙이는 곳] 투명 벽 오브젝트 (Box Collider만 있고 Mesh Renderer는 없는 빈 오브젝트. Is Trigger 끔 = 실제로 막힘)
/// [방식] 강의 03장 OnCollisionEnter (플레이어는 Rigidbody가 있어서 부딪히면 불린다) + 강의 07장 CompareTag.
///        글자는 PlayerKeys.ShowMessage (열쇠 안내와 같은 자리·같은 글꼴)
/// [괴물] 괴물은 NavMesh로 움직이고 Rigidbody가 Is Kinematic이라 이 벽에 막히지 않는다.
///        Mesh Renderer가 없으면 NavMesh Bake에도 안 들어가서 괴물 길도 그대로다.
/// </summary>
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
