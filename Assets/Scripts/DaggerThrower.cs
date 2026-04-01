using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 마우스 좌클릭으로 대거를 배치합니다.
/// 충전이 남아 있는 한 최대 3개까지 동시에 배치할 수 있습니다.
/// 실제 생성/소비는 DaggerManager에 위임합니다.
/// </summary>
/// <remarks>
/// [의존]
/// - DaggerManager.cs : 단검 생성(SpawnDagger) / 제거(RemoveDagger) 위임
/// [참조하는 곳]
/// - Dash.cs : CurrentDagger(대시 목표), ConsumeDagger()(대시 완료 후 소비) 사용
/// [주의] 좌클릭(MouseButton 0)을 Repeller.cs와 공유 → 동시 동작 충돌 가능
/// </remarks>
[RequireComponent(typeof(DaggerManager))]
public class DaggerThrower : MonoBehaviour
{
    // Dash.cs 호환용: 가장 최근에 배치된 단검을 반환합니다.
    public GameObject CurrentDagger => _activeDaggers.Count > 0
        ? _activeDaggers[_activeDaggers.Count - 1]
        : null;

    private DaggerManager        _manager;
    private readonly List<GameObject> _activeDaggers = new List<GameObject>();

    void Awake()
    {
        _manager = GetComponent<DaggerManager>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && _manager.HasCharge)
            ThrowDagger();
    }

    private void ThrowDagger()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GameObject dagger = _manager.SpawnDagger(mousePos);

        if (dagger != null)
            _activeDaggers.Add(dagger);
    }

    /// <summary>
    /// 대시 완료 후 단검을 소비합니다 (Dash.cs 호환).
    /// CurrentDagger(가장 최근 단검)를 제거합니다.
    /// </summary>
    public void ConsumeDagger()
    {
        if (CurrentDagger == null) return;

        GameObject toRemove = CurrentDagger;
        _activeDaggers.Remove(toRemove);
        _manager.RemoveDagger(toRemove); // 쿨타임 후 충전 회복
    }

    /// <summary>
    /// 특정 단검 오브젝트를 지정하여 소비합니다.
    /// Dash.OnTriggerEnter2D에서 충돌한 단검을 직접 넘길 때 사용합니다.
    /// </summary>
    public void ConsumeDagger(GameObject dagger)
    {
        if (dagger == null) return;

        _activeDaggers.Remove(dagger);
        _manager.RemoveDagger(dagger);
    }
}
