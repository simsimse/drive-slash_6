using System.Collections;
using UnityEngine;

public class SmallBreathDamageZone : MonoBehaviour
{
    public int tickDamage = 1;
    public float tickInterval = 0.5f;

    private Coroutine damageRoutine;
    private PlayerMovement player;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement foundPlayer =
            other.GetComponentInParent<PlayerMovement>();

        if (foundPlayer != null)
        {
            player = foundPlayer;

            if (damageRoutine == null)
                damageRoutine = StartCoroutine(TickDamageRoutine());
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (player != null)
            return;

        PlayerMovement foundPlayer =
            other.GetComponentInParent<PlayerMovement>();

        if (foundPlayer != null)
        {
            player = foundPlayer;

            if (damageRoutine == null)
                damageRoutine = StartCoroutine(TickDamageRoutine());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        PlayerMovement foundPlayer =
            other.GetComponentInParent<PlayerMovement>();

        if (foundPlayer != null && foundPlayer == player)
        {
            player = null;

            if (damageRoutine != null)
            {
                StopCoroutine(damageRoutine);
                damageRoutine = null;
            }
        }
    }

    IEnumerator TickDamageRoutine()
    {
        while (player != null)
        {
            player.TakeDamage(tickDamage);
            yield return new WaitForSeconds(tickInterval);
        }

        damageRoutine = null;
    }
}