using System.Collections;
using UnityEngine;

/// <summary>
/// 카메라에 붙여 화면 흔들림을 담당하는 컴포넌트.
/// 다른 스크립트에서 CameraShake.Instance.Shake(duration, magnitude) 로 호출합니다.
/// </summary>
/// <remarks>
/// [사용법]
/// 1. Main Camera(또는 흔들 카메라)에 이 스크립트를 붙입니다.
/// 2. 흔들고 싶은 곳에서 CameraShake.Instance?.Shake(0.3f, 0.4f); 형태로 호출.
///
/// 흔들림은 카메라의 로컬 좌표를 랜덤 오프셋으로 흔든 뒤 원위치로 복구합니다.
/// hitstop(timeScale=0) 중에도 동작하도록 unscaled 시간을 사용합니다.
/// </remarks>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Vector3   _originalLocalPos;
    private Coroutine _routine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        _originalLocalPos = transform.localPosition;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>지정한 시간(duration) 동안 크기(magnitude)만큼 화면을 흔듭니다.</summary>
    public void Shake(float duration, float magnitude)
    {
        if (duration <= 0f || magnitude <= 0f) return;

        if (_routine != null)
            StopCoroutine(_routine);
        _routine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 남은 시간에 비례해 흔들림을 점점 줄임(감쇠)
            float damper = 1f - (elapsed / duration);
            float offsetX = (Random.value * 2f - 1f) * magnitude * damper;
            float offsetY = (Random.value * 2f - 1f) * magnitude * damper;

            transform.localPosition = _originalLocalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = _originalLocalPos;
        _routine = null;
    }
}
