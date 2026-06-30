using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 대거의 생성/등록/해제를 전담합니다.
/// 최대 3개 충전, 단검 회수(RemoveDagger) 시 쿨타임 후 충전 회복.
/// </summary>
/// <remarks>
/// [참조하는 곳]
/// - DaggerThrower.cs : SpawnDagger(), RemoveDagger(), HasCharge 사용
/// [같은 GameObject에 필요한 컴포넌트]
/// - DaggerThrower (RequireComponent로 강제)
/// </remarks>
public class DaggerManager : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject daggerPrefab;

    [Header("충전 설정")]
    [SerializeField] private int   maxCharges        = 3;
    [SerializeField] private float rechargeCooldown  = 3f;

    // ── 내부 상태 ──────────────────────────────────────────────
    private int                      _currentCharges;
    private readonly List<Transform> _daggers = new List<Transform>();
    // 회수됐지만 아직 채워지지 않은 충전 수. 한 번에 하나씩 순차적으로 회복됩니다.
    private int                      _pendingRecharges;
    // 현재 채워지고 있는 충전이 끝나는 시각(Time.time 기준).
    private float                    _nextRechargeEndTime;
    private Coroutine                _rechargeRoutine;

    // ── 외부 읽기 전용 프로퍼티 ───────────────────────────────
    public int   CurrentCharges    => _currentCharges;
    public int   MaxCharges        => maxCharges;
    public bool  HasCharge         => _currentCharges > 0;
    public float RechargeCooldown  => rechargeCooldown;
    /// <summary>충전 중인 슬롯이 하나라도 있는지.</summary>
    public bool  HasPendingRecharge => _pendingRecharges > 0;
    /// <summary>가장 먼저 끝날 충전의 남은 시간(초). 대기 중이 없으면 0.</summary>
    public float NextRechargeRemaining => HasPendingRecharge
        ? Mathf.Max(0f, _nextRechargeEndTime - Time.time)
        : 0f;
    /// <summary>가장 먼저 끝날 충전의 진행도 (0=시작, 1=완료). 대기 중이 없으면 1.</summary>
    public float NextRechargeProgress => (HasPendingRecharge && rechargeCooldown > 0f)
        ? 1f - (NextRechargeRemaining / rechargeCooldown)
        : 1f;
    /// <summary>현재 맵에 배치된 플레이어 핀 수 (RandomPinSpawner의 합산용)</summary>
    public int  ActiveDaggerCount => _daggers.Count;

    /// <summary>충전 수가 바뀔 때 호출됩니다. (현재 충전, 최대 충전)</summary>
    public event System.Action<int, int> OnChargeChanged;

    // ── Unity 생명 주기 ────────────────────────────────────────
    private void Awake()
    {
        _currentCharges = maxCharges;
    }

    // ── 공개 API ───────────────────────────────────────────────

    /// <summary>
    /// 충전이 남아 있으면 단검을 생성하고 충전 1 소비.
    /// 충전이 없으면 null 반환.
    /// </summary>
    public GameObject SpawnDagger(Vector2 position)
    {
        if (!HasCharge) return null;

        _currentCharges--;
        OnChargeChanged?.Invoke(_currentCharges, maxCharges);

        GameObject obj = Instantiate(daggerPrefab, position, Quaternion.identity);
        _daggers.Add(obj.transform);
        return obj;
    }

    /// <summary>
    /// 단검을 제거하고 목록에서 해제합니다.
    /// 쿨타임 후 충전을 1 회복합니다.
    /// </summary>
    public void RemoveDagger(GameObject dagger)
    {
        if (dagger == null) return;

        _daggers.Remove(dagger.transform);
        Destroy(dagger);

        // 회수 1건당 충전 1개 예약. 회복은 순차적으로(한 개씩) 진행됩니다.
        _pendingRecharges++;
        if (_rechargeRoutine == null)
            _rechargeRoutine = StartCoroutine(RechargeLoop());
    }

    /// <summary>가장 가까운 대거를 반환합니다. 없으면 null.</summary>
    public Transform GetNearestDagger()
    {
        Transform nearest = null;
        float     minDist = Mathf.Infinity;

        foreach (Transform d in _daggers)
        {
            if (d == null) continue;

            float dist = Vector2.Distance(transform.position, d.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = d;
            }
        }

        return nearest;
    }

    /// <summary>진행 중인 모든 리차지를 취소하고 충전을 최대로 초기화합니다.</summary>
    public void ResetCharges()
    {
        if (_rechargeRoutine != null)
        {
            StopCoroutine(_rechargeRoutine);
            _rechargeRoutine = null;
        }

        _pendingRecharges = 0;
        _currentCharges   = maxCharges;
        OnChargeChanged?.Invoke(_currentCharges, maxCharges);
    }

    // ── 내부 코루틴 ────────────────────────────────────────────

    /// <summary>
    /// 예약된 충전(_pendingRecharges)을 한 번에 하나씩 순차적으로 회복합니다.
    /// 한 칸이 다 차야 다음 칸이 차오르기 시작하므로 UI 표시와 일치합니다.
    /// </summary>
    private IEnumerator RechargeLoop()
    {
        while (_pendingRecharges > 0)
        {
            _nextRechargeEndTime = Time.time + rechargeCooldown;
            yield return new WaitForSeconds(rechargeCooldown);

            if (_currentCharges < maxCharges)
            {
                _currentCharges++;
                OnChargeChanged?.Invoke(_currentCharges, maxCharges);
            }

            _pendingRecharges--;
        }

        _rechargeRoutine = null;
    }
}
