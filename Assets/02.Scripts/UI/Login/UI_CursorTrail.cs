using UnityEngine;
using UnityEngine.UI;

// 마우스를 따라 노을빛 불씨가 꼬리처럼 남고, 클릭하면 불씨가 사방으로 터진다.
// 불씨 Image는 미리 만들어 두고 돌려 쓴다.
public class UI_CursorTrail : MonoBehaviour
{
    private class Spark
    {
        public RectTransform Rect;
        public Image Image;
        public bool IsAlive;
        public Vector2 Position;
        public Vector2 Velocity;
        public float Life;
        public float MaxLife;
        public float Size;
        public float Gravity;
    }

    [Header("참조")]
    [SerializeField] private Sprite _sprite;
    [SerializeField] private GameObject _fader; // 켜져 있으면(등장·전환 중) 새 불씨를 만들지 않는다

    [Header("풀")]
    [SerializeField] private int _poolSize = 40;

    [Header("꼬리")]
    [SerializeField] private float _spawnDistance = 14f;                  // 이만큼 움직일 때마다 불씨 하나(px)
    [SerializeField] private Vector2 _trailLife = new Vector2(0.4f, 0.6f);
    [SerializeField] private Vector2 _trailSize = new Vector2(26f, 40f);  // Glow 텍스처는 가장자리가 옅어 실제로 보이는 크기는 1/3 정도다
    [SerializeField] private float _trailRise = 25f;                       // 위로 떠오르는 속도(px/초)

    [Header("클릭 버스트")]
    [SerializeField] private int _burstCount = 10;
    [SerializeField] private Vector2 _burstSpeed = new Vector2(150f, 320f);
    [SerializeField] private Vector2 _burstLife = new Vector2(0.4f, 0.7f);
    [SerializeField] private Vector2 _burstSize = new Vector2(18f, 30f);
    [SerializeField] private float _burstGravity = 500f;
    [SerializeField] private float _burstDrag = 3f;

    [Header("색")]
    [SerializeField] private Color _colorA = new Color(1f, 0.55f, 0.15f, 0.9f);
    [SerializeField] private Color _colorB = new Color(1f, 0.85f, 0.4f, 0.9f);

    private RectTransform _rectTransform;
    private Spark[] _sparks;
    private int _nextIndex;
    private int _activeCount; // 세기 설정에 따라 실제로 쓰는 개수
    private Vector2 _lastSpawnPosition;
    private bool _hasLastPosition;

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;

        _sparks = new Spark[_poolSize];
        for (int i = 0; i < _sparks.Length; i++)
        {
            GameObject go = new GameObject("Spark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = gameObject.layer;
            go.transform.SetParent(transform, false);

            Image image = go.GetComponent<Image>();
            image.sprite = _sprite;
            image.raycastTarget = false;
            image.enabled = false;

            _sparks[i] = new Spark { Rect = (RectTransform)go.transform, Image = image };
        }

        _activeCount = _sparks.Length;
    }

    private void Update()
    {
        bool canSpawn = _activeCount > 0 && (_fader == null || !_fader.activeInHierarchy);
        if (canSpawn)
        {
            if (TryGetLocalPoint(Input.mousePosition, out Vector2 mouse))
            {
                if (!_hasLastPosition)
                {
                    _lastSpawnPosition = mouse;
                    _hasLastPosition = true;
                }

                // 많이 움직였으면 그 사이를 일정 간격으로 채워, 빠르게 움직여도 꼬리가 끊기지 않게 한다.
                float distance = Vector2.Distance(mouse, _lastSpawnPosition);
                while (distance >= _spawnDistance)
                {
                    _lastSpawnPosition = Vector2.MoveTowards(_lastSpawnPosition, mouse, _spawnDistance);
                    SpawnTrail(_lastSpawnPosition);
                    distance -= _spawnDistance;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                BurstAt(Input.mousePosition);
            }
        }

        UpdateSparks(Time.unscaledDeltaTime);
    }

    // 화면 좌표에 꼬리 불씨 하나를 놓는다(테스트용으로도 쓴다).
    public void SpawnAt(Vector2 screenPosition)
    {
        if (TryGetLocalPoint(screenPosition, out Vector2 local))
        {
            SpawnTrail(local);
        }
    }

    // 화면 좌표에서 불씨를 사방으로 터뜨린다.
    public void BurstAt(Vector2 screenPosition)
    {
        if (_activeCount <= 0) return;
        if (!TryGetLocalPoint(screenPosition, out Vector2 local)) return;

        int count = Mathf.Max(1, Mathf.RoundToInt(_burstCount * (float)_activeCount / _sparks.Length));
        for (int i = 0; i < count; i++)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            Spark spark = NextSpark();
            Activate(spark, local, direction * Random.Range(_burstSpeed.x, _burstSpeed.y),
                Random.Range(_burstLife.x, _burstLife.y), Random.Range(_burstSize.x, _burstSize.y), _burstGravity);
        }
    }

    // 세기 설정: 0이면 끄고, 0.5면 절반만 쓴다.
    public void SetDensity(float ratio)
    {
        _activeCount = Mathf.RoundToInt(_sparks.Length * Mathf.Clamp01(ratio));
        _nextIndex = 0;

        for (int i = _activeCount; i < _sparks.Length; i++)
        {
            _sparks[i].IsAlive = false;
            _sparks[i].Image.enabled = false;
        }
    }

    private void SpawnTrail(Vector2 local)
    {
        Spark spark = NextSpark();
        if (spark == null) return;

        Vector2 velocity = new Vector2(Random.Range(-10f, 10f), _trailRise);
        Activate(spark, local, velocity, Random.Range(_trailLife.x, _trailLife.y), Random.Range(_trailSize.x, _trailSize.y), 0f);
    }

    private Spark NextSpark()
    {
        if (_activeCount <= 0) return null;

        // 가장 오래된 불씨부터 다시 쓴다.
        Spark spark = _sparks[_nextIndex];
        _nextIndex = (_nextIndex + 1) % _activeCount;
        return spark;
    }

    private void Activate(Spark spark, Vector2 position, Vector2 velocity, float life, float size, float gravity)
    {
        if (spark == null) return;

        spark.IsAlive = true;
        spark.Position = position;
        spark.Velocity = velocity;
        spark.Life = 0f;
        spark.MaxLife = life;
        spark.Size = size;
        spark.Gravity = gravity;
        spark.Image.color = Color.Lerp(_colorA, _colorB, Random.value);
        spark.Image.enabled = true;
    }

    private void UpdateSparks(float deltaTime)
    {
        float drag = Mathf.Exp(-_burstDrag * deltaTime);

        foreach (Spark spark in _sparks)
        {
            if (!spark.IsAlive) continue;

            spark.Life += deltaTime;
            if (spark.Life >= spark.MaxLife)
            {
                spark.IsAlive = false;
                spark.Image.enabled = false;
                continue;
            }

            // 버스트 불씨는 감속하면서 아래로 떨어진다.
            if (spark.Gravity > 0f)
            {
                spark.Velocity *= drag;
                spark.Velocity.y -= spark.Gravity * deltaTime;
            }

            spark.Position += spark.Velocity * deltaTime;

            // 수명이 다할수록 작아지고 투명해진다.
            float remain = 1f - spark.Life / spark.MaxLife;
            spark.Rect.anchoredPosition = spark.Position;
            spark.Rect.sizeDelta = Vector2.one * (spark.Size * remain);

            Color color = spark.Image.color;
            color.a = Mathf.Lerp(0f, Mathf.Max(_colorA.a, _colorB.a), remain);
            spark.Image.color = color;
        }
    }

    private bool TryGetLocalPoint(Vector2 screenPosition, out Vector2 local)
    {
        // Overlay 캔버스이므로 카메라는 null이다.
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, screenPosition, null, out local);
    }
}
