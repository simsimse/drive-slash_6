using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 튜토리얼 단계를 순서대로 진행시키는 매니저.
/// 각 단계의 완료는 기존 플레이어 컴포넌트의 공개 API를 폴링해서 자동 감지합니다.
/// (기존 스크립트를 수정하지 않고 동작)
/// </summary>
/// <remarks>
/// [의존 — 모두 읽기 전용으로 참조]
/// - PlayerMovement : 이동 거리 감지
/// - DaggerManager  : 핀 설치 감지(ActiveDaggerCount)
/// - Dash           : 대시 발동 감지(IsDashing)
/// - Repeller       : 패링 발동 감지(IsParrying)
///
/// [Inspector 설정]
/// 1. 빈 GameObject를 만들고 이 스크립트를 붙입니다.
/// 2. promptText : 안내 문구를 표시할 UI Text (Canvas 아래 Text 하나).
/// 3. 플레이어 참조는 비워두면 씬에서 자동으로 찾습니다.
/// 4. teach* 토글로 가르칠 단계를 켜고 끕니다.
/// 5. nextSceneName : 완료 후 넘어갈 씬 이름(Build Settings에 등록 필요). 비우면 전환 안 함.
/// </remarks>
public class TutorialManager : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("안내 문구를 표시할 UI Text")]
    [SerializeField] private Text promptText;

    [Header("플레이어 참조 (비우면 자동 탐색)")]
    [SerializeField] private PlayerMovement player;
    [SerializeField] private DaggerManager  daggerManager;
    [SerializeField] private Dash           dash;
    [SerializeField] private Repeller       repeller;

    [Header("가르칠 단계")]
    [SerializeField] private bool teachMove  = true;
    [SerializeField] private bool teachPin   = true;
    [SerializeField] private bool teachDash  = true;
    [SerializeField] private bool teachParry = true;

    [Header("단계 파라미터")]
    [Tooltip("이동 단계 완료까지 필요한 누적 좌우 이동 거리")]
    [SerializeField] private float moveDistanceGoal = 4f;
    [Tooltip("한 단계 완료 후 다음 안내까지의 대기 시간(초)")]
    [SerializeField] private float stepDelay = 1f;
    [Tooltip("모든 단계 완료 후 표시할 문구")]
    [SerializeField] private string finishText = "튜토리얼 완료!";

    [Header("완료 후 씬 전환")]
    [Tooltip("튜토리얼 완료 후 넘어갈 씬 이름. 비우면 전환하지 않습니다. (Build Settings에 등록 필요)")]
    [SerializeField] private string nextSceneName = "SampleScene";
    [Tooltip("완료 문구를 보여준 뒤 다음 씬으로 넘어가기까지의 대기 시간(초)")]
    [SerializeField] private float nextSceneDelay = 2f;

    private readonly List<Step> _steps = new List<Step>();
    private int  _index;
    private bool _advancing;

    // 단계별 추적 상태
    private float _moveAccum;
    private float _lastX;
    private int   _pinBaseline;
    private bool  _prevDashing;
    private bool  _prevParrying;

    private class Step
    {
        public string     Prompt;
        public Action     OnEnter;
        public Func<bool> IsComplete;
    }

    private void Awake()
    {
        if (player        == null) player        = FindAnyObjectByType<PlayerMovement>();
        if (daggerManager == null) daggerManager = FindAnyObjectByType<DaggerManager>();
        if (dash          == null) dash          = FindAnyObjectByType<Dash>();
        if (repeller      == null) repeller      = FindAnyObjectByType<Repeller>();
    }

    private void Start()
    {
        BuildSteps();

        if (_steps.Count == 0)
        {
            SetPrompt(finishText);
            enabled = false;
            return;
        }

        EnterStep(0);
    }

    private void BuildSteps()
    {
        if (teachMove && player != null)
        {
            _steps.Add(new Step
            {
                Prompt  = "A / D 키로 좌우로 움직여 보세요",
                OnEnter = () => { _moveAccum = 0f; _lastX = player.transform.position.x; },
                IsComplete = () =>
                {
                    _moveAccum += Mathf.Abs(player.transform.position.x - _lastX);
                    _lastX = player.transform.position.x;
                    return _moveAccum >= moveDistanceGoal;
                }
            });
        }

        if (teachPin && daggerManager != null)
        {
            _steps.Add(new Step
            {
                Prompt     = "마우스 우클릭으로 핀을 설치하세요",
                OnEnter    = () => { _pinBaseline = daggerManager.ActiveDaggerCount; },
                IsComplete = () => daggerManager.ActiveDaggerCount > _pinBaseline
            });
        }

        if (teachDash && dash != null)
        {
            _steps.Add(new Step
            {
                Prompt  = "좌클릭으로 핀을 향해 대시하세요",
                OnEnter = () => { _prevDashing = dash.IsDashing; },
                IsComplete = () =>
                {
                    bool rising = !_prevDashing && dash.IsDashing;
                    _prevDashing = dash.IsDashing;
                    return rising;
                }
            });
        }

        if (teachParry && repeller != null)
        {
            _steps.Add(new Step
            {
                Prompt  = "Shift 키로 패링하세요",
                OnEnter = () => { _prevParrying = repeller.IsParrying; },
                IsComplete = () =>
                {
                    bool rising = !_prevParrying && repeller.IsParrying;
                    _prevParrying = repeller.IsParrying;
                    return rising;
                }
            });
        }
    }

    private void Update()
    {
        // ESC로 튜토리얼을 건너뛰고 즉시 다음 씬으로
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SkipToNextScene();
            return;
        }

        if (_advancing || _index >= _steps.Count) return;

        Step step = _steps[_index];
        if (step.IsComplete != null && step.IsComplete())
            StartCoroutine(AdvanceAfterDelay());
    }

    private IEnumerator AdvanceAfterDelay()
    {
        _advancing = true;
        yield return new WaitForSeconds(stepDelay);
        _advancing = false;

        int next = _index + 1;
        if (next >= _steps.Count)
        {
            _index = next;
            SetPrompt(finishText);
            yield return Finish();
            yield break;
        }

        EnterStep(next);
    }

    private IEnumerator Finish()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            // 전환할 씬이 없으면 완료 문구만 남기고 종료
            enabled = false;
            yield break;
        }

        yield return new WaitForSeconds(nextSceneDelay);
        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>ESC 입력 시 튜토리얼을 건너뛰고 즉시 다음 씬으로 전환합니다.</summary>
    private void SkipToNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName)) return;
        SceneManager.LoadScene(nextSceneName);
    }

    private void EnterStep(int i)
    {
        _index = i;
        _steps[i].OnEnter?.Invoke();
        SetPrompt(_steps[i].Prompt);
    }

    private void SetPrompt(string s)
    {
        if (promptText != null) promptText.text = s;
    }
}
