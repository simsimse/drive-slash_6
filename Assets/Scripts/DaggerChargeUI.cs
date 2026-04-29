using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 단검 충전 수와 쿨타임을 화면에 표시하는 UI.
/// Inspector에서 스프라이트를 할당하여 이미지로 표시합니다.
/// Player 오브젝트에 붙이면 DaggerManager를 자동으로 찾습니다.
/// </summary>
public class DaggerChargeUI : MonoBehaviour
{
    [Header("스프라이트")]
    [SerializeField] private Sprite chargedSprite;    // 충전된 상태 이미지
    [SerializeField] private Sprite emptySprite;      // 비어있는 상태 이미지
    [SerializeField] private Sprite cooldownSprite;   // 쿨타임 중 이미지 (없으면 emptySprite 사용)

    [Header("위치 설정")]
    [SerializeField] private Vector2 anchorPosition = new Vector2(20f, -20f);

    [Header("슬롯 설정")]
    [SerializeField] private float slotSize    = 50f;
    [SerializeField] private float slotSpacing = 10f;

    [Header("쿨타임 오버레이 색상")]
    [SerializeField] private Color cooldownFillColor = new Color(1f, 1f, 1f, 0.8f);

    private DaggerManager _manager;
    private Canvas        _canvas;
    private Image[]       _slotImages;
    private Image[]       _cooldownFills;
    private Text          _cooldownText;

    void Start()
    {
        _manager = GetComponent<DaggerManager>();
        if (_manager == null)
            _manager = GetComponentInParent<DaggerManager>();

        if (_manager == null)
        {
            Debug.LogError("DaggerChargeUI: DaggerManager를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        BuildUI();
        RefreshSlots();
    }

    void Update()
    {
        RefreshSlots();
        UpdateCooldown();
    }

    // ── UI 빌드 ──────────────────────────────────────────────

    private void BuildUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("DaggerChargeCanvas");
        _canvas = canvasObj.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // 슬롯 컨테이너 (좌상단)
        GameObject container = CreateUIObject("ChargeContainer", canvasObj.transform);
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot     = new Vector2(0, 1);
        containerRect.anchoredPosition = anchorPosition;

        int max = _manager.MaxCharges;
        float totalWidth = max * slotSize + (max - 1) * slotSpacing;
        containerRect.sizeDelta = new Vector2(totalWidth, slotSize + 22f);

        // 슬롯 생성
        _slotImages    = new Image[max];
        _cooldownFills = new Image[max];

        for (int i = 0; i < max; i++)
        {
            float x = i * (slotSize + slotSpacing);

            // 메인 슬롯 이미지
            GameObject slot = CreateUIObject($"Slot_{i}", container.transform);
            RectTransform slotRect = slot.GetComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0, 1);
            slotRect.anchorMax = new Vector2(0, 1);
            slotRect.pivot     = new Vector2(0, 1);
            slotRect.anchoredPosition = new Vector2(x, 0);
            slotRect.sizeDelta = new Vector2(slotSize, slotSize);

            _slotImages[i] = slot.AddComponent<Image>();
            _slotImages[i].sprite = chargedSprite;
            _slotImages[i].preserveAspect = true;

            // 쿨타임 Fill 오버레이 (슬롯 위에 겹침)
            GameObject fill = CreateUIObject($"CooldownFill_{i}", slot.transform);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            _cooldownFills[i] = fill.AddComponent<Image>();
            _cooldownFills[i].sprite     = chargedSprite;
            _cooldownFills[i].type       = Image.Type.Filled;
            _cooldownFills[i].fillMethod = Image.FillMethod.Vertical;
            _cooldownFills[i].fillOrigin = 0; // bottom to top
            _cooldownFills[i].fillAmount = 0f;
            _cooldownFills[i].color      = cooldownFillColor;
            _cooldownFills[i].preserveAspect = true;
            _cooldownFills[i].gameObject.SetActive(false);
        }

        // 쿨타임 텍스트
        GameObject textObj = CreateUIObject("CooldownText", container.transform);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 0);
        textRect.pivot     = new Vector2(0.5f, 0);
        textRect.anchoredPosition = new Vector2(0, 0);
        textRect.sizeDelta = new Vector2(0, 20f);

        _cooldownText = textObj.AddComponent<Text>();
        _cooldownText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _cooldownText.fontSize  = 14;
        _cooldownText.alignment = TextAnchor.MiddleCenter;
        _cooldownText.color     = Color.white;
        _cooldownText.text      = "";
    }

    private GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    // ── 갱신 ─────────────────────────────────────────────────

    private void RefreshSlots()
    {
        int current = _manager.CurrentCharges;
        int max     = _manager.MaxCharges;

        for (int i = 0; i < max; i++)
        {
            if (i < current)
            {
                // 충전됨 — chargedSprite 표시
                _slotImages[i].sprite = chargedSprite;
                _slotImages[i].color  = Color.white;
                _cooldownFills[i].gameObject.SetActive(false);
            }
            else if (i == current && _manager.IsRecharging)
            {
                // 쿨타임 중 — emptySprite 배경 + Fill 오버레이로 진행률 표시
                _slotImages[i].sprite = emptySprite;
                _slotImages[i].color  = Color.white;

                _cooldownFills[i].gameObject.SetActive(true);
                _cooldownFills[i].sprite     = cooldownSprite != null ? cooldownSprite : chargedSprite;
                _cooldownFills[i].fillAmount = _manager.RechargeTimer / _manager.RechargeCooldown;
            }
            else
            {
                // 비어있음 — emptySprite 표시
                _slotImages[i].sprite = emptySprite;
                _slotImages[i].color  = Color.white;
                _cooldownFills[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateCooldown()
    {
        if (_manager.IsRecharging)
        {
            float remaining = _manager.RechargeCooldown - _manager.RechargeTimer;
            _cooldownText.text = remaining.ToString("F1") + "s";
        }
        else
        {
            _cooldownText.text = "";
        }
    }

    void OnDestroy()
    {
        if (_canvas != null)
            Destroy(_canvas.gameObject);
    }
}
