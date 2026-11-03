using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 로그인 배경을 천천히 확대·이동(켄번스)하고, 마우스 위치에 따라 반대로 살짝 움직인다(패럴랙스).
[RequireComponent(typeof(RawImage))]
public class UI_BackgroundMotion : MonoBehaviour
{
    [Header("켄번스")]
    [SerializeField] private float _kenBurnsDuration = 25f;
    [SerializeField] private float _kenBurnsZoom = 0.06f;                       // UV를 얼마나 좁힐지(=확대 정도)
    [SerializeField] private Vector2 _kenBurnsPan = new Vector2(0.015f, 0.01f); // UV 이동량

    [Header("마우스 패럴랙스")]
    [SerializeField] private float _parallaxAmount = 15f;   // 최대 이동 거리(px)
    [SerializeField] private float _parallaxSmooth = 3f;    // 클수록 빨리 따라감
    [SerializeField] private float _overscale = 1.03f;      // 움직여도 가장자리가 비지 않도록 키우는 배율

    private RawImage _rawImage;
    private RectTransform _rectTransform;
    private Vector2 _parallaxOffset;
    private Tween _kenBurnsTween;
    private bool _isZooming;

    // 다른 연출(플레어 등)이 같이 쓰도록 부드럽게 보정된 마우스 입력(-1 ~ 1)을 공개한다.
    public Vector2 MouseInput { get; private set; }

    private void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        _rectTransform = (RectTransform)transform;
        _rectTransform.localScale = Vector3.one * _overscale;
    }

    private void Start()
    {
        Rect target = new Rect(
            _kenBurnsZoom * 0.5f + _kenBurnsPan.x,
            _kenBurnsZoom * 0.5f + _kenBurnsPan.y,
            1f - _kenBurnsZoom,
            1f - _kenBurnsZoom);

        _kenBurnsTween = DOTween.To(() => _rawImage.uvRect, rect => _rawImage.uvRect = rect, target, _kenBurnsDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);
    }

    private void Update()
    {
        // 화면 전환 중에는 마우스를 따라 움직이지 않는다.
        if (_isZooming) return;

        // 화면 중심 기준 마우스 위치를 -1 ~ 1로 바꾼다.
        Vector2 mouse = Input.mousePosition;
        Vector2 normalized = new Vector2(
            mouse.x / Screen.width * 2f - 1f,
            mouse.y / Screen.height * 2f - 1f);
        normalized = Vector2.ClampMagnitude(normalized, 1f);

        float t = 1f - Mathf.Exp(-_parallaxSmooth * Time.unscaledDeltaTime);
        MouseInput = Vector2.Lerp(MouseInput, normalized, t);

        // 배경은 마우스 반대 방향으로 움직인다.
        _parallaxOffset = -MouseInput * _parallaxAmount;
        _rectTransform.anchoredPosition = _parallaxOffset;
    }

    // 배경 이미지의 한 점(UV)을 향해 빨려 들어가듯 확대한다. 게임 시작 전환에서 쓴다.
    // zoom이 4면 화면에 이미지의 1/4 영역만 보이게 된다.
    public Tween ZoomTo(Vector2 uv, float zoom, float duration)
    {
        _isZooming = true;
        _kenBurnsTween?.Kill();

        Vector2 size = Vector2.one / zoom;
        Rect target = new Rect(uv - size * 0.5f, size);

        return DOTween.To(() => _rawImage.uvRect, rect => _rawImage.uvRect = rect, target, duration)
            .SetEase(Ease.InExpo)
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    // 배경 이미지 안의 한 점(UV)이 지금 캔버스 위 어디에 그려지는지 구한다.
    // 켄번스와 패럴랙스로 배경이 움직여도 해 위치에 플레어를 붙여 두기 위해 쓴다.
    public Vector2 UvToCanvasPosition(Vector2 uv)
    {
        Rect uvRect = _rawImage.uvRect;
        Vector2 normalized = new Vector2(
            (uv.x - uvRect.x) / uvRect.width,
            (uv.y - uvRect.y) / uvRect.height);

        Vector2 local = Vector2.Scale(normalized - _rectTransform.pivot, _rectTransform.rect.size);
        return _rectTransform.anchoredPosition + Vector2.Scale(local, _rectTransform.localScale);
    }
}
