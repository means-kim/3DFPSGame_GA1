using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 입력칸을 선택하면 살짝 커지면서 배경색이 노을빛으로 바뀌고, 선택을 풀면 원래대로 돌아온다.
[RequireComponent(typeof(Image))]
public class UI_InputFieldFocus : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("포커스 연출")]
    [SerializeField] private float _focusScale = 1.03f;
    [SerializeField] private Color _focusColor = new Color(1f, 0.94f, 0.82f, 1f);
    [SerializeField] private float _duration = 0.15f;

    private Image _image;
    private Color _normalColor;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _normalColor = _image.color;
    }

    public void OnSelect(BaseEventData eventData)
    {
        Play(_focusScale, _focusColor, Ease.OutBack);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        Play(1f, _normalColor, Ease.OutQuad);
    }

    private void Play(float scale, Color color, Ease ease)
    {
        // 빠르게 선택을 바꿔도 이전 트윈과 겹치지 않게 먼저 끊는다.
        transform.DOKill();
        _image.DOKill();

        transform.DOScale(scale, _duration).SetEase(ease).SetUpdate(true).SetLink(gameObject);
        _image.DOColor(color, _duration).SetUpdate(true).SetLink(gameObject);
    }

    private void OnDisable()
    {
        // 꺼졌다 켜질 때 커진 채로 남지 않게 되돌린다.
        transform.DOKill();
        _image.DOKill();
        transform.localScale = Vector3.one;
        _image.color = _normalColor;
    }
}
