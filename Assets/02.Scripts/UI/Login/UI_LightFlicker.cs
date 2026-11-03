using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 창문·램프 불빛이 숨쉬듯 은은하게 밝기가 변하고, 가끔 형광등처럼 지직거리며 깜빡인다.
[RequireComponent(typeof(Image))]
public class UI_LightFlicker : MonoBehaviour
{
    [Header("숨쉬기")]
    [SerializeField] private float _breathSpeed = 0.8f;
    [SerializeField] private float _breathAmount = 0.15f; // 원래 밝기에서 최대 얼마나 어두워질지(비율)

    [Header("깜빡임")]
    [SerializeField] private Vector2 _flickerInterval = new Vector2(3f, 10f); // 다음 깜빡임까지의 시간(초)
    [SerializeField] private Vector2Int _flickerCount = new Vector2Int(2, 5); // 한 번에 지직거리는 횟수
    [SerializeField] private float _flickerOffAlpha = 0.15f;                  // 꺼졌을 때 밝기(비율)

    private Image _image;
    private float _baseAlpha;
    private float _phase;
    private float _flickerMultiplier = 1f;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _baseAlpha = _image.color.a;
        _phase = Random.Range(0f, Mathf.PI * 2f); // 불빛마다 다른 박자로 숨쉬게 한다
    }

    private void OnEnable()
    {
        StartCoroutine(Flicker_Coroutine());
    }

    private void Update()
    {
        float breath = 1f - _breathAmount * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * _breathSpeed + _phase));

        Color color = _image.color;
        color.a = _baseAlpha * breath * _flickerMultiplier;
        _image.color = color;
    }

    private IEnumerator Flicker_Coroutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(Random.Range(_flickerInterval.x, _flickerInterval.y));

            int count = Random.Range(_flickerCount.x, _flickerCount.y + 1);
            for (int i = 0; i < count; i++)
            {
                _flickerMultiplier = Random.Range(_flickerOffAlpha, 0.5f);
                yield return new WaitForSecondsRealtime(Random.Range(0.03f, 0.08f));

                _flickerMultiplier = 1f;
                yield return new WaitForSecondsRealtime(Random.Range(0.03f, 0.12f));
            }
        }
    }
}
