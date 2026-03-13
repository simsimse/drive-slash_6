using UnityEngine;

/// <summary>
/// 마우스 좌클릭으로 대거를 배치합니다.
/// 실제 생성/소비는 DaggerManager에 위임합니다.
/// </summary>
[RequireComponent(typeof(DaggerManager))]
public class DaggerThrower : MonoBehaviour
{
    public GameObject CurrentDagger { get; private set; }

    private DaggerManager _manager;

    void Awake()
    {
        _manager = GetComponent<DaggerManager>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && CurrentDagger == null)
            ThrowDagger();
    }

    private void ThrowDagger()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CurrentDagger = _manager.SpawnDagger(mousePos);
    }

    /// <summary>대시 완료 후 대거를 소비합니다.</summary>
    public void ConsumeDagger()
    {
        _manager.RemoveDagger(CurrentDagger);
        CurrentDagger = null;
    }
}
