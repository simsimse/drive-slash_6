using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 보스를 처치(보스 오브젝트 제거)했을 때 표시되는 승리 UI 및 게임플레이 정지 처리.
/// 재시작(OnRestart) / 나가기(OnQuit) 버튼을 제공합니다.
/// </summary>
/// <remarks>
/// [사용법]
/// 1. Canvas 아래에 VictoryPanel(비활성)을 만들고 이 스크립트를 붙입니다.
/// 2. panel 필드에 그 VictoryPanel을 연결.
/// 3. 재시작/나가기 버튼의 OnClick에 각각 OnRestart, OnQuit 을 연결.
/// 4. Boss.cs 사망 처리에서 VictoryUI.Instance.Show() 가 호출됩니다.
///
/// [게임플레이 정지]
/// Show() 호출 시 플레이어·시스템 스크립트를 비활성화하고,
/// 모든 Rigidbody2D 시뮬레이션을 멈춰 남은 발사체도 그 자리에 정지시킵니다.
/// 그 외 추가로 멈추고 싶은 스크립트는 extraScriptsToDisable 에 드래그하세요.
/// </remarks>
public class VictoryUI : MonoBehaviour
{
    public static VictoryUI Instance { get; private set; }

    [Header("UI 참조")]
    public GameObject panel;

    [Header("추가로 비활성화할 스크립트(선택)")]
    public MonoBehaviour[] extraScriptsToDisable;

    [Header("옵션")]
    [Tooltip("씬의 모든 Rigidbody2D 시뮬레이션을 멈춥니다.")]
    public bool freezeAllRigidbodies2D = true;

    private bool _shown;

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
        if (_shown) return;
        _shown = true;

        if (panel != null) panel.SetActive(true);

        DisableAllOfType<Dash>();
        DisableAllOfType<DaggerThrower>();
        DisableAllOfType<DaggerManager>();
        DisableAllOfType<Repeller>();
        DisableAllOfType<MagnetForce>();
        DisableAllOfType<RandomPinSpawner>();

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
