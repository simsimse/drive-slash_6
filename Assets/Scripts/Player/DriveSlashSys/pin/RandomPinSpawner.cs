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
    [Tooltip("스폰 영역을 정의하는 오브젝트. 하이어라키에서 빈 오브젝트의 위치와 Scale(X=가로, Y=세로)로 영역을 맞추고 여기에 할당하세요.")]
    public Transform spawnArea;

    [Tooltip("영역을 안쪽으로 줄이는 여백(가로, 세로). 값이 클수록 스폰/설치 범위가 작아집니다.")]
    public Vector2 areaPadding = new Vector2(0f, 3f);

    [Tooltip("spawnArea가 비어있을 때 사용하는 fallback 범위")]
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

        Vector2    pos = GetRandomPosition();
        GameObject pin = Instantiate(prefab, pos, Quaternion.identity);
        _randomPins.Add(pin);
    }

    private Vector2 GetRandomPosition()
    {
        Bounds b = SpawnBounds;
        return new Vector2(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y));
    }

    /// <summary>스폰 영역 오브젝트가 할당돼 있는지.</summary>
    public bool HasSpawnArea => spawnArea != null;

    /// <summary>
    /// 패딩이 적용되지 않은 원본 영역 Bounds. (마우스 설치 범위용 — 오브젝트 크기 그대로)
    /// </summary>
    public Bounds AreaBounds => spawnArea != null
        ? GetAreaBounds(spawnArea)
        : new Bounds(
            new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f),
            new Vector3(maxX - minX, maxY - minY, 0f));

    /// <summary>
    /// 랜덤 스폰용 Bounds. AreaBounds에서 areaPadding만큼 안쪽으로 줄어든 범위입니다.
    /// </summary>
    public Bounds SpawnBounds
    {
        get
        {
            Bounds b = AreaBounds;

            // 여백만큼 양쪽에서 줄임 (음수가 되지 않게 clamp)
            Vector3 size = b.size;
            size.x = Mathf.Max(0f, size.x - areaPadding.x * 2f);
            size.y = Mathf.Max(0f, size.y - areaPadding.y * 2f);
            b.size = size;
            return b;
        }
    }

    // 스폰 영역 계산: Renderer가 있으면 보이는 크기, 없으면 lossyScale 기준
    private static Bounds GetAreaBounds(Transform area)
    {
        var renderer = area.GetComponent<Renderer>();
        if (renderer != null)
            return renderer.bounds;

        return new Bounds(area.position, area.lossyScale);
    }

    // 씬 뷰에서 실제 스폰/설치 범위를 시각적으로 확인 (오프셋 반영)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Bounds b = SpawnBounds;
        Gizmos.DrawWireCube(b.center, b.size);
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
