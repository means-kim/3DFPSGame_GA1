using System;
using UnityEngine;
using Lean.Pool;

// 마우스 오른쪽 버튼을 누르면 시선이 바라보는 방향으로 수류탄을 던지고 싶다.
public class PlayerBombFire : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Bomb _bombPrefab;

    [Header("수류탄 파워")]
    [SerializeField] private float _throwPower = 15f;

    [SerializeField] private float _maxThrowPower = 30f;

    [Header("수류탄 개수")]
    [SerializeField] private int _maxBomb = 10;

    [SerializeField] private float _maxChargeTime = 2f;

    [Header("던지기 애니메이션")]
    [SerializeField] private int _throwHoldFrame = 16; // 차징 중 멈출 프레임

    [SerializeField] private int _throwTotalFrames = 66; // Throw 클립의 전체 프레임 수
    [SerializeField] private string _fireLayerName = "Fire Layer";

    private static readonly int ThrowHash = Animator.StringToHash("Throw");
    private static readonly int ThrowSpeedHash = Animator.StringToHash("ThrowSpeed");
    private const string ThrowStateName = "Throw";

    private int _currentBomb = 0;
    private float _chargeTime = 0f;

    private Animator _animator;
    private int _fireLayerIndex;
    private float _holdNormalizedTime;

    private Player _player;

    // 남은 폭탄 개수 (_currentBomb은 던진 개수)
    public int RemainBomb => _maxBomb - _currentBomb;

    // 남은 폭탄 개수가 바뀔 때 알린다.
    public event Action<int> OnBombChanged = null;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _fireLayerIndex = _animator.GetLayerIndex(_fireLayerName);
        _holdNormalizedTime = (float)_throwHoldFrame / _throwTotalFrames;
        _player = GetComponent<Player>();
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        // 수류탄 던지기(애니메이션 포함)는 일반 모드에서만 가능하다.
        if (_player.WeaponMode != WeaponMode.Normal)
        {
            // 차징 중 모드가 바뀌면 멈춰 있던 던지기 애니메이션을 다시 재생시킨다.
            if (_animator.GetFloat(ThrowSpeedHash) <= 0f)
            {
                _animator.SetFloat(ThrowSpeedHash, 1f);
            }

            return;
        }

        // 1. 마우스 오른쪽 버튼 입력 받기
        if (Input.GetButtonDown("Fire2"))
        {
            _chargeTime = 0f;

            // 남은 수류탄이 있을 때만 상체 던지기 애니메이션을 시작한다.
            if (_currentBomb < _maxBomb)
            {
                _animator.SetFloat(ThrowSpeedHash, 1f);
                _animator.SetTrigger(ThrowHash);
            }
        }

        if (Input.GetButton("Fire2"))
        {
            _chargeTime += Time.deltaTime;

            // 차징 중에는 지정한 프레임에서 애니메이션을 멈춘다.
            if (_animator.GetFloat(ThrowSpeedHash) > 0f && HasReachedHoldFrame())
            {
                _animator.SetFloat(ThrowSpeedHash, 0f);
            }
        }

        if (Input.GetButtonUp("Fire2"))
        {
            // 버튼을 놓으면 멈춘 지점부터 이후 애니메이션을 재생한다.
            _animator.SetFloat(ThrowSpeedHash, 1f);
            Shoot();
        }
    }

    // Throw 상태가 멈출 프레임까지 재생됐는지 확인한다.
    private bool HasReachedHoldFrame()
    {
        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(_fireLayerIndex);

        // 전환 중에는 현재 상태가 아직 이전 상태이므로 다음 상태를 확인한다.
        if (!info.IsName(ThrowStateName))
        {
            if (!_animator.IsInTransition(_fireLayerIndex)) return false;

            info = _animator.GetNextAnimatorStateInfo(_fireLayerIndex);
            if (!info.IsName(ThrowStateName)) return false;
        }

        return info.normalizedTime >= _holdNormalizedTime;
    }

    private void Shoot()
    {
        if (_currentBomb < _maxBomb)
        {
            // 2. 발사 위치에서 수류탄 생성하기
            Bomb bomb = LeanPool.Spawn(_bombPrefab, _firePoint.position, Quaternion.identity);

            // 3. 차징 시간에 따라 던지는 힘 정하기
            float chargeRatio = Mathf.Clamp01(_chargeTime / _maxChargeTime);
            float power = Mathf.Lerp(_throwPower, _maxThrowPower, chargeRatio);

            // 4. 생성된 수류탄을 시선 방향으로 던지기
            Rigidbody rigidbody = bomb.GetComponent<Rigidbody>();

            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;

            rigidbody.AddForce(Camera.main.transform.forward * power, ForceMode.Impulse);

            _currentBomb++;
            OnBombChanged?.Invoke(RemainBomb);
        }
    }
}