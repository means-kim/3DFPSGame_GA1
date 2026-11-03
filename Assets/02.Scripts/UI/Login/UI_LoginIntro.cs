using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 로그인 화면 등장 연출.
// 검은 화면에서 해가 떠오르듯 밝아지고 → 제목이 떨어지며 나타나고 → 입력칸과 버튼이 차례로 톡톡 튀어나온다.
// 연출이 끝날 때까지 로그인 패널은 입력을 받지 않는다.
public class UI_LoginIntro : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Image _fader;
    [SerializeField] private UI_SunFlare _sunFlare;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private CanvasGroup _panel;
    [SerializeField] private RectTransform[] _panelItems; // 등장 순서대로 넣는다

    [Header("페이드인")]
    [SerializeField] private float _fadeDuration = 1.5f;

    [Header("제목")]
    [SerializeField] private float _titleDelay = 0.9f;
    [SerializeField] private float _titleDropHeight = 80f;
    [SerializeField] private float _titleDuration = 0.6f;

    [Header("패널 항목")]
    [SerializeField] private float _itemsDelay = 1.3f;
    [SerializeField] private float _itemInterval = 0.07f;
    [SerializeField] private float _itemDuration = 0.35f;
    [SerializeField] private float _itemStartScale = 0.85f;

    [Header("시작 대기")]
    [SerializeField] private int _waitFrames = 3; // 씬 로딩 직후의 큰 프레임 간격에 연출이 한 번에 지나가 버리지 않도록 기다린다

    private IEnumerator Start()
    {
        // 처음 상태는 바로 잡아 두어 첫 프레임부터 검은 화면이 보이게 한다.
        Sequence sequence = Prepare();
        sequence.Pause();

        for (int i = 0; i < _waitFrames; i++)
        {
            yield return null;
        }

        sequence.Play();
    }

    private Sequence Prepare()
    {
        // 처음 상태: 화면은 검게, 제목과 패널 항목은 보이지 않게 둔다.
        _fader.gameObject.SetActive(true);
        _fader.raycastTarget = true; // 연출 중 클릭을 막는다
        SetAlpha(_fader, 1f);

        _panel.interactable = false;
        _panel.blocksRaycasts = false;

        RectTransform titleRect = _title.rectTransform;
        Vector2 titlePosition = titleRect.anchoredPosition;
        titleRect.anchoredPosition = titlePosition + Vector2.up * _titleDropHeight;
        SetAlpha(_title, 0f);

        CanvasGroup[] itemGroups = new CanvasGroup[_panelItems.Length];
        for (int i = 0; i < _panelItems.Length; i++)
        {
            itemGroups[i] = GetOrAddCanvasGroup(_panelItems[i]);
            itemGroups[i].alpha = 0f;
            _panelItems[i].localScale = Vector3.one * _itemStartScale;
        }

        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

        // 1. 검은 화면이 걷히면서 해 빛이 번쩍인다.
        sequence.Append(_fader.DOFade(0f, _fadeDuration).SetEase(Ease.InQuad));
        sequence.InsertCallback(0f, () => _sunFlare.PlayIntroBurst(_fadeDuration + 0.4f));

        // 2. 제목이 위에서 떨어지며 나타난다.
        sequence.Insert(_titleDelay, titleRect.DOAnchorPos(titlePosition, _titleDuration).SetEase(Ease.OutBack));
        sequence.Insert(_titleDelay, _title.DOFade(1f, _titleDuration * 0.8f));

        // 3. 입력칸과 버튼이 차례로 톡톡 튀어나온다.
        // 레이아웃 그룹이 위치를 정하므로, 위치 대신 크기와 투명도로 등장시킨다.
        for (int i = 0; i < _panelItems.Length; i++)
        {
            float time = _itemsDelay + i * _itemInterval;
            sequence.Insert(time, itemGroups[i].DOFade(1f, _itemDuration));
            sequence.Insert(time, _panelItems[i].DOScale(1f, _itemDuration).SetEase(Ease.OutBack));
        }

        // 4. 끝나면 입력을 풀고 페이더를 치운다(게임 시작 전환 때 다시 쓴다).
        sequence.OnComplete(() =>
        {
            _panel.interactable = true;
            _panel.blocksRaycasts = true;
            _fader.raycastTarget = false;
            _fader.gameObject.SetActive(false);
        });

        return sequence;
    }

    private static CanvasGroup GetOrAddCanvasGroup(Component target)
    {
        CanvasGroup group = target.GetComponent<CanvasGroup>();
        return group != null ? group : target.gameObject.AddComponent<CanvasGroup>();
    }

    private static void SetAlpha(Graphic graphic, float alpha)
    {
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }
}
