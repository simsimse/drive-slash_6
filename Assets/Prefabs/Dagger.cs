using UnityEngine;

/// <summary>
/// 대거 오브젝트 자체의 데이터만 보유합니다.
/// 등록/해제는 DaggerManager가 전담합니다.
/// </summary>
public class Dagger : MonoBehaviour
{
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}