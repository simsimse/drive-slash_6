using System.Collections;
using UnityEngine;

public class GroundStomp : MonoBehaviour, IBossPattern
{
    public GameObject damageZonePrefab;
    public Transform damageZoneSpawnPoint;

    public float chargeTime = 2f;
    public int damage = 30;

    private GameObject currentDamageZone;

    public Animator bossAnimator;
    public string groundStompTrigger = "isGS";
    public float stompStartDelay = 0.3f;

    public float stompEndDelay = 0.4f;

    public float PatternDuration
    {
        get { return stompStartDelay + chargeTime + stompEndDelay;}
    }

    public bool CanExecute()
    {
        return true;
    }
    public void Execute()
    {
        if (bossAnimator != null)
        {
            bossAnimator.SetTrigger(groundStompTrigger);
        }

        StartCoroutine(StompRoutine());
    }

    IEnumerator StompRoutine()
{
    // 애니메이션 선딜
    yield return new WaitForSeconds(stompStartDelay);

    // 데미지 존 생성
    currentDamageZone = Instantiate(
        damageZonePrefab,
        damageZoneSpawnPoint.position,
        Quaternion.identity
    );

    // 차징 시간
    yield return new WaitForSeconds(chargeTime);

    // 데미지 적용
    DamageZone zone = currentDamageZone.GetComponent<DamageZone>();

    if (zone != null)
        zone.GiveDamage(damage);

    // 데미지 존 제거
    Destroy(currentDamageZone);

    // 내려찍은 후 후딜레이
    yield return new WaitForSeconds(stompEndDelay);
}
}