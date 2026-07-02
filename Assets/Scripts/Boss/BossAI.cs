using System.Collections.Generic;
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
    public MonoBehaviour priorityPatternScript; // DragonClaw 넣기

    // StartPattern에서 실행 가능한 패턴 후보를 담는 재사용 버퍼
    private readonly List<IBossPattern> _executable = new List<IBossPattern>();

    [Header("이동 설정")]
    public float moveSpeed = 2f;
    public float moveTimeMin = 2f;
    public float moveTimeMax = 4f;

    private bool isMoving;

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
{
    isPattern = false;
    StartIdle();
    return;
}

    return;
}
else
{
    LookAtPlayer();

    if (isMoving)
    {
        MoveToPlayer();

        if (timer <= 0)
        {
            isMoving = false;
            bossAnimator.SetBool("isMove", false);
            StartPattern();
        }
    }
    else
    {
        if (timer <= 0)
            StartPattern();
    }
}
    }

    void StartIdle()
    {
        isPattern = false;

        // Idle 또는 Move 랜덤 선택
        if (Random.Range(0, 2) == 0)
        {
            isMoving = false;
            timer = Random.Range(idleTimeMin, idleTimeMax);
        }
        else
        {
            StartMove();
            return;
        }

        bossAnimator.SetBool("isAttack", false);
        bossAnimator.SetBool("isMove", false);
    }

void MoveToPlayer()
{
    if (player == null) return;

    int moveDir = player.position.x > transform.position.x ? 1 : -1;

    transform.Translate(
        Vector2.right * moveDir * moveSpeed * Time.deltaTime
    );
}
void StartMove()
{
    isMoving = true;

    timer = Random.Range(moveTimeMin, moveTimeMax);

    bossAnimator.SetBool("isMove", true);
}

    void StartPattern()
{
    if (boss != null && boss.hp <= 0)
        return;

    // 패턴 시작하면 이동 중지
    isMoving = false;
    bossAnimator.SetBool("isMove", false);

    isPattern = true;

    // 1. 우선순위 패턴 검사
    IBossPattern priorityPattern = priorityPatternScript as IBossPattern;

    if (priorityPattern != null && priorityPattern.CanExecute())
    {
        currentPatternScript = priorityPattern;
        timer = currentPatternScript.PatternDuration;
        currentPatternScript.Execute();
        return;
    }

    // 2. 일반 랜덤 패턴 — 지금 실행 가능한(CanExecute) 패턴만 후보로 모음
    if (patternScripts == null || patternScripts.Length == 0)
    {
        Debug.LogWarning("patternScripts 배열이 비어있습니다.");
        StartIdle();
        return;
    }

    _executable.Clear();
    foreach (MonoBehaviour mb in patternScripts)
    {
        IBossPattern p = mb as IBossPattern;
        if (p != null && p.CanExecute())
            _executable.Add(p);
    }

    if (_executable.Count == 0)
    {
        // 실행 가능한 패턴이 없으면 이번엔 대기
        StartIdle();
        return;
    }

    currentPatternScript = _executable[Random.Range(0, _executable.Count)];

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