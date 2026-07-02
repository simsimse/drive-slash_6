using System.Collections;
using UnityEngine;

/// <summary>
/// 보스 HP 관리. 데미지를 받으면 HP를 감소시키고 0 이하면 파괴됩니다.
/// 사망 시 남아있는 BossPart 들도 순차적으로 폭발 연출과 함께 제거됩니다.
/// </summary>
/// <remarks>
/// [참조하는 곳]
/// - Dash.cs : OnTriggerEnter2D에서 TakeDamage() 호출
/// [태그] 이 오브젝트에는 "Boss" 태그가 필요합니다 (Dash.cs에서 CompareTag 사용)
/// </remarks>
public class Boss : MonoBehaviour
{
    public int hp = 20;

    [Header("사망 연출")]
    [Tooltip("보스 본체 폭발 이펙트 프리팹 (선택)")]
    public GameObject deathEffectPrefab;

    [Tooltip("이펙트 생성 위치 오프셋")]
    public Vector3 deathEffectOffset;

    [Tooltip("남은 BossPart 폭발 사이의 간격(초)")]
    public float partStaggerDelay = 0.15f;

    [Tooltip("파츠 연출 종료 후 본체 제거까지 대기 시간(초)")]
    public float finalDestroyDelay = 0.6f;

    [Header("피격 연출")]
    [Tooltip("피격 시 화면 흔들림 사용")]
    public bool  shakeOnHit      = true;
    public float hitShakeDuration  = 0.15f;
    public float hitShakeMagnitude = 0.2f;

    private bool isDying = false;

    public void TakeDamage(int damage)
    {
        if (isDying) return;

        hp -= damage;

        if (shakeOnHit && CameraShake.Instance != null)
            CameraShake.Instance.Shake(hitShakeDuration, hitShakeMagnitude);

        if (hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDying) return;
        isDying = true;

        BossAI ai = GetComponent<BossAI>();

        if (ai != null)
            ai.enabled = false;

        IBossPattern[] patterns = GetComponents<IBossPattern>();

        foreach (IBossPattern pattern in patterns)
        {
            MonoBehaviour mb = pattern as MonoBehaviour;
            if (mb != null)
                mb.StopAllCoroutines();
        }

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        BossPart[] parts = GetComponentsInChildren<BossPart>(true);

        foreach (BossPart part in parts)
        {
            if (part == null || part.IsDestroyed) continue;

            part.DestroyPart();

            if (partStaggerDelay > 0f)
                yield return new WaitForSeconds(partStaggerDelay);
        }

        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position + deathEffectOffset, Quaternion.identity);
        }

        if (finalDestroyDelay > 0f)
            yield return new WaitForSeconds(finalDestroyDelay);

        // 보스 처치 → 승리 UI 표시
        if (VictoryUI.Instance != null)
            VictoryUI.Instance.Show();

        Destroy(gameObject);
    }
}
