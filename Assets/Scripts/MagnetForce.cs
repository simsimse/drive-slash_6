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

        ApplyMagnet();
    }

    private void ApplyMagnet()
    {
        Vector2      dir = arrow.right;
        RaycastHit2D hit = Physics2D.Raycast(arrow.position, dir, magnetRange);

        if (!hit || !hit.collider.CompareTag("Metal")) return;

        float strength = magnetForce * (1f - hit.distance / magnetRange);

        // 인력: 방향으로 당김 / 척력: 반대 방향으로 밀기
        _rb.AddForce(_forceMode.IsPullMode ? dir * strength : -dir * strength);
    }
}
