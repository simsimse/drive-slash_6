using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 패링(Shift) 쿨타임을 아이콘 하나로 표시합니다.
/// 쿨타임 중에는 fillAmount로 진행도가 차오르고 어둡게, 준비되면 밝게 표시됩니다.
/// 선택적으로 남은 초를 TMP 텍스트로 보여줄 수 있습니다.
/// </summary>
/// <remarks>
/// [Inspector 설정]
/// 1. Canvas 아래에 아이콘용 Image 를 하나 배치하고 Image Type 을 'Filled' 로 설정합니다.
///    (Fill Method: Radial 360 을 쓰면 원형 쿨다운처럼 보입니다.)
/// 2. 이 스크립트를 Canvas(또는 컨테이너)에 붙이고:
///    - repeller : 표시할 Repeller (비워두면 씬에서 자동 탐색)
///    - fillIcon : 위에서 만든 Filled Image
///    - (선택) cooldownText : 남은 초를 표시할 TMP_Text
/// 3. 더 보기 좋게 하려면 아이콘 뒤에 어두운 배경 Image 를 한 장 더 깔아두세요.
///
/// [의존]
/// - Repeller.cs : CooldownProgress / IsReady / CooldownRemaining
/// </remarks>
public class ParryCooldownUI : MonoBehaviour
{
    [Header("참조")]
    [Tooltip("표시할 Repeller. 비워두면 씬에서 자동으로 찾습니다.")]
    [SerializeField] private Repeller repeller;

    [Header("아이콘")]
    [Tooltip("쿨타임 진행도를 표시할 Image. Image Type 을 'Filled' 로 설정하세요.")]
    [SerializeField] private Image fillIcon;

    [Tooltip("(선택) 남은 쿨타임 초를 표시할 텍스트. 없으면 비워두세요.")]
    [SerializeField] private TMPro.TMP_Text cooldownText;

    [Header("색상")]
    [Tooltip("사용 가능(쿨타임 완료) 상태 색")]
    [SerializeField] private Color readyColor    = Color.white;
    [Tooltip("쿨타임 진행 중 색")]
    [SerializeField] private Color cooldownColor = new Color(1f, 1f, 1f, 0.5f);

    private void Awake()
    {
        if (repeller == null)
            repeller = FindAnyObjectByType<Repeller>();

        if (repeller == null)
            Debug.LogWarning("[ParryCooldownUI] Repeller를 찾지 못했습니다. UI가 갱신되지 않습니다.", this);

        if (fillIcon == null)
        {
            Debug.LogWarning("[ParryCooldownUI] fillIcon이 비어 있습니다. 인스펙터에서 Image를 등록하세요.", this);
            return;
        }

        // fillAmount가 동작하려면 Image Type이 'Filled' 여야 합니다. 자동 보정.
        if (fillIcon.type != Image.Type.Filled)
            fillIcon.type = Image.Type.Filled;
    }

    private void Update()
    {
        if (repeller == null || fillIcon == null) return;

        if (repeller.IsReady)
        {
            // 사용 가능: 가득 찬 상태로 밝게
            fillIcon.color      = readyColor;
            fillIcon.fillAmount = 1f;

            if (cooldownText != null)
                cooldownText.text = string.Empty;
        }
        else
        {
            // 쿨타임 중: 진행도만큼 차오르고 어둡게
            fillIcon.color      = cooldownColor;
            fillIcon.fillAmount = repeller.CooldownProgress;

            if (cooldownText != null)
                cooldownText.text = Mathf.CeilToInt(repeller.CooldownRemaining).ToString();
        }
    }
}
