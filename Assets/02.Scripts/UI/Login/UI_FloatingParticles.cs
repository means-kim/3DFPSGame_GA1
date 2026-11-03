using UnityEngine;
using UnityEngine.UI;

// 햇빛 속을 떠다니는 먼지와 아래에서 올라가는 불씨를 Image로 흉내 낸다.
// Overlay 캔버스는 ParticleSystem을 그리지 못하므로, 미리 만든 Image를 계속 재사용한다.
public class UI_FloatingParticles : MonoBehaviour
{
    private class Particle
    {
        public RectTransform Rect;
        public Image Image;
        public bool IsEmber;
        public Vector2 Position;
        public Vector2 Velocity;
        public float SwayAmount;
        public float SwaySpeed;
        public float Phase;
        public float Life;
        public float MaxLife;
        public float BaseAlpha;
        public bool IsEnabled = true; // 세기 설정으로 꺼진 입자는 움직이지도 그리지도 않는다
    }

    [Header("참조")]
    [SerializeField] private Sprite _sprite;
    [SerializeField] private RectTransform _sun; // 먼지가 해 근처에서 더 밝게 보이도록 쓴다

    [Header("먼지")]
    [SerializeField] private int _dustCount = 18;
    [SerializeField] private Color _dustColor = new Color(1f, 0.95f, 0.85f, 0.5f);
    [SerializeField] private Vector2 _dustSize = new Vector2(6f, 16f);
    [SerializeField] private Vector2 _dustSpeed = new Vector2(5f, 20f);
    [SerializeField] private Vector2 _dustLife = new Vector2(6f, 12f);
    [SerializeField] private float _sunBoostRadius = 500f; // 이 거리 안에서 먼지가 밝아진다
    [SerializeField] private float _sunBoost = 1.5f;

    [Header("불씨")]
    [SerializeField] private int _emberCount = 12;
    [SerializeField] private Color _emberColor = new Color(1f, 0.55f, 0.15f, 0.9f);
    [SerializeField] private Vector2 _emberSize = new Vector2(5f, 11f);
    [SerializeField] private Vector2 _emberSpeed = new Vector2(40f, 90f);
    [SerializeField] private Vector2 _emberLife = new Vector2(4f, 8f);
    [SerializeField] private float _emberFlickerSpeed = 9f;

    private const float FadeRatio = 0.2f; // 수명의 앞뒤 20% 동안 나타나고 사라진다

    private RectTransform _rectTransform;
    private Particle[] _particles;

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;

        _particles = new Particle[_dustCount + _emberCount];
        for (int i = 0; i < _particles.Length; i++)
        {
            _particles[i] = CreateParticle(i >= _dustCount);
        }
    }

    private void Start()
    {
        // 처음부터 화면에 고루 퍼져 있도록 수명을 무작위로 앞당겨 둔다.
        foreach (Particle particle in _particles)
        {
            Respawn(particle, true);
        }
    }

    // 세기 설정: 먼지와 불씨를 각각 비율만큼만 쓴다(1이면 전부, 0.3이면 30%).
    public void SetDensity(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        int dustLimit = Mathf.RoundToInt(_dustCount * ratio);
        int emberLimit = Mathf.RoundToInt(_emberCount * ratio);

        for (int i = 0; i < _particles.Length; i++)
        {
            bool isEmber = i >= _dustCount;
            int indexInGroup = isEmber ? i - _dustCount : i;
            bool isEnabled = indexInGroup < (isEmber ? emberLimit : dustLimit);

            _particles[i].IsEnabled = isEnabled;
            _particles[i].Image.enabled = isEnabled;
        }
    }

    private Particle CreateParticle(bool isEmber)
    {
        GameObject go = new GameObject(isEmber ? "Ember" : "Dust", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = gameObject.layer;
        go.transform.SetParent(transform, false);

        Image image = go.GetComponent<Image>();
        image.sprite = _sprite;
        image.raycastTarget = false;

        return new Particle { Rect = (RectTransform)go.transform, Image = image, IsEmber = isEmber };
    }

    private void Respawn(Particle particle, bool randomizeLife)
    {
        Vector2 half = _rectTransform.rect.size * 0.5f;
        float size;

        if (particle.IsEmber)
        {
            // 불씨: 화면 아래쪽에서 태어나 위로 올라간다.
            particle.Position = new Vector2(Random.Range(-half.x, half.x), Random.Range(-half.y, -half.y * 0.3f));
            particle.Velocity = new Vector2(Random.Range(-15f, 15f), Random.Range(_emberSpeed.x, _emberSpeed.y));
            particle.MaxLife = Random.Range(_emberLife.x, _emberLife.y);
            particle.BaseAlpha = _emberColor.a;
            particle.Image.color = _emberColor;
            size = Random.Range(_emberSize.x, _emberSize.y);
        }
        else
        {
            // 먼지: 화면 어디서나 태어나 느리게 떠다닌다.
            particle.Position = new Vector2(Random.Range(-half.x, half.x), Random.Range(-half.y, half.y));
            particle.Velocity = Random.insideUnitCircle.normalized * Random.Range(_dustSpeed.x, _dustSpeed.y) + Vector2.up * 4f;
            particle.MaxLife = Random.Range(_dustLife.x, _dustLife.y);
            particle.BaseAlpha = _dustColor.a;
            particle.Image.color = _dustColor;
            size = Random.Range(_dustSize.x, _dustSize.y);
        }

        particle.SwayAmount = Random.Range(5f, 20f);
        particle.SwaySpeed = Random.Range(0.5f, 1.5f);
        particle.Phase = Random.Range(0f, Mathf.PI * 2f);
        particle.Life = randomizeLife ? Random.Range(0f, particle.MaxLife) : 0f;
        particle.Rect.sizeDelta = new Vector2(size, size);
    }

    private void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        float time = Time.unscaledTime;
        Vector2 half = _rectTransform.rect.size * 0.5f + Vector2.one * 30f;
        Vector2 sunPosition = _sun != null ? (Vector2)_rectTransform.InverseTransformPoint(_sun.position) : Vector2.zero;

        foreach (Particle particle in _particles)
        {
            if (!particle.IsEnabled) continue;

            particle.Life += deltaTime;
            particle.Position += particle.Velocity * deltaTime;

            bool outside = Mathf.Abs(particle.Position.x) > half.x || Mathf.Abs(particle.Position.y) > half.y;
            if (particle.Life >= particle.MaxLife || outside)
            {
                Respawn(particle, false);
            }

            // 좌우로 살랑이며 흔들린다.
            float sway = Mathf.Sin(time * particle.SwaySpeed + particle.Phase) * particle.SwayAmount;
            particle.Rect.anchoredPosition = particle.Position + new Vector2(sway, 0f);

            // 수명의 처음과 끝에서 서서히 나타나고 사라진다.
            float lifeRatio = particle.Life / particle.MaxLife;
            float fade = Mathf.Min(Mathf.Clamp01(lifeRatio / FadeRatio), Mathf.Clamp01((1f - lifeRatio) / FadeRatio));
            float alpha = particle.BaseAlpha * fade;

            if (particle.IsEmber)
            {
                // 불씨는 반짝인다.
                alpha *= 0.6f + 0.4f * Mathf.Sin(time * _emberFlickerSpeed + particle.Phase);
            }
            else if (_sun != null)
            {
                // 먼지는 해에 가까울수록 빛을 받아 밝아진다.
                float closeness = 1f - Mathf.Clamp01(Vector2.Distance(particle.Rect.anchoredPosition, sunPosition) / _sunBoostRadius);
                alpha *= Mathf.Lerp(1f, _sunBoost, closeness);
            }

            Color color = particle.Image.color;
            color.a = Mathf.Clamp01(alpha);
            particle.Image.color = color;
        }
    }
}
