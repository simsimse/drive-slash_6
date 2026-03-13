using UnityEngine;

/// <summary>
/// Shift 키로 배치된 대거 위치로 플레이어를 끌어당기는 인력 대시 전담 클래스.
/// 대거 투척은 DaggerThrower, 모드 전환은 ForceMode가 담당합니다.
/// </summary>
[RequireComponent(typeof(DaggerThrower))]
public class Dash : MonoBehaviour
{
    [Header("대시 설정")]
    public float pullForce      = 25f;
    public float boostMultiplier = 2f;
    public float boostDistance   = 3f;
    public int   dashDamage      = 5;

    public bool IsDashing { get; private set; } = false;

    private Rigidbody2D  _rb;
    private DaggerThrower _thrower;

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
            PullToDagger();
        }
    }

    private void PullToDagger()
    {
        Vector2 dir  = (_thrower.CurrentDagger.transform.position - transform.position).normalized;
        float   dist = Vector2.Distance(transform.position, _thrower.CurrentDagger.transform.position);
        float   force = pullForce * (dist < boostDistance ? boostMultiplier : 1f);

        _rb.AddForce(dir * force, ForceMode2D.Impulse);
        IsDashing = true;
    }

    void OnCollisionEnter2D(Collision2D _)
    {
        IsDashing = false;
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
            _thrower.ConsumeDagger(); // 대거 소비를 DaggerThrower에 위임
        }
    }
}
