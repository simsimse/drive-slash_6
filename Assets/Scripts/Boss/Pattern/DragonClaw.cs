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
        {
            bossAnimator.SetTrigger(clawTriger);
        }
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

    // 데미지존이 1초 동안 보이면서 플레이어에게 회피 시간을 줌
    yield return new WaitForSeconds(duration);

    // 사라지기 직전에 데미지 판정
    DamageZone zone = zoneObj.GetComponent<DamageZone>();

    if (zone != null)
        zone.GiveDamage(damage);

    Destroy(zoneObj);
}
}