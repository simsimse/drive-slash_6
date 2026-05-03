using UnityEngine;

public class BossAI : MonoBehaviour
{
    public Animator bossAnimator;
    public Transform player;

    public GameObject patternMarker; // 원 표시 오브젝트

    public float idleTimeMin = 6f;
    public float idleTimeMax = 8f;

    public float patternDuration = 3f; // 패턴 지속 시간

    private float timer;
    private bool isPattern;
    private int currentPattern;

    private int facingDir = 1;
    private Vector3 originalScale;

    public MonoBehaviour[] patternScripts;
    private IBossPattern currentPatternScript;


    void Start()
    {
        originalScale = transform.localScale;

        if (player == null)
        {
            GameObject p = GameObject.Find("Player");
            if (p != null)
                player = p.transform;
        }

        patternMarker.SetActive(false);

        StartIdle();
    }

    void Update()
    {
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

        patternMarker.SetActive(false);
    }

   void StartPattern()
    {
        isPattern = true;
        timer = patternDuration;

        // 1. 배열 체크
        if (patternScripts == null || patternScripts.Length == 0)
        {
            Debug.LogWarning("patternScripts 배열이 비어있습니다.");
            StartIdle();
            return;
        }

        // 2. 안전하게 인덱스 선택
        int index = Random.Range(0, patternScripts.Length);

        // 3. 인터페이스 캐스팅
        currentPatternScript = patternScripts[index] as IBossPattern;

        // 4. 캐스팅 실패 체크 (중요)
        if (currentPatternScript == null)
        {
            Debug.LogWarning("선택된 패턴이 IBossPattern을 구현하지 않음");
            StartIdle();
            return;
        }

        // 5. 패턴 실행
        currentPatternScript.Execute();

        // 6. 마커 표시
        ShowPatternMarker(index + 1);
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

    void ShowPatternMarker(int pattern)
    {
        patternMarker.SetActive(true);

        SpriteRenderer sr = patternMarker.GetComponent<SpriteRenderer>();

        if (pattern == 1)
            sr.color = Color.yellow;
        else if (pattern == 2)
            sr.color = Color.blue;
        else if (pattern == 3)
            sr.color = new Color(1f, 0.5f, 0f); // 주황
    }
}