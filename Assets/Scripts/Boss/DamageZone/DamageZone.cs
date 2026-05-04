using UnityEngine;

public class DamageZone : MonoBehaviour
{
    private bool playerInside;
    private PlayerMovement playerMovement;

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("데미지존에 들어옴: " + collision.gameObject.name);

        PlayerMovement pm = collision.GetComponent<PlayerMovement>();

        if (pm != null)
        {
            playerInside = true;
            playerMovement = pm;
            Debug.Log("PlayerMovement 감지 성공");
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("데미지존에서 나감: " + collision.gameObject.name);

        if (collision.GetComponent<PlayerMovement>() != null)
        {
            playerInside = false;
            playerMovement = null;
        }
    }

    public void GiveDamage(int damage)
    {
        Debug.Log("GiveDamage 호출됨");

        if (playerInside && playerMovement != null)
        {
            playerMovement.hp -= damage;
            Debug.Log("데미지 적용됨! 현재 HP: " + playerMovement.hp);
        }
        else
        {
            Debug.LogWarning("데미지 실패: playerInside=" + playerInside + ", playerMovement=" + playerMovement);
        }
    }
}