using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Dash))]
[RequireComponent(typeof(ForceMode))]
public class Repeller : MonoBehaviour
{
    [Header("척력 설정")]
    public float repelPower = 15f;
    public float metalJumpPower = 20f;
    public float rayCastRange = 8f;

    [Header("패링 설정")]
    public float parryDuration = 0.5f;
    public float cooldown = 5f;
    public float freezeDuration = 0.15f;

    [Header("UI")]
    public Image parryCooldownImage;

    [Header("디버그")]
    public bool debugLog = true;
    public bool IsParrying { get; private set; } = false;

    private Rigidbody2D _rb;
    private Dash _dash;
    private ForceMode _forceMode;
    private float _parryTimer = 0f;
    private float _cooldownTimer = 0f;
    private bool _freezing = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _dash = GetComponent<Dash>();
        _forceMode = GetComponent<ForceMode>();
    }

    void Start()
    {
        UpdateCooldownUI();
    }

    void Update()
    {
        if (_dash.IsDashing) return;

        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= Time.unscaledDeltaTime;
            if (_cooldownTimer < 0f) _cooldownTimer = 0f;
        }

        UpdateCooldownUI();

        if (IsParrying)
        {
            _parryTimer -= Time.unscaledDeltaTime;
            if (_parryTimer <= 0f)
            {
                IsParrying = false;
                if (debugLog) Debug.Log("[Parry] 윈도우 종료");
            }
        }

        bool shiftDown = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);

        if (shiftDown)
        {
            if (_forceMode.IsPullMode)
            {
                if (debugLog) Debug.Log("[Parry] 인력 모드라 패링 불가");
            }
            else if (_cooldownTimer > 0f)
            {
                if (debugLog) Debug.Log($"[Parry] 쿨타임 {_cooldownTimer:F2}s 남음");
            }
            else if (!IsParrying)
            {
                ActivateParry();
            }
        }
    }

    private void UpdateCooldownUI()
    {
        if (parryCooldownImage == null) return;

        if (_cooldownTimer <= 0f)
        {
            parryCooldownImage.fillAmount = 1f;
        }
        else
        {
            parryCooldownImage.fillAmount = 1f - (_cooldownTimer / cooldown);
        }
    }

    private void ActivateParry()
    {
        IsParrying = true;
        _parryTimer = parryDuration;
        _cooldownTimer = cooldown;

        UpdateCooldownUI();

        if (debugLog) Debug.Log($"[Parry] 발동! 쿨타임 {cooldown}s");

        FireRepel();
    }

    public bool TryParry(GameObject attacker)
    {
        if (!IsParrying) return false;

        if (!_freezing)
            StartCoroutine(FreezeRoutine());

        return true;
    }

    private IEnumerator FreezeRoutine()
    {
        _freezing = true;

        float prevScale = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(freezeDuration);

        if (Mathf.Approximately(Time.timeScale, 0f))
            Time.timeScale = prevScale > 0f ? prevScale : 1f;

        _freezing = false;
    }

    private void FireRepel()
    {
        Vector2 dir = MouseDirection();

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, rayCastRange);

        if (hit && hit.collider.CompareTag("Metal"))
            _rb.AddForce(-dir * metalJumpPower, ForceMode2D.Impulse);
        else
            _rb.AddForce(-dir * repelPower, ForceMode2D.Impulse);
    }

    private Vector2 MouseDirection()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return ((Vector2)(mouse - transform.position)).normalized;
    }
}