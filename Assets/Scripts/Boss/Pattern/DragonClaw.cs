using System.Collections;
using UnityEngine;

public class DragonClaw : MonoBehaviour, IBossPattern
{
    [Header("참조")]
    public Transform player;
    public Transform bossTransform;
    public Transform clawSpawnPoint;
    public Animator bossAnimator;
    public string clawTriger = "isClaw";

    [Header("데미지존")]
    public GameObject clawZonePrefab;

    [Header("이펙트")]
    public GameObject clawEffectPrefab;
    public float effectDestroyTime = 2f;

    [Header("사운드")]
    public AudioClip clawSound;
    [Range(0f, 1f)]
    public float clawSoundVolume = 1f;

    [Header("패턴 설정")]
    public float triggerRange = 3f;
    public float duration = 1f;
    public int damage = 2;

    public float PatternDuration => duration;

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
        if (player == null || bossTransform == null)
            return false;

        float xDist = Mathf.Abs(player.position.x - bossTransform.position.x);

        return xDist <= triggerRange;
    }

    public void Execute()
    {
        if (bossAnimator != null)
            bossAnimator.SetTrigger(clawTriger);

        StartCoroutine(ClawRoutine());
    }

    IEnumerator ClawRoutine()
    {
        if (player == null || bossTransform == null || clawSpawnPoint == null)
            yield break;

        int dir = player.position.x > bossTransform.position.x ? 1 : -1;

        GameObject zoneObj = Instantiate(
            clawZonePrefab,
            clawSpawnPoint.position,
            Quaternion.identity
        );

        Vector3 scale = zoneObj.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        zoneObj.transform.localScale = scale;

        // 플레이어에게 회피 시간 제공
        yield return new WaitForSeconds(duration);

        // 데미지 적용
        DamageZone zone = zoneObj.GetComponent<DamageZone>();

        if (zone != null)
            zone.GiveDamage(damage);

        // 이펙트 생성
        SpawnClawEffect(zoneObj.transform.position, dir);

        // 효과음 재생
        if (clawSound != null)
        {
            AudioSource.PlayClipAtPoint(
                clawSound,
                zoneObj.transform.position,
                clawSoundVolume
            );
        }

        Destroy(zoneObj);
    }

    void SpawnClawEffect(Vector3 position, int dir)
    {
        if (clawEffectPrefab == null)
            return;

        GameObject effect = Instantiate(
            clawEffectPrefab,
            position,
            clawEffectPrefab.transform.rotation
        );

        // 클로 방향에 맞춰 이펙트 좌우 반전
        Vector3 scale = effect.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        effect.transform.localScale = scale;

        Destroy(effect, effectDestroyTime);
    }
}