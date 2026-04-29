using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 일정 간격마다 맵 임의 위치에 랜덤 핀을 생성합니다.
/// 플레이어 핀 + 랜덤 핀 합산이 maxTotalPins 미만일 때만 스폰합니다.
/// </summary>
/// <remarks>
/// [의존]
/// - DaggerManager.cs : ActiveDaggerCount(플레이어 배치 핀 수) 참조
/// [핀 종류]
/// - 노랑 핀: 데미지 0.5x, 속도 0.5x  (PinData 설정 필요)
/// - 빨강 핀: 일반 핀과 동일           (PinData 없거나 기본값)
/// - 보라 핀: 데미지 1.5x, 무적        (PinData 설정 필요)
/// </remarks>
public class RandomPinSpawner : MonoBehaviour
{
    [Header("핀 프리팹 (Inspector에서 할당)")]
    public GameObject yellowPinPrefab;   // 데미지 0.5x, 속도 0.5x
    public GameObject redPinPrefab;      // 일반 핀
    public GameObject purplePinPrefab;   // 데미지 1.5x, 슬래시 중 무적

    [Header("스폰 설정")]
    public float spawnInterval = 10f;
    public int   maxTotalPins  = 5;

    [Header("스폰 범위")]
    public float minX = -10f;
    public float maxX =  10f;
    public float minY =  -5f;
    public float maxY =   5f;

    private DaggerManager            _daggerManager;
    private readonly List<GameObject> _randomPins = new List<GameObject>();
    private float                    _timer       = 0f;

    void Awake()
    {
        _daggerManager = FindObjectOfType<DaggerManager>();
        if (_daggerManager == null)
            Debug.LogWarning("[RandomPinSpawner] DaggerManager를 찾을 수 없습니다.");
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= spawnInterval)
        {
            _timer = 0f;
            TrySpawn();
        }
    }

    private void TrySpawn()
    {
        // 이미 파괴된 핀 정리
        _randomPins.RemoveAll(p => p == null);

        int playerPins = _daggerManager != null ? _daggerManager.ActiveDaggerCount : 0;
        int total      = playerPins + _randomPins.Count;

        if (total >= maxTotalPins) return;

        GameObject prefab = PickRandomPrefab();
        if (prefab == null)
        {
            Debug.LogWarning("[RandomPinSpawner] 할당된 핀 프리팹이 없습니다.");
            return;
        }

        Vector2    pos = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        GameObject pin = Instantiate(prefab, pos, Quaternion.identity);
        _randomPins.Add(pin);
    }

    private GameObject PickRandomPrefab()
    {
        // null인 슬롯을 제외하고 랜덤 선택
        var available = new List<GameObject>();
        if (yellowPinPrefab != null) available.Add(yellowPinPrefab);
        if (redPinPrefab    != null) available.Add(redPinPrefab);
        if (purplePinPrefab != null) available.Add(purplePinPrefab);

        if (available.Count == 0) return null;
        return available[Random.Range(0, available.Count)];
    }
}
