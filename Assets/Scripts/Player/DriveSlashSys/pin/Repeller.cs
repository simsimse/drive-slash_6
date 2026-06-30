using System.Collections;
using UnityEngine;

/// <summary>
/// 척력 모드일 때 Shift 키로 패링을 발동합니다.
/// - 0.5초 동안 패링 윈도우가 유지되며, 그 동안 들어온 공격은 무효화됩니다.
/// - 공격을 튕겨내면 화면이 잠시 멈춥니다(hitstop).
/// - 투사체는 별도로 방향이 반전되어 보스 쪽으로 되돌아갑니다(AirSlashBullet 측에서 처리).
/// 발동 후 cooldown 초의 쿨타임이 적용됩니다.
/// </summary>
/// <remarks>
/// [의존]
/// - Dash.cs       : IsDashing 중에는 발동 비활성화
/// - ForceMode.cs  : IsPullMode=false(척력) 일 때만 발동
/// - 발사 방향은 마우스 위치 기준으로 계산합니다.
/// [참조하는 곳]
/// - PlayerMovement.cs : TakeDamage 시 TryParry 호출로 패링 판정
/// - AirSlashBullet.cs : TryParry 호출 후 성공 시 방향 반전
/// [같은 GameObject에 필요한 컴포넌트]
/// - Dash, ForceMode, Rigidbody2D
/// </remarks>
[RequireComponent(typeof(Dash))]
[RequireComponent(typeof(ForceMode))]
public class Repeller : MonoBehaviour
{
    [Header("척력 설정")]
    public float repelPower    = 15f;
    public float metalJumpPower = 20f;
    public float rayCastRange   = 8f;

    [Header("패링 설정")]
    [Tooltip("Shift 입력 후 패링이 유지되는 시간(초)")]
    public float parryDuration = 0.5f;
    [Tooltip("패링 발동 후 다시 쓸 수 있을 때까지의 쿨타임(초)")]
    public float cooldown      = 5f;
    [Tooltip("공격을 튕겨냈을 때 화면이 멈추는 시간(초, 실시간)")]
    public float freezeDuration = 0.15f;

    [Header("디버그")]
    [Tooltip("패링 상태 전환을 콘솔에 로그로 출력")]
    public bool debugLog = true;

    /// <summary>현재 패링 윈도우가 활성화되어 있는지. 외부 공격 스크립트가 참조.</summary>
    public bool IsParrying { get; private set; } = false;

    private Rigidbody2D _rb;
    private Dash        _dash;
    private ForceMode   _forceMode;
    private float       _parryTimer    = 0f;
    private float       _cooldownTimer = 0f;
    private bool        _freezing      = false;

    void Awake()
    {
        _rb        = GetComponent<Rigidbody2D>();
        _dash      = GetComponent<Dash>();
        _forceMode = GetComponent<ForceMode>();
    }

    void Update()
    {
        if (_dash.IsDashing) return;

        // 쿨타임 카운트다운 (실시간 사용: hitstop 중에도 흐름)
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.unscaledDeltaTime;

        // 패링 윈도우 카운트다운 (실시간 사용: hitstop 중에도 흐름)
        if (IsParrying)
        {
            _parryTimer -= Time.unscaledDeltaTime;
            if (_parryTimer <= 0f)
            {
                IsParrying = false;
                if (debugLog) Debug.Log("[Parry] 윈도우 종료 — 패링 실패(아무것도 못 튕김)");
            }
        }

        // 척력 모드일 때만 Shift 로 패링 발동
        bool shiftDown = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
        if (shiftDown)
        {
            if (_forceMode.IsPullMode)
            {
                if (debugLog) Debug.Log("[Parry] Shift 입력 무시 — 현재 인력 모드(척력 모드에서만 패링 가능)");
            }
            else if (_cooldownTimer > 0f)
            {
                if (debugLog) Debug.Log($"[Parry] Shift 입력 무시 — 쿨타임 {_cooldownTimer:F2}s 남음");
            }
            else if (IsParrying)
            {
                if (debugLog) Debug.Log("[Parry] Shift 입력 무시 — 이미 패링 윈도우 진행 중");
            }
            else
            {
                ActivateParry();
            }
        }
    }

    private void ActivateParry()
    {
        IsParrying     = true;
        _parryTimer    = parryDuration;
        _cooldownTimer = cooldown;

        if (debugLog) Debug.Log($"[Parry] 발동! 윈도우 {parryDuration}s, 쿨타임 {cooldown}s");

        FireRepel();
    }

    /// <summary>
    /// 공격 측이 호출. 패링 중이면 화면을 잠시 멈추고 true 를 반환합니다.
    /// 호출자는 true 를 받으면 데미지 적용을 건너뛰고, 투사체라면 방향을 반전합니다.
    /// </summary>
    public bool TryParry(GameObject attacker)
    {
        if (!IsParrying)
        {
            if (debugLog) Debug.Log($"[Parry] TryParry 실패 — 패링 중 아님 (attacker={(attacker != null ? attacker.name : "null")})");
            return false;
        }

        if (debugLog) Debug.Log($"[Parry] 성공! attacker={(attacker != null ? attacker.name : "null")} 남은 윈도우 {_parryTimer:F2}s");

        if (!_freezing)
            StartCoroutine(FreezeRoutine());

        return true;
    }

    private IEnumerator FreezeRoutine()
    {
        _freezing = true;
        float prevScale = Time.timeScale;
        Time.timeScale = 0f;
        if (debugLog) Debug.Log($"[Parry] Hitstop 시작 — timeScale {prevScale} → 0 ({freezeDuration}s)");
        yield return new WaitForSecondsRealtime(freezeDuration);
        // 다른 시스템이 timeScale 을 바꾼게 아니라면 원래 값으로 복원
        if (Mathf.Approximately(Time.timeScale, 0f))
            Time.timeScale = prevScale > 0f ? prevScale : 1f;
        if (debugLog) Debug.Log($"[Parry] Hitstop 종료 — timeScale {Time.timeScale} 복원");
        _freezing = false;
    }

    private void FireRepel()
    {
        Vector2 dir = MouseDirection();

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, rayCastRange);

        if (hit && hit.collider.CompareTag("Metal"))
            _rb.AddForce(-dir * metalJumpPower, ForceMode2D.Impulse);
        else
            _rb.AddForce(-dir * repelPower, ForceMode2D.Impulse);
    }

    /// <summary>플레이어 → 마우스 방향의 단위 벡터.</summary>
    private Vector2 MouseDirection()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return ((Vector2)(mouse - transform.position)).normalized;
    }
}