using UnityEngine;

// 폭발 순간 번쩍였다가 빠르게 사그라드는 조명
[RequireComponent(typeof(Light))]
public class ExplosionLight : MonoBehaviour
{
    [Header("밝기")]
    [SerializeField] private float _peakIntensity = 300f;
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private Light _light;
    private float _elapsed;

    private void Awake()
    {
        _light = GetComponent<Light>();
    }

    // 풀에서 다시 꺼내질 때마다 최대 밝기로 되돌린다.
    private void OnEnable()
    {
        _elapsed = 0f;
        _light.intensity = _peakIntensity;
        _light.enabled = true;
    }

    private void Update()
    {
        if (!_light.enabled) return;

        _elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(_elapsed / _fadeDuration);
        _light.intensity = _peakIntensity * _fadeCurve.Evaluate(progress);

        // 다 사그라들면 조명을 꺼서 비용을 없앤다.
        if (progress >= 1f)
        {
            _light.enabled = false;
        }
    }
}
