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

    private Rigidbody2D _rb;
    private Vector2     _move;
    private Dash        _dash;

    void Awake()
    {
        _rb   = GetComponent<Rigidbody2D>();
        _dash = GetComponent<Dash>();
    }

    void Update()
    {
        _move.x = Input.GetAxisRaw("Horizontal");
        _move.y = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        if (_dash != null && _dash.IsDashing) return;

        _rb.linearVelocity = _move.normalized * moveSpeed;
    }

    private bool _isDead;

    public void TakeDamage(int damage)
    {
        if (_isDead) return;

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
        _move = Vector2.zero;
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
        enabled = false;

        if (GameOverUI.Instance != null)
            GameOverUI.Instance.Show();
    }
}
