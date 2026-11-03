using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 게임 시작 전환: 화면이 해 쪽으로 빨려 들어가며 하얗게 번진 뒤 다음 씬으로 넘어간다.
// 로그인에 성공했을 때 로그인 로직에서 PlayStart()를 호출한다.
public class UI_SceneTransition : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private UI_BackgroundMotion _background;
    [SerializeField] private UI_SunFlare _sunFlare;
    [SerializeField] private Image _fader;
    [SerializeField] private CanvasGroup _panel;

    [Header("전환")]
    [SerializeField] private string _sceneName = "GameScene";
    [SerializeField] private Vector2 _sunUv = new Vector2(0.571f, 0.654f);
    [SerializeField] private float _zoom = 6f;
    [SerializeField] private float _zoomDuration = 1.2f;
    [SerializeField] private float _whiteStart = 0.55f;    // 확대 진행 비율이 이만큼일 때 하얗게 덮기 시작
    [SerializeField] private float _whiteDuration = 0.8f;
    [SerializeField] private Color _flashColor = new Color(1f, 0.97f, 0.9f, 1f);

    private bool _isPlaying;

    public void PlayStart()
    {
        PlayStart(_sceneName);
    }

    public void PlayStart(string sceneName)
    {
        if (_isPlaying) return;
        _isPlaying = true;

        // 전환 중에는 아무것도 누르지 못하게 한다.
        _panel.interactable = false;
        _panel.blocksRaycasts = false;

        _fader.DOKill();
        _fader.gameObject.SetActive(true);
        _fader.raycastTarget = true;
        _fader.color = new Color(_flashColor.r, _flashColor.g, _flashColor.b, 0f);

        DOTween.Sequence()
            .Append(_background.ZoomTo(_sunUv, _zoom, _zoomDuration))
            .Join(_sunFlare.PlayFlash(_zoomDuration))
            .Insert(_zoomDuration * _whiteStart, _fader.DOFade(1f, _whiteDuration).SetEase(Ease.InQuad))
            .OnComplete(() => SceneManager.LoadSceneAsync(sceneName))
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    [ContextMenu("게임 시작 전환 테스트")]
    private void PlayStartTest()
    {
        PlayStart(_sceneName);
    }
}
