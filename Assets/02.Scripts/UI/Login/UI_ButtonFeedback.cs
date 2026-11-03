using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 버튼에 손맛을 더한다.
// - 호버: 살짝 커지고 노을빛 테두리가 살아난다.
// - 누름: 쏙 눌렸다가, 뗄 때 탱글하게 튀어 오른다.
// - 클릭 리플: 누른 자리에서 빛이 퍼지며 사라진다.
[RequireComponent(typeof(Button))]
public class UI_ButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("참조")]
    [SerializeField] private Image _hoverBorder;    // 버튼 위에 얹은 테두리
    [SerializeField] private RectTransform _rippleArea; // RectMask2D가 달린 리플 영역
    [SerializeField] private Image _ripple;

    [Header("크기")]
    [SerializeField] private float _hoverScale = 1.06f;
    [SerializeField] private float _pressScale = 0.92f;
    [SerializeField] private float _hoverDuration = 0.15f;
    [SerializeField] private float _releaseDuration = 0.35f;
    [SerializeField] private float _releaseOvershoot = 4f;

    [Header("리플")]
    [SerializeField] private float _rippleDuration = 0.45f;
    [SerializeField] private float _rippleAlpha = 0.6f;

    private Button _button;
    private bool _isHovered;

    private void Awake()
    {
        _button = GetComponent<Button>();
        SetAlpha(_hoverBorder, 0f);
        SetAlpha(_ripple, 0f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        if (!_button.IsInteractable()) return;

        ScaleTo(_hoverScale, _hoverDuration, Ease.OutBack);
        FadeBorder(1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;

        ScaleTo(1f, _hoverDuration, Ease.OutQuad);
        FadeBorder(0f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_button.IsInteractable()) return;

        ScaleTo(_pressScale, 0.06f, Ease.OutQuad);
        PlayRipple(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_button.IsInteractable()) return;

        // 큰 오버슈트로 탱글하게 돌아온다.
        float target = _isHovered ? _hoverScale : 1f;
        transform.DOKill();
        transform.DOScale(target, _releaseDuration).SetEase(Ease.OutBack, _releaseOvershoot).SetUpdate(true).SetLink(gameObject);
    }

    private void PlayRipple(PointerEventData eventData)
    {
        RectTransform rippleRect = _ripple.rectTransform;

        // 누른 화면 좌표를 리플 영역 안의 좌표로 바꾼다.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rippleArea, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        rippleRect.anchoredPosition = localPoint;

        // 버튼 어디를 눌러도 끝까지 덮을 만큼 크게 퍼진다.
        float size = _rippleArea.rect.width * 2.2f;
        rippleRect.sizeDelta = new Vector2(size, size);

        rippleRect.DOKill();
        _ripple.DOKill();
        rippleRect.localScale = Vector3.zero;
        SetAlpha(_ripple, _rippleAlpha);

        rippleRect.DOScale(1f, _rippleDuration).SetEase(Ease.OutQuad).SetUpdate(true).SetLink(gameObject);
        _ripple.DOFade(0f, _rippleDuration).SetEase(Ease.InQuad).SetUpdate(true).SetLink(gameObject);
    }

    private void ScaleTo(float scale, float duration, Ease ease)
    {
        transform.DOKill();
        transform.DOScale(scale, duration).SetEase(ease).SetUpdate(true).SetLink(gameObject);
    }

    private void FadeBorder(float alpha)
    {
        _hoverBorder.DOKill();
        _hoverBorder.DOFade(alpha, _hoverDuration).SetUpdate(true).SetLink(gameObject);
    }

    private void OnDisable()
    {
        // 꺼졌다 켜질 때 커진 채로 남지 않게 되돌린다.
        transform.DOKill();
        transform.localScale = Vector3.one;
        _isHovered = false;
        SetAlpha(_hoverBorder, 0f);
    }

    private static void SetAlpha(Graphic graphic, float alpha)
    {
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }
}
