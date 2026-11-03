using UnityEngine;

// 배경 사진 속 한 점(UV)에 이 RectTransform을 붙여 둔다.
// 배경이 켄번스·패럴랙스로 움직여도 창문 불빛, 좀비 무리 같은 연출이 사진에서 벗어나지 않게 한다.
// 부모는 화면 전체를 덮는 RectTransform이고, 이 오브젝트의 앵커는 가운데여야 한다.
public class UI_BackgroundAnchor : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private UI_BackgroundMotion _background;

    [Header("붙일 위치 (배경 이미지 UV)")]
    [SerializeField] private Vector2 _uv = new Vector2(0.5f, 0.5f);

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;
    }

    private void LateUpdate()
    {
        _rectTransform.anchoredPosition = _background.UvToCanvasPosition(_uv);
    }
}
