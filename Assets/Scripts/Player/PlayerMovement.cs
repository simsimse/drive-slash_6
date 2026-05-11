using UnityEngine;

/// <summary>
/// 플레이어 기본 이동. 대시 중에는 이동을 멈춥니다.
/// </summary>
/// <remarks>
/// [의존]
/// - Dash.cs : IsDashing 이 true이면 FixedUpdate에서 velocity 적용 중단
/// [같은 GameObject에 필요한 컴포넌트]
/// - Dash, Rigidbody2D
/// </remarks>
public class PlayerMovement : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;
    public int   hp        = 10;

    [Header("중력 설정")]
    [Tooltip("최대 낙하 속도 (단위: m/s)")]
    public float maxFallSpeed = 15f;

    private Rigidbody2D _rb;
    private float       _moveX;
    private Dash        _dash;

    void Awake()
    {
        _rb   = GetComponent<Rigidbody2D>();
        _dash = GetComponent<Dash>();
    }

    void Update()
    {
        _moveX = Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate()
    {
        if (_dash != null && _dash.IsDashing) return;

        // X축만 입력 제어, Y축은 물리(중력)에 맡김
        float vy = _rb.linearVelocity.y;
        if (vy < -maxFallSpeed) vy = -maxFallSpeed;
        _rb.linearVelocity = new Vector2(_moveX * moveSpeed, vy);
    }

    private bool _isDead;

    public void TakeDamage(int damage)
    {
        if (_isDead) return;
        if (_dash != null && _dash.IsInvincible) return;

        hp -= damage;
        if (hp <= 0)
        {
            hp = 0;
            Die();
        }
    }

    private void Die()
    {
        _isDead = true;
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
        enabled = false;

        if (GameOverUI.Instance != null)
            GameOverUI.Instance.Show();
    }
}
