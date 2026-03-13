using UnityEngine;

/// <summary>
/// 인력(Pull) / 척력(Repel) 모드 상태를 보관하는 단일 진실 공급원(Single Source of Truth).
/// Dash, PlayerForceController, ArrowVisualizer 등이 모두 이 컴포넌트를 참조합니다.
/// </summary>
public class ForceMode : MonoBehaviour
{
    public bool IsPullMode { get; private set; } = false;

    /// <summary>우클릭으로 모드 전환</summary>
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
            Toggle();
    }

    public void Toggle()
    {
        IsPullMode = !IsPullMode;
        Debug.Log($"모드 전환 → {(IsPullMode ? "인력(Pull)" : "척력(Repel)")}");
    }
}
