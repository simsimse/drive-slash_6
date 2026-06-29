using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WindStorm : MonoBehaviour, IBossPattern
{
    public GameObject damageZonePrefab;
    public Transform[] spawnPoints;

    [Header("이펙트")]
    public GameObject windStormEffectPrefab;
    public float effectDestroyTime = 2f;

    [Header("사운드")]
    public AudioClip windStormSound;
    public float windStormSoundVolume = 1f;

    public Animator bossAnimator;
    public string windStormBool = "isWS";

    public float chargeTime = 2f;
    public int damage = 20;

    private List<GameObject> damageZones = new List<GameObject>();

    private Vector3[] effectPositions =
    {
        new Vector3(-17f, -30f, 0f),
        new Vector3(10f, -30f, 0f),
        new Vector3(37f, -30f, 0f)
    };

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
            bossAnimator.SetBool(windStormBool, true);

        StartCoroutine(StomRoutine());
    }

    IEnumerator StomRoutine()
    {
        damageZones.Clear();

        foreach (Transform point in spawnPoints)
        {
            GameObject zone = Instantiate(
                damageZonePrefab,
                point.position,
                Quaternion.identity
            );

            damageZones.Add(zone);
        }

        yield return new WaitForSeconds(chargeTime);

        foreach (GameObject zoneObj in damageZones)
        {
            if (zoneObj == null) continue;

            DamageZone zone = zoneObj.GetComponent<DamageZone>();

            if (zone != null)
                zone.GiveDamage(damage);
        }

        // 데미지존이 끝나는 순간 사운드 재생
        if (windStormSound != null)
        {
            AudioSource.PlayClipAtPoint(
                windStormSound,
                effectPositions[1],
                windStormSoundVolume
            );
        }

        foreach (GameObject zoneObj in damageZones)
        {
            if (zoneObj != null)
                Destroy(zoneObj);
        }

        damageZones.Clear();

        SpawnWindStormEffects();

        if (bossAnimator != null)
            bossAnimator.SetBool(windStormBool, false);
    }

    void SpawnWindStormEffects()
    {
        if (windStormEffectPrefab == null)
            return;

        foreach (Vector3 pos in effectPositions)
        {
            GameObject effect = Instantiate(
                windStormEffectPrefab,
                pos,
                windStormEffectPrefab.transform.rotation
            );

            ParticleSystem[] systems = effect.GetComponentsInChildren<ParticleSystem>();

            foreach (ParticleSystem ps in systems)
            {
                var main = ps.main;
                main.simulationSpeed = 2f;
            }

            Destroy(effect, effectDestroyTime);
        }
    }
}