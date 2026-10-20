using UnityEngine.UI;
using UnityEngine;

// 키보드 방향키에 따라 그 방향으로 이동하고 싶다.
public class PlayerMove : MonoBehaviour
{
    private const float _gravity = -20f;
    private const int _maxJumps = 2;
    private const float _maxStamina = 100f;

    private float _yVelocity = 0f; // 위 아래 속도, 매 프레임마다 중력 가속도가 붙는다.
    private int _jumpCount = 0; // 점프 횟수
    private float _stamina = 0f;

    // 코요테 타임: 발판에서 조금 늦게 눌러도 첫 점프를 허용해 조작 실수를 보완한다.
    [Header("코요테 타임")]
    [Min(0f)] public float coyoteTime = 0.12f;
    private float _coyoteTimer;

    // 점프 버퍼: 착지 직전의 입력을 잠시 기억해 점프 타이밍을 맞추기 쉽게 한다.
    [Header("점프 버퍼")]
    [Min(0f)] public float jumpBufferTime = 0.15f;
    private float _jumpBufferTimer;
    private bool _hasBufferedJump;

    // 가변 점프 높이: 버튼을 일찍 떼면 상승 속도를 줄여 낮은 점프도 조절할 수 있다.
    [Header("가변 점프 높이")]
    [Range(0f, 1f)] public float jumpReleaseMultiplier = 0.5f;
    private bool _canCutJump;

    [Header("스탯")]
    [SerializeField] private float _moveSpeed = 7f;

    [SerializeField] private float _sprintSpeed = 12f;
    [SerializeField] private float _jumpPower = 10f;
    [SerializeField] private float _staminaRecovery = 15f;
    [SerializeField] private float _sprintCost = 10f;
    [SerializeField] private float _jumpCost = 20f;

    [Header("컴포넌트")]
    [SerializeField] private CharacterController _characterController;

    [Header("스태미나 UI")]
    [SerializeField] private Slider _staminaSlider;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _stamina = _maxStamina;
    }

    private void Update()
    {
        // 1. 키보드 입력을 받는다.
        float h = Input.GetAxisRaw("Horizontal"); // -1, 0, 1 값 반환
        float v = Input.GetAxisRaw("Vertical"); // -1, 0, 1 값 반환

        // 2. 키보드 입력에 따라 방향을 구한다.
        Vector3 direction = new Vector3(h, 0, v);
        direction.Normalize(); // 대각선 방향으로 이동해도 길이를 1로 바꾸기 위해'
        direction = transform.TransformDirection(direction);
        // 로컬 좌표게: 한 대상을 중심 
        // 월드 좌표계: 세상을 중심

        // 점프 버퍼: 누른 프레임은 시간 설정이 0이어도 처리하고, 이후에는 유효 시간만 유지한다.
        _jumpBufferTimer -= Time.deltaTime;
        if (_jumpBufferTimer <= 0f)
        {
            _hasBufferedJump = false;
        }

        if (Input.GetButtonDown("Jump"))
        {
            _jumpBufferTimer = Mathf.Max(0f, jumpBufferTime);
            _hasBufferedJump = true;
        }

        // 코요테 타임: 상승 중에는 이전 Move의 접지 정보로 점프 횟수를 초기화하지 않는다.
        bool isGrounded = _characterController.isGrounded && _yVelocity <= 0f;
        if (isGrounded)
        {
            _yVelocity = -1f;
            _jumpCount = 0;
            _coyoteTimer = Mathf.Max(0f, coyoteTime);
            _canCutJump = false;
        }
        else
        {
            _coyoteTimer -= Time.deltaTime;
            _yVelocity += _gravity * Time.deltaTime;

            // 코요테 타임: 유예 시간이 끝난 낙하는 첫 점프 기회를 사용한 것으로 처리해
            // 발판에서 걸어 떨어져도 공중 점프는 기존 2단 점프 중 한 번만 남긴다.
            if (_coyoteTimer <= 0f && _jumpCount == 0)
            {
                _jumpCount = 1;
            }
        }

        // 점프 버퍼 + 코요테 타임: 저장된 입력을 가능한 순간 한 번만 사용한다.
        // 공중 점프가 남아 있으면 즉시 2단 점프하고, 모두 사용했다면 착지까지 입력을 기억한다.
        bool canJump = isGrounded || _coyoteTimer > 0f || _jumpCount < _maxJumps;
        if (_hasBufferedJump && canJump && _stamina >= _jumpCost)
        {
            _yVelocity = _jumpPower;
            _jumpCount++;
            _stamina -= _jumpCost;
            _coyoteTimer = 0f;
            _hasBufferedJump = false;
            _canCutJump = true;
        }

        // 가변 점프 높이: 상승 속도는 한 번만 줄여 프레임 수에 따라 과하게 감속되지 않게 한다.
        // 버퍼 대기 중 이미 버튼을 뗀 경우에도 낮은 점프로 반응한다.
        if (_canCutJump && !Input.GetButton("Jump") && _yVelocity > 0f)
        {
            _yVelocity *= Mathf.Clamp01(jumpReleaseMultiplier);
            _canCutJump = false;
        }

        // 5. 속도를 구한다.
        float requiredStamina = _sprintCost * Time.deltaTime;
        bool isSprint = Input.GetButton("Sprint") && _stamina > requiredStamina;
        float currentSpeed = _moveSpeed;

        if (isSprint)
        {
            // 질주 속도에는 소모량이 아닌 설정된 이동 속도를 적용한다.
            currentSpeed = _sprintSpeed;
            _stamina -= requiredStamina;
        }
        else
        {
            _stamina = Mathf.Min(_maxStamina, _stamina + _staminaRecovery * Time.deltaTime);
        }

        _staminaSlider.value = _stamina / _maxStamina;

        Vector3 velocity = direction * currentSpeed;
        velocity.y = _yVelocity;

        // 6. 속도(방향과 속력)에 따라 이동한다.
        // transform.position += direction * _moveSpeed * Time.deltaTime;
        _characterController.Move(velocity * Time.deltaTime);
    }
}
