using UnityEngine;

// E로 줍는 물건 (손전등, 건전지). 바라보면 [E] 줍기가 뜨고, E를 누르면 효과가 생기고 사라진다.
public class ItemPickup : MonoBehaviour
{
    public enum ItemType { Flashlight, Battery, Key }

    [Tooltip("무슨 물건인지")]
    public ItemType type = ItemType.Battery;

    [Tooltip("손전등: 얻을 때 배터리 양(0~100) / 건전지: 충전량")]
    public float amount = 50f;

    [Tooltip("열쇠일 때 이름. 문(DraggableDoor)의 Required Key와 글자가 똑같아야 한다. 예: 보일러실 열쇠")]
    public string keyName = "보일러실 열쇠";

    [Tooltip("주울 때 소리 (없어도 됨). 소리 파일을 드래그")]
    public AudioClip pickupSound;

    public string DisplayName
    {
        get
        {
            if (this.type == ItemType.Key) return this.keyName;
            return this.type == ItemType.Flashlight ? "손전등" : "건전지";
        }
    }

    private void Start()
    {
        if (this.type == ItemType.Key && PlayerKeys.Has(this.keyName))
        {
            Destroy(this.gameObject);   // 강의 05장: Destroy
        }
    }

    public void PickUp(PlayerFlashlight flashlight)
    {
        if (this.type == ItemType.Key)
        {
            PlayerKeys.Add(this.keyName);
            PlayerKeys.ShowMessage(this.keyName + "를 얻었다");
        }
        else if (flashlight != null)
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
            AudioSource.PlayClipAtPoint(this.pickupSound, this.transform.position);   // 강의 07장: AudioSource.PlayClipAtPoint
        }

        Destroy(this.gameObject);
    }
}
