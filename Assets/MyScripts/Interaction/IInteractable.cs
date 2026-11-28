using UnityEngine;

// "플레이어가 마우스로 잡을 수 있는 물체"의 공통 약속(인터페이스).
// [강의 외] C# interface: 문·서랍·레버·물건을 같은 방식으로 잡기
public interface IInteractable
{
    bool LocksLook { get; }

    void OnGrabStart(Transform cameraTransform);

    void OnGrabUpdate(Vector2 mouseDelta);

    void OnGrabEnd();
}
