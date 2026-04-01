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
    private readonly List<Transform> _daggers      = new List<Transform>();
    private readonly Queue<Coroutine> _rechargeQueue = new Queue<Coroutine>();

    // ── 외부 읽기 전용 프로퍼티 ───────────────────────────────
    public int  CurrentCharges => _currentCharges;
    public int  MaxCharges     => maxCharges;
    public bool HasCharge      => _currentCharges > 0;

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

        if (_currentCharges < maxCharges)
        {
            Coroutine c = StartCoroutine(RechargeRoutine());
            _rechargeQueue.Enqueue(c);
        }
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
        foreach (Coroutine c in _rechargeQueue)
            if (c != null) StopCoroutine(c);

        _rechargeQueue.Clear();
        _currentCharges = maxCharges;
        OnChargeChanged?.Invoke(_currentCharges, maxCharges);
    }

    // ── 내부 코루틴 ────────────────────────────────────────────

    private IEnumerator RechargeRoutine()
    {
        yield return new WaitForSeconds(rechargeCooldown);

        if (_currentCharges < maxCharges)
        {
            _currentCharges++;
            OnChargeChanged?.Invoke(_currentCharges, maxCharges);
        }

        if (_rechargeQueue.Count > 0)
            _rechargeQueue.Dequeue();
    }
}
