using UnityEngine;

/// <summary>
/// 대거 오브젝트 자체의 데이터만 보유합니다.
/// 등록/해제는 DaggerManager가 전담합니다.
/// </summary>
/// <remarks>
/// [참조하는 곳]
/// - DaggerManager.cs : SpawnDagger()로 이 프리팹을 Instantiate
/// - Dash.cs          : OnTriggerEnter2D에서 "Dagger" 태그로 감지
/// [태그] 이 프리팹에는 "Dagger" 태그가 필요합니다 (Dash.cs에서 CompareTag 사용)
/// </remarks>
public class Dagger : MonoBehaviour
{
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}