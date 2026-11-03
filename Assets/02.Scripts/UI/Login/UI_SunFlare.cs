using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 배경 속 해 위치에 빛무리·빛줄기·스트릭을 붙이고, 해와 화면 중심을 잇는 선 위에 렌즈 플레어 고스트를 흘린다.
public class UI_SunFlare : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private UI_BackgroundMotion _background;
    [SerializeField] private Image _sunGlow;
    [SerializeField] private Image _godRays;
    [SerializeField] private Image _streak;
    [SerializeField] private Image[] _ghosts;

    [Header("해 위치 (배경 이미지 UV)")]
    [SerializeField] private Vector2 _sunUv = new Vector2(0.573f, 0.657f);

    [Header("빛무리 숨쉬기")]
    [SerializeField] private float _glowBreathDuration = 4f;
    [SerializeField] private float _glowBreathScale = 1.08f;
    [SerializeField] private float _glowBreathAlpha = 0.75f; // 원래 알파 대비 가장 어두울 때의 비율

    [Header("빛줄기")]
    [SerializeField] private float _raysRotateDuration = 60f; // 한 바퀴 도는 시간
    [SerializeField] private float _raysFadeDuration = 6f;
    [SerializeField] private float _raysFadeAlpha = 0.5f;

    [Header("스트릭")]
    [SerializeField] private float _streakStretch = 1.15f;

    [Header("고스트")]
    [SerializeField] private float[] _ghostRatios = { 0.35f, 0.7f, 1.25f, 1.6f, 2.1f }; // 1 = 화면 중심, 2 = 해의 정반대 위치
    [SerializeField] private float _flareParallax = 60f;   // 마우스에 따라 가상 광원이 추가로 움직이는 거리(px)
    [SerializeField] private float _ghostWobble = 6f;      // 마우스가 없어도 천천히 흐르는 흔들림(px)
    [SerializeField] private float _ghostWobbleSpeed = 0.4f;

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;
    }

    private void Start()
    {
        // 빛무리: 크기와 밝기가 숨쉬듯 반복된다.
        _sunGlow.transform.DOScale(_glowBreathScale, _glowBreathDuration)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(_sunGlow.gameObject);
        _sunGlow.DOFade(_sunGlow.color.a * _glowBreathAlpha, _glowBreathDuration * 0.8f)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(_sunGlow.gameObject);

        // 빛줄기: 아주 천천히 돌면서 밝기가 일렁인다.
        _godRays.transform.DORotate(new Vector3(0f, 0f, -360f), _raysRotateDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart).SetLink(_godRays.gameObject);
        _godRays.DOFade(_godRays.color.a * _raysFadeAlpha, _raysFadeDuration)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(_godRays.gameObject);

        // 스트릭: 빛무리보다 살짝 늦게 가로로 늘었다 줄어든다.
        _streak.transform.DOScaleX(_streakStretch, _glowBreathDuration)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetDelay(0.5f).SetLink(_streak.gameObject);
    }

    private void LateUpdate()
    {
        // 배경이 움직여도 해 위치에 붙어 있도록 매 프레임 따라간다.
        Vector2 sunPosition = _background.UvToCanvasPosition(_sunUv);
        _rectTransform.anchoredPosition = sunPosition;

        // 렌즈 플레어는 광원이 멀리 있는 것처럼 마우스에 더 민감하게 움직인다.
        Vector2 virtualSun = sunPosition - _background.MouseInput * _flareParallax;
        float time = Time.unscaledTime * _ghostWobbleSpeed;

        int count = Mathf.Min(_ghosts.Length, _ghostRatios.Length);
        for (int i = 0; i < count; i++)
        {
            // 화면 중심(0, 0)을 지나 해의 반대편으로 이어지는 선 위에 놓는다.
            Vector2 onAxis = virtualSun * (1f - _ghostRatios[i]);
            Vector2 wobble = new Vector2(Mathf.Sin(time + i * 1.7f), Mathf.Cos(time * 0.8f + i * 2.3f)) * _ghostWobble;

            ((RectTransform)_ghosts[i].transform).anchoredPosition = onAxis + wobble;
        }
    }
}
