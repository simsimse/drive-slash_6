using UnityEngine;

/// <summary>
/// 좌클릭으로 마우스에 가장 가까운 핀 방향으로 돌진합니다.
/// 대시 중 Boss 태그에 닿으면 데미지, Dagger 태그에 닿으면 단검 소비 후 정지.
/// 목표 핀의 PinData를 읽어 데미지 배율·속도 배율·무적 여부를 적용합니다.
/// </summary>
/// <remarks>
/// [의존]
/// - DaggerThrower.cs : ConsumeDagger()(단검 소비)
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

    [Header("연속 대시 콤보")]
    [Tooltip("콤보 1스택당 추가되는 속도/데미지 비율 (0.15 = +15%)")]
    public float comboBonusPerStack = 0.15f;
    [Tooltip("콤보 최대 스택 수")]
    public int   maxComboStacks    = 5;
    [Tooltip("마지막 대시 종료 후 N초 안에 다음 대시를 안 하면 콤보 초기화")]
    public float comboResetTime    = 1.5f;

    [Header("보라핀 무적 시각 효과")]
    [Range(0f, 1f)] public float invincibleAlpha = 0.4f;

    public bool IsDashing    { get; private set; } = false;
    /// <summary>보라 핀 효과: 슬래시 중 무적 여부. 플레이어 피격 판정 스크립트에서 참조.</summary>
    public bool IsInvincible { get; private set; } = false;
    /// <summary>현재 콤보 스택 수 (UI 표시 등에 사용 가능).</summary>
    public int  ComboStacks { get; private set; } = 0;

    private Rigidbody2D   _rb;
    private DaggerThrower _thrower;
    private Vector2       _dashDir;   // 돌진 방향 고정용
    private GameObject    _targetDagger; // 현재 대시 목표 핀

    // 현재 슬래시에 적용 중인 PinData 효과
    private float _activeDamageMultiplier = 1f;
    private float _activeSpeedMultiplier  = 1f;

    // 콤보 상태
    private float _activeComboMultiplier = 1f;
    private float _lastDashEndTime       = -999f;

    private SpriteRenderer[] _renderers;
    private float[]          _originalAlphas;
    private float            _baseGravityScale;

    void Awake()
    {
        _rb      = GetComponent<Rigidbody2D>();
        _thrower = GetComponent<DaggerThrower>();
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _baseGravityScale = _rb.gravityScale;

        _renderers      = GetComponentsInChildren<SpriteRenderer>();
        _originalAlphas = new float[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _originalAlphas[i] = _renderers[i].color.a;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !IsDashing)
        {
            _targetDagger = FindNearestPinToMouse();
            if (_targetDagger != null)
                StartDash();
        }
    }

    void FixedUpdate()
    {
        if (!IsDashing) return;

        if (_targetDagger == null)
        {
            StopDash();
            return;
        }

        // 중력·외부 힘 무시하고 고정 방향/속도로 직접 이동
        float dist  = Vector2.Distance(transform.position, _targetDagger.transform.position);
        float speed = dashSpeed * _activeSpeedMultiplier * _activeComboMultiplier * (dist < boostDistance ? boostMultiplier : 1f);
        float stepDistance = speed * Time.fixedDeltaTime;

        // 이번 스텝 경로상 보스 콜라이더 통과 방지 — 콤보 가속 시 트리거 누락 방지
        ScanBossHitsInPath(stepDistance);

        // 이번 스텝에 핀을 지나칠지 raycast로 미리 검사 (고속 이동 시 트리거 통과 방지)
        if (TryHitPinInPath(stepDistance)) return;

        _rb.linearVelocity = _dashDir * speed;
    }

    // 한 번의 대시 동안 raycast 로 이미 데미지 준 콜라이더 — OnTrigger 와 중복 방지
    private readonly System.Collections.Generic.HashSet<Collider2D> _bossHitThisDash =
        new System.Collections.Generic.HashSet<Collider2D>();

    /// <summary>
    /// 진행 방향 stepDistance 안에 있는 모든 "Boss" 콜라이더에 데미지를 적용합니다.
    /// 통과는 막지 않고(보스를 베고 지나감) 한 대시 동안 같은 콜라이더에 중복 적용되지 않도록 추적합니다.
    /// </summary>
    private void ScanBossHitsInPath(float stepDistance)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(_rb.position, _dashDir, stepDistance);
        if (hits == null) return;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D col = hits[i].collider;
            if (col == null || col.gameObject == gameObject) continue;
            if (!col.CompareTag("Boss")) continue;
            if (_bossHitThisDash.Contains(col)) continue;

            _bossHitThisDash.Add(col);
            ApplyBossDamage(col);
        }
    }

    /// <summary>
    /// 진행 방향으로 stepDistance 만큼 raycast 하여 가장 가까운 Dagger 핀에 스냅·소비합니다.
    /// 핀을 적중하면 true 를 반환하고 대시를 종료합니다.
    /// </summary>
    private bool TryHitPinInPath(float stepDistance)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(_rb.position, _dashDir, stepDistance);
        if (hits == null || hits.Length == 0) return false;

        RaycastHit2D nearest = default;
        float        minDist = Mathf.Infinity;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D col = hits[i].collider;
            if (col == null || col.gameObject == gameObject) continue;
            if (!col.CompareTag("Dagger")) continue;

            if (hits[i].distance < minDist)
            {
                minDist = hits[i].distance;
                nearest = hits[i];
            }
        }

        if (nearest.collider == null) return false;

        _rb.position       = nearest.collider.transform.position;
        _rb.linearVelocity = Vector2.zero;
        _thrower.ConsumeDagger(nearest.collider.gameObject);
        StopDash();
        return true;
    }

    private void StartDash()
    {
        _bossHitThisDash.Clear();

        // 콤보 만료 검사 — 마지막 대시 후 너무 오래 지났으면 초기화
        if (Time.time - _lastDashEndTime > comboResetTime)
            ComboStacks = 0;

        _activeComboMultiplier = 1f + Mathf.Min(ComboStacks, maxComboStacks) * comboBonusPerStack;

        // 목표 핀의 PinData 읽기
        PinData pinData = _targetDagger.GetComponent<PinData>();
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

        _dashDir = (_targetDagger.transform.position - transform.position).normalized;

        // 기존 속도·힘 초기화 → 직선 보장
        _rb.linearVelocity  = Vector2.zero;
        _rb.angularVelocity = 0f;

        // 돌진 중 중력 끄기
        _rb.gravityScale = 0f;

        IsDashing = true;

        if (IsInvincible) ApplyAlpha(invincibleAlpha);
    }

    private void StopDash()
    {
        // 콤보 스택 누적 (대시 종료 시점 기준)
        if (IsDashing)
        {
            ComboStacks      = Mathf.Min(ComboStacks + 1, maxComboStacks);
            _lastDashEndTime = Time.time;
        }

        IsDashing    = false;
        if (IsInvincible) RestoreAlpha();
        IsInvincible = false;
        _targetDagger = null;

        _activeDamageMultiplier = 1f;
        _activeSpeedMultiplier  = 1f;
        _activeComboMultiplier  = 1f;

        _rb.gravityScale   = _baseGravityScale;   // Inspector 원본 값으로 복구
        _rb.linearVelocity = Vector2.zero;
    }

    private void ApplyAlpha(float alpha)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            Color c = _renderers[i].color;
            c.a = alpha;
            _renderers[i].color = c;
        }
    }

    private void RestoreAlpha()
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            Color c = _renderers[i].color;
            c.a = _originalAlphas[i];
            _renderers[i].color = c;
        }
    }

    private GameObject FindNearestPinToMouse()
    {
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GameObject[] daggers = GameObject.FindGameObjectsWithTag("Dagger");

        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject d in daggers)
        {
            float dist = Vector2.Distance(mouseWorld, d.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = d;
            }
        }

        return nearest;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 솔리드 콜라이더로 박힌 경우(트리거 아님)에도 보스 부위/본체에 데미지 적용
        if (IsDashing && collision.collider.CompareTag("Boss"))
            ApplyBossDamage(collision.collider);

        StopDash();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsDashing && other.CompareTag("Boss") && !_bossHitThisDash.Contains(other))
        {
            _bossHitThisDash.Add(other);
            ApplyBossDamage(other);
        }

        if (other.CompareTag("Dagger"))
        {
            _thrower.ConsumeDagger(other.gameObject);
            StopDash();
        }
    }

    /// <summary>
    /// 콜라이더가 보스 본체/부위인지 판별해 적절한 곳에 데미지를 전달합니다.
    /// 부위(BossPart)가 있으면 부위 우선, 없으면 본체(Boss) 처리.
    /// </summary>
    private void ApplyBossDamage(Collider2D col)
    {
        if (col == null) return;

        int finalDamage = Mathf.RoundToInt(dashDamage * _activeDamageMultiplier * _activeComboMultiplier);

        // GetComponentInParent: 콜라이더가 BossPart 의 자식 GO 여도 위로 올라가며 검색
        BossPart part = col.GetComponentInParent<BossPart>();
        if (part != null)
        {
            part.TakeDamage(finalDamage);
            return;
        }

        Boss boss = col.GetComponentInParent<Boss>();
        boss?.TakeDamage(finalDamage);
    }
}
