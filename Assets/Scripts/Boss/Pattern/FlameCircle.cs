using System.Collections;
using UnityEngine;

public class FlameCircle : MonoBehaviour, IBossPattern
{
    public Transform player;
    public float spawnRadiusAroundPlayer = 3f;

    [Header("데미지 존")]
    public GameObject damageZonePrefab;
    public SpriteRenderer corkBoard;

    [Header("폭발 이펙트")]
    public GameObject outerFireEffectPrefab;
    public GameObject innerFireEffectPrefab;

    [Header("사운드")]
    public AudioClip flameCircleStartSound;
    public float flameCircleStartVolume = 1f;

    public AudioClip flameCircleSound;
    public float flameCircleSoundVolume = 1f;

    [Header("패턴 설정")]
    public int spawnCount = 6;
    public float warningTime = 1.2f;
    public float interval = 0.3f;
    public int damage = 3;

    public Animator bossAnimator;
    public string flameCircleBool = "isFC";

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

    public float PatternDuration
    {
        get { return spawnCount * (warningTime + interval); }
    }

    public void Execute()
    {
        if (bossAnimator != null)
            bossAnimator.SetBool(flameCircleBool, true);

        if (flameCircleStartSound != null)
        {
            AudioSource.PlayClipAtPoint(
                flameCircleStartSound,
                transform.position,
                flameCircleStartVolume
            );
        }

        StartCoroutine(FlameCircleRoutine());
    }

    IEnumerator FlameCircleRoutine()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 randomPos = GetRandomPositionInCorkBoard();

            GameObject zoneObj = Instantiate(
                damageZonePrefab,
                randomPos,
                Quaternion.identity
            );

            yield return new WaitForSeconds(warningTime);

            if (zoneObj != null)
            {
                Vector3 effectPos = zoneObj.transform.position;

                CreateFireEffect(effectPos);

                DamageZone zone = zoneObj.GetComponent<DamageZone>();

                if (zone != null)
                    zone.GiveDamage(damage);

                if (flameCircleSound != null)
                {
                    AudioSource.PlayClipAtPoint(
                        flameCircleSound,
                        effectPos,
                        flameCircleSoundVolume
                    );
                }

                Destroy(zoneObj);
            }

            yield return new WaitForSeconds(interval);
        }

        if (bossAnimator != null)
            bossAnimator.SetBool(flameCircleBool, false);
    }

    void CreateFireEffect(Vector3 effectPos)
    {
        if (outerFireEffectPrefab != null)
        {
            GameObject outer = Instantiate(
                outerFireEffectPrefab,
                effectPos,
                Quaternion.identity
            );

            outer.transform.localScale = Vector3.one * 3f;
            SetParticleColor(outer, Color.red);
            Destroy(outer, 2f);
        }

        if (innerFireEffectPrefab != null)
        {
            GameObject inner = Instantiate(
                innerFireEffectPrefab,
                effectPos,
                Quaternion.identity
            );

            inner.transform.localScale = Vector3.one * 1.4f;
            SetParticleColor(inner, Color.yellow);
            Destroy(inner, 2f);
        }
    }

    void SetParticleColor(GameObject effectObj, Color color)
    {
        ParticleSystem[] particles = effectObj.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem ps in particles)
        {
            var main = ps.main;
            main.startColor = color;
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