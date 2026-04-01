using UnityEngine;

/// <summary>
/// Shift 키로 가장 최근 배치된 단검 방향으로 돌진합니다.
/// 대시 중 Boss 태그에 닿으면 데미지, Dagger 태그에 닿으면 단검 소비 후 정지.
/// </summary>
/// <remarks>
/// [의존]
/// - DaggerThrower.cs : CurrentDagger(목표 위치), ConsumeDagger()(단검 소비)
/// - Boss.cs          : TakeDamage() 호출 (OnTriggerEnter2D)
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

    public bool IsDashing { get; private set; } = false;

    private Rigidbody2D   _rb;
    private DaggerThrower _thrower;
    private Vector2       _dashDir;   // 돌진 방향 고정용

    void Awake()
    {
        _rb      = GetComponent<Rigidbody2D>();
        _thrower = GetComponent<DaggerThrower>();
    }

    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
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
        float   dist  = Vector2.Distance(transform.position, _thrower.CurrentDagger.transform.position);
        float   speed = dashSpeed * (dist < boostDistance ? boostMultiplier : 1f);
        _rb.linearVelocity = _dashDir * speed;
    }

    private void StartDash()
    {
        _dashDir = (_thrower.CurrentDagger.transform.position - transform.position).normalized;

        // 기존 속도·힘 초기화 → 직선 보장
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        // 돌진 중 중력 끄기
        _rb.gravityScale = 0f;

        IsDashing = true;
    }

    private void StopDash()
    {
        IsDashing = false;
        _rb.gravityScale  = 1f;          // 중력 복구 (Inspector 기본값에 맞게 조정)
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
            other.GetComponent<Boss>()?.TakeDamage(dashDamage);
            Debug.Log($"보스에게 {dashDamage} 데미지!");
        }

        if (other.CompareTag("Dagger"))
        {
            _thrower.ConsumeDagger(other.gameObject);
            StopDash();
        }
    }
}