using UnityEngine;

/// <summary>
/// Metal 태그 오브젝트를 향해 지속적으로 인력 또는 척력을 가하는 자석 클래스.
/// ForceMode.IsPullMode 에 따라 방향이 결정됩니다.
/// </summary>
[RequireComponent(typeof(ForceMode))]
[RequireComponent(typeof(Dash))]
public class MagnetForce : MonoBehaviour
{
    [Header("자석 설정")]
    public float magnetForce = 25f;
    public float magnetRange = 8f;

    public Transform arrow; // ArrowVisualizer와 같은 arrow Transform 공유

    private Rigidbody2D _rb;
    private ForceMode   _forceMode;
    private Dash        _dash;

    void Awake()
    {
        _rb        = GetComponent<Rigidbody2D>();
        _forceMode = GetComponent<ForceMode>();
        _dash      = GetComponent<Dash>();
    }

    void FixedUpdate()
    {
        if (_dash.IsDashing) return;
    }

    
}
