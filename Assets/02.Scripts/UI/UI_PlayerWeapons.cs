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

    // 값이 바뀔 때만 텍스트를 갱신하기 위한 이전 값
    private WeaponMode? _lastMode = null;
    private int _lastBullet = -1;
    private int _lastMaxBullet = -1;
    private int _lastBomb = -1;

    private void Awake()
    {
        if (_player == null) _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        if (_gunFire == null) _gunFire = _player.GetComponent<PlayerGunFire>();
        if (_bombFire == null) _bombFire = _player.GetComponent<PlayerBombFire>();
    }

    private void Update()
    {
        RefreshMode();
        RefreshMagazine();
        RefreshBomb();
    }

    private void RefreshMode()
    {
        if (_lastMode == _player.WeaponMode) return;

        _lastMode = _player.WeaponMode;

        bool isSniper = _lastMode == WeaponMode.Sniper;
        _weaponModeText.text = isSniper ? "저격 모드" : "일반 모드";
        _weaponIcon.sprite = isSniper ? _sniperSprite : _normalSprite;
    }

    private void RefreshMagazine()
    {
        if (_lastBullet == _gunFire.BulletCount && _lastMaxBullet == _gunFire.MaxBulletCount) return;

        _lastBullet = _gunFire.BulletCount;
        _lastMaxBullet = _gunFire.MaxBulletCount;
        _magazineText.text = $"{_lastBullet} / {_lastMaxBullet}";
    }

    private void RefreshBomb()
    {
        if (_lastBomb == _bombFire.RemainBomb) return;

        _lastBomb = _bombFire.RemainBomb;
        _bombText.text = $"폭탄 {_lastBomb}";
    }
}
