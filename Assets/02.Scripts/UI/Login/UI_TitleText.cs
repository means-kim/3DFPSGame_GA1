using TMPro;
using UnityEngine;

// 제목 글자를 하나씩 파도치듯 움직이고(웨이브), 주기적으로 노을빛 하이라이트가 글자를 쓸고 지나가게 한다(샤인).
// 둘 다 TMP 메쉬 정점을 고치므로, 서로 덮어쓰지 않게 한 컴포넌트에서 처리한다.
[RequireComponent(typeof(TMP_Text))]
public class UI_TitleText : MonoBehaviour
{
    [Header("웨이브")]
    [SerializeField] private float _waveAmplitude = 8f;  // 위아래 움직임(px)
    [SerializeField] private float _waveSpeed = 2.5f;
    [SerializeField] private float _wavePhase = 0.6f;    // 글자마다 엇갈리는 정도

    [Header("샤인")]
    [SerializeField] private Color _shineColor = new Color(1f, 0.8f, 0.45f, 1f);
    [SerializeField] private float _shineInterval = 4f;  // 다음 샤인까지 걸리는 시간(지나가는 시간 포함)
    [SerializeField] private float _shineDuration = 0.9f;
    [SerializeField] private float _shineWidth = 120f;   // 띠의 너비(px)
    [SerializeField] private float _shineSlant = 0.5f;   // 띠의 기울기

    private TMP_Text _text;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    private void LateUpdate()
    {
        // 매 프레임 원래 메쉬로 되돌린 뒤 그 위에 움직임을 더한다.
        _text.ForceMeshUpdate();
        TMP_TextInfo textInfo = _text.textInfo;
        if (textInfo.characterCount == 0) return;

        float time = Time.unscaledTime;
        Bounds bounds = _text.textBounds;

        // 샤인 띠의 현재 x 위치. 지나가는 시간이 아니면 화면 밖에 둔다.
        float shineTime = time % _shineInterval;
        float shineProgress = shineTime / _shineDuration;
        bool isShining = shineProgress <= 1f;
        float shineX = Mathf.Lerp(bounds.min.x - _shineWidth, bounds.max.x + _shineWidth, shineProgress);

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
            Color32[] colors = textInfo.meshInfo[materialIndex].colors32;

            Vector3 offset = new Vector3(0f, Mathf.Sin(time * _waveSpeed + i * _wavePhase) * _waveAmplitude, 0f);

            // 글자 한 개는 정점 4개로 이루어진다.
            for (int j = 0; j < 4; j++)
            {
                int index = vertexIndex + j;
                vertices[index] += offset;

                if (isShining)
                {
                    float distance = Mathf.Abs(vertices[index].x + vertices[index].y * _shineSlant - shineX);
                    float amount = 1f - Mathf.Clamp01(distance / _shineWidth);
                    colors[index] = Color32.Lerp(colors[index], _shineColor, amount);
                }
            }
        }

        _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32);
    }
}
