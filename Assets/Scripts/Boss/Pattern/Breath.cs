using System.Collections;
using UnityEngine;

public class Breath : MonoBehaviour, IBossPattern
{
    public Animator bossAnimator;

    public GameObject damageZonePrefab;
    public Transform damageZoneSpawnPoint;

    public Transform bossRoot;
    public Transform outsidePoint;

    public float flyTime = 1.5f;     // 날아가는 애니메이션 시간
    public float chargeTime = 6f;    // 데미지 존 유지 시간
    public float landTime = 1.5f;    // 내려오는 애니메이션 시간

    public int damage = 2000;

    private GameObject currentDamageZone;
    private Vector3 originalPosition;

    // BossAI가 이 패턴이 총 몇 초 걸리는지 알 수 있게 함
    public float PatternDuration
    {
        get { return flyTime + chargeTime + landTime; }
    }

    public void Execute()
    {
        StartCoroutine(BreathRoutine());
    }

    IEnumerator BreathRoutine()
    {
        // 1. 보스 원래 위치 저장
        originalPosition = bossRoot.position;

        // 2. 날아가는 애니메이션 실행
        if (bossAnimator != null)
            bossAnimator.SetTrigger("isFly");

        // 3. fly 애니메이션이 보일 시간 대기
        yield return new WaitForSeconds(flyTime);

        // 4. 보스를 맵 밖으로 이동
        bossRoot.position = outsidePoint.position;

        // 5. 데미지 존 생성
        currentDamageZone = Instantiate(
            damageZonePrefab,
            damageZoneSpawnPoint.position,
            Quaternion.identity
        );

        // 6. 데미지 존 유지 시간 대기
        yield return new WaitForSeconds(chargeTime);

        // 7. 데미지 적용
        if (currentDamageZone != null)
        {
            DamageZone zone = currentDamageZone.GetComponent<DamageZone>();

            if (zone != null)
                zone.GiveDamage(damage);
        }

        // 8. 데미지 존 제거
        if (currentDamageZone != null)
            Destroy(currentDamageZone);

        // 9. 보스를 원래 위치로 복귀
        bossRoot.position = originalPosition;

        // 10. 내려오는 애니메이션 실행
        if (bossAnimator != null)
            bossAnimator.SetTrigger("land");

        // 11. 내려오는 애니메이션이 끝날 때까지 대기
        yield return new WaitForSeconds(landTime);
    }
}