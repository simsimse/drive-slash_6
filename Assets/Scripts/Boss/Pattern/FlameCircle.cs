using System.Collections;
using UnityEngine;

public class FlameCircle : MonoBehaviour, IBossPattern
{
    public Transform player;
    public float spawnRadiusAroundPlayer = 3f;

    [Header("데미지 존")]
    public GameObject damageZonePrefab;   // 원형 데미지 존 프리팹
    public SpriteRenderer corkBoard;      // cork_0 오브젝트의 SpriteRenderer

    [Header("패턴 설정")]
    public int spawnCount = 6;             // 총 6번 발동
    public float warningTime = 1.2f;       // 데미지 존이 생기고 터지기까지 시간
    public float interval = 0.3f;          // 다음 원이 나오기까지 간격
    public int damage = 3;


    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.Find("Player");
            if (p != null)
                player = p.transform;
        }
    }

    public bool CanExecute()
    {
        return true;
    }

    // BossAI가 패턴 전체 시간을 알 수 있게 하는 값
    public float PatternDuration
    {
        get { return spawnCount * (warningTime + interval); }
    }

    public void Execute()
    {
        StartCoroutine(FlameCircleRoutine());
    }

    IEnumerator FlameCircleRoutine()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            // 1. 코르크 보드 범위 안에서 랜덤 위치 뽑기
            Vector3 randomPos = GetRandomPositionInCorkBoard();

            // 2. 데미지 존 생성
            GameObject zoneObj = Instantiate(
                damageZonePrefab,
                randomPos,
                Quaternion.identity
            );

            // 3. 경고 시간 대기
            yield return new WaitForSeconds(warningTime);

            // 4. 데미지 적용
            if (zoneObj != null)
            {
                DamageZone zone = zoneObj.GetComponent<DamageZone>();

                if (zone != null)
                    zone.GiveDamage(damage);
            }

            // 5. 데미지 존 삭제
            if (zoneObj != null)
                Destroy(zoneObj);

            // 6. 다음 원 생성 전 짧은 대기
            yield return new WaitForSeconds(interval);
        }
    }

    Vector3 GetRandomPositionInCorkBoard()
    {
        Bounds bounds = corkBoard.bounds;

        Vector2 center;

        if (player != null)
            center = player.position;
        else
            center = bounds.center;

        Vector2 randomOffset = Random.insideUnitCircle * spawnRadiusAroundPlayer;

        float x = center.x + randomOffset.x;
        float y = center.y + randomOffset.y;

        x = Mathf.Clamp(x, bounds.min.x, bounds.max.x);
        y = Mathf.Clamp(y, bounds.min.y, bounds.max.y);

        return new Vector3(x, y, 0f);
    }
}