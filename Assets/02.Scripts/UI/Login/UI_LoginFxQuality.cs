using UnityEngine;
using UnityEngine.UI;

// 로그인 화면 연출의 전체 세기를 한곳에서 정한다(낮음/보통/높음).
// 높음이 기본 화면이고, 낮출수록 입자 수를 줄이고 부가 레이어를 끈다.
// 이후 옵션 메뉴에서 SetLevel()을 호출해 바꿀 수 있다.
public class UI_LoginFxQuality : MonoBehaviour
{
    public enum Level
    {
        Low,
        Medium,
        High,
    }

    [Header("세기")]
    [SerializeField] private Level _level = Level.High;

    [Header("참조")]
    [SerializeField] private UI_FloatingParticles _particles;
    [SerializeField] private UI_CursorTrail _cursorTrail;
    [SerializeField] private RawImage _filmGrain;
    [SerializeField] private RawImage _background;     // 아지랑이 머티리얼이 붙어 있는 배경
    [SerializeField] private GameObject _sky;          // 구름·새
    [SerializeField] private GameObject _zombies;
    [SerializeField] private UI_LightFlicker[] _lights;

    private const string HazeStrengthProperty = "_Strength";

    private float _grainAlpha;
    private Material _hazeSource;   // 원본 머티리얼 에셋(직접 고치지 않는다)
    private Material _hazeInstance; // 세기를 바꿀 때 쓰는 런타임 복사본
    private float _hazeStrength;

    public Level CurrentLevel => _level;

    private void Awake()
    {
        _grainAlpha = _filmGrain.color.a;

        // 공유 머티리얼 에셋을 고치면 에디터에서 에셋 자체가 바뀌므로, 런타임 복사본을 만들어 쓴다.
        _hazeSource = _background.material;
        _hazeInstance = new Material(_hazeSource);
        _hazeStrength = _hazeSource.GetFloat(HazeStrengthProperty);
    }

    private void Start()
    {
        Apply(_level);
    }

    private void OnDestroy()
    {
        if (_hazeInstance != null) Destroy(_hazeInstance);
    }

    public void SetLevel(Level level)
    {
        _level = level;
        Apply(level);
    }

    private void Apply(Level level)
    {
        bool isLow = level == Level.Low;
        bool isHigh = level == Level.High;

        // 입자 개수: 낮음 30%, 보통 60%, 높음 100%
        _particles.SetDensity(isLow ? 0.3f : isHigh ? 1f : 0.6f);
        _cursorTrail.SetDensity(isLow ? 0f : isHigh ? 1f : 0.5f);

        // 필름 그레인: 낮음 끔, 보통 절반
        _filmGrain.gameObject.SetActive(!isLow);
        SetAlpha(_filmGrain, isHigh ? _grainAlpha : _grainAlpha * 0.5f);

        // 아지랑이: 낮음은 기본 UI 머티리얼, 보통은 세기 절반
        if (isLow)
        {
            _background.material = null;
        }
        else
        {
            _hazeInstance.SetFloat(HazeStrengthProperty, isHigh ? _hazeStrength : _hazeStrength * 0.5f);
            _background.material = _hazeInstance;
        }

        // 부가 레이어: 낮음에서만 끈다
        _sky.SetActive(!isLow);
        _zombies.SetActive(!isLow);

        foreach (UI_LightFlicker light in _lights)
        {
            light.SetFlickerEnabled(!isLow);
        }
    }

    private static void SetAlpha(Graphic graphic, float alpha)
    {
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }
}
