using UnityEngine;

/// <summary>
/// 핀 프리팹에 부착하여 슬래시 효과를 정의합니다.
/// Dash.cs가 StartDash 시점에 읽어 해당 슬래시에 적용합니다.
/// </summary>
/// <remarks>
/// [참조하는 곳]
/// - Dash.cs : StartDash() 에서 CurrentDagger 의 PinData 를 읽음
/// </remarks>
public class PinData : MonoBehaviour
{
    [Header("슬래시 효과")]
    [Tooltip("슬래시 데미지 배율 (1 = 기본)")]
    public float damageMultiplier = 1f;

    [Tooltip("슬래시 속도 배율 (1 = 기본)")]
    public float speedMultiplier  = 1f;

    [Tooltip("슬래시 중 플레이어 무적 여부")]
    public bool  isInvincible     = false;
}
