using System.Collections;
using UnityEngine;

public class SmallBreath : MonoBehaviour, IBossPattern
{
    [Header("참조")]
    public Transform bossTransform;
    public Transform breathSpawnPoint;
    public Animator bossAnimator;
    public string smallBreathBool = "isSB";

    [Header("데미지존")]
    public GameObject breathZonePrefab;

    [Header("브레스 이펙트")]
    public GameObject breathParticlePrefab;
    public Vector3 breathParticleOffset = Vector3.zero;
    public Vector3 breathParticleRotation = Vector3.zero;
    public Vector3 breathParticleScale = new Vector3(4f, 4f, 1f);

    [Header("사운드")]
    public AudioClip breathStartSound;
    public float breathStartSoundVolume = 1f;

    [Header("패턴 설정")]
    public float startDelay = 0.6f;
    public float duration = 4f;
    public int tickDamage = 1;
    public float tickInterval = 0.5f;

    private GameObject currentBreathZone;
    private GameObject currentBreathParticle;

    public float PatternDuration
    {
        get { return startDelay + duration; }
    }

    public bool CanExecute()
    {
        return true;
    }

    public void Execute()
    {
        if (bossAnimator != null)
            bossAnimator.SetBool(smallBreathBool, true);

        StartCoroutine(BreathRoutine());
    }

    IEnumerator BreathRoutine()
    {
        if (bossTransform == null || breathSpawnPoint == null || breathZonePrefab == null)
            yield break;

        yield return new WaitForSeconds(startDelay);

        int dir = bossTransform.localScale.x >= 0 ? 1 : -1;

        Quaternion zoneRotation;

        if (dir == 1)
            zoneRotation = Quaternion.Euler(0f, 0f, 90f);
        else
            zoneRotation = Quaternion.Euler(0f, 0f, -90f);

        currentBreathZone = Instantiate(
            breathZonePrefab,
            breathSpawnPoint.position,
            zoneRotation
        );

        currentBreathZone.transform.localScale =
            breathZonePrefab.transform.localScale;

        SmallBreathDamageZone zone =
            currentBreathZone.GetComponent<SmallBreathDamageZone>();

        if (zone != null)
        {
            zone.tickDamage = tickDamage;
            zone.tickInterval = tickInterval;
        }

        SpawnBreathParticle(dir);

        if (breathStartSound != null)
        {
            AudioSource.PlayClipAtPoint(
                breathStartSound,
                breathSpawnPoint.position,
                breathStartSoundVolume
            );
        }

        yield return new WaitForSeconds(duration);

        if (currentBreathZone != null)
            Destroy(currentBreathZone);

        if (currentBreathParticle != null)
            Destroy(currentBreathParticle);

        if (bossAnimator != null)
            bossAnimator.SetBool(smallBreathBool, false);
    }

    void SpawnBreathParticle(int dir)
    {
        if (breathParticlePrefab == null)
            return;

        Vector3 particlePos = breathSpawnPoint.position;

        particlePos += new Vector3(
            breathParticleOffset.x * dir,
            breathParticleOffset.y,
            breathParticleOffset.z
        );

        Quaternion particleRotation = Quaternion.Euler(
            breathParticleRotation.x,
            breathParticleRotation.y,
            breathParticleRotation.z * dir
        );

        currentBreathParticle = Instantiate(
            breathParticlePrefab,
            particlePos,
            particleRotation
        );

        currentBreathParticle.transform.localScale = breathParticleScale;
    }
}