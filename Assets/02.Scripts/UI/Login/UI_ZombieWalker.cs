using UnityEngine;

// 멀리 있는 좀비 실루엣이 정해진 범위 안에서 비틀거리며 천천히 좌우로 걷는다.
// 가끔 멈춰 서서 두리번거리듯 쉬었다가 다시 걷는다.
public class UI_ZombieWalker : MonoBehaviour
{
    [Header("걷기")]
    [SerializeField] private float _walkSpeed = 12f;   // px/초
    [SerializeField] private float _range = 50f;       // 처음 위치에서 좌우로 움직일 수 있는 거리
    [SerializeField] private Vector2 _pauseInterval = new Vector2(3f, 8f);
    [SerializeField] private Vector2 _pauseDuration = new Vector2(0.8f, 2.5f);

    [Header("비틀거림")]
    [SerializeField] private float _stepFrequency = 4f;
    [SerializeField] private float _bobAmount = 1.5f;  // 걸음마다 위아래로 흔들리는 거리(px)
    [SerializeField] private float _tiltAngle = 7f;    // 몸이 좌우로 기우는 각도

    private RectTransform _rectTransform;
    private Vector2 _home;
    private float _offsetX;
    private float _direction;
    private float _phase;
    private float _pauseTimer;
    private float _nextPause;

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;
        _home = _rectTransform.anchoredPosition;

        // 좀비마다 시작 위치·방향·박자를 다르게 해서 같이 움직이는 느낌이 나지 않게 한다.
        _offsetX = Random.Range(-_range, _range);
        _direction = Random.value < 0.5f ? -1f : 1f;
        _phase = Random.Range(0f, 10f);
        _nextPause = Random.Range(_pauseInterval.x, _pauseInterval.y);
    }

    private void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        _phase += deltaTime;

        bool isWalking = _pauseTimer <= 0f;
        if (isWalking)
        {
            _offsetX += _direction * _walkSpeed * deltaTime;

            // 범위 끝에 닿으면 돌아선다.
            if (Mathf.Abs(_offsetX) > _range)
            {
                _offsetX = Mathf.Clamp(_offsetX, -_range, _range);
                _direction = -_direction;
            }

            _nextPause -= deltaTime;
            if (_nextPause <= 0f)
            {
                _pauseTimer = Random.Range(_pauseDuration.x, _pauseDuration.y);
                _nextPause = Random.Range(_pauseInterval.x, _pauseInterval.y);
            }
        }
        else
        {
            _pauseTimer -= deltaTime;
        }

        // 걸을 때는 걸음마다 흔들리고, 멈췄을 때는 아주 약하게만 흔들린다.
        float stepSpeed = isWalking ? _stepFrequency : _stepFrequency * 0.3f;
        float step = Mathf.Sin(_phase * stepSpeed);
        float bob = Mathf.Abs(step) * (isWalking ? _bobAmount : _bobAmount * 0.3f);

        _rectTransform.anchoredPosition = _home + new Vector2(_offsetX, bob);
        _rectTransform.localRotation = Quaternion.Euler(0f, 0f, step * _tiltAngle);

        // 실루엣은 오른쪽을 보고 있으므로, 왼쪽으로 걸을 때 뒤집는다.
        _rectTransform.localScale = new Vector3(_direction, 1f, 1f);
    }
}
