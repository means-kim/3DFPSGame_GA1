using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class UI_HitEffect : MonoBehaviour
{
    [SerializeField] private float _flashTime = 0.6f;
    [SerializeField] private float _fadeTime = 0.3f;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _image.enabled = false;
    }

    public void Flash(float alpha)
    {
        SetAlpha(alpha);

        StartCoroutine(Flash_Coroutine(alpha));
    }

    private IEnumerator Flash_Coroutine(float alpha)
    {
        _image.enabled = true;
        _image.canvasRenderer.SetAlpha(0f);

        yield return new WaitForSeconds(_flashTime);

        _image.enabled = false;
    }

    public void Fade(float alpha)
    {
        SetAlpha(alpha);

        StartCoroutine(Fade_Coroutine());
    }

    private IEnumerator Fade_Coroutine()
    {
        _image.enabled = true;

        float elpasedTime = 0f;

        float startAlpha = _image.color.a;

        while (elpasedTime < _fadeTime)
        {
            float alpha = Mathf.Lerp(startAlpha, 0f, elpasedTime / _fadeTime);

            SetAlpha(alpha);

            elpasedTime += Time.deltaTime;
            yield return null; // 한 프레임씩 쉰다.
        }

        SetAlpha(0f);
        _image.enabled = false;
    }

    private void SetAlpha(float alpha)
    {
        Color color = _image.color;
        color.a = alpha;
        _image.color = color;
    }
}