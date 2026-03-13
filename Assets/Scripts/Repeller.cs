using UnityEngine;

/// <summary>
/// 마우스 좌클릭 차지 후 릴리즈로 척력을 발동하는 클래스.
/// Metal 태그 오브젝트에 닿으면 반대 방향으로 점프합니다.
/// </summary>
[RequireComponent(typeof(Dash))]
public class Repeller : MonoBehaviour
{
    [Header("척력 설정")]
    public float repelPower    = 15f;
    public float maxCharge     = 2f;
    public float chargeSpeed   = 1.5f;
    public float metalJumpPower = 20f;
    public float rayCastRange   = 8f;

    public Transform arrow; // ArrowVisualizer와 같은 arrow Transform 공유

    private Rigidbody2D _rb;
    private Dash        _dash;
    private float       _charge    = 0f;
    private bool        _charging  = false;

    void Awake()
    {
        _rb   = GetComponent<Rigidbody2D>();
        _dash = GetComponent<Dash>();
    }

    void Update()
    {
        if (_dash.IsDashing) return;

        HandleChargeInput();
    }

    private void HandleChargeInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _charging = true;
            _charge   = 0f;
        }

        if (_charging)
            _charge = Mathf.Clamp(_charge + Time.deltaTime * chargeSpeed, 0, maxCharge);

        if (Input.GetMouseButtonUp(0))
        {
            FireRepel();
            _charging = false;
        }
    }

    private void FireRepel()
    {
        Vector2 dir = arrow.right;

        RaycastHit2D hit = Physics2D.Raycast(arrow.position, dir, rayCastRange);

        if (hit && hit.collider.CompareTag("Metal"))
            _rb.AddForce(-dir * metalJumpPower, ForceMode2D.Impulse);
        else
            _rb.AddForce(-dir * (repelPower * _charge), ForceMode2D.Impulse);
    }
}
