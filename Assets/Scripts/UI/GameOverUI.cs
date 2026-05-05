using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 플레이어 사망 시 표시되는 게임오버 UI 및 게임플레이 정지 처리.
/// </summary>
/// <remarks>
/// [사용법]
/// 1. Canvas 아래에 GameOverPanel(비활성)을 만들고 이 스크립트를 붙입니다.
/// 2. panel 필드에 그 GameOverPanel을 연결.
/// 3. 재시작/종료 버튼이 있으면 OnRestart, OnQuit 을 OnClick에 연결.
/// 4. PlayerMovement.Die()에서 GameOverUI.Instance.Show() 호출.
///
/// [게임플레이 정지]
/// Show() 호출 시 자동으로 다음 타입의 MonoBehaviour 들을 모두 비활성화합니다:
///  - 플레이어: Dash, DaggerThrower, DaggerManager, Repeller, MagnetForce, ForceMode, ArrowVisualizer
///  - 보스: BossAI, Boss, Breath, FlameCircle, GroundStomp, WindStorm, DamageZone
///  - 시스템: RandomPinSpawner
/// 그 외에 추가로 멈추고 싶은 스크립트는 extraScriptsToDisable 에 드래그하세요.
/// 모든 Rigidbody2D 의 simulated 도 false 로 만들어 발사체/물리 오브젝트도 그 자리에 멈춥니다.
/// </remarks>
public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    [Header("UI 참조")]
    public GameObject panel;

    [Header("추가로 비활성화할 스크립트(선택)")]
    public MonoBehaviour[] extraScriptsToDisable;

    [Header("옵션")]
    [Tooltip("씬의 모든 Rigidbody2D 시뮬레이션을 멈춥니다.")]
    public bool freezeAllRigidbodies2D = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (panel != null) panel.SetActive(false);
    }

    public void Show()
    {
        if (panel != null) panel.SetActive(true);

        DisableAllOfType<Dash>();
        DisableAllOfType<DaggerThrower>();
        DisableAllOfType<DaggerManager>();
        DisableAllOfType<Repeller>();
        DisableAllOfType<MagnetForce>();
        DisableAllOfType<ForceMode>();
        DisableAllOfType<ArrowVisualizer>();
        DisableAllOfType<RandomPinSpawner>();
        DisableAllOfType<BossAI>();
        DisableAllOfType<Boss>();
        DisableAllOfType<Breath>();
        DisableAllOfType<FlameCircle>();
        DisableAllOfType<GroundStomp>();
        DisableAllOfType<WindStorm>();
        DisableAllOfType<DamageZone>();

        if (extraScriptsToDisable != null)
        {
            foreach (var s in extraScriptsToDisable)
                if (s != null) s.enabled = false;
        }

        if (freezeAllRigidbodies2D)
        {
            var rbs = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
            foreach (var rb in rbs)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.simulated = false;
            }
        }
    }

    private static void DisableAllOfType<T>() where T : MonoBehaviour
    {
        var found = FindObjectsByType<T>(FindObjectsSortMode.None);
        foreach (var c in found)
            c.enabled = false;
    }

    public void OnRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
