using UnityEngine;

/// <summary>
/// E로 줍는 물건 (손전등, 건전지). 바라보면 [E] 줍기가 뜨고, E를 누르면 효과가 생기고 사라진다.
///
/// [붙이는 곳] 줍는 물건 오브젝트 (FlashlightPickup, Battery 프리팹). Collider 필요 (조준점 Raycast가 맞아야 함)
/// [동작] PlayerInteract가 바라보는 물건을 찾고(LookItem), PlayerPickup이 E를 받으면 PickUp()을 부른다.
///        강의 07장 PlayClipAtPoint(사라져도 그 자리에서 소리), 강의 05장 Destroy.
///
/// [종류]
///   - Flashlight : 손전등을 얻는다 (PlayerFlashlight.GiveFlashlight, 바로 켜짐). 배터리는 amount로 맞춘다
///   - Battery    : 손전등 배터리를 amount만큼 충전한다
/// </summary>
public class ItemPickup : MonoBehaviour
{
    public enum ItemType { Flashlight, Battery }

    [Tooltip("무슨 물건인지")]
    public ItemType type = ItemType.Battery;

    [Tooltip("손전등: 얻을 때 배터리 양(0~100) / 건전지: 충전량")]
    public float amount = 50f;

    [Tooltip("주울 때 소리 (없어도 됨). 소리 파일을 드래그")]
    public AudioClip pickupSound;

    /// <summary>화면에 보여 줄 이름 ("[E] 손전등 줍기")</summary>
    public string DisplayName { get { return this.type == ItemType.Flashlight ? "손전등" : "건전지"; } }

    /// <summary>PlayerPickup이 E를 받았을 때 부른다.</summary>
    public void PickUp(PlayerFlashlight flashlight)
    {
        if (flashlight != null)
        {
            if (this.type == ItemType.Flashlight)
            {
                flashlight.battery = this.amount;
                flashlight.GiveFlashlight();
            }
            else
            {
                flashlight.AddBattery(this.amount);
            }
        }

        if (this.pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(this.pickupSound, this.transform.position);   // 강의 07장
        }

        Destroy(this.gameObject);   // 강의 05장
    }
}
