using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 입력칸에 글자를 칠 때마다 반응을 준다.
// - 글자가 늘면 입력 영역이 살짝 튀고, 새로 친 글자(비밀번호는 *)가 톡 튀어나온다.
// - 비밀번호 확인 칸이면 비밀번호와 같을 때 체크 아이콘을 띄운다.
// 입력칸 자체의 크기는 UI_InputFieldFocus가 다루므로, 여기서는 안쪽 Text Area만 움직여 서로 부딪히지 않게 한다.
[RequireComponent(typeof(TMP_InputField))]
public class UI_TypingFeedback : MonoBehaviour
{
    [Header("입력 반응")]
    [SerializeField] private float _punchDistance = 2f;
    [SerializeField] private float _punchScale = 0.02f;
    [SerializeField] private float _punchDuration = 0.12f;

    [Header("글자 팝")]
    [SerializeField] private float _popScale = 1.6f;
    [SerializeField] private float _popDuration = 0.15f;

    [Header("비밀번호 일치 (확인 칸에서만 연결)")]
    [SerializeField] private TMP_InputField _matchTarget;
    [SerializeField] private Image _checkIcon;

    private TMP_InputField _input;
    private RectTransform _textArea;
    private int _lastLength;
    private int _popIndex = -1;
    private float _popTimer;
    private bool _isMatched;

    private void Awake()
    {
        _input = GetComponent<TMP_InputField>();
        _textArea = _input.textViewport;

        if (_checkIcon != null)
        {
            _checkIcon.transform.localScale = Vector3.zero;
            _checkIcon.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        _input.onValueChanged.AddListener(OnValueChanged);
        if (_matchTarget != null) _matchTarget.onValueChanged.AddListener(OnTargetChanged);

        _lastLength = _input.text.Length;
    }

    private void OnDisable()
    {
        _input.onValueChanged.RemoveListener(OnValueChanged);
        if (_matchTarget != null) _matchTarget.onValueChanged.RemoveListener(OnTargetChanged);
    }

    private void OnValueChanged(string value)
    {
        bool isAdded = value.Length > _lastLength;
        _lastLength = value.Length;

        // 지울 때는 더 약하게 반응한다.
        float strength = isAdded ? 1f : 0.5f;

        _textArea.DOKill(true);
        _textArea.DOPunchAnchorPos(new Vector2(_punchDistance * strength, 0f), _punchDuration, 10, 0f).SetUpdate(true).SetLink(gameObject);
        _textArea.DOPunchScale(Vector3.one * _punchScale * strength, _punchDuration, 6, 0f).SetUpdate(true).SetLink(gameObject);

        if (isAdded)
        {
            // 커서 바로 앞 글자가 방금 친 글자다(중간에 끼워 넣어도 맞다).
            _popIndex = _input.caretPosition - 1;
            _popTimer = 0f;
        }

        RefreshMatch();
    }

    private void OnTargetChanged(string value)
    {
        RefreshMatch();
    }

    private void LateUpdate()
    {
        if (_popIndex < 0) return;

        _popTimer += Time.unscaledDeltaTime;
        float progress = Mathf.Clamp01(_popTimer / _popDuration);

        TMP_Text text = _input.textComponent;
        text.ForceMeshUpdate();
        TMP_TextInfo textInfo = text.textInfo;

        if (_popIndex < textInfo.characterCount && textInfo.characterInfo[_popIndex].isVisible)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[_popIndex];
            Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            int vertexIndex = charInfo.vertexIndex;

            // 글자 중심을 기준으로 크게 시작해 원래 크기로 돌아온다.
            Vector3 center = (vertices[vertexIndex] + vertices[vertexIndex + 2]) * 0.5f;
            float scale = Mathf.Lerp(_popScale, 1f, DOVirtual.EasedValue(0f, 1f, progress, Ease.OutBack));

            for (int i = 0; i < 4; i++)
            {
                vertices[vertexIndex + i] = center + (vertices[vertexIndex + i] - center) * scale;
            }

            text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }

        if (progress >= 1f)
        {
            _popIndex = -1;
        }
    }

    private void RefreshMatch()
    {
        if (_matchTarget == null || _checkIcon == null) return;

        bool isMatched = _input.text.Length > 0 && _input.text == _matchTarget.text;
        if (isMatched == _isMatched) return;

        _isMatched = isMatched;
        Transform icon = _checkIcon.transform;
        icon.DOKill();

        if (isMatched)
        {
            _checkIcon.gameObject.SetActive(true);
            icon.DOScale(1f, 0.25f).SetEase(Ease.OutBack, 3f).SetUpdate(true).SetLink(gameObject);
        }
        else
        {
            icon.DOScale(0f, 0.12f).SetEase(Ease.InQuad).SetUpdate(true).SetLink(gameObject)
                .OnComplete(() => _checkIcon.gameObject.SetActive(false));
        }
    }
}
