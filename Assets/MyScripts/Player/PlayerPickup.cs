using UnityEngine;
using UnityEngine.InputSystem;

// 바라보는 물건(ItemPickup)을 E로 줍는다. 화면 가운데 아래에 "[E] 손전등 줍기"를 보여 준다.
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

    private void OnInteract(InputValue value)   // 강의 02장: Player Input 메시지 함수 OnInteract
    {
        if (value.isPressed == false || this.playerInteract == null)   // 강의 02장: value.isPressed (Press And Release)
        {
            return;
        }

        ItemPickup item = this.playerInteract.LookItem;
        if (item != null)
        {
            item.PickUp(this.playerFlashlight);
        }
    }

    private void OnGUI()   // 강의 04장: OnGUI
    {
        if (this.playerInteract == null || this.playerInteract.LookItem == null)
        {
            return;
        }
        UIStyle.Prompt("[E] " + this.playerInteract.LookItem.DisplayName + " 줍기");
    }
}
