using System.Collections;
using UnityEngine;

public class GroundStomp : MonoBehaviour, IBossPattern
{
    public GameObject damageZonePrefab;
    public Transform damageZoneSpawnPoint;

    public float chargeTime = 2f;
    public int damage = 20;

    private GameObject currentDamageZone;

    public void Execute()
    {
        StartCoroutine(StompRoutine());
    }

    IEnumerator StompRoutine()
    {
        // 1. 빨간 데미지 존 생성
        currentDamageZone = Instantiate(
            damageZonePrefab,
            damageZoneSpawnPoint.position,
            Quaternion.identity
        );

        // 2. 차징 시간 대기
        yield return new WaitForSeconds(chargeTime);

        // 3. 데미지 존 안에 있는 플레이어에게 데미지
        DamageZone zone = currentDamageZone.GetComponent<DamageZone>();

        if (zone != null)
        {
            zone.GiveDamage(damage);
        }

        // 4. 데미지 존 제거
        Destroy(currentDamageZone);
    }
}