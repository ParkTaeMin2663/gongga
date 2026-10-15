using UnityEngine;
using UnityEngine.InputSystem; // Input System Package를 쓰기 위해 반드시 포함 (강의 03장)

/// <summary>
/// 손전등 켜고 끄기 (F 키).
///
/// [붙이는 곳] Player 오브젝트 (Player Input이 있는 오브젝트에 붙여야 OnFlashlight 메시지를 받는다)
/// [Inspector 연결] Flashlight 칸에 Main Camera의 자식 Spotlight를 드래그
///
/// [설계] 손전등은 Main Camera의 자식 Spotlight다. (강의 02장 Spotlight, 04장 계층구조)
///        자식이라서 카메라가 돌면 손전등도 같이 돌아 → 보는 곳을 비춘다.
///
/// [입력] 강의 04장 "새 Input Action 만들기" 방식
///   - Player 맵에 Flashlight 액션 추가, Path = Keyboard F, Interaction = Press (Press Only)
///   - 액션 이름이 Flashlight라서 Player Input이 OnFlashlight 함수를 불러준다.
/// </summary>
public class PlayerFlashlight : MonoBehaviour
{
    [Header("연결")]
    [Tooltip("Main Camera의 자식으로 만든 Spotlight를 여기에 드래그")]
    public Light flashlight;

    [Header("시작 상태")]
    [Tooltip("체크하면 게임 시작할 때 손전등이 켜진 상태. 1장에서는 관리사무소에서 얻기 전이라 꺼 둔다.")]
    public bool isOnAtStart = false;

    private void Start()
    {
        if (this.flashlight == null)
        {
            Debug.LogError("[PlayerFlashlight] Flashlight 칸이 비어 있습니다. Inspector에서 Spotlight를 드래그해 주세요.");
            return;
        }

        // 시작할 때 켤지 끌지 정한다. enabled = 컴포넌트 켜기/끄기 (Inspector의 체크박스와 같다)
        this.flashlight.enabled = this.isOnAtStart;
    }

    // F를 누른 순간 Player Input이 호출한다.
    private void OnFlashlight(InputValue value)
    {
        if (this.flashlight == null)
        {
            return;
        }

        // 켜져 있으면 끄고, 꺼져 있으면 켠다. (! = 반대로)
        this.flashlight.enabled = !this.flashlight.enabled;
    }
}
