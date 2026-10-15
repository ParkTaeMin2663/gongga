using UnityEngine;

/// <summary>
/// "플레이어가 마우스로 잡을 수 있는 물체"의 공통 약속(인터페이스).
///
/// 문, 서랍, 레버, 집을 수 있는 물건이 전부 이 약속을 지키면
/// PlayerInteract는 상대가 무엇인지 몰라도 똑같이 "잡기 → 끌기 → 놓기"를 시킬 수 있다.
///
/// [강의 외] C#의 interface 문법. 강의에는 없지만 여러 종류의 물체를 한 가지 방식으로 다루려고 사용한다.
/// </summary>
public interface IInteractable
{
    /// <summary>마우스 왼쪽을 누른 순간 (잡기 시작)</summary>
    void OnGrabStart(Transform cameraTransform);

    /// <summary>잡고 있는 동안 매 프레임. mouseDelta = 이번 프레임 마우스 움직임 (x = 좌우, y = 앞뒤)</summary>
    void OnGrabUpdate(Vector2 mouseDelta);

    /// <summary>마우스 왼쪽을 뗀 순간, 또는 너무 멀어져서 놓친 순간</summary>
    void OnGrabEnd();
}
