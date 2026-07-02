using UnityEngine;
using UnityEngine.UI;

public class PlayerHPUI : MonoBehaviour
{
    public PlayerMovement player;
    public Slider hpSlider;

    public int maxHp = 10;

    void Start()
    {
        if (player == null)
            ResolvePlayer();

        if (hpSlider == null)
        {
            Debug.LogWarning("[PlayerHPUI] hpSlider가 할당되지 않았습니다.", this);
            return;
        }

        // Slider가 0~1 구조이므로 고정
        hpSlider.minValue = 0f;
        hpSlider.maxValue = 1f;

        RefreshBar();
    }

    void Update()
    {
        RefreshBar();
    }

    private void RefreshBar()
    {
        if (hpSlider == null) return;

        // 인스펙터 참조가 비었거나 도중에 사라졌으면 다시 찾음
        if (player == null)
            ResolvePlayer();
        if (player == null) return;

        hpSlider.value = (float)player.hp / maxHp;
    }

    private void ResolvePlayer()
    {
        // 이름/태그/활성 상태와 무관하게 씬에서 PlayerMovement를 찾음 (비활성 포함)
        player = FindObjectOfType<PlayerMovement>(true);
    }
}