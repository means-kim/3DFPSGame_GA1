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

    private void LateUpdate()
    {
        // 0 ~ 100% 0 ~ 1
        _healthSlider.value = _player.Health / _player.MaxHealth;
        _staminaSlider.value = _stamina.Stamina / _stamina.MaxStamina;

        // 빌보드: 체력바가 카메라를 쳐다보게 한다.
        // transform.forward = Camera.main.transform.forward;
    }
}