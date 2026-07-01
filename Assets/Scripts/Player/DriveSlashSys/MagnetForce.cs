using UnityEngine;

/// <summary>
/// Metal 태그 오브젝트를 향해 지속적으로 힘을 가하는 자석 클래스.
/// </summary>
/// <remarks>
/// [의존]
/// - Dash.cs      : IsDashing 중에는 자력 비활성화
/// [같은 GameObject에 필요한 컴포넌트]
/// - Dash, Rigidbody2D
/// [주의] FixedUpdate 내 실제 자력 적용 로직 미구현
/// </remarks>
[RequireComponent(typeof(Dash))]
public class MagnetForce : MonoBehaviour
{
    [Header("자석 설정")]
    public float magnetForce = 25f;
    public float magnetRange = 8f;

    private Rigidbody2D _rb;
    private Dash        _dash;

    void Awake()
    {
        _rb   = GetComponent<Rigidbody2D>();
        _dash = GetComponent<Dash>();
    }

    void FixedUpdate()
    {
        if (_dash.IsDashing) return;
    }

    
}