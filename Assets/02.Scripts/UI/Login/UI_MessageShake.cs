using DG.Tweening;
using TMPro;
using UnityEngine;

// 로그인 안내/오류 메시지를 보여 줄 때 좌우로 흔들고 색을 번쩍여 눈에 띄게 한다.
// 로그인 로직에서 Show("...")로 호출한다.
[RequireComponent(typeof(TextMeshProUGUI))]
public class UI_MessageShake : MonoBehaviour
{
    [Header("흔들기")]
    [SerializeField] private float _shakeDuration = 0.4f;
    [SerializeField] private float _shakeStrength = 12f;
    [SerializeField] private int _shakeVibrato = 20;

    [Header("번쩍임")]
    [SerializeField] private Color _flashColor = Color.white;
    [SerializeField] private float _flashDuration = 0.4f;

    private TextMeshProUGUI _text;
    private RectTransform _rectTransform;
    private Color _normalColor;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _rectTransform = (RectTransform)transform;
        _normalColor = _text.color;
    }

    public void Show(string message)
    {
        _text.text = message;

        // 이전 흔들기를 끝까지 되돌린 뒤 다시 시작해야 위치가 밀리지 않는다.
        _rectTransform.DOKill(true);
        _text.DOKill();

        _rectTransform.DOShakeAnchorPos(_shakeDuration, new Vector2(_shakeStrength, 0f), _shakeVibrato, 0f, false, true)
            .SetUpdate(true).SetLink(gameObject);

        _text.color = _flashColor;
        _text.DOColor(_normalColor, _flashDuration).SetUpdate(true).SetLink(gameObject);
    }

    public void Clear()
    {
        _rectTransform.DOKill(true);
        _text.DOKill();
        _text.color = _normalColor;
        _text.text = string.Empty;
    }
}
