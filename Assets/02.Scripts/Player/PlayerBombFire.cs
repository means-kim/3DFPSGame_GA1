using UnityEngine;

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

    private int _currentBomb = 0;
    private float _chargeTime = 0f;

    private void Update()
    {
        // 1. 마우스 오른쪽 버튼 입력 받기
        if (Input.GetButtonDown("Fire2"))
        {
            _chargeTime = 0f;
        }

        if (Input.GetButton("Fire2"))
        {
            _chargeTime += Time.deltaTime;
        }

        if (Input.GetButtonUp("Fire2"))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (_currentBomb < _maxBomb)
        {
            // 2. 발사 위치에서 수류탄 생성하기
            Bomb bomb = Instantiate(_bombPrefab, _firePoint.position, Quaternion.identity);

            // 3. 차징 시간에 따라 던지는 힘 정하기
            float chargeRatio = Mathf.Clamp01(_chargeTime / _maxChargeTime);
            float power = Mathf.Lerp(_throwPower, _maxThrowPower, chargeRatio);

            // 4. 생성된 수류탄을 시선 방향으로 던지기
            Rigidbody rigidbody = bomb.GetComponent<Rigidbody>();
            rigidbody.AddForce(Camera.main.transform.forward * power, ForceMode.Impulse);

            _currentBomb++;
        }
    }
}