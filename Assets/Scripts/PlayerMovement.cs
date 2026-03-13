using UnityEngine;

/// <summary>
/// 플레이어 기본 이동. 대시 중에는 이동을 멈춥니다.
/// </summary>
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
}
