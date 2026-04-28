using UnityEngine;

public class BossAI : MonoBehaviour
{
    public Animator bossAnimator;
    public Transform player;

    public float moveSpeed = 3f;
    public float moveTimeMin = 1f;
    public float moveTimeMax = 3.5f;
    public float idleTimeMin = 1f;
    public float idleTimeMax = 2.5f;

    private int moveDir;
    private bool isMoving;
    private float timer;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;

        if (player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        StartIdle();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (isMoving)
        {
            transform.Translate(Vector2.right * moveDir * moveSpeed * Time.deltaTime);

            if (timer <= 0)
                StartIdle();
        }
        else
        {
            if (timer <= 0)
                StartMove();
        }
    }

    void StartMove()
    {
        isMoving = true;

        // 이동 시작 순간에만 Player 방향 결정
        if (player != null)
        {
            if (player.position.x > transform.position.x)
                moveDir = 1;
            else
                moveDir = -1;
        }
        else
        {
            moveDir = Random.Range(0, 2) == 0 ? -1 : 1;
        }

        timer = Random.Range(moveTimeMin, moveTimeMax);

        bossAnimator.SetBool("isMove", true);

        transform.localScale = new Vector3(
            Mathf.Abs(originalScale.x) * moveDir,
            originalScale.y,
            originalScale.z
        );
    }

    void StartIdle()
    {
        isMoving = false;
        timer = Random.Range(idleTimeMin, idleTimeMax);

        bossAnimator.SetBool("isMove", false);
    }

    void ReverseDirection()
    {
        moveDir *= -1;

        transform.localScale = new Vector3(
            Mathf.Abs(originalScale.x) * moveDir,
            originalScale.y,
            originalScale.z
        );
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Ground (1)" ||
            collision.gameObject.name == "Ground (2)")
        {
            ReverseDirection();
        }
    }
}