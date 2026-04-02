using UnityEngine;

/// <summary>
/// 좌클릭으로 가장 최근 배치된 단검 방향으로 돌진합니다.
/// 대시 중 Boss 태그에 닿으면 데미지, Dagger 태그에 닿으면 단검 소비 후 정지.
/// 목표 핀의 PinData를 읽어 데미지 배율·속도 배율·무적 여부를 적용합니다.
/// </summary>
/// <remarks>
/// [의존]
/// - DaggerThrower.cs : CurrentDagger(목표 위치), ConsumeDagger()(단검 소비)
/// - Boss.cs          : TakeDamage() 호출 (OnTriggerEnter2D)
/// - PinData.cs       : 핀 효과(데미지 배율, 속도 배율, 무적 여부)
/// [참조하는 곳]
/// - PlayerMovement.cs : IsDashing 으로 이동 억제
/// - MagnetForce.cs    : IsDashing 으로 자력 억제
/// - Repeller.cs       : IsDashing 으로 차지 억제
/// [같은 GameObject에 필요한 컴포넌트]
/// - DaggerThrower, Rigidbody2D
/// </remarks>
[RequireComponent(typeof(DaggerThrower))]
public class Dash : MonoBehaviour
{
    [Header("대시 설정")]
    public float dashSpeed       = 20f;   // 돌진 속도 (AddForce 대신 직접 속도 제어)
    public float boostMultiplier = 1.5f;
    public float boostDistance   = 3f;
    public int   dashDamage      = 5;

    public bool IsDashing    { get; private set; } = false;
    /// <summary>보라 핀 효과: 슬래시 중 무적 여부. 플레이어 피격 판정 스크립트에서 참조.</summary>
    public bool IsInvincible { get; private set; } = false;

    private Rigidbody2D   _rb;
    private DaggerThrower _thrower;
    private Vector2       _dashDir;   // 돌진 방향 고정용

    // 현재 슬래시에 적용 중인 PinData 효과
    private float _activeDamageMultiplier = 1f;
    private float _activeSpeedMultiplier  = 1f;

    void Awake()
    {
        _rb      = GetComponent<Rigidbody2D>();
        _thrower = GetComponent<DaggerThrower>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)
            && _thrower.CurrentDagger != null && !IsDashing)
        {
            StartDash();
        }
    }

    void FixedUpdate()
    {
        if (!IsDashing) return;

        if (_thrower.CurrentDagger == null)
        {
            StopDash();
            return;
        }

        // 중력·외부 힘 무시하고 고정 방향/속도로 직접 이동
        float dist  = Vector2.Distance(transform.position, _thrower.CurrentDagger.transform.position);
        float speed = dashSpeed * _activeSpeedMultiplier * (dist < boostDistance ? boostMultiplier : 1f);
        _rb.linearVelocity = _dashDir * speed;
    }

    private void StartDash()
    {
        // 목표 핀의 PinData 읽기
        PinData pinData = _thrower.CurrentDagger.GetComponent<PinData>();
        if (pinData != null)
        {
            _activeDamageMultiplier = pinData.damageMultiplier;
            _activeSpeedMultiplier  = pinData.speedMultiplier;
            IsInvincible            = pinData.isInvincible;
        }
        else
        {
            _activeDamageMultiplier = 1f;
            _activeSpeedMultiplier  = 1f;
            IsInvincible            = false;
        }

        _dashDir = (_thrower.CurrentDagger.transform.position - transform.position).normalized;

        // 기존 속도·힘 초기화 → 직선 보장
        _rb.linearVelocity  = Vector2.zero;
        _rb.angularVelocity = 0f;

        // 돌진 중 중력 끄기
        _rb.gravityScale = 0f;

        IsDashing = true;
    }

    private void StopDash()
    {
        IsDashing    = false;
        IsInvincible = false;

        _activeDamageMultiplier = 1f;
        _activeSpeedMultiplier  = 1f;

        _rb.gravityScale   = 1f;          // 중력 복구 (Inspector 기본값에 맞게 조정)
        _rb.linearVelocity = Vector2.zero;
    }

    void OnCollisionEnter2D(Collision2D _)
    {
        StopDash();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsDashing && other.CompareTag("Boss"))
        {
            int dmg = Mathf.RoundToInt(dashDamage * _activeDamageMultiplier);
            other.GetComponent<Boss>()?.TakeDamage(dmg);
            Debug.Log($"보스에게 {dmg} 데미지! (배율 {_activeDamageMultiplier}x)");
        }

        if (other.CompareTag("Dagger"))
        {
            _thrower.ConsumeDagger(other.gameObject);
            StopDash();
        }
    }
}
