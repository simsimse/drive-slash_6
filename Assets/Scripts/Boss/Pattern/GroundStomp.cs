using System.Collections;
using UnityEngine;

public class GroundStomp : MonoBehaviour, IBossPattern
{
    public GameObject damageZonePrefab;
    public Transform damageZoneSpawnPoint;

    [Header("이펙트")]
    public GameObject stompEffectPrefab;
    public Vector3 stompEffectPosition = new Vector3(13f, -32f, 0f);
    public float effectDestroyTime = 2f;

    [Header("사운드")]
    public AudioClip stompSound;
    public float stompSoundVolume = 1f;

    public float chargeTime = 2f;
    public int damage = 30;

    private GameObject currentDamageZone;

    public Animator bossAnimator;
    public string groundStompTrigger = "isGS";
    public float stompStartDelay = 0.3f;
    public float stompEndDelay = 0.4f;

    public float PatternDuration
    {
        get { return stompStartDelay + chargeTime + stompEndDelay; }
    }

    public bool CanExecute()
    {
        return true;
    }

    public void Execute()
    {
        if (bossAnimator != null)
            bossAnimator.SetTrigger(groundStompTrigger);

        StartCoroutine(StompRoutine());
    }

    IEnumerator StompRoutine()
    {
        yield return new WaitForSeconds(stompStartDelay);

        currentDamageZone = Instantiate(
            damageZonePrefab,
            damageZoneSpawnPoint.position,
            Quaternion.identity
        );

        yield return new WaitForSeconds(chargeTime);

        if (currentDamageZone != null)
        {
            DamageZone zone = currentDamageZone.GetComponent<DamageZone>();

            if (zone != null)
                zone.GiveDamage(damage);

            Destroy(currentDamageZone);
        }

        PlayStompSound();
        SpawnStompEffect();

        yield return new WaitForSeconds(stompEndDelay);
    }

    void PlayStompSound()
    {
        if (stompSound == null)
            return;

        AudioSource.PlayClipAtPoint(
            stompSound,
            stompEffectPosition,
            stompSoundVolume
        );
    }

    void SpawnStompEffect()
    {
        if (stompEffectPrefab == null)
            return;

        GameObject effect = Instantiate(
            stompEffectPrefab,
            stompEffectPosition,
            stompEffectPrefab.transform.rotation
        );

        Destroy(effect, effectDestroyTime);
    }
}