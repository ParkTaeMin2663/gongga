using UnityEngine;
using UnityEngine.InputSystem; // Input System Package (강의 03장)

/// <summary>
/// 바라보는 물건(ItemPickup)을 E로 줍는다. 화면 가운데 아래에 "[E] 손전등 줍기"를 보여 준다.
///
/// [붙이는 곳] Player 오브젝트 (Player Input이 있는 오브젝트) — Player 프리팹에 들어 있음
/// [입력] Interact 액션(E, Press And Release) → OnInteract. 누른 순간만 처리한다.
///        같은 E를 장롱 숨기(PlayerHide)도 쓰지만, 장롱을 볼 때는 물건을 안 보고 있으므로 겹치지 않는다.
/// </summary>
public class PlayerPickup : MonoBehaviour
{
    [Header("연결 (비워 두면 자동으로 찾음)")]
    public PlayerInteract playerInteract;
    public PlayerFlashlight playerFlashlight;

    private void Start()
    {
        if (this.playerInteract == null) this.playerInteract = this.GetComponent<PlayerInteract>();
        if (this.playerFlashlight == null) this.playerFlashlight = this.GetComponent<PlayerFlashlight>();
    }

    // E를 누른 순간 (뗀 순간은 무시)
    private void OnInteract(InputValue value)
    {
        if (value.isPressed == false || this.playerInteract == null)
        {
            return;
        }

        ItemPickup item = this.playerInteract.LookItem;
        if (item != null)
        {
            item.PickUp(this.playerFlashlight);
        }
    }

    // 강의 04장 OnGUI: 물건을 보고 있으면 안내 문구
    private void OnGUI()
    {
        if (this.playerInteract == null || this.playerInteract.LookItem == null)
        {
            return;
        }
        // 공통 UI 스타일 (UIStyle.cs): 조준점 아래 가운데, [E]는 노란색
        UIStyle.Prompt("[E] " + this.playerInteract.LookItem.DisplayName + " 줍기");
    }
}
