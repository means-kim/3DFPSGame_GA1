using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerStats : MonoBehaviour
{
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _staminaSlider;

    private Player _player;
    private PlayerMove _stamina;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _stamina = GetComponent<PlayerMove>();
    }

    private void Start()
    {
        _player.OnDamaged += RefreshHealth;
    }

    // 업데이트가 초당 60회 이상은 실행안된다.
    private void LateUpdate()
    {
        //RefreshHealth(); // 폴링

        _staminaSlider.value = _stamina.Stamina / _stamina.MaxStamina;

        // 빌보드: 체력바가 카메라를 쳐다보게 한다.
        // transform.forward = Camera.main.transform.forward;
    }

    private void RefreshHealth()
    {
        // 문제: 체력값이 변하지 않았음에도 초당 60회 이상 캔버스를 불필요하게 다시 그린다. -> 성능 낭비가 일어난다.
        // 이거 하나 정도는 괜찮아 보이지만.. 이런 불필요한 연산이 다른 UI에서 누적될수록 느려진다.
        // 해결: 값이 바뀌었을 때만 캔버스 갱신을 한다.

        // 이때 쓰는게: 옵저버 패턴
        // 옵저버 패턴은 한 객체(Subject)의 상태가 바뀌면 이를 구독한 여러 객체(Observer)에게 자동으로 알려주는 패턴으로, 서로 직접 의존하지 않고 느슨하게 연결됩니다.
        // 유튜브 구독 패턴은 유튜버가 영상을 올릴 때 이를 구독 중인 여러 구독자에게 알려주는 패턴이다.
        // C#에서는 이것을 Action으로 구현할 수 있다.

        _healthSlider.value = _player.Health / _player.MaxHealth;
    }
}