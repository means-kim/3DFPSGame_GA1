using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 한동안 입력이 없으면 로그인 패널이 흐려지고 배경이 더 깊게 확대되어 "멍하니 바라보는" 화면이 된다.
// 마우스를 움직이거나 키를 누르면 곧바로 원래대로 돌아온다.
public class UI_IdleAttract : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private CanvasGroup _panel;
    [SerializeField] private UI_BackgroundMotion _background;
    [SerializeField] private Image _vignette;

    [Header("유휴 진입")]
    [SerializeField] private float _idleTime = 20f;
    [SerializeField] private float _enterDuration = 2f;
    [SerializeField] private float _idlePanelAlpha = 0.25f;
    [SerializeField] private float _idleZoom = 1.08f;
    [SerializeField] private float _idleVignetteAlpha = 0.75f;

    [Header("복귀")]
    [SerializeField] private float _exitDuration = 0.4f;
    [SerializeField] private float _mouseThreshold = 2f; // 이보다 작은 마우스 떨림은 입력으로 보지 않는다(px)

    private float _timer;
    private bool _isIdle;
    private float _normalVignetteAlpha;
    private Vector3 _lastMousePosition;

    public bool IsIdle => _isIdle;

    private void Awake()
    {
        _normalVignetteAlpha = _vignette.color.a;
        _lastMousePosition = Input.mousePosition;
    }

    private void Update()
    {
        if (HasInput())
        {
            _timer = 0f;
            if (_isIdle) Exit();
            return;
        }

        // 등장 연출이나 게임 시작 전환 중에는 기다리지 않는다.
        if (!_panel.interactable)
        {
            _timer = 0f;
            return;
        }

        _timer += Time.unscaledDeltaTime;
        if (!_isIdle && _timer >= _idleTime)
        {
            Enter();
        }
    }

    private bool HasInput()
    {
        Vector3 mouse = Input.mousePosition;
        bool isMoved = (mouse - _lastMousePosition).sqrMagnitude > _mouseThreshold * _mouseThreshold;
        _lastMousePosition = mouse;

        return isMoved || Input.anyKeyDown || Input.mouseScrollDelta.sqrMagnitude > 0f;
    }

    public void Enter()
    {
        _isIdle = true;
        Play(_idlePanelAlpha, _idleZoom, _idleVignetteAlpha, _enterDuration);
    }

    public void Exit()
    {
        _isIdle = false;
        _timer = 0f;
        Play(1f, 1f, _normalVignetteAlpha, _exitDuration);
    }

    private void Play(float panelAlpha, float zoom, float vignetteAlpha, float duration)
    {
        _panel.DOKill();
        _vignette.DOKill();

        _panel.DOFade(panelAlpha, duration).SetEase(Ease.InOutSine).SetUpdate(true).SetLink(gameObject);
        _vignette.DOFade(vignetteAlpha, duration).SetEase(Ease.InOutSine).SetUpdate(true).SetLink(gameObject);
        _background.SetExtraZoom(zoom, duration);
    }
}
