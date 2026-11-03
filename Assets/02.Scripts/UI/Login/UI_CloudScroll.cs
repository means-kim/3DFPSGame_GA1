using UnityEngine;
using UnityEngine.UI;

// 구름 텍스처를 가로로 천천히 흘려보낸다. 텍스처는 Wrap Mode가 Repeat이어야 한다.
[RequireComponent(typeof(RawImage))]
public class UI_CloudScroll : MonoBehaviour
{
    [Header("구름 흐름")]
    [SerializeField] private float _speed = 0.006f;                  // 초당 UV 이동량
    [SerializeField] private Vector2 _tiling = new Vector2(1.5f, 1f); // 가로로 몇 번 반복할지

    private RawImage _rawImage;
    private float _offset;

    private void Awake()
    {
        _rawImage = GetComponent<RawImage>();
    }

    private void Update()
    {
        _offset = Mathf.Repeat(_offset + _speed * Time.unscaledDeltaTime, 1f);
        _rawImage.uvRect = new Rect(_offset, 0f, _tiling.x, _tiling.y);
    }
}
