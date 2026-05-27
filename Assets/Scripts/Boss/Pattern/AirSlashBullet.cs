using UnityEngine;

public class AirSlashBullet : MonoBehaviour
{
    private Vector2 moveDir;
    private float moveSpeed;
    private int damage;
    private bool _deflected = false;

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
        // 튕겨낸 후엔 보스에만 데미지
        if (_deflected)
        {
            BossPart part = collision.GetComponentInParent<BossPart>();
            if (part != null)
            {
                Debug.Log($"[Parry] 튕겨낸 투사체 → BossPart({part.name}) 적중, dmg={damage}");
                part.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            Boss boss = collision.GetComponentInParent<Boss>();
            if (boss != null)
            {
                Debug.Log($"[Parry] 튕겨낸 투사체 → Boss({boss.name}) 적중, dmg={damage}");
                boss.TakeDamage(damage);
                Destroy(gameObject);
            }
            return;
        }

        PlayerMovement player = collision.GetComponent<PlayerMovement>();
        if (player == null) return;

        // 패링 윈도우면 튕겨내고 데미지는 무효화
        Repeller repeller = collision.GetComponent<Repeller>();
        if (repeller != null && repeller.TryParry(gameObject))
        {
            Deflect();
            return;
        }

        player.TakeDamage(damage);
        Debug.Log("AirSlash 피격! 현재 HP: " + player.hp);
        Destroy(gameObject);
    }

    private void Deflect()
    {
        _deflected = true;
        moveDir = -moveDir;

        float zRot = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, zRot);

        Debug.Log($"[Parry] 투사체 튕겨냄 — 새 방향 {moveDir}, 회전 {zRot:F1}°");
    }
}
