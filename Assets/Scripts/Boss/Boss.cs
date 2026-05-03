using UnityEngine;

/// <summary>
/// 보스 HP 관리. 데미지를 받으면 HP를 감소시키고 0 이하면 파괴됩니다.
/// </summary>
/// <remarks>
/// [참조하는 곳]
/// - Dash.cs : OnTriggerEnter2D에서 TakeDamage() 호출
/// [태그] 이 오브젝트에는 "Boss" 태그가 필요합니다 (Dash.cs에서 CompareTag 사용)
/// </remarks>
public class Boss : MonoBehaviour
{
    public int hp = 20;
    
    public void TakeDamage(int damage)
    {
        hp -= damage;

        if (hp <= 0)
        {
            Die();
        }
        
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
