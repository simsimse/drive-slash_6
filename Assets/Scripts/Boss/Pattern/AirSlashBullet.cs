using UnityEngine;

public class AirSlashBullet : MonoBehaviour
{
    private Vector2 moveDir;
    private float moveSpeed;
    private int damage;

    public float lifeTime = 4f;

    public void SetDirection(Vector2 dir, float speed, int dmg)
    {
        moveDir = dir.normalized;
        moveSpeed = speed;
        damage = dmg;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerMovement player = collision.GetComponent<PlayerMovement>();

        if (player != null)
        {
            player.TakeDamage(damage); // 여기만 핵심
            Debug.Log("AirSlash 피격! 현재 HP: " + player.hp);

            Destroy(gameObject);
        }
    }
}