using UnityEngine;
using UnityEngine.UI;

// 화면 전체에 옅은 노이즈 텍스처를 깔고, 초당 몇 번씩 위치를 무작위로 바꿔 필름처럼 지글거리게 한다.
[RequireComponent(typeof(RawImage))]
public class UI_FilmGrain : MonoBehaviour
{
    [Header("필름 그레인")]
    [SerializeField] private float _framesPerSecond = 24f;
    [SerializeField] private float _grainScale = 1f; // 클수록 알갱이가 작아진다

    private RawImage _rawImage;
    private Vector2 _tiling;
    private float _timer;

    private void Awake()
    {
        _rawImage = GetComponent<RawImage>();
    }

    private void Start()
    {
        // 노이즈 텍스처 한 픽셀이 화면 한 픽셀 정도가 되도록 반복 횟수를 정한다.
        Vector2 size = ((RectTransform)transform).rect.size;
        Texture texture = _rawImage.texture;
        _tiling = new Vector2(size.x / texture.width, size.y / texture.height) * _grainScale;
    }

    private void Update()
    {
        _timer += Time.unscaledDeltaTime;
        if (_timer < 1f / _framesPerSecond) return;

        _timer = 0f;
        _rawImage.uvRect = new Rect(Random.value, Random.value, _tiling.x, _tiling.y);
    }
}
