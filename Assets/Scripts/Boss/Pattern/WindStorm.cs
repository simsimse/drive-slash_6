using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WindStorm : MonoBehaviour, IBossPattern
{
    public GameObject damageZonePrefab;
    public Transform[] spawnPoints; // 3개 위치 지정
    public Animator bossAnimator;
    public string windStormBool = "isWS";

    public float chargeTime = 2f;
    public int damage = 20;

    private List<GameObject> damageZones = new List<GameObject>();

    public float PatternDuration
    {
        get { return chargeTime; }
    }
    public bool CanExecute()
    {
        return true;
    }
    public void Execute()
    {
        if (bossAnimator != null)
        {
            bossAnimator.SetBool(windStormBool, true);
        }
        StartCoroutine(StomRoutine());
    }

    IEnumerator StomRoutine()
    {
        damageZones.Clear();

        // 1. 데미지 존 3개 생성
        foreach (Transform point in spawnPoints)
        {
            GameObject zone = Instantiate(
                damageZonePrefab,
                point.position,
                Quaternion.identity
            );

            damageZones.Add(zone);
        }

        // 2. 차징
        yield return new WaitForSeconds(chargeTime);

        // 3. 각각 데미지 적용
        foreach (GameObject zoneObj in damageZones)
        {
            if (zoneObj == null) continue;

            DamageZone zone = zoneObj.GetComponent<DamageZone>();

            if (zone != null)
                zone.GiveDamage(damage);
        }

        // 4. 전부 삭제
        foreach (GameObject zoneObj in damageZones)
        {
            if (zoneObj != null)
                Destroy(zoneObj);
        }

        damageZones.Clear();

        if (bossAnimator != null)
        {
            bossAnimator.SetBool(windStormBool, false);
        }
    }
}