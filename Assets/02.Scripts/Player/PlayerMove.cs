using UnityEngine;

// 키보드 방향키에 따라 그 방향으로 이동하고 싶다.
public class PlayerMove : MonoBehaviour
{
    private static float Gravity = -20f;
    private float _yVelocity = 0f; // 위 아래 속도, 매 프레임마다 중력 가속도가 붙는다.

    [SerializeField] private float _moveSpeed = 7f;
    [SerializeField] private float _jumpPower = 10f;
    [SerializeField] private CharacterController _characterController;

    private float _currentSpeed;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
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

        // 3. 땅에 서있지 않을때만 중력을 누적한다.
        if (_characterController.isGrounded == false)
        {
            _yVelocity += Gravity * Time.deltaTime; // -3000, -4000, -5000
        }
        else
        {
            // 땅에 서있으면 중력 초기화
            _yVelocity = 0f;
        }

        // 4. 만약에 점프키를 누르면 && 땅이라면... 점프를 한다.
        if (Input.GetButtonDown("Jump") && _characterController.isGrounded) // 중재자 패턴
        {
            _yVelocity = _jumpPower;
        }

        // 5. Shift를 누르면 스피드를 12로 올린다.
        if (Input.GetKey(KeyCode.LeftShift))
        {
            _currentSpeed = 12f;
        }
        else
        {
            _currentSpeed = _moveSpeed;
        }

        // 6. 속도를 구한다.
        Vector3 velocity = direction * _currentSpeed;
        velocity.y = _yVelocity;

        // 7. 속도(방향과 속력)에 따라 이동한다.
        // transform.position += direction * _moveSpeed * Time.deltaTime;
        _characterController.Move(velocity * Time.deltaTime);
    }
}