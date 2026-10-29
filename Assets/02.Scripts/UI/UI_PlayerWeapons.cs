using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 오른쪽 아래에 현재 무기 모드, 무기 아이콘, 남은 총알, 남은 폭탄을 표시한다.
public class UI_PlayerWeapons : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Player _player;

    [SerializeField] private PlayerGunFire _gunFire;
    [SerializeField] private PlayerBombFire _bombFire;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _weaponModeText;

    [SerializeField] private Image _weaponIcon;
    [SerializeField] private TextMeshProUGUI _magazineText;
    [SerializeField] private TextMeshProUGUI _bombText;

    [Header("모드별 무기 아이콘")]
    [SerializeField] private Sprite _normalSprite;

    [SerializeField] private Sprite _sniperSprite;

    private void Awake()
    {
        if (_player == null) _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        if (_gunFire == null) _gunFire = _player.GetComponent<PlayerGunFire>();
        if (_bombFire == null) _bombFire = _player.GetComponent<PlayerBombFire>();
    }

    private void OnEnable()
    {
        _player.OnWeaponModeChanged += RefreshMode;
        _gunFire.OnBulletChanged += RefreshMagazine;
        _bombFire.OnBombChanged += RefreshBomb;
    }

    private void OnDisable()
    {
        _player.OnWeaponModeChanged -= RefreshMode;
        _gunFire.OnBulletChanged -= RefreshMagazine;
        _bombFire.OnBombChanged -= RefreshBomb;
    }

    // 이벤트는 값이 바뀔 때만 오므로 초기 표시는 직접 한 번 그린다.
    private void Start()
    {
        RefreshMode(_player.WeaponMode);
        RefreshMagazine(_gunFire.BulletCount, _gunFire.MaxBulletCount);
        RefreshBomb(_bombFire.RemainBomb);
    }

    private void RefreshMode(WeaponMode mode)
    {
        bool isSniper = mode == WeaponMode.Sniper;
        _weaponModeText.text = isSniper ? "저격 모드" : "일반 모드";
        _weaponIcon.sprite = isSniper ? _sniperSprite : _normalSprite;
    }

    private void RefreshMagazine(int current, int max)
    {
        _magazineText.text = $"{current} / {max}";
    }

    private void RefreshBomb(int remain)
    {
        _bombText.text = $"폭탄 {remain}";
    }
}
