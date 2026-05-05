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
            player = GameObject.Find("Player").GetComponent<PlayerMovement>();

        // Slider가 0~1 구조이므로 고정
        hpSlider.minValue = 0f;
        hpSlider.maxValue = 1f;

        hpSlider.value = (float)player.hp / maxHp;
    }

    void Update()
    {
        hpSlider.value = (float)player.hp / maxHp;
    }
}