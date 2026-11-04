using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Loading : MonoBehaviour
{
    private const string DefaultSceneName = "GameScene";
    private const float ReadyProgress = 0.9f; // 비동기 로드는 allowSceneActivation이 false면 0.9에서 멈춘다.

    [Header("UI")]
    [SerializeField] private Slider _progressSlider;

    [SerializeField] private TextMeshProUGUI _progressText;

    [Header("설정")]
    [SerializeField] private float _fillSpeed = 1f; // 초당 채워지는 비율 (너무 빨리 끝나도 바가 부드럽게 차도록)

    private void Start()
    {
        StartCoroutine(Load_Coroutine());
    }

    private IEnumerator Load_Coroutine()
    {
        Application.backgroundLoadingPriority = ThreadPriority.Low;

        // SceneLoader가 넘겨준 다음 씬. 로딩 씬을 직접 실행했을 때는 기본값 사용
        string sceneName = string.IsNullOrEmpty(SceneLoader.NextSceneName)
            ? DefaultSceneName
            : SceneLoader.NextSceneName;

        SetProgress(0f);

        // 비동기 씬 로드. 반환값으로 진행 상황을 확인한다.
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // 100%를 보여준 뒤에 씬을 전환하기 위해 자동 활성화를 막아둔다.
        operation.allowSceneActivation = false;

        float displayed = 0f;
        while (displayed < 1f)
        {
            // 0 ~ 0.9 구간을 0 ~ 1로 환산 (준비가 끝나면 부동소수점 오차 없이 1로 맞춘다)
            float target = operation.progress >= ReadyProgress ? 1f : operation.progress / ReadyProgress;
            displayed = Mathf.MoveTowards(displayed, target, _fillSpeed * Time.unscaledDeltaTime);
            SetProgress(displayed);

            yield return null;
        }

        // 100% 표시 후 씬 전환
        operation.allowSceneActivation = true;
    }

    private void SetProgress(float progress)
    {
        _progressSlider.value = progress;
        _progressText.text = $"불러오는 중 {Mathf.RoundToInt(progress * 100f)}%";
    }
}
