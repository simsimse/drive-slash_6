using UnityEngine;

public class DamageZoneBlink : MonoBehaviour
{
    public float minAlpha = 0.5f;
    public float maxAlpha = 1f;

    public float startSpeed = 1f;
    public float endSpeed = 10f;
    public float duration = 2f;

    private SpriteRenderer sr;
    private float timer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        // 패턴이 새로 시작될 때마다 초기화
        timer = 0f;

        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            Color c = sr.color;
            c.a = minAlpha;
            sr.color = c;
        }
    }

    void Update()
    {
        if (sr == null) return;

        timer += Time.deltaTime;

        float progress = Mathf.Clamp01(timer / duration);
        float currentSpeed = Mathf.Lerp(startSpeed, endSpeed, progress);

        float t = Mathf.PingPong(timer * currentSpeed, 1f);

        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }
}