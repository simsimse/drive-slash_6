using UnityEngine;

public class BossAI : MonoBehaviour
{
    private Boss boss;
    public Animator bossAnimator;
    public Transform player;

    public float idleTimeMin = 6f;
    public float idleTimeMax = 8f;

    public float patternDuration = 3f;

    private float timer;
    private bool isPattern;

    private int facingDir = 1;
    private Vector3 originalScale;

    public MonoBehaviour[] patternScripts;
    private IBossPattern currentPatternScript;

    void Start()
    {
        boss = GetComponentInChildren<Boss>();
        originalScale = transform.localScale;

        if (player == null)
        {
            GameObject p = GameObject.Find("Player");
            if (p != null)
                player = p.transform;
        }

        StartIdle();
    }

    void Update()
    {
        if (boss == null || boss.hp <= 0)
        {
            StopAllCoroutines();
            enabled = false;
            return;
        }

        timer -= Time.deltaTime;

        if (isPattern)
        {
            if (timer <= 0)
                StartIdle();
        }
        else
        {
            // Idle 상태일 때만 플레이어 바라보기
            LookAtPlayer();

            if (timer <= 0)
                StartPattern();
        }
    }

    void StartIdle()
    {
        isPattern = false;
        timer = Random.Range(idleTimeMin, idleTimeMax);

        bossAnimator.SetBool("isAttack", false);
    }

    void StartPattern()
    {
        if (boss != null && boss.hp <= 0)
        return;

        isPattern = true;

        if (patternScripts == null || patternScripts.Length == 0)
        {
            Debug.LogWarning("patternScripts 배열이 비어있습니다.");
            StartIdle();
            return;
        }

        int index = Random.Range(0, patternScripts.Length);

        currentPatternScript = patternScripts[index] as IBossPattern;

        if (currentPatternScript == null)
        {
            Debug.LogWarning("선택된 패턴이 IBossPattern을 구현하지 않음");
            StartIdle();
            return;
        }

        timer = currentPatternScript.PatternDuration;

        currentPatternScript.Execute();
    }

    void LookAtPlayer()
    {
        if (player == null) return;

        if (player.position.x > transform.position.x)
            facingDir = 1;
        else
            facingDir = -1;

        transform.localScale = new Vector3(
            Mathf.Abs(originalScale.x) * facingDir,
            originalScale.y,
            originalScale.z
        );
    }
}