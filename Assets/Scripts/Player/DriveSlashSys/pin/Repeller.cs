using UnityEngine;

/// <summary>
/// Shift 키로 Repersion 모드(0.5초 무적 튕겨내기)를 발동합니다.
/// 발동 후 5초 쿨타임.
/// </summary>
/// <remarks>
/// [의존]
/// - Dash.cs          : IsDashing 중에는 발동 비활성화
/// - arrow (Transform): ArrowVisualizer 와 동일한 arrow Transform 공유 (Inspector 연결 필요)
/// [같은 GameObject에 필요한 컴포넌트]
/// - Dash, Rigidbody2D
/// </remarks>
[RequireComponent(typeof(Dash))]
public class Repeller : MonoBehaviour
{
    [Header("척력 설정")]
    public float repelPower    = 15f;
    public float metalJumpPower = 20f;
    public float rayCastRange   = 8f;

    [Header("Repersion 설정")]
    public float invincibleDuration = 0.5f;  // 무적 지속 시간
    public float cooldown           = 5f;    // 쿨타임

    public Transform arrow; // ArrowVisualizer와 같은 arrow Transform 공유

    public bool IsInvincible { get; private set; } = false;

    private Rigidbody2D _rb;
    private Dash        _dash;
    private float       _invincibleTimer = 0f;
    private float       _cooldownTimer   = 0f;

    void Awake()
    {
        _rb   = GetComponent<Rigidbody2D>();
        _dash = GetComponent<Dash>();
    }

    void Update()
    {
        if (_dash.IsDashing) return;

        // 쿨타임 카운트다운
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;

        // 무적 시간 카운트다운
        if (IsInvincible)
        {
            _invincibleTimer -= Time.deltaTime;
            if (_invincibleTimer <= 0f)
                IsInvincible = false;
        }

        // Shift 입력 → Repersion 발동
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            && _cooldownTimer <= 0f)
        {
            ActivateRepersion();
        }
    }

    private void ActivateRepersion()
    {
        // 무적 판정 시작
        IsInvincible     = true;
        _invincibleTimer = invincibleDuration;
        _cooldownTimer   = cooldown;

        FireRepel();
    }

    private void FireRepel()
    {
        Vector2 dir = arrow.right;

        RaycastHit2D hit = Physics2D.Raycast(arrow.position, dir, rayCastRange);

        if (hit && hit.collider.CompareTag("Metal"))
            _rb.AddForce(-dir * metalJumpPower, ForceMode2D.Impulse);
        else
            _rb.AddForce(-dir * repelPower, ForceMode2D.Impulse);
    }
}
