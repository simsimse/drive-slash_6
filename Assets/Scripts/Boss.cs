using UnityEngine;

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
