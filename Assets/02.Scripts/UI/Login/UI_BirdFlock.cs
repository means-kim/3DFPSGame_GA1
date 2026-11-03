using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 몇 초마다 새 무리가 하늘을 가로질러 날아간다. 새 Image는 미리 만들어 두고 재사용한다.
public class UI_BirdFlock : MonoBehaviour
{
    private class Bird
    {
        public RectTransform Rect;
        public Image Image;
        public bool IsFlying;
        public Vector2 Position;
        public float Speed;
        public float Phase;
        public float FlapSpeed;
    }

    [Header("참조")]
    [SerializeField] private Sprite _birdSprite;

    [Header("무리")]
    [SerializeField] private Vector2Int _flockSize = new Vector2Int(4, 7);
    [SerializeField] private Vector2 _interval = new Vector2(12f, 20f); // 다음 무리까지의 시간(초)
    [SerializeField] private float _firstDelay = 3f;
    [SerializeField] private Color _birdColor = new Color(0.15f, 0.08f, 0.08f, 0.85f);

    [Header("비행")]
    [SerializeField] private Vector2 _speed = new Vector2(90f, 140f);  // px/초
    [SerializeField] private Vector2 _birdSize = new Vector2(16f, 28f);
    [SerializeField] private Vector2 _flapSpeed = new Vector2(9f, 13f);
    [SerializeField] private float _waveAmount = 12f;                   // 위아래로 출렁이는 거리(px)

    private const float Margin = 40f; // 화면 밖 여유 거리

    private RectTransform _rectTransform;
    private Bird[] _birds;
    private float _direction = 1f;

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;

        _birds = new Bird[_flockSize.y];
        for (int i = 0; i < _birds.Length; i++)
        {
            GameObject go = new GameObject("Bird", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = gameObject.layer;
            go.transform.SetParent(transform, false);

            Image image = go.GetComponent<Image>();
            image.sprite = _birdSprite;
            image.color = _birdColor;
            image.raycastTarget = false;
            image.enabled = false;

            _birds[i] = new Bird { Rect = (RectTransform)go.transform, Image = image };
        }
    }

    private void OnEnable()
    {
        StartCoroutine(Spawn_Coroutine());
    }

    private IEnumerator Spawn_Coroutine()
    {
        yield return new WaitForSecondsRealtime(_firstDelay);

        while (true)
        {
            SpawnFlock();
            yield return new WaitForSecondsRealtime(Random.Range(_interval.x, _interval.y));
        }
    }

    private void SpawnFlock()
    {
        Vector2 half = _rectTransform.rect.size * 0.5f;
        _direction = Random.value < 0.5f ? -1f : 1f;

        float startX = -_direction * (half.x + Margin);
        float startY = Random.Range(-half.y * 0.2f, half.y * 0.6f);
        float speed = Random.Range(_speed.x, _speed.y);
        int count = Random.Range(_flockSize.x, _flockSize.y + 1);

        for (int i = 0; i < _birds.Length; i++)
        {
            Bird bird = _birds[i];
            bird.IsFlying = i < count;
            bird.Image.enabled = bird.IsFlying;
            if (!bird.IsFlying) continue;

            // 뒤로 갈수록 조금씩 뒤처지고 위아래로 흩어져 느슨한 V자가 되게 한다.
            float behind = i * Random.Range(25f, 40f);
            float spread = (i % 2 == 0 ? 1f : -1f) * Mathf.Ceil(i * 0.5f) * Random.Range(10f, 18f);

            bird.Position = new Vector2(startX - _direction * behind, startY + spread);
            bird.Speed = speed * Random.Range(0.95f, 1.05f);
            bird.Phase = Random.Range(0f, Mathf.PI * 2f);
            bird.FlapSpeed = Random.Range(_flapSpeed.x, _flapSpeed.y);

            float size = Random.Range(_birdSize.x, _birdSize.y);
            bird.Rect.sizeDelta = new Vector2(size, size * 0.5f);
        }
    }

    private void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        float time = Time.unscaledTime;
        float limitX = _rectTransform.rect.width * 0.5f + Margin * 2f;

        foreach (Bird bird in _birds)
        {
            if (!bird.IsFlying) continue;

            bird.Position.x += _direction * bird.Speed * deltaTime;

            // 화면을 다 지나가면 숨긴다(뒤처진 새는 아직 화면에 들어오기 전일 수 있으므로 진행 방향 쪽만 본다).
            if (bird.Position.x * _direction > limitX)
            {
                bird.IsFlying = false;
                bird.Image.enabled = false;
                continue;
            }

            float wave = Mathf.Sin(bird.Position.x * 0.01f + bird.Phase) * _waveAmount;
            bird.Rect.anchoredPosition = new Vector2(bird.Position.x, bird.Position.y + wave);

            // 날갯짓: 세로로 접었다 폈다 한다.
            float flap = 0.35f + 0.65f * Mathf.Abs(Mathf.Sin(time * bird.FlapSpeed + bird.Phase));
            bird.Rect.localScale = new Vector3(1f, flap, 1f);
        }
    }
}
