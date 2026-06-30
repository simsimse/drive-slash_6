using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 핀(대거) 충전 상태를 아이콘 N개로 표시합니다.
/// 충전된 슬롯은 밝게, 비어 있는 슬롯은 어둡게,
/// 가장 먼저 회복될 슬롯은 fillAmount로 진행도가 채워집니다.
/// </summary>
/// <remarks>
/// [Inspector 설정]
/// 1. Canvas 아래에 핀 슬롯용 Image 를 maxCharges 개(보통 3개) 배치합니다.
///    각 Image 의 Image Type 을 'Filled' 로 두고, Fill Method/Origin 은 취향대로.
/// 2. 이 스크립트를 그 Canvas(또는 컨테이너) 위에 붙이고:
///    - manager : 표시할 DaggerManager (비워두면 씬에서 자동 탐색)
///    - slotIcons : 위에서 만든 Image 들을 순서대로 드래그
/// 3. 더 보기 좋게 하려면 각 슬롯마다 어두운 배경 Image 를 한 장 더 깔아두세요.
///    (배경 Image 는 이 스크립트가 건드리지 않습니다.)
///
/// [의존]
/// - DaggerManager.cs : CurrentCharges / MaxCharges / NextRechargeProgress / HasPendingRecharge
/// </remarks>
public class PinChargeUI : MonoBehaviour
{
    [Header("참조")]
    [Tooltip("표시할 DaggerManager. 비워두면 씬에서 자동으로 찾습니다.")]
    [SerializeField] private DaggerManager manager;

    [Header("슬롯")]
    [Tooltip("핀 슬롯 아이콘. Image Type 을 'Filled' 로 설정하세요.")]
    [SerializeField] private Image[] slotIcons;

    [Header("색상")]
    [Tooltip("충전 완료된 슬롯 색")]
    [SerializeField] private Color readyColor       = Color.white;
    [Tooltip("충전 중 / 빈 슬롯 색")]
    [SerializeField] private Color rechargingColor  = new Color(1f, 1f, 1f, 0.5f);

    private void Awake()
    {
        if (manager == null)
            manager = FindAnyObjectByType<DaggerManager>();
    }

    private void Update()
    {
        if (manager == null || slotIcons == null) return;

        int   charges    = manager.CurrentCharges;
        bool  hasPending = manager.HasPendingRecharge;
        float progress   = manager.NextRechargeProgress;

        for (int i = 0; i < slotIcons.Length; i++)
        {
            Image icon = slotIcons[i];
            if (icon == null) continue;

            if (i < charges)
            {
                // 충전 완료
                icon.color      = readyColor;
                icon.fillAmount = 1f;
            }
            else if (i == charges && hasPending)
            {
                // 가장 먼저 회복될 슬롯 → 진행도만큼 차오름
                icon.color      = rechargingColor;
                icon.fillAmount = progress;
            }
            else
            {
                // 그 뒤로 대기 중인 빈 슬롯
                icon.color      = rechargingColor;
                icon.fillAmount = 0f;
            }
        }
    }
}
