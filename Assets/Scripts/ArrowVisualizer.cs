using UnityEngine;

/// <summary>
/// 마우스 방향으로 화살표를 회전시키고,
/// 인력/척력 모드에 따라 색상을 바꾸는 시각적 피드백 전담 클래스.
/// </summary>
/// <remarks>
/// [의존]
/// - ForceMode.cs : IsPullMode 로 화살표 색상 결정
/// [참조하는 곳]
/// - MagnetForce.cs : arrow Transform 공유 (Inspector에서 동일 오브젝트 연결)
/// - Repeller.cs    : arrow Transform 공유 (Inspector에서 동일 오브젝트 연결)
/// [같은 GameObject에 필요한 컴포넌트]
/// - ForceMode
/// </remarks>
[RequireComponent(typeof(ForceMode))]
public class ArrowVisualizer : MonoBehaviour
{
    [Header("화살표 설정")]
    public Transform arrow;
    public SpriteRenderer arrowSprite;
    public float arrowDistance = 1.5f;

    [Header("색상")]
    public Color pullColor  = Color.blue;
    public Color repelColor = Color.red;

    private ForceMode _forceMode;
    private bool _lastPullMode;

    void Awake()
    {
        _forceMode = GetComponent<ForceMode>();
    }

    void Update()
    {
        RotateArrow();
        RefreshColor();
    }

    private void RotateArrow()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mouse - transform.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        arrow.rotation = Quaternion.Euler(0, 0, angle);
        arrow.position  = (Vector2)transform.position + dir * arrowDistance;
    }

    private void RefreshColor()
    {
        if (_forceMode.IsPullMode == _lastPullMode) return; // 변경 없으면 스킵

        _lastPullMode = _forceMode.IsPullMode;
        arrowSprite.color = _forceMode.IsPullMode ? pullColor : repelColor;
    }
}
