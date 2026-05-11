using UnityEngine;

/// <summary>
/// 보스의 파괴 가능한 부위(날개, 꼬리 등). 자체 HP를 가지며
/// HP가 0이 되면 지정된 GameObject 들을 모두 제거합니다. 본체 HP와는 별개입니다.
/// 여러 콜라이더가 하나의 HP 풀을 공유하려면 forwardTo 로 마스터를 지정합니다.
/// </summary>
/// <remarks>
/// [세팅]
/// - 부위 GameObject 에 Collider2D + "Boss" 태그 + 이 컴포넌트를 부착
/// - Visuals To Destroy 비우면 자기 자신(gameObject)만 제거
/// - 여러 파츠가 하나의 부위(예: 꼬리 3마디)라면 한 곳을 마스터로 두고
///   나머지 파츠는 Forward To 에 마스터를 지정 → 데미지·파괴가 마스터로 일원화
/// [참조하는 곳]
/// - Dash.cs : "Boss" 태그 콜라이더에서 BossPart 컴포넌트를 우선 검색해 데미지 라우팅
/// </remarks>
public class BossPart : MonoBehaviour
{
    [Header("부위 설정")]
    public string partName = "Part";

    [Tooltip("데미지를 이 BossPart 로 위임. 설정되어 있으면 hp/visualsToDestroy 는 무시됨.")]
    public BossPart forwardTo;

    [Tooltip("부위 HP (forwardTo 가 비어 있을 때만 사용)")]
    public int hp = 10;

    [Tooltip("HP 소진 시 한 번에 제거할 GameObject 들. 비어 있으면 자기 자신만 제거.")]
    public GameObject[] visualsToDestroy;

    public bool IsDestroyed { get; private set; } = false;

    public void TakeDamage(int damage)
    {
        if (IsDestroyed) return;

        if (forwardTo != null)
        {
            forwardTo.TakeDamage(damage);
            return;
        }

        hp -= damage;

        if (hp <= 0)
        {
            hp = 0;
            DestroyPart();
        }
    }

    private void DestroyPart()
    {
        IsDestroyed = true;

        if (visualsToDestroy != null && visualsToDestroy.Length > 0)
        {
            for (int i = 0; i < visualsToDestroy.Length; i++)
            {
                if (visualsToDestroy[i] != null)
                    Destroy(visualsToDestroy[i]);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
